namespace GameFoundation.Scripts.Features.AudioSystem.Services
{
    using System;

    public interface IAudioManagerService
    {
        public void PlaySfx(string   audioId, bool isLoop = false);
        public void PlaySfx(string   audioId, float pitch, Action onComplete = null);
        public void PlayMusic(string audioId, bool force  = false, bool isLoop = true);
        // Cue gain multiplies the existing master volume; it never changes that master.
        // Unsupported legacy adapters fail explicitly instead of changing global volume.
        public void PlaySfxWithVolume(string audioId, float volume, bool isLoop = false)
            => throw new NotSupportedException("Per-cue SFX volume is not implemented by this audio adapter.");
        public void PlayMusicWithVolume(string audioId, float volume, bool force = false, bool isLoop = true)
            => throw new NotSupportedException("Per-cue music volume is not implemented by this audio adapter.");
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