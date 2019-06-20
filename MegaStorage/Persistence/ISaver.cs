namespace MegaStorage.Persistence
{
    public interface ISaver
    {
        void LoadCustomChests();
        void ReAddCustomChests();
        void HideAndSaveCustomChests();
    }
}
