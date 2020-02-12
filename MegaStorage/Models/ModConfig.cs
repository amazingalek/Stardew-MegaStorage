namespace MegaStorage.Models
{
    public class ModConfig
    {
        public static ModConfig Instance { get; private set; }
        public bool EnableCategories { get; set; }
        
        public ModConfig()
        {
            Instance = this;
            EnableCategories = true;
        }
    }
}