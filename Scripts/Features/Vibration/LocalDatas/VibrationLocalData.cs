namespace GameFoundation.Scripts.Features.Vibration.LocalDatas
{
    using GameFoundation.Scripts.LocalData.Interfaces;
    using GameFoundation.Scripts.LocalData.Service;

    public class VibrationLocalData : ILocalData
    {
        public bool   IsVibrationEnabled { get; set; } = true;
        public string GetKey()           => this.GetType().ToString();

        public void Reset()
        {
            this.IsVibrationEnabled = true;
        }
    }

    public class VibrationLocalDataService : BaseLocalDataService<VibrationLocalData>
    {
        public bool IsVibrationEnabled
        {
            get => this.Data.IsVibrationEnabled;
            set
            {
                this.Data.IsVibrationEnabled = value;
                this.Save();
            }
        }
    }
}