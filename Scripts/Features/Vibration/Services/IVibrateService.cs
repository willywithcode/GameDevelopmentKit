namespace GameFoundation.Scripts.Features.Vibration.Services
{
    using GameFoundation.Scripts.Features.Vibration.Enums;

    public interface IVibrateService
    {
        void PlayPresetType(VibrationPresetType vibrationPresetType);
        void PlayEmphasis(float                 amplitude, float frequency);
        void PlayConstant(float                 amplitude, float frequency, float duration);
        bool IsVibrationEnabled();
        void SetVibrationEnabled(bool isEnabled);
    }
}