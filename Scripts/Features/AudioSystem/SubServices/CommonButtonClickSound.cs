namespace GameFoundation.Scripts.Features.AudioSystem.SubServices
{
    using System;
    using GameFoundation.Scripts.Features.AudioSystem.Services;
    using GameFoundation.Scripts.Patterns.SignalBus;
    using GameFoundation.Scripts.Signals;
    using VContainer.Unity;

    public class CommonButtonClickSound : IInitializable, IDisposable
    {
        private readonly SignalBus            signalBus;
        private readonly IAudioManagerService audioManagerService;

        public CommonButtonClickSound(
            SignalBus            signalBus,
            IAudioManagerService audioManagerService
        )
        {
            this.signalBus           = signalBus;
            this.audioManagerService = audioManagerService;
        }

        public void Initialize()
        {
            this.signalBus.Subscribe<OnButtonClickSignal>(this.OnButtonClick);
        }

        public void Dispose()
        {
            this.signalBus.Unsubscribe<OnButtonClickSignal>(this.OnButtonClick);
        }

        private void OnButtonClick(OnButtonClickSignal obj)
        {
            this.audioManagerService.PlaySfx("tap-click");
        }
    }
}