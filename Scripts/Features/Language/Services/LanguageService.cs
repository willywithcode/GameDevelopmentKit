namespace GameFoundation.Scripts.Features.Language.Services
{
    using System.Collections.Generic;
    using GameFoundation.Scripts.Addressable;
    using GameFoundation.Scripts.Features.Language.Blueprints;
    using GameFoundation.Scripts.Features.Language.LocalDatas;
    using GameFoundation.Scripts.Features.Language.Signals;
    using GameFoundation.Scripts.Features.UserExperience.Services;
    using GameFoundation.Scripts.Patterns.SignalBus;
    using UnityEngine;
    using VContainer.Unity;
    using ZLinq;

    public class LanguageService : IInitializable
    {
        #region Inject

        private readonly LanguageLocalDataService languageLocalDataService;
        private readonly SignalBus                signalBus;
        private readonly UserExperienceService    userExperienceService;
        private readonly LanguageBlueprint        languageBlueprint;

        public LanguageService(
            LanguageLocalDataService languageLocalDataService,
            IAssetsManager           assetsManager,
            SignalBus                signalBus,
            UserExperienceService    userExperienceService
        )
        {
            this.languageLocalDataService = languageLocalDataService;
            this.signalBus                = signalBus;
            this.userExperienceService    = userExperienceService;
            this.languageBlueprint        = assetsManager.LoadAsset<LanguageBlueprint>("LanguageBlueprint");
        }

        #endregion

        public void Initialize()
        {
            if (this.userExperienceService.GetTimePlayed() <= 0)
            {
                this.languageLocalDataService.CurrentLanguage = this.languageBlueprint.initLanguage;
            }
        }

        public string GetCurrentLanguage()
        {
            return this.languageLocalDataService.CurrentLanguage;
        }

        public void SetLanguage(string language)
        {
            this.languageLocalDataService.CurrentLanguage = language;
            this.signalBus.Fire(new OnLanguageChange(language));
        }

        public bool TryGetTranslation(string key, out string translation)
        {
            var languageData = this.languageBlueprint.LanguageDatas.AsValueEnumerable()
                .FirstOrDefault(data => data.languageName == this.GetCurrentLanguage());
            if (languageData is null) languageData = this.languageBlueprint.defaultLanguageData;
            if (languageData != null && languageData.translations.TryGetValue(key, out translation))
            {
                return true;
            }
            if (this.languageBlueprint.defaultLanguageData.translations.TryGetValue(key, out translation))
            {
                return true;
            }

            translation = null;
            return false;
        }

        public List<string> GetAvailableLanguages()
        {
            return this.languageBlueprint.LanguageDatas.AsValueEnumerable()
                .Select(data => data.languageName)
                .ToList();
        }
    }
}