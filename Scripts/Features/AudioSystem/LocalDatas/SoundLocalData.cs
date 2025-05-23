namespace GameFoundation.Scripts.Features.AudioSystem.LocalDatas
{
    using GameFoundation.Scripts.LocalData.Interfaces;
    using GameFoundation.Scripts.LocalData.Service;

    public class SoundLocalData : ILocalData
    {
        public bool   IsMusicMute { get; set; } = false;
        public bool   IsSfxMute   { get; set; } = false;
        public string GetKey()    => this.GetType().ToString();

        public void Reset()
        {
            this.IsMusicMute = false;
            this.IsSfxMute   = false;
        }
    }

    public class SoundLocalDataService : BaseLocalDataService<SoundLocalData>
    {
        public bool IsMusicMute
        {
            get => this.Data.IsMusicMute;
            set
            {
                this.Data.IsMusicMute = value;
                this.Save();
            }
        }

        public bool IsSfxMute
        {
            get => this.Data.IsSfxMute;
            set
            {
                this.Data.IsSfxMute = value;
                this.Save();
            }
        }
    }
}