namespace GameFoundation.Scripts.Tests.EditMode
{
    using GameFoundation.Scripts.Features.InternetChecking.Services;

    internal class FakeReachability : IInternetReachability
    {
        public bool IsReachable { get; set; }
    }

    internal class FakeSettingsOpener : INetworkSettingsOpener
    {
        public int  OpenCount { get; private set; }
        public bool Succeeds  { get; set; } = true;

        public bool Open()
        {
            this.OpenCount++;
            return this.Succeeds;
        }
    }
}
