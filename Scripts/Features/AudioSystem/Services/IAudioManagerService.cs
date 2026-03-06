namespace GameFoundation.Scripts.Features.AudioSystem.Services
{
    public interface IAudioManagerService
    {
        public void PlaySfx(string   audioId, bool isLoop = false);
        public void PlayMusic(string audioId, bool force  = false, bool isLoop = true);
        public void StopMusic();
        public void PauseMusic();
        public void ResumeMusic();
        public void StopSfx(string audioId);
        public void MuteSfx(bool   isMute);
        public void MuteMusic(bool isMute);
        public bool IsSfxOn();
        public bool IsMusicOn();
        public void SetSfxVolume(float volume);
        public void SetMusicVolume(float volume);
        public void CrossfadeMusic(string audioId, float duration = 1f);
        public void DuckMusic(float targetVolume, float restoreDelay = 1f);
    }
}