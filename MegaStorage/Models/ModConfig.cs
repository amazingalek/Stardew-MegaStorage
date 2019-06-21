namespace MegaStorage.Models
{
    public class ModConfig
    {
        public CustomChestConfig LargeChest { get; set; }
        public CustomChestConfig MagicChest { get; set; }

        public ModConfig()
        {
            Instance = this;
        }

        public static ModConfig Instance { get; private set; }
    }

    public class CustomChestConfig
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Recipe { get; set; }
        public string SpritePath { get; set; }
        public string SpriteBWPath { get; set; }
        public string SpriteBracesPath { get; set; }
    }

}
