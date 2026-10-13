namespace GameFoundation.Scripts.Features.Rating.Services
{
    using System;
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using UnityEngine;

    /// <summary>
    /// The Google Play store front: the In-App Reviews flow for the sheet, and the Play listing as
    /// the fallback the API's own guide recommends for an explicit Rate button.
    /// <para>
    /// Play answers one thing only - the flow finished - so <see cref="ReviewFlowResult.Completed"/>
    /// never means the player rated. The sheet is also quota'd per user: a spent quota is a silent
    /// success.
    /// </para>
    /// Requires <c>com.google.android.play:review</c> in the Android build. Without it the JNI
    /// lookup fails and every request answers <see cref="ReviewFlowResult.Unavailable"/>, so the
    /// store page keeps working.
    /// </summary>
    public class GooglePlayRatingService : IRatingService
    {
        private const string LogPrefix               = "[Rating]";
        private const string ReviewManagerFactory    = "com.google.android.play.core.review.ReviewManagerFactory";
        private const string OnCompleteListener      = "com.google.android.gms.tasks.OnCompleteListener";
        private const string UnityPlayer             = "com.unity3d.player.UnityPlayer";
        private const string MarketUrlFormat         = "market://details?id={0}";
        private const string WebListingUrlFormat     = "https://play.google.com/store/apps/details?id={0}";

        /// <summary>
        /// How long Play may take to answer either half of the flow, measured in real time.
        /// A finished task is checked before the deadline when the Unity player loop resumes.
        /// </summary>
        private const double DefaultTimeoutSeconds = 10d;

#if UNITY_ANDROID && !UNITY_EDITOR
        private readonly TimeSpan timeout;

        private bool? isLibraryPresent;
#endif

        public GooglePlayRatingService() : this(TimeSpan.FromSeconds(DefaultTimeoutSeconds)) { }

        public GooglePlayRatingService(TimeSpan timeout)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            this.timeout = timeout;
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR

        /// <summary>
        /// Whether the review library is in this build. Cached: the answer cannot change at
        /// runtime. It says nothing about the quota - only a request shows that.
        /// </summary>
        public bool IsInAppReviewAvailable
        {
            get
            {
                if (this.isLibraryPresent is { } present) return present;

                try
                {
                    using var factory = new AndroidJavaClass(ReviewManagerFactory);
                    this.isLibraryPresent = factory != null;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"{LogPrefix} The Play review library is missing from this build: {e.Message}");
                    this.isLibraryPresent = false;
                }

                return this.isLibraryPresent.Value;
            }
        }

        public async UniTask<ReviewFlowResult> RequestReviewAsync(CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) return ReviewFlowResult.Cancelled;
            if (!this.IsInAppReviewAvailable) return ReviewFlowResult.Unavailable;

            try
            {
                using var player   = new AndroidJavaClass(UnityPlayer);
                using var activity = player.GetStatic<AndroidJavaObject>("currentActivity");
                using var factory  = new AndroidJavaClass(ReviewManagerFactory);
                using var manager  = factory.CallStatic<AndroidJavaObject>("create", activity);

                using var request = new TaskWatcher();
                using (var requestTask = manager.Call<AndroidJavaObject>("requestReviewFlow"))
                {
                    requestTask.Call<AndroidJavaObject>("addOnCompleteListener", request)?.Dispose();
                }

                var requested = await this.WaitForAsync(request, cancellationToken);
                if (requested != ReviewFlowResult.Completed) return requested;

                if (!request.Task.Call<bool>("isSuccessful"))
                {
                    Debug.LogWarning($"{LogPrefix} Play refused the review request. No sheet is shown.");
                    return ReviewFlowResult.Failed;
                }

                using var reviewInfo = request.Task.Call<AndroidJavaObject>("getResult");

                // launchReviewFlow must run on the Android UI thread, and the ReviewInfo is single
                // use and short lived, so it is launched immediately after it arrives.
                if (cancellationToken.IsCancellationRequested) return ReviewFlowResult.Cancelled;

                using var launch = new TaskWatcher();
                QueueReviewLaunch(activity, manager, reviewInfo, launch, cancellationToken);

                var launched = await this.WaitForAsync(launch, cancellationToken);
                if (launched != ReviewFlowResult.Completed) return launched;
                if (launch.HasFailed || !launch.Task.Call<bool>("isSuccessful"))
                {
                    Debug.LogWarning($"{LogPrefix} Play could not complete the review flow.");
                    return ReviewFlowResult.Failed;
                }

                // Whether the sheet appeared, and what the player did with it, is never reported.
                return ReviewFlowResult.Completed;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"{LogPrefix} The Play review flow failed: {e.Message}");
                return ReviewFlowResult.Failed;
            }
        }

        public bool OpenStorePage()
        {
            var packageName = Application.identifier;
            if (string.IsNullOrWhiteSpace(packageName))
            {
                Debug.LogWarning($"{LogPrefix} This build has no package name, so the Play listing cannot be opened.");
                return false;
            }

            // The Play app first, the web listing second: a device without the Play app still ends
            // up on the listing in a browser.
            return TryViewUri(string.Format(MarketUrlFormat, packageName))
                || TryViewUri(string.Format(WebListingUrlFormat, packageName));
        }

        /// <summary>
        /// Polls on the Unity main thread instead of resuming inside the Java callback: a proxy
        /// call arrives on the Android UI thread. Polling stops while Unity is paused; the real-time
        /// deadline still advances, so check cancellation and completion before checking timeout.
        /// </summary>
        private async UniTask<ReviewFlowResult> WaitForAsync(TaskWatcher watcher, CancellationToken cancellationToken)
        {
            var deadline = Time.realtimeSinceStartupAsDouble + this.timeout.TotalSeconds;

            while (true)
            {
                if (cancellationToken.IsCancellationRequested) return ReviewFlowResult.Cancelled;
                if (watcher.IsFinished) return ReviewFlowResult.Completed;

                if (Time.realtimeSinceStartupAsDouble >= deadline)
                {
                    Debug.LogWarning($"{LogPrefix} Play did not answer within {this.timeout.TotalSeconds:0.#}s.");
                    return ReviewFlowResult.TimedOut;
                }

                await UniTask.Yield(PlayerLoopTiming.Update);
            }
        }

        private static void QueueReviewLaunch(
            AndroidJavaObject activity,
            AndroidJavaObject manager,
            AndroidJavaObject reviewInfo,
            TaskWatcher       launch,
            CancellationToken cancellationToken
        )
        {
            // The UI runnable owns independent JNI references. The caller may finish and dispose
            // its originals before this runnable executes, or while launchReviewFlow is running.
            AndroidJavaObject launchActivity   = null;
            AndroidJavaObject launchManager    = null;
            AndroidJavaObject launchReviewInfo = null;

            try
            {
                launchActivity   = activity.CloneReference();
                launchManager    = manager.CloneReference();
                launchReviewInfo = reviewInfo.CloneReference();

                activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                {
                    using (launchActivity)
                    using (launchManager)
                    using (launchReviewInfo)
                    {
                        // A cancelled or timed-out caller must not start a queued review later.
                        // Once launch has started, Play owns the native flow and cannot cancel it.
                        if (cancellationToken.IsCancellationRequested || launch.IsDisposed) return;

                        try
                        {
                            using var launchTask = launchManager.Call<AndroidJavaObject>(
                                "launchReviewFlow", launchActivity, launchReviewInfo);
                            launchTask.Call<AndroidJavaObject>("addOnCompleteListener", launch)?.Dispose();
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning($"{LogPrefix} Play could not launch the review flow: {e.Message}");
                            launch.Fail();
                        }
                    }
                }));
            }
            catch
            {
                // Cloning or dispatch failed, so no runnable owns these references.
                launchActivity?.Dispose();
                launchManager?.Dispose();
                launchReviewInfo?.Dispose();
                throw;
            }
        }

        private static bool TryViewUri(string url)
        {
            try
            {
                using var player   = new AndroidJavaClass(UnityPlayer);
                using var activity = player.GetStatic<AndroidJavaObject>("currentActivity");
                using var uriClass = new AndroidJavaClass("android.net.Uri");
                using var uri      = uriClass.CallStatic<AndroidJavaObject>("parse", url);
                using var intent   = new AndroidJavaObject("android.content.Intent", "android.intent.action.VIEW", uri);
                activity.Call("startActivity", intent);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"{LogPrefix} Could not open {url}: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// One Play task's completion. Java calls <c>onComplete</c> on the Android UI thread, so it
        /// only stores the task and raises a flag; everything else happens back on the Unity main
        /// thread in <see cref="WaitForAsync"/>.
        /// </summary>
        private sealed class TaskWatcher : AndroidJavaProxy, IDisposable
        {
            private readonly object           sync = new object();
            private volatile bool             disposed;
            private volatile bool             finished;
            private          AndroidJavaObject completedTask;
            private volatile bool             failed;

            public TaskWatcher() : base(OnCompleteListener) { }

            public bool              IsDisposed => this.disposed;
            public bool              IsFinished => this.finished;
            public bool              HasFailed  => this.failed;
            public AndroidJavaObject Task       => this.completedTask;

            public void Fail()
            {
                lock (this.sync)
                {
                    if (this.disposed) return;
                    this.failed   = true;
                    this.finished = true;
                }
            }

            public void onComplete(AndroidJavaObject task)
            {
                lock (this.sync)
                {
                    if (this.disposed)
                    {
                        task.Dispose();
                        return;
                    }

                    this.completedTask = task;
                    this.finished      = true;
                }
            }

            public void Dispose()
            {
                lock (this.sync)
                {
                    this.disposed = true;
                    this.completedTask?.Dispose();
                    this.completedTask = null;
                }
            }
        }

#else

        /// <summary>Only an Android player has Play; the Editor and other platforms have no sheet.</summary>
        public bool IsInAppReviewAvailable => false;

        public UniTask<ReviewFlowResult> RequestReviewAsync(CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return UniTask.FromResult(ReviewFlowResult.Cancelled);
            }

            Debug.Log($"{LogPrefix} The Play review sheet only runs in an Android player.");
            return UniTask.FromResult(ReviewFlowResult.Unavailable);
        }

        /// <summary>
        /// The web listing, which is the half of the store front that works off-device: it opens in
        /// the Editor's browser, which is enough to check the link a build will use.
        /// </summary>
        public bool OpenStorePage()
        {
            var packageName = Application.identifier;
            if (string.IsNullOrWhiteSpace(packageName))
            {
                Debug.LogWarning($"{LogPrefix} This build has no package name, so the Play listing cannot be opened.");
                return false;
            }

            Application.OpenURL(string.Format(WebListingUrlFormat, packageName));
            return true;
        }

#endif
    }
}
