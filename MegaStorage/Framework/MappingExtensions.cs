using System;
using System.Linq;
using MegaStorage.Framework.Models;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace MegaStorage.Framework
{
    public static class MappingExtensions
    {
        public static Item ToObject(this Item item) =>
            item is CustomChest customChest
                ? new SObject(customChest.TileLocation, customChest.ParentSheetIndex, customChest.Stack)
                : item;

        public static Item ToObject(this Item item, ChestType chestType) =>
            item is CustomChest customChest
                ? new SObject(customChest.TileLocation, CustomChestFactory.CustomChests[chestType], customChest.Stack)
                : item;

        public static Chest ToChest(this Item item)
        {
            if (!(item is CustomChest customChest))
                throw new InvalidOperationException($"Cannot convert {item?.Name} to Chest.");

            var chest = new Chest(customChest.playerChest.Value, customChest.TileLocation)
            {
                name = customChest.name,
                Stack = customChest.Stack
            };

            chest.items.AddRange(customChest.items);
            chest.playerChoiceColor.Value = customChest.playerChoiceColor.Value;

            return chest;
        }

        public static CustomChest ToCustomChest(this Item item) =>
            item.ToCustomChest(Vector2.Zero);
        public static CustomChest ToCustomChest(this Item item, Vector2 tileLocation) =>
            item.ToCustomChest(
                CustomChestFactory.CustomChests.FirstOrDefault(x => x.Value == item.ParentSheetIndex).Key,
                tileLocation);
        public static CustomChest ToCustomChest(this Item item, ChestType chestType) =>
            item.ToCustomChest(chestType, Vector2.Zero);
        public static CustomChest ToCustomChest(this Item item, ChestType chestType, Vector2 tileLocation)
        {
            if (!(item is Chest chest))
                throw new InvalidOperationException($"Cannot convert {item?.Name} to CustomChest");

            var customChest = CustomChestFactory.Create(chestType, tileLocation);
            customChest.name = chest.name;
            customChest.Stack = chest.Stack;
            customChest.items.AddRange(chest.items);
            customChest.playerChoiceColor.Value = chest.playerChoiceColor.Value;

            return customChest;
        }

        public static DeserializedChest ToDeserializedChest(this Item item, string locationName, Vector2 position) =>
            item is CustomChest customChest
                ? new DeserializedChest
                {
                    LocationName = locationName,
                    PositionX = position.X,
                    PositionY = position.Y,
                    ChestType = customChest.ChestType,
                    Name = customChest.name
                }
                : throw new InvalidOperationException($"Cannot convert {item?.Name} to DeserializedChest");
    }
}
