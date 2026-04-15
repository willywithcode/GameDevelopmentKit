namespace GameFoundation.Scripts.Features.Language.Components
{
    using System;
    using GameFoundation.Scripts.DI;
    using GameFoundation.Scripts.Features.Language.Services;
    using GameFoundation.Scripts.Features.Language.Signals;
    using MessagePipe;
    using Sirenix.OdinInspector;
    using TMPro;
    using UnityEngine;
    using VContainer;

    public class Language_TMP_Text : MonoBehaviour
    {
        private                  LanguageService                languageService;
        private                  ISubscriber<OnLanguageChange>  languageChangeSubscriber;
        private                  IDisposable                    subscription;
        [SerializeField] private TMP_Text                       textMeshPro;

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
            var container = this.GetCurrentContainer();
            this.languageService          = container.Resolve<LanguageService>();
            this.languageChangeSubscriber = container.Resolve<ISubscriber<OnLanguageChange>>();
        }

        protected void Start()
        {
            if (this.textMeshPro == null)
            {
                this.textMeshPro = this.GetComponent<TMP_Text>();
            }
            this.subscription = this.languageChangeSubscriber.Subscribe(this.OnLanguageChange);
            if (this.languageService.TryGetTranslation(this.key, out var translation))
            {
                this.textMeshPro.text = translation;
            }
        }

        protected void OnDestroy()
        {
            this.subscription?.Dispose();
            this.subscription = null;
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
