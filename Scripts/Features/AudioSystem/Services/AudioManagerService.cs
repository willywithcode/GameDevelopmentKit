namespace GameFoundation.Scripts.Features.AudioSystem.Services
{
    using System;
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Addressable;
    using GameFoundation.Scripts.Features.AudioSystem.Entities;
    using GameFoundation.Scripts.Features.AudioSystem.LocalDatas;
    using IGameLogger = GameFoundation.Scripts.Features.Logger.Services.ILogger;
    using LoggerService = GameFoundation.Scripts.Features.Logger.Services.LoggerService;
    using GameFoundation.Scripts.Patterns.ObjectPooling;
    using UnityEngine;
    using VContainer.Unity;

    public class AudioManagerService : IAudioManagerService, IInitializable
    {
        #region Config

        private const float MusicFadeDuration  = 0.5f;  // fade in/out duration (seconds)
        private const float LowPassFadeDuration = 0.3f;  // pause muffle effect duration
        private const float LowPassCutoffHz     = 500f;  // cutoff frequency when muffled

        #endregion

        #region Inject

        private readonly IObjectPooling        objectPooling;
        private readonly IAssetsManager        assetsManager;
        private readonly SoundLocalDataService soundLocalDataService;
        private readonly IGameLogger           logger;

        public AudioManagerService(
            IObjectPooling        objectPooling,
            IAssetsManager        assetsManager,
            SoundLocalDataService soundLocalDataService,
            IGameLogger           logger = null)
        {
            this.objectPooling         = objectPooling;
            this.assetsManager         = assetsManager;
            this.soundLocalDataService = soundLocalDataService;
            this.logger                = logger ?? new LoggerService();
        }

        #endregion

        private SoundEmitter                          musicSource;
        private SoundEmitter                          prevMusicSource;  // held during crossfade
        private Dictionary<string, SoundEmitter>      sfxLoopEmitters = new();
        private readonly Queue<(string id, bool isLoop)> musicQueue   = new();

        private float musicVolume = 0.75f;
        private float sfxVolume   = 0.75f;

        public void Initialize()
        {
            this.objectPooling.CreatePool<SoundEmitter>("SoundEmitter", 32);
        }

        #region SFX

        public void PlaySfx(string audioId, bool isLoop = false)
        {
            var emitter = this.objectPooling.Spawn<SoundEmitter>("SoundEmitter");
            if (isLoop)
            {
                if (this.sfxLoopEmitters.ContainsKey(audioId)) this.StopSfx(audioId);
                this.sfxLoopEmitters.Add(audioId, emitter);
            }

            emitter.SetAudioClip(this.assetsManager.LoadAsset<AudioClip>(audioId))
                .SetLoop(isLoop)
                .SetMute(this.soundLocalDataService.IsSfxMute)
                .SetVolume(this.soundLocalDataService.IsSfxMute ? 0 : this.sfxVolume)
                .SetOnPlayComplete(() => this.objectPooling.Despawn(emitter))
                .Play().Forget();
        }

        public void PlaySfx(string audioId, float pitch, Action onComplete = null)
        {
            var emitter = this.objectPooling.Spawn<SoundEmitter>("SoundEmitter");

            emitter.SetAudioClip(this.assetsManager.LoadAsset<AudioClip>(audioId))
                .SetLoop(false)
                .SetMute(this.soundLocalDataService.IsSfxMute)
                .SetVolume(this.soundLocalDataService.IsSfxMute ? 0 : this.sfxVolume)
                .SetPitch(pitch)
                .SetOnPlayComplete(() =>
                {
                    onComplete?.Invoke();
                    this.objectPooling.Despawn(emitter);
                })
                .Play().Forget();
        }

        public void StopSfx(string audioId)
        {
            if (this.sfxLoopEmitters.TryGetValue(audioId, out var emitter))
            {
                emitter.Stop();
                this.objectPooling.Despawn(emitter);
                this.sfxLoopEmitters.Remove(audioId);
            }
            else
            {
                this.logger.Warning($"SFX with ID {audioId} is not currently playing or does not exist.");
            }
        }

        public void MuteSfx(bool isMute) => this.soundLocalDataService.IsSfxMute = isMute;

        public bool IsSfxOn() => !this.soundLocalDataService.IsSfxMute;

        public void SetSfxVolume(float volume)
        {
            this.sfxVolume = volume;
            foreach (var emitter in this.sfxLoopEmitters.Values)
                emitter.SetVolume(volume);
        }

        #endregion

        #region Music

        public void PlayMusic(string audioId, bool force = false, bool isLoop = true)
        {
            this.musicSource ??= this.SpawnMusicEmitter();

            if (!force)
            {
                this.musicQueue.Enqueue((audioId, isLoop));
                if (!this.musicSource.IsPlaying)
                {
                    var (id, loop) = this.musicQueue.Dequeue();
                    this.PlayMusic(id, true, loop);
                }
                return;
            }

            // Start silent then spring fade in
            this.musicSource.SetVolumeInstant(0);
            this.musicSource
                .SetAudioClip(this.assetsManager.LoadAsset<AudioClip>(audioId))
                .SetLoop(isLoop)
                .SetOnPlayComplete(() =>
                {
                    if (this.musicQueue.Count > 0)
                    {
                        var (id, loop) = this.musicQueue.Dequeue();
                        this.PlayMusic(id, false, loop);
                    }
                })
                .Play().Forget();

            this.musicSource.FadeVolumeTo(this.soundLocalDataService.IsMusicMute ? 0 : this.musicVolume);
        }

        public void StopMusic()
        {
            this.musicQueue.Clear();
            if (this.musicSource == null) return;

            if (this.musicSource.HasFeelComponents)
                this.FadeOutAndStopAsync(this.musicSource).Forget();
            else
                this.musicSource.Stop();
        }

        public void PauseMusic()
        {
            if (this.musicSource == null || !this.musicSource.IsPlaying) return;
            this.PauseMusicAsync().Forget();
        }

        public void ResumeMusic()
        {
            if (this.musicSource == null || !this.musicSource.IsPaused) return;
            this.musicSource.Resume();
            this.musicSource.RemoveLowPassEffect(LowPassFadeDuration);
        }

        public void MuteMusic(bool isMute)
        {
            this.soundLocalDataService.IsMusicMute = isMute;
            if (this.musicSource == null) return;

            if (this.musicSource.HasFeelComponents)
                this.musicSource.FadeVolumeTo(isMute ? 0 : this.musicVolume);
            else
                this.musicSource.SetMute(isMute);
        }

        public bool IsMusicOn() => !this.soundLocalDataService.IsMusicMute;

        public void SetMusicVolume(float volume)
        {
            this.musicVolume = volume;
            if (this.musicSource == null) return;

            if (this.musicSource.HasFeelComponents)
                this.musicSource.FadeVolumeTo(this.soundLocalDataService.IsMusicMute ? 0 : volume);
            else
                this.musicSource.SetVolume(volume);
        }

        /// <summary>
        /// Smoothly transitions from the current track to <paramref name="audioId"/> by fading both simultaneously.
        /// </summary>
        public void CrossfadeMusic(string audioId, float duration = 1f)
        {
            this.CrossfadeMusicAsync(audioId, duration).Forget();
        }

        /// <summary>
        /// Temporarily lowers music volume to <paramref name="targetVolume"/>, then springs back after <paramref name="restoreDelay"/> seconds.
        /// </summary>
        public void DuckMusic(float targetVolume, float restoreDelay = 1f)
        {
            if (this.musicSource == null || !this.musicSource.HasFeelComponents) return;
            this.DuckMusicAsync(targetVolume, restoreDelay).Forget();
        }

        #endregion

        #region Async helpers

        private async UniTaskVoid FadeOutAndStopAsync(SoundEmitter emitter)
        {
            emitter.FadeVolumeTo(0);
            await UniTask.Delay(TimeSpan.FromSeconds(MusicFadeDuration));
            emitter.Stop();
            emitter.SetVolumeInstant(0);
        }

        private async UniTaskVoid PauseMusicAsync()
        {
            this.musicSource.ApplyLowPassEffect(LowPassCutoffHz, LowPassFadeDuration);
            await UniTask.Delay(TimeSpan.FromSeconds(LowPassFadeDuration));
            this.musicSource.Pause();
        }

        private async UniTaskVoid CrossfadeMusicAsync(string audioId, float duration)
        {
            // Fade out current track
            this.prevMusicSource = this.musicSource;
            this.prevMusicSource?.FadeVolumeTo(0);

            // Spawn and fade in new track
            this.musicSource = this.SpawnMusicEmitter();
            this.musicSource.SetVolumeInstant(0);
            this.musicSource
                .SetAudioClip(this.assetsManager.LoadAsset<AudioClip>(audioId))
                .SetLoop(true)
                .Play().Forget();
            this.musicSource.FadeVolumeTo(this.soundLocalDataService.IsMusicMute ? 0 : this.musicVolume);

            // Clean up old source once crossfade completes
            await UniTask.Delay(TimeSpan.FromSeconds(duration));
            if (this.prevMusicSource == null) return;
            this.prevMusicSource.Stop();
            this.objectPooling.Despawn(this.prevMusicSource);
            this.prevMusicSource = null;
        }

        private async UniTaskVoid DuckMusicAsync(float targetVolume, float restoreDelay)
        {
            this.musicSource.FadeVolumeTo(targetVolume);
            await UniTask.Delay(TimeSpan.FromSeconds(restoreDelay));
            if (this.musicSource == null) return;
            this.musicSource.FadeVolumeTo(this.soundLocalDataService.IsMusicMute ? 0 : this.musicVolume);
        }

        private SoundEmitter SpawnMusicEmitter()
        {
            var emitter = this.objectPooling.Spawn<SoundEmitter>("SoundEmitter");
            emitter.InitializeFeelComponents();
            return emitter;
        }

        #endregion
    }
}
