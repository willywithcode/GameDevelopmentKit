namespace GameFoundation.Scripts.Features.AntiCheat.Services
{
    using CodeStage.AntiCheat.Detectors;
    using VContainer.Unity;

    public class AntiCheatService : IAntiCheatService, IInitializable, System.IDisposable
    {
        public bool IsCheatDetected { get; private set; }
        public event System.Action<CheatType> OnCheatDetected;

        public void Initialize() => this.StartDetection();

        public void StartDetection()
        {
            SpeedHackDetector.StartDetection(() => this.OnCheat(CheatType.SpeedHack));
            ObscuredCheatingDetector.StartDetection(() => this.OnCheat(CheatType.MemoryEdit));

#if !UNITY_EDITOR && (UNITY_STANDALONE || UNITY_ANDROID)
            InjectionDetector.StartDetection(_ => this.OnCheat(CheatType.Injection));
#endif
        }

        public void StopDetection()
        {
            SpeedHackDetector.StopDetection();
            ObscuredCheatingDetector.StopDetection();

#if !UNITY_EDITOR && (UNITY_STANDALONE || UNITY_ANDROID)
            InjectionDetector.StopDetection();
#endif
        }

        public void Dispose()
        {
            SpeedHackDetector.Dispose();
            ObscuredCheatingDetector.Dispose();

#if !UNITY_EDITOR && (UNITY_STANDALONE || UNITY_ANDROID)
            InjectionDetector.Dispose();
#endif
        }

        private void OnCheat(CheatType type)
        {
            this.IsCheatDetected = true;
            this.OnCheatDetected?.Invoke(type);
        }
    }
}
