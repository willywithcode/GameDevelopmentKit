namespace GameFoundation.Scripts.Features.Language.Components
{
    using System.Collections;
    using GameFoundation.Scripts.DI;
    using GameFoundation.Scripts.Features.Language.Editor;
    using GameFoundation.Scripts.Features.Language.Services;
    using Sirenix.OdinInspector;
    using UnityEngine;
    using UnityEngine.UI;
    using VContainer;

    [RequireComponent(typeof(Button))]
    public class ChangeLanguageButton : MonoBehaviour
    {
        #if UNITY_EDITOR
        [ValueDropdown(nameof(ShowListLanguage), DropdownTitle = "Select Language")]
        #endif
        [SerializeField]
        private string languageKey;

        [SerializeField] private Button languageButton;
        #if UNITY_EDITOR
        private IEnumerable ShowListLanguage()
        {
            return LanguageGlobalConfig.Instance.Languages;
        }

        [OnInspectorGUI]
        private void OnInspectorGUI()
        {
            if (this.languageButton == null)
            {
                this.languageButton = this.GetComponent<Button>();
            }
        }
        #endif
        private LanguageService languageService;

        protected void Awake()
        {
            this.languageService = this.GetCurrentContainer().Resolve<LanguageService>();
        }

        protected void Start()
        {
            if (this.languageButton == null)
            {
                this.languageButton = this.GetComponent<Button>();
            }
            this.languageButton.onClick.AddListener(this.OnButtonClick);
        }

        protected void OnDestroy()
        {
            this.languageButton.onClick.RemoveListener(this.OnButtonClick);
        }

        private void OnButtonClick()
        {
            this.languageService.SetLanguage(this.languageKey);
        }
    }
}