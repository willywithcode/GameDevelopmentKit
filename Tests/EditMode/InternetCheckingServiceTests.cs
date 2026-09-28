namespace GameFoundation.Scripts.Tests.EditMode
{
    using GameFoundation.Scripts.Features.InternetChecking.Services;
    using NUnit.Framework;

    public class InternetCheckingServiceTests
    {
        private FakeReachability        reachability;
        private InternetCheckingService service;

        [SetUp]
        public void SetUp()
        {
            this.reachability = new FakeReachability { IsReachable = true };
            this.service      = new InternetCheckingService(this.reachability);
        }

        [Test]
        public void Sample_ReportsLostOnlyOnTheThirdConsecutiveOfflineSample()
        {
            this.reachability.IsReachable = false;

            Assert.AreEqual(ConnectivityChange.None, this.service.Sample());
            Assert.AreEqual(ConnectivityChange.None, this.service.Sample());
            Assert.AreEqual(ConnectivityChange.Lost, this.service.Sample());
            Assert.IsTrue(this.service.IsFirmlyOffline);
        }

        [Test]
        public void Sample_ReportsLostOncePerOutage()
        {
            this.reachability.IsReachable = false;
            this.SampleTimes(3);

            for (var i = 0; i < 5; i++) Assert.AreEqual(ConnectivityChange.None, this.service.Sample());
        }

        [Test]
        public void Sample_OnlineBlipResetsTheCount()
        {
            this.reachability.IsReachable = false;
            this.SampleTimes(2);
            this.reachability.IsReachable = true;
            this.service.Sample();
            this.reachability.IsReachable = false;

            Assert.AreEqual(ConnectivityChange.None, this.service.Sample());
            Assert.AreEqual(ConnectivityChange.None, this.service.Sample());
            Assert.AreEqual(ConnectivityChange.Lost, this.service.Sample());
        }

        [Test]
        public void Sample_ReportsRestoredOnTheFirstOnlineSampleAfterAnOutage()
        {
            this.reachability.IsReachable = false;
            this.SampleTimes(3);
            this.reachability.IsReachable = true;

            Assert.AreEqual(ConnectivityChange.Restored, this.service.Sample());
            Assert.IsFalse(this.service.IsFirmlyOffline);
            Assert.AreEqual(ConnectivityChange.None, this.service.Sample());
        }

        [Test]
        public void ForceOffline_ReadsAsOfflineOnAConnectedDevice()
        {
            this.service.ForceOffline = true;

            Assert.IsFalse(this.service.IsOnline);
            this.SampleTimes(2);
            Assert.AreEqual(ConnectivityChange.Lost, this.service.Sample());
        }

        private void SampleTimes(int times)
        {
            for (var i = 0; i < times; i++) this.service.Sample();
        }
    }
}
