namespace GameFoundation.Scripts.Features.Rating.Services
{
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using UnityEngine;

    /// <summary>
    /// The App Store front: <c>SKStoreReviewController</c> for the sheet, through Unity's own
    /// binding (<c>UnityEngine.iOS.Device.RequestStoreReview</c>), so no native plugin is needed.
    /// <para>
    /// iOS is even quieter than Play: the request is handed to the system, which shows the sheet
    /// when it feels like it - at most three times a year per user - and reports neither the
    /// showing nor the rating. <see cref="ReviewFlowResult.Completed"/> means "asked".
    /// </para>
    /// </summary>
    public class AppStoreRatingService : IRatingService
    {
        private const string LogPrefix = "[Rating]";

#if UNITY_IOS && !UNITY_EDITOR

        public bool IsInAppReviewAvailable => true;

        public UniTask<ReviewFlowResult> RequestReviewAsync(CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return UniTask.FromResult(ReviewFlowResult.Cancelled);
            }

            // Returns whether the request reached the system, never what the player did with it.
            // There is nothing to await: iOS decides when, and whether, to show its own sheet.
            var requested = UnityEngine.iOS.Device.RequestStoreReview();
            if (!requested)
            {
                Debug.LogWarning($"{LogPrefix} iOS declined the review request.");
            }

            return UniTask.FromResult(requested ? ReviewFlowResult.Completed : ReviewFlowResult.Failed);
        }

#else

        /// <summary>Only an iOS player has the App Store sheet.</summary>
        public bool IsInAppReviewAvailable => false;

        public UniTask<ReviewFlowResult> RequestReviewAsync(CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return UniTask.FromResult(ReviewFlowResult.Cancelled);
            }

            Debug.Log($"{LogPrefix} The App Store review sheet only runs in an iOS player.");
            return UniTask.FromResult(ReviewFlowResult.Unavailable);
        }

#endif

    }
}
