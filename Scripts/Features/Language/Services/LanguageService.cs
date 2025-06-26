namespace GameFoundation.Scripts.Features.Language.Services
{
    using System;
    using System.Collections.Generic;
    using GameFoundation.Scripts.Addressable;
    using GameFoundation.Scripts.Features.Language.Blueprints;
    using GameFoundation.Scripts.Features.Language.LocalDatas;
    using GameFoundation.Scripts.Features.Language.Signals;
    using GameFoundation.Scripts.Patterns.SignalBus;
    using ZLinq;

    public class LanguageService
    {
        #region Inject

        private readonly LanguageLocalDataService languageLocalDataService;
        private readonly SignalBus                signalBus;
        private readonly LanguageBlueprint        languageBlueprint;

        public LanguageService(
            LanguageLocalDataService languageLocalDataService,
            IAssetsManager           assetsManager,
            SignalBus                signalBus
        )
        {
            this.languageLocalDataService = languageLocalDataService;
            this.signalBus                = signalBus;
            this.languageBlueprint        = assetsManager.LoadAsset<LanguageBlueprint>("LanguageBlueprint");
        }

        #endregion

        public List<string> GetAvailableLanguages()
        {
            return this.languageBlueprint.Languages.AsValueEnumerable().ToList();
        }

        public string GetCurrentLanguage()
        {
            return this.languageLocalDataService.CurrentLanguage;
        }

        public void SetLanguage(string language)
        {
            if (!this.GetAvailableLanguages().Contains(language))
            {
                throw new ArgumentException($"Language '{language}' is not available.");
            }
            this.languageLocalDataService.CurrentLanguage = language;
            this.signalBus.Fire(new OnLanguageChange(language));
        }
    }
}