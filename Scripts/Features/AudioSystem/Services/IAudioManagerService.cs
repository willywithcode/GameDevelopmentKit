namespace GameFoundation.Scripts.Features.AudioSystem.Services
{
    public interface IAudioManagerService
    {
        public void PlaySfx(string   audioId, bool isLoop = false);
        public void PlayMusic(string audioId, bool force  = false, bool isLoop = true);
        public void StopMusic();
        public void StopSfx(string audioId);
        public void MuteSfx(bool   isMute);
        public void MuteMusic(bool isMute);
        public bool IsSfxOn();
        public bool IsMusicOn();
        public void SetSfxVolume(float volume);
        public void SetMusicVolume(float volume);
    }
}