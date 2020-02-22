using System.Collections.Generic;
using SObject = StardewValley.Object;

namespace MegaStorage.Framework.Models
{
    public class ModConfig
    {
        public CustomChestConfig LargeChest { get; set; } = new CustomChestConfig
        {
            SpritePath = "LargeChest.png",
            SpriteBWPath = "LargeChestBW.png",
            SpriteBracesPath = "LargeChestBraces.png"
        };
        public CustomChestConfig MagicChest { get; set; } = new CustomChestConfig
        {
            SpritePath = "MagicChest.png",
            SpriteBWPath = "MagicChestBW.png",
            SpriteBracesPath = "MagicChestBraces.png"
        };
        public CustomChestConfig SuperMagicChest { get; set; } = new CustomChestConfig
        {
            SpritePath = "SuperMagicChest.png",
            SpriteBWPath = "SuperMagicChestBW.png",
            SpriteBracesPath = "SuperMagicChestBraces.png"
        };
        public bool EnableCategories { get; set; } = true;
        public IDictionary<string, IList<int>> Categories { get; set; } = new Dictionary<string, IList<int>>()
        {
            {
                "Crops", new List<int>()
                {
                    SObject.GreensCategory,
                    SObject.flowersCategory,
                    SObject.FruitsCategory,
                    SObject.VegetableCategory
                }
            },
            {
                "Seeds", new List<int>()
                {
                    SObject.SeedsCategory,
                    SObject.fertilizerCategory
                }
            },
            {
                "Materials", new List<int>()
                {
                    SObject.metalResources,
                    SObject.buildingResources,
                    SObject.GemCategory,
                    SObject.mineralsCategory,
                    SObject.CraftingCategory,
                    SObject.monsterLootCategory
                }
            },
            {
                "Cooking", new List<int>()
                {
                    SObject.ingredientsCategory,
                    SObject.CookingCategory,
                    SObject.sellAtPierresAndMarnies,
                    SObject.meatCategory,
                    SObject.MilkCategory,
                    SObject.EggCategory,
                    SObject.syrupCategory,
                    SObject.artisanGoodsCategory
                }
            },
            {
                "Fishing", new List<int>()
                {
                    SObject.FishCategory,
                    SObject.baitCategory,
                    SObject.tackleCategory
                }
            },
            {
                "Misc", new List<int>()
                {
                    SObject.furnitureCategory,
                    SObject.junkCategory
                }
            }
        };
        public ModConfig()
        {
            Instance = this;
        }
        public static ModConfig Instance { get; private set; }
    }
}