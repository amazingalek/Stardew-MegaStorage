using MegaStorage.Framework.Models;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;
using System;
using System.Linq;
using SObject = StardewValley.Object;

namespace MegaStorage.Framework
{
    public static class MappingExtensions
    {
        public static Item ToObject(this Item item) =>
            item is CustomChest chest
                ? new SObject(chest.TileLocation, chest.ParentSheetIndex) { Stack = chest.Stack }
                : item;

        public static Item ToObject(this Item item, ChestType chestType) =>
            item is Chest chest
                ? new SObject(chest.TileLocation, CustomChestFactory.CustomChests[chestType]) { Stack = chest.Stack }
                : item;

        public static Chest ToChest(this Item item)
        {
            if (!(item is CustomChest customChest))
                throw new InvalidOperationException($"Cannot convert {item?.Name} to Chest.");

            var chest = new Chest(customChest.playerChest.Value, customChest.TileLocation)
            {
                name = customChest.name,
                Stack = customChest.Stack,
                ParentSheetIndex = customChest.ParentSheetIndex
            };

            chest.items.AddRange(customChest.items);
            chest.playerChoiceColor.Value = customChest.playerChoiceColor.Value;

            MegaStorageMod.ConvenientChests?.CopyChestData(customChest, chest);

            return chest;
        }

        public static CustomChest ToCustomChest(this Item item, Vector2 tileLocation) =>
            item.ToCustomChest(CustomChestFactory.CustomChests.FirstOrDefault(x => x.Value == item.ParentSheetIndex).Key, tileLocation);
        public static CustomChest ToCustomChest(this Item item, ChestType chestType, Vector2 tileLocation)
        {
            if (!(item is SObject obj))
                throw new InvalidOperationException($"Cannot convert {item?.Name} to CustomChest");

            var customChest = CustomChestFactory.Create(chestType, tileLocation);
            customChest.name = obj.name;
            customChest.Stack = obj.Stack;

            if (!(obj is Chest chest))
                return customChest;

            customChest.TileLocation = chest.TileLocation;
            customChest.items.AddRange(chest.items);
            customChest.playerChoiceColor.Value = chest.playerChoiceColor.Value;
            MegaStorageMod.ConvenientChests?.CopyChestData(chest, customChest);

            return customChest;
        }

        public static DeserializedChest ToDeserializedChest(this CustomChest customChest, string locationName) =>
            !(customChest is null)
                ? new DeserializedChest()
                {
                    LocationName = locationName,
                    PositionX = customChest.TileLocation.X,
                    PositionY = customChest.TileLocation.Y
                }
                : throw new NullReferenceException("customChest is null");
    }
}
