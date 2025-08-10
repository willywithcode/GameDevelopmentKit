namespace GameFoundation.Scripts.Features.Language.Components
{
    using GameFoundation.Scripts.DI;
    using GameFoundation.Scripts.Features.Language.Services;
    using GameFoundation.Scripts.Features.Language.Signals;
    using GameFoundation.Scripts.Patterns.SignalBus;
    using TMPro;
    using UnityEngine;
    using VContainer;

    public class Language_TMP_Text : TextMeshProUGUI
    {
        private LanguageService languageService;
        private SignalBus       signalBus;

        protected override void Awake()
        {
            base.Awake();
            this.languageService = this.GetCurrentContainer().Resolve<LanguageService>();
            this.signalBus       = this.GetCurrentContainer().Resolve<SignalBus>();
        }

        protected override void Start()
        {
            base.Start();
            this.signalBus.Subscribe<OnLanguageChange>(this.OnLanguageChange);
            if (this.languageService.TryGetTranslation(this.key, out var translation))
            {
                this.text = translation;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            this.signalBus.Unsubscribe<OnLanguageChange>(this.OnLanguageChange);
        }

        private void OnLanguageChange(OnLanguageChange signal)
        {
            if (this.languageService.TryGetTranslation(this.key, out var translation))
            {
                this.text = translation;
            }
        }

        [SerializeField] private string key;

        public string Key
        {
            get => this.key;
            set
            {
                this.key = value;
                if (this.languageService.TryGetTranslation(this.key, out var translation))
                {
                    this.text = translation;
                }
            }
        }
    }
}