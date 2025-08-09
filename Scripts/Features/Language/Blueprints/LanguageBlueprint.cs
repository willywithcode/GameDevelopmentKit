namespace GameFoundation.Scripts.Features.Language.Blueprints
{
    using System.Collections.Generic;
    using System.Linq;
    using GameFoundation.Scripts.Features.Language.Editor;
    using Sirenix.OdinInspector;
    using UnityEngine;

    [CreateAssetMenu(fileName = "LanguageBlueprint", menuName = "HyperCasual/Language/LanguageBlueprint")]
    public class LanguageBlueprint : ScriptableObject
    {
        public List<string>       languages;
        public LanguageData       defaultLanguageData;
        public List<LanguageData> LanguageDatas;
        #if UNITY_EDITOR
        [Button]
        public void SyncGlobalConfig()
        {
            this.languages.Clear();
            this.languages = LanguageGlobalConfig.Instance.Languages.ToList();
        }
        #endif
    }
}