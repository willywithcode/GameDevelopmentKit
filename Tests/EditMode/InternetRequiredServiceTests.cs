namespace GameFoundation.Scripts.Tests.EditMode
{
    using GameFoundation.Scripts.Features.InternetChecking.Services;
    using NUnit.Framework;

    public class InternetRequiredServiceTests
    {
        private FakeReachability        reachability;
        private FakeSettingsOpener      opener;
        private InternetCheckingService checking;
        private InternetRequiredService service;

        [SetUp]
        public void SetUp()
        {
            this.reachability = new FakeReachability { IsReachable = true };
            this.opener       = new FakeSettingsOpener();
            this.checking     = new InternetCheckingService(this.reachability);
            this.service      = new InternetRequiredService(this.checking, this.opener);
        }

        [Test]
        public void Lost_BlocksAndOpensTheSettings()
        {
            this.service.HandleInternetLost();

            Assert.IsTrue(this.service.IsBlocking);
            Assert.AreEqual(1, this.opener.OpenCount);
        }

        [Test]
        public void ReturningStillOffline_OpensTheSettingsAgain()
        {
            this.reachability.IsReachable = false;
            this.service.HandleInternetLost();

            this.service.ReopenIfStillOffline();
            this.service.ReopenIfStillOffline();

            Assert.AreEqual(3, this.opener.OpenCount);
        }

        [Test]
        public void ReturningOnline_DoesNotReopenBeforeTheDebouncedRestore()
        {
            this.reachability.IsReachable = false;
            this.service.HandleInternetLost();
            this.reachability.IsReachable = true;

            this.service.ReopenIfStillOffline();

            Assert.AreEqual(1, this.opener.OpenCount);
        }

        [Test]
        public void AfterRestore_ReturningDoesNotReopen()
        {
            this.reachability.IsReachable = false;
            this.service.HandleInternetLost();
            this.service.HandleInternetRestored();

            this.service.ReopenIfStillOffline();

            Assert.IsFalse(this.service.IsBlocking);
            Assert.AreEqual(1, this.opener.OpenCount);
        }

        [Test]
        public void NeverLost_ReturningDoesNotOpen()
        {
            this.reachability.IsReachable = false;

            this.service.ReopenIfStillOffline();

            Assert.AreEqual(0, this.opener.OpenCount);
        }

        [Test]
        public void OpenerThatFails_StillCountsAsBlockingAndRetriesOnReturn()
        {
            this.opener.Succeeds          = false;
            this.reachability.IsReachable = false;

            Assert.DoesNotThrow(this.service.HandleInternetLost);
            this.service.ReopenIfStillOffline();

            Assert.IsTrue(this.service.IsBlocking);
            Assert.AreEqual(2, this.opener.OpenCount);
        }
    }
}
