namespace GameFoundation.Scripts.Features.Vibration.Services
{
    using System;
    using GameFoundation.Scripts.Features.Vibration.Enums;
    using GameFoundation.Scripts.Features.Vibration.LocalDatas;
    using Lofelt.NiceVibrations;

    public class VibrationService : IVibrateService
    {
        #region Inject

        private readonly VibrationLocalDataService vibrationLocalDataService;

        public VibrationService(VibrationLocalDataService vibrationLocalDataService)
        {
            this.vibrationLocalDataService = vibrationLocalDataService;
        }

        #endregion

        private HapticPatterns.PresetType GetHapticPatternsPresetType(VibrationPresetType vibrationPresetType)
        {
            return vibrationPresetType switch
            {
                VibrationPresetType.Selection    => HapticPatterns.PresetType.Selection,
                VibrationPresetType.Success      => HapticPatterns.PresetType.Success,
                VibrationPresetType.Warning      => HapticPatterns.PresetType.Warning,
                VibrationPresetType.Failure      => HapticPatterns.PresetType.Failure,
                VibrationPresetType.LightImpact  => HapticPatterns.PresetType.LightImpact,
                VibrationPresetType.MediumImpact => HapticPatterns.PresetType.MediumImpact,
                VibrationPresetType.HeavyImpact  => HapticPatterns.PresetType.HeavyImpact,
                VibrationPresetType.RigidImpact  => HapticPatterns.PresetType.RigidImpact,
                VibrationPresetType.SoftImpact   => HapticPatterns.PresetType.SoftImpact,
                VibrationPresetType.None         => HapticPatterns.PresetType.None,
                _                                => throw new ArgumentOutOfRangeException(nameof(vibrationPresetType), vibrationPresetType, null),
            };
        }

        public void PlayPresetType(VibrationPresetType vibrationPresetType)
        {
            if (!this.vibrationLocalDataService.IsVibrationEnabled) return;
            HapticPatterns.PlayPreset(this.GetHapticPatternsPresetType(vibrationPresetType));
        }

        public void PlayEmphasis(float amplitude, float frequency)
        {
            if (!this.vibrationLocalDataService.IsVibrationEnabled) return;
            HapticPatterns.PlayEmphasis(amplitude, frequency);
        }

        public void PlayConstant(float amplitude, float frequency, float duration)
        {
            if (!this.vibrationLocalDataService.IsVibrationEnabled) return;
            HapticPatterns.PlayConstant(amplitude, frequency, duration);
        }

        public bool IsVibrationEnabled() => this.vibrationLocalDataService.IsVibrationEnabled;
        public void SetVibrationEnabled(bool isEnabled)
        {
            if (this.vibrationLocalDataService.IsVibrationEnabled == isEnabled) return;
            this.vibrationLocalDataService.IsVibrationEnabled = isEnabled;
        }
    }
}