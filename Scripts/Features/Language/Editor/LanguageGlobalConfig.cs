namespace GameFoundation.Scripts.Features.Language.Editor
{
    using Sirenix.Utilities;
    using UnityEngine;

    [GlobalConfig("Assets/Resources/GlobalConfigs/")]
    [CreateAssetMenu(fileName = "LanguageGlobalConfig", menuName = "GlobalConfigs/LanguageGlobalConfig")]
    public class LanguageGlobalConfig : GlobalConfig<LanguageGlobalConfig>
    {
        public string[] Languages;
    }
}