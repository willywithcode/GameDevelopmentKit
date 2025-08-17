namespace GameFoundation.Scripts.Features.Language.Components
{
    using GameFoundation.Scripts.DI;
    using GameFoundation.Scripts.Features.Language.Services;
    using GameFoundation.Scripts.Features.Language.Signals;
    using GameFoundation.Scripts.Patterns.SignalBus;
    using Sirenix.OdinInspector;
    using TMPro;
    using UnityEngine;
    using VContainer;

    public class Language_TMP_Text : MonoBehaviour
    {
        private                  LanguageService languageService;
        private                  SignalBus       signalBus;
        [SerializeField] private TMP_Text        textMeshPro;

        [OnInspectorGUI]
        private void OnInspectorGUI()
        {
            if (this.textMeshPro == null)
            {
                this.textMeshPro = this.GetComponent<TMP_Text>();
            }
        }

        protected void Awake()
        {
            this.languageService = this.GetCurrentContainer().Resolve<LanguageService>();
            this.signalBus       = this.GetCurrentContainer().Resolve<SignalBus>();
        }

        protected void Start()
        {
            if (this.textMeshPro == null)
            {
                this.textMeshPro = this.GetComponent<TMP_Text>();
            }
            this.signalBus.Subscribe<OnLanguageChange>(this.OnLanguageChange);
            if (this.languageService.TryGetTranslation(this.key, out var translation))
            {
                this.textMeshPro.text = translation;
            }
        }

        protected void OnDestroy()
        {
            this.signalBus.Unsubscribe<OnLanguageChange>(this.OnLanguageChange);
        }

        private void OnLanguageChange(OnLanguageChange signal)
        {
            if (this.languageService.TryGetTranslation(this.key, out var translation))
            {
                this.textMeshPro.text = translation;
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
                    this.textMeshPro.text = translation;
                }
            }
        }
    }
}