namespace GameFoundation.Scripts.Features.AudioSystem.SubServices
{
    using System;
    using GameFoundation.Scripts.Features.AudioSystem.Services;
    using GameFoundation.Scripts.Signals;
    using MessagePipe;
    using VContainer.Unity;

    public class CommonButtonClickSound : IInitializable, IDisposable
    {
        private readonly ISubscriber<OnButtonClickSignal> buttonClickSubscriber;
        private readonly IAudioManagerService             audioManagerService;
        private IDisposable subscription;

        public CommonButtonClickSound(
            ISubscriber<OnButtonClickSignal> buttonClickSubscriber,
            IAudioManagerService             audioManagerService
        )
        {
            this.buttonClickSubscriber = buttonClickSubscriber;
            this.audioManagerService   = audioManagerService;
        }

        public void Initialize()
        {
            this.subscription = this.buttonClickSubscriber.Subscribe(this.OnButtonClick);
        }

        public void Dispose()
        {
            this.subscription?.Dispose();
            this.subscription = null;
        }

        private void OnButtonClick(OnButtonClickSignal obj)
        {
            try
            {
                this.audioManagerService.PlaySfx("tap-click");
            }
            catch (Exception e)
            {
                // UnityEngine.Debug.LogError($"[CommonButtonClickSound] Error when play button click sound: {e}");
            }
        }
    }
}
