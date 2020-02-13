namespace MegaStorage.Models
{
    public class ModConfig
    {
        public CustomChestConfig LargeChest { get; set; }
        public CustomChestConfig MagicChest { get; set; }
        public bool EnableCategories { get; set; }
        public ModConfig()
        {
            Instance = this;
            
            LargeChest = new CustomChestConfig()
            {
                SpritePath = "LargeChest.png",
                SpriteBWPath = "LargeChestBW.png",
                SpriteBracesPath = "LargeChestBraces.png"
            };
            
            MagicChest = new CustomChestConfig()
            {
                SpritePath = "MagicChest.png",
                SpriteBWPath = "MagicChestBW.png",
                SpriteBracesPath = "MagicChestBraces.png"
            };

            EnableCategories = true;
        }
        public static ModConfig Instance { get; private set; }
    }
}