namespace GameFoundation.Scripts.Features.Rating.Services
{
    using System;
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using UnityEngine;
#if UNITY_ANDROID && !UNITY_EDITOR
    using Google.Play.Review;
#endif

    /// <summary>
    /// Google Play in-app review flow.
    /// A completed flow does not indicate that the review dialog appeared or that the player rated.
    /// </summary>
    public class GooglePlayRatingService : IRatingService
    {
        private const string LogPrefix = "[Rating]";
        private const double DefaultTimeoutSeconds = 10d;

        private readonly TimeSpan timeout;

        public GooglePlayRatingService() : this(TimeSpan.FromSeconds(DefaultTimeoutSeconds)) { }

        public GooglePlayRatingService(TimeSpan timeout)
        {
            this.timeout = timeout;
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        public bool IsInAppReviewAvailable => true;

        public async UniTask<ReviewFlowResult> RequestReviewAsync(CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) return ReviewFlowResult.Cancelled;

            try
            {
                var manager = new ReviewManager();
                var request = manager.RequestReviewFlow();
                var requestResult = await this.WaitForAsync(() => request.IsDone, cancellationToken);
                if (requestResult != ReviewFlowResult.Completed) return requestResult;

                if (request.Error != ReviewErrorCode.NoError)
                {
                    Debug.LogWarning($"{LogPrefix} Play review request failed: {request.Error}.");
                    return MapError(request.Error);
                }

                PlayReviewInfo reviewInfo = request.GetResult();
                if (cancellationToken.IsCancellationRequested) return ReviewFlowResult.Cancelled;

                var launch = manager.LaunchReviewFlow(reviewInfo);
                // Once the sheet is launched, wait for Play to finish. The player may keep it open
                // longer than the request timeout.
                var launchResult = await this.WaitForAsync(() => launch.IsDone, cancellationToken, false);
                if (launchResult != ReviewFlowResult.Completed) return launchResult;

                if (launch.Error != ReviewErrorCode.NoError)
                {
                    Debug.LogWarning($"{LogPrefix} Play review flow failed: {launch.Error}.");
                    return MapError(launch.Error);
                }

                return ReviewFlowResult.Completed;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"{LogPrefix} The Play review flow failed: {e.Message}");
                return ReviewFlowResult.Failed;
            }
        }

        private static ReviewFlowResult MapError(ReviewErrorCode error)
        {
            return error == ReviewErrorCode.PlayStoreNotFound
                ? ReviewFlowResult.Unavailable
                : ReviewFlowResult.Failed;
        }

        private async UniTask<ReviewFlowResult> WaitForAsync(
            Func<bool> isDone, CancellationToken cancellationToken, bool useTimeout = true)
        {
            var deadline = Time.realtimeSinceStartupAsDouble + this.timeout.TotalSeconds;
            while (true)
            {
                if (cancellationToken.IsCancellationRequested) return ReviewFlowResult.Cancelled;
                if (isDone()) return ReviewFlowResult.Completed;
                if (useTimeout && Time.realtimeSinceStartupAsDouble >= deadline)
                {
                    Debug.LogWarning($"{LogPrefix} Play did not answer within {this.timeout.TotalSeconds:0.#}s.");
                    return ReviewFlowResult.TimedOut;
                }

                await UniTask.Yield(PlayerLoopTiming.Update);
            }
        }

#else
        public bool IsInAppReviewAvailable => false;

        public UniTask<ReviewFlowResult> RequestReviewAsync(CancellationToken cancellationToken = default)
        {
            return UniTask.FromResult(cancellationToken.IsCancellationRequested
                ? ReviewFlowResult.Cancelled
                : ReviewFlowResult.Unavailable);
        }

#endif
    }
}
