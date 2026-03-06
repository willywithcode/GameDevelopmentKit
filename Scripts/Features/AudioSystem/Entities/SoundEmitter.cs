namespace GameFoundation.Scripts.Features.AudioSystem.Entities
{
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Patterns.ObjectPooling;
    using MoreMountains.Feedbacks;
    using UnityEngine;
    using UnityEngine.Events;

    public class SoundEmitter : CommonPoolElement
    {
        [SerializeField] private AudioSource audioSource;

        private UnityAction             onPlayComplete;
        private CancellationTokenSource cancellationTokenSource;

        // Feel components — added at runtime via InitializeFeelComponents()
        private MMSpringAudioSourceVolume  volumeSpring;
        private MMSpringAudioSourcePitch   pitchSpring;
        private MMAudioFilterLowPassShaker lowPassShaker;
        private AudioLowPassFilter         lowPassFilter;

        public bool HasFeelComponents => this.volumeSpring != null;

        #region Fluent Builder

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

        #endregion

        #region Playback

        public bool IsPlaying => this.audioSource.isPlaying;
        public bool IsPaused  => this.audioSource.clip != null && !this.audioSource.isPlaying && this.audioSource.time > 0;

        public async UniTask Play()
        {
            if (this.cancellationTokenSource != null)
            {
                this.cancellationTokenSource.Cancel();
                this.cancellationTokenSource.Dispose();
            }
            this.cancellationTokenSource = new();
            this.audioSource.Play();
            if (this.audioSource.loop) return;

            await UniTask.WaitWhile(() => this.audioSource.isPlaying, cancellationToken: this.cancellationTokenSource.Token);
            this.onPlayComplete?.Invoke();
            this.onPlayComplete = null;
        }

        public void Stop()
        {
            this.cancellationTokenSource?.Cancel();
            if (this.audioSource.isPlaying) this.audioSource.Stop();
            this.onPlayComplete?.Invoke();
            this.onPlayComplete = null;
        }

        public void Pause()
        {
            if (this.audioSource.isPlaying) this.audioSource.Pause();
        }

        public void Resume()
        {
            if (!this.audioSource.isPlaying && this.audioSource.clip != null)
                this.audioSource.UnPause();
        }

        #endregion

        #region Feel Integration

        /// <summary>
        /// Adds MMSpring and LowPass shaker components at runtime.
        /// Call once after spawning the emitter for music use.
        /// </summary>
        public void InitializeFeelComponents()
        {
            this.volumeSpring = this.gameObject.AddComponent<MMSpringAudioSourceVolume>();
            this.pitchSpring  = this.gameObject.AddComponent<MMSpringAudioSourcePitch>();

            this.lowPassFilter                 = this.gameObject.AddComponent<AudioLowPassFilter>();
            this.lowPassFilter.cutoffFrequency = 22000f;
            this.lowPassShaker                 = this.gameObject.AddComponent<MMAudioFilterLowPassShaker>();
        }

        /// <summary>Spring-based smooth volume transition.</summary>
        public void FadeVolumeTo(float target) => this.volumeSpring?.MoveTo(target);

        /// <summary>Sets volume instantly, syncing the spring's internal state.</summary>
        public void SetVolumeInstant(float volume)
        {
            this.audioSource.volume = volume;
            this.volumeSpring?.MoveToInstant(volume);
        }

        /// <summary>Spring-based smooth pitch transition (e.g. for time-scale effects).</summary>
        public void SetPitchSmooth(float pitch) => this.pitchSpring?.MoveTo(pitch);

        /// <summary>
        /// Applies a low-pass muffle effect, sweeping cutoff down to <paramref name="cutoffHz"/> over <paramref name="duration"/> seconds.
        /// </summary>
        public void ApplyLowPassEffect(float cutoffHz, float duration)
        {
            if (this.lowPassShaker == null) return;
            this.lowPassShaker.ShakeDuration    = duration;
            this.lowPassShaker.RemapLowPassZero = cutoffHz;
            this.lowPassShaker.RemapLowPassOne  = 22000f;
            this.lowPassShaker.ShakeLowPass     = AnimationCurve.Linear(0f, 1f, 1f, 0f);
            this.lowPassShaker.Play();
        }

        /// <summary>
        /// Restores the low-pass filter to full range over <paramref name="duration"/> seconds.
        /// </summary>
        public void RemoveLowPassEffect(float duration)
        {
            if (this.lowPassShaker == null) return;
            this.lowPassShaker.ShakeDuration    = duration;
            this.lowPassShaker.RemapLowPassZero = 22000f;
            this.lowPassShaker.RemapLowPassOne  = 22000f;
            this.lowPassShaker.ShakeLowPass     = AnimationCurve.Linear(0f, 0f, 1f, 1f);
            this.lowPassShaker.Play();
        }

        #endregion
    }
}
