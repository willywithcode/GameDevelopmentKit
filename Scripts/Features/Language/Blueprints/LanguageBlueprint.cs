namespace GameFoundation.Scripts.Features.Language.Blueprints
{
    using System.Collections;
    using System.Collections.Generic;
    using GameFoundation.Scripts.Features.Language.Editor;
    using Sirenix.OdinInspector;
    using UnityEngine;

    [CreateAssetMenu(fileName = "LanguageBlueprint", menuName = "HyperCasual/Language/LanguageBlueprint")]
    public class LanguageBlueprint : ScriptableObject
    {
        #if UNITY_EDITOR
        [ValueDropdown(nameof(ShowListLanguage), IsUniqueList = true)]
        #endif
        public string initLanguage;

        public LanguageData       defaultLanguageData;
        public List<LanguageData> LanguageDatas;

        #if UNITY_EDITOR
        private IEnumerable ShowListLanguage()
        {
            return LanguageGlobalConfig.Instance.Languages;
        }
        #endif
    }
}