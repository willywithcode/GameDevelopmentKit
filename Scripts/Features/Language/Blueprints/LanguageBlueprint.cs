namespace GameFoundation.Scripts.Features.Language.Blueprints
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class LanguageBlueprint
    {
        public string                      initLanguage;
        public string                      defaultLanguage;
        public bool                        autoSelectLanguage;
        public List<LanguageFileReference> languages = new();
    }

    [Serializable]
    public class LanguageFileReference
    {
        public string languageName;
        public string address;
        public string code;
    }
}