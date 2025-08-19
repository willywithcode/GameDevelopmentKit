namespace GameFoundation.Scripts.Features.NewDayReset.Services
{
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Features.NewDayReset.LocalDatas;
    using UnityEngine.Events;
    using VContainer.Unity;

    public class NewDayResetService : IAsyncStartable
    {
        private readonly NewDayResetLocalDataService newDayResetLocalDataService;
        public           UnityAction                 OnNewDayReset;

        public NewDayResetService(NewDayResetLocalDataService newDayResetLocalDataService)
        {
            this.newDayResetLocalDataService = newDayResetLocalDataService;
        }

        private void ResetNewDay()
        {
            if (this.newDayResetLocalDataService.IsNewDay())
            {
                this.OnNewDayReset?.Invoke();
            }
            this.newDayResetLocalDataService.SetLastResetDate();
        }

        public async UniTask StartAsync(CancellationToken cancellation = new CancellationToken())
        {
            await UniTask.Yield();
            this.ResetNewDay();
        }
    }
}