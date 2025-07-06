namespace GameFoundation.Scripts.Features.InternetChecking.Services
{
    using Cysharp.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Events;

    public class InternetCheckingService
    {
        private int count                 = 0;
        private int maxCountToFirmlyCheck = 3;

        public async UniTask StartCheckingInternetAsync(UnityAction onNoInternet, UnityAction onHasInternet)
        {
            if (this.CheckInternet())
                this.count = 0;
            else
                this.count++;
            if (this.count >= this.maxCountToFirmlyCheck)
            {
                this.count = 0;
                onNoInternet?.Invoke();
                await this.CheckConnectionAsync(onHasInternet);
            }
            await UniTask.Delay(1000);
            this.StartCheckingInternetAsync(onNoInternet, onHasInternet).Forget();
        }

        private async UniTask CheckConnectionAsync(UnityAction action)
        {
            while (!this.CheckInternet())
            {
                await UniTask.Delay(1000);
            }
            action?.Invoke();
        }

        private bool CheckInternet() => Application.internetReachability != NetworkReachability.NotReachable;
    }
}