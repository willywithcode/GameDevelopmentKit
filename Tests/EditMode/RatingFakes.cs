namespace GameFoundation.Scripts.Tests.EditMode
{
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Features.Rating.Services;

    /// <summary>
    /// An <see cref="IRatingService"/> whose answer is scripted, so the store fallback rule can be
    /// asserted without a store. Completes synchronously: every test below reads the result
    /// straight off the returned UniTask.
    /// </summary>
    internal class FakeRatingService : IRatingService
    {
        public ReviewFlowResult NextResult { get; set; } = ReviewFlowResult.Completed;

        public bool IsInAppReviewAvailable { get; set; } = true;

        public int RequestCount   { get; private set; }
        public int StorePageCount { get; private set; }

        public bool StorePageOpens { get; set; } = true;

        public UniTask<ReviewFlowResult> RequestReviewAsync(CancellationToken cancellationToken = default)
        {
            this.RequestCount++;
            return UniTask.FromResult(cancellationToken.IsCancellationRequested ? ReviewFlowResult.Cancelled : this.NextResult);
        }

        public bool OpenStorePage()
        {
            this.StorePageCount++;
            return this.StorePageOpens;
        }
    }
}
