namespace GameFoundation.Scripts.Features.AudioSystem.Entities
{
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Patterns.ObjectPooling;
    using UnityEngine;
    using UnityEngine.Events;

    public class SoundEmitter : CommonPoolElement
    {
        [SerializeField] private AudioSource             audioSource;
        private                  UnityAction             onPlayComplete;
        private                  CancellationTokenSource cancellationTokenSource;

        public SoundEmitter SetAudioClip(AudioClip audioClip)
        {
            this.audioSource.clip = audioClip;
            return this;
        }

        public SoundEmitter SetVolume(float volume)
        {
            this.audioSource.volume = volume;
            return this;
        }

        public SoundEmitter SetPitch(float pitch)
        {
            this.audioSource.pitch = pitch;
            return this;
        }

        public SoundEmitter SetLoop(bool loop)
        {
            this.audioSource.loop = loop;
            return this;
        }

        public SoundEmitter SetSpatialBlend(float spatialBlend)
        {
            this.audioSource.spatialBlend = spatialBlend;
            return this;
        }

        public SoundEmitter SetMaxDistance(float maxDistance)
        {
            this.audioSource.maxDistance = maxDistance;
            return this;
        }

        public SoundEmitter SetMinDistance(float minDistance)
        {
            this.audioSource.minDistance = minDistance;
            return this;
        }

        public SoundEmitter SetOnPlayComplete(UnityAction onPlayComplete)
        {
            this.onPlayComplete += onPlayComplete;
            return this;
        }

        public SoundEmitter SetMute(bool mute)
        {
            this.audioSource.mute = mute;
            return this;
        }

        public bool IsPlaying => this.audioSource.isPlaying;

        public async UniTask Play()
        {
            if (this.cancellationTokenSource != null)
            {
                this.cancellationTokenSource.Cancel();
                this.cancellationTokenSource.Dispose();
            }
            this.cancellationTokenSource = new();
            this.audioSource.Play();
            if (this.audioSource.loop)
            {
                return;
            }

            await UniTask.WaitWhile(() => this.audioSource.isPlaying, cancellationToken: this.cancellationTokenSource.Token);
            this.onPlayComplete?.Invoke();
            this.onPlayComplete = null;
        }

        public void Stop()
        {
            if (this.audioSource.isPlaying)
            {
                this.audioSource.Stop();
            }
            this.onPlayComplete?.Invoke();
            this.onPlayComplete = null;
        }
    }
}