namespace GameFoundation.Scripts.Features.Language.Blueprints
{
    using System.Collections;
    using System.Collections.Generic;
    using GameFoundation.Scripts.Features.Language.Editor;
    using Sirenix.OdinInspector;
    using UnityEngine;

    [CreateAssetMenu(fileName = "LanguageData", menuName = "HyperCasual/Language/LanguageData")]
    public class LanguageData : SerializedScriptableObject
    {
        #if UNITY_EDITOR
        [ValueDropdown(nameof(ShowListLanguage), IsUniqueList = true)]
        #endif
        public string languageName;

        [DictionaryDrawerSettings(KeyLabel = "Key", ValueLabel = "Translation")] public Dictionary<string, string> translations = new();

        #if UNITY_EDITOR
        private IEnumerable ShowListLanguage()
        {
            return LanguageGlobalConfig.Instance.Languages;
        }
        #endif
    }
}