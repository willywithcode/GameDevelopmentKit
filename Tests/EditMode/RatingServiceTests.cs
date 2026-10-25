namespace GameFoundation.Scripts.Tests.EditMode
{
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Features.Rating.Services;
    using NUnit.Framework;

    /// <summary>
    /// The native Play and App Store flows require a device; these tests cover the stand-in
    /// service used when no native review flow is available.
    /// </summary>
    public class RatingServiceTests
    {
        [Test]
        public void TheDummyService_AnswersUnavailable()
        {
            var dummy = new DummyRatingService();

            Assert.IsFalse(dummy.IsInAppReviewAvailable);
            Assert.AreEqual(ReviewFlowResult.Unavailable, Run(dummy.RequestReviewAsync()));
        }

        [Test]
        public void TheDummyService_ReportsACancelledRequest()
        {
            using var cancelled = new CancellationTokenSource();
            cancelled.Cancel();

            Assert.AreEqual(ReviewFlowResult.Cancelled, Run(new DummyRatingService().RequestReviewAsync(cancelled.Token)));
        }

        private static ReviewFlowResult Run(UniTask<ReviewFlowResult> request) => request.GetAwaiter().GetResult();
    }
}
