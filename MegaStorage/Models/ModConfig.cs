namespace MegaStorage.Models
{
    public class ModConfig
    {
        public CustomChestConfig LargeChest { get; set; }
        public CustomChestConfig MagicChest { get; set; }

        public ModConfig()
        {
            Instance = this;

            LargeChest = new CustomChestConfig()
            {
                Id = 816,
                Name = "Large Chest",
                Description = "A large place to store your items.",
                Recipe = "388 100 334 5 335 5",
                SpritePath = "Sprites/LargeChest.png",
                SpriteBWPath = "Sprites/LargeChestBW.png",
                SpriteBracesPath = "Sprites/LargeChestBraces.png"
            };

            MagicChest = new CustomChestConfig()
            {
                Id = 817,
                Name = "Magic Chest",
                Description = "A magical place to store your items.",
                Recipe = "709 100 336 5 337 5 768 50 769 50",
                SpritePath = "Sprites/MagicChest.png",
                SpriteBWPath = "Sprites/MagicChestBW.png",
                SpriteBracesPath = "Sprites/MagicChestBraces.png"
            };
        }

        public static ModConfig Instance { get; private set; }
    }
}