namespace GameFoundation.Scripts.Features.Language.LocalDatas
{
    using GameFoundation.Scripts.LocalData.Interfaces;
    using GameFoundation.Scripts.LocalData.Service;

    public class LanguageLocalData : ILocalData
    {
        public string CurrentLanguage { get; set; } = "vietnamese";
        public string GetKey()        => this.GetType().ToString();

        public void Reset()
        {
        }
    }
    public class LanguageLocalDataService : BaseLocalDataService<LanguageLocalData>
    {
        public string CurrentLanguage
        {
            get => this.Data.CurrentLanguage;
            set
            {
                this.Data.CurrentLanguage = value;
                this.Save();
            }
        }
    }
}