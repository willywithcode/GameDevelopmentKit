namespace GameFoundation.Scripts.Tests.EditMode
{
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Features.Rating.Services;
    using NUnit.Framework;

    /// <summary>
    /// The rating feature's only testable half: the fallback rule a Rate button relies on, and the
    /// stand-in service every non-store build gets. The Play and App Store flows are native and are
    /// proven on a device instead.
    /// </summary>
    public class RatingServiceTests
    {
        private FakeRatingService service;

        [SetUp]
        public void SetUp() { this.service = new FakeRatingService(); }

        [Test]
        public void ACompletedFlow_DoesNotOpenTheStorePage()
        {
            this.service.NextResult = ReviewFlowResult.Completed;

            var result = Run(this.service.RequestReviewOrOpenStoreAsync());

            Assert.AreEqual(ReviewFlowResult.Completed, result);
            Assert.AreEqual(1, this.service.RequestCount);
            Assert.AreEqual(0, this.service.StorePageCount);
        }

        [TestCase(ReviewFlowResult.Unavailable)]
        [TestCase(ReviewFlowResult.Failed)]
        [TestCase(ReviewFlowResult.TimedOut)]
        public void AFlowThatNeverRan_OpensTheStorePageOnce(ReviewFlowResult outcome)
        {
            this.service.NextResult = outcome;

            var result = Run(this.service.RequestReviewOrOpenStoreAsync());

            // The outcome is reported as it happened, so a caller can still log what was attempted.
            Assert.AreEqual(outcome, result);
            Assert.AreEqual(1, this.service.StorePageCount);
        }

        [Test]
        public void ACancelledRequest_OpensNothing()
        {
            using var cancelled = new CancellationTokenSource();
            cancelled.Cancel();

            var result = Run(this.service.RequestReviewOrOpenStoreAsync(cancelled.Token));

            Assert.AreEqual(ReviewFlowResult.Cancelled, result);
            Assert.AreEqual(0, this.service.StorePageCount);
        }

        [Test]
        public void RequestReviewAsync_NeverOpensTheStorePageItself()
        {
            this.service.NextResult = ReviewFlowResult.Failed;

            var result = Run(this.service.RequestReviewAsync());

            Assert.AreEqual(ReviewFlowResult.Failed, result);
            Assert.AreEqual(0, this.service.StorePageCount);
        }

        [Test]
        public void TheDummyService_AnswersUnavailableAndOpensNothing()
        {
            var dummy = new DummyRatingService();

            Assert.IsFalse(dummy.IsInAppReviewAvailable);
            Assert.AreEqual(ReviewFlowResult.Unavailable, Run(dummy.RequestReviewAsync()));
            Assert.IsFalse(dummy.OpenStorePage());
        }

        [Test]
        public void TheDummyService_ReportsACancelledRequest()
        {
            using var cancelled = new CancellationTokenSource();
            cancelled.Cancel();

            Assert.AreEqual(ReviewFlowResult.Cancelled, Run(new DummyRatingService().RequestReviewAsync(cancelled.Token)));
        }

        /// <summary>
        /// Safe because every service under test answers synchronously; nothing here waits on a
        /// store.
        /// </summary>
        private static ReviewFlowResult Run(UniTask<ReviewFlowResult> request) => request.GetAwaiter().GetResult();
    }
}
