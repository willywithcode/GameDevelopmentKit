namespace GameFoundation.Scripts.Features.AudioSystem.Services
{
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Addressable;
    using GameFoundation.Scripts.Features.AudioSystem.Entities;
    using GameFoundation.Scripts.Features.AudioSystem.LocalDatas;
    using GameFoundation.Scripts.Patterns.ObjectPooling;
    using UnityEngine;
    using VContainer.Unity;

    public class AudioManagerService : IAudioManagerService, IInitializable
    {
        #region Inject

        private readonly IObjectPooling        objectPooling;
        private readonly IAssetsManager        assetsManager;
        private readonly SoundLocalDataService soundLocalDataService;

        public AudioManagerService(
            IObjectPooling        objectPooling,
            IAssetsManager        assetsManager,
            SoundLocalDataService soundLocalDataService
        )
        {
            this.objectPooling         = objectPooling;
            this.assetsManager         = assetsManager;
            this.soundLocalDataService = soundLocalDataService;
        }

        #endregion

        private          SoundEmitter                     musicSource;
        private          Dictionary<string, SoundEmitter> sfxLoopEmitters = new();
        private readonly Queue<(string id, bool isLoop)>  musicQueue      = new();
        private          float                            sfxVolume       = .75f;

        public void Initialize()
        {
            this.objectPooling.CreatePool<SoundEmitter>("SoundEmitter", 32);
        }

        public void PlaySfx(string audioId, bool isLoop = false)
        {
            var soundEmitter = this.objectPooling.Spawn<SoundEmitter>("SoundEmitter");
            if (isLoop)
            {
                this.sfxLoopEmitters.Add(audioId, soundEmitter);
            }
            soundEmitter.SetAudioClip(this.assetsManager.LoadAsset<AudioClip>(audioId))
                .SetLoop(isLoop)
                .SetMute(this.soundLocalDataService.IsSfxMute)
                .SetVolume(this.soundLocalDataService.IsSfxMute ? 0 : this.sfxVolume)
                .SetOnPlayComplete(() => this.objectPooling.Despawn(soundEmitter))
                .Play().Forget();
        }

        public void PlayMusic(string audioId, bool force = false, bool isLoop = true)
        {
            this.musicSource ??= this.objectPooling.Spawn<SoundEmitter>("SoundEmitter");
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
            this.musicSource.SetAudioClip(this.assetsManager.LoadAsset<AudioClip>(audioId))
                .SetLoop(isLoop)
                .SetMute(this.soundLocalDataService.IsMusicMute)
                .SetOnPlayComplete(() =>
                {
                    if (this.musicQueue.Count > 0)
                    {
                        var (id, loop) = this.musicQueue.Dequeue();
                        this.PlayMusic(id, false, loop);
                    }
                })
                .Play().Forget();
        }

        public void StopMusic()
        {
            if (this.musicSource != null)
            {
                this.musicSource.Stop();
                this.musicSource.SetOnPlayComplete(null);
            }
            this.musicQueue.Clear();
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
                Debug.LogWarning($"SFX with ID {audioId} is not currently playing or does not exist.");
            }
        }

        public void MuteSfx(bool isMute)
        {
            this.soundLocalDataService.IsSfxMute = isMute;
        }

        public void MuteMusic(bool isMute)
        {
            this.soundLocalDataService.IsMusicMute = isMute;
            if (this.musicSource != null)
            {
                this.musicSource.SetMute(isMute);
            }
        }

        public bool IsSfxOn() => !this.soundLocalDataService.IsSfxMute;

        public bool IsMusicOn() => !this.soundLocalDataService.IsMusicMute;

        public void SetSfxVolume(float volume)
        {
            foreach (var emitter in this.sfxLoopEmitters.Values)
            {
                emitter.SetVolume(volume);
            }
            this.sfxVolume = volume;
        }

        public void SetMusicVolume(float volume)
        {
            if (this.musicSource != null)
            {
                this.musicSource.SetVolume(volume);
            }
        }
    }
}