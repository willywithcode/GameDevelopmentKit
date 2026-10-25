namespace GameFoundation.Scripts.Features.Rating.Services
{
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using UnityEngine;

    /// <summary>
    /// The last fallback: the Editor, and every platform with no store front of its own. It answers
    /// like a store that declined, so a caller behaves exactly as it will on a device, and it logs
    /// what was asked so a trigger can be tuned without building.
    /// </summary>
    public class DummyRatingService : IRatingService
    {
        private const string LogPrefix = "[Rating]";

        public bool IsInAppReviewAvailable => false;

        public UniTask<ReviewFlowResult> RequestReviewAsync(CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return UniTask.FromResult(ReviewFlowResult.Cancelled);
            }

            Debug.Log($"{LogPrefix} An in-app review was requested. This build has no store front, so nothing is shown.");
            return UniTask.FromResult(ReviewFlowResult.Unavailable);
        }

    }
}
