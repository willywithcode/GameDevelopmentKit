namespace GameFoundation.Scripts.Features.Language.Blueprints
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class LanguageData
    {
        public string languageName;

        public Dictionary<string, string> translations = new();
    }
}
