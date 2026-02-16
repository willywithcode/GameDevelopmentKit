namespace GameFoundation.Scripts.Features.LiveFeature.LocalData
{
    using GameFoundation.Scripts.LocalData.Interfaces;
    using GameFoundation.Scripts.LocalData.Service;

    public class LiveLocalData : ILocalData
    {
        public string StartLivesTimeStamp;
        public string InfinityStartTimeStamp;
        public string InfinityLivesTimeStamp;

        public string GetKey() => nameof(LiveLocalData);

        public void Reset()
        {
            this.StartLivesTimeStamp    = "";
            this.InfinityStartTimeStamp = "";
            this.InfinityLivesTimeStamp = "";
        }
    }

    public class LiveLocalDataService : BaseLocalDataService<LiveLocalData>
    {
        public string StartLivesTimeStamp
        {
            get => this.Data.StartLivesTimeStamp;
            set
            {
                this.Data.StartLivesTimeStamp = value;
                this.Save();
            }
        }

        public string InfinityStartTimeStamp
        {
            get => this.Data.InfinityStartTimeStamp;
            set
            {
                this.Data.InfinityStartTimeStamp = value;
                this.Save();
            }
        }

        public string InfinityLivesTimeStamp
        {
            get => this.Data.InfinityLivesTimeStamp;
            set
            {
                this.Data.InfinityLivesTimeStamp = value;
                this.Save();
            }
        }
    }
}
