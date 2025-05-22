namespace GameFoundation.Scripts.LocalData.Interfaces
{
    public interface ILocalDataService<T> where T : ILocalData, new()
    {
        T Data { get; }
        
        void Save();
        void Load();
        void DeleteData();
    }
}