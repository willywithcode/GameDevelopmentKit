namespace GameFoundation.Scripts.Features.NewDayReset.Services
{
    using System;
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Features.UserExperience.Services;
    using UnityEngine.Events;
    using VContainer.Unity;

    public class NewDayResetService : IAsyncStartable
    {
        public           UnityAction           OnNewDayReset;
        private readonly UserExperienceService userExperienceService;

        public NewDayResetService(UserExperienceService userExperienceService)
        {
            this.userExperienceService = userExperienceService;
        }

        private void ResetNewDay()
        {
            if (this.userExperienceService.GetLastLoginDate().Day < DateTime.Now.Day)
            {
                this.OnNewDayReset?.Invoke();
            }
        }

        public async UniTask StartAsync(CancellationToken cancellation = new CancellationToken())
        {
            await UniTask.Yield();
            this.ResetNewDay();
        }
    }
}