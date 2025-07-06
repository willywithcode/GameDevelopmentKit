namespace GameFoundation.Scripts.Features.Language.Blueprints
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "LanguageBlueprint", menuName = "HyperCasual/Language/LanguageBlueprint")]
    public class LanguageBlueprint : ScriptableObject
    {
        public string[] Languages;
    }
}