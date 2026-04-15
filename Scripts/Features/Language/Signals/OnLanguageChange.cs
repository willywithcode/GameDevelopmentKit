namespace GameFoundation.Scripts.Features.Language.Signals
{
    public readonly struct OnLanguageChange
    {
        public string Language { get; }

        public OnLanguageChange(string language)
        {
            this.Language = language;
        }
    }
}
