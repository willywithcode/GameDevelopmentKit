namespace GameFoundation.Scripts.Features.AntiCheat.Services
{
    using System;

    public enum CheatType
    {
        SpeedHack,
        MemoryEdit,       // ObscuredCheatingDetector
        Injection,
        TimeManipulation,
        WallHack,
    }

    public interface IAntiCheatService
    {
        bool IsCheatDetected { get; }
        event Action<CheatType> OnCheatDetected;

        void StartDetection();
        void StopDetection();
    }
}
