using MegaStorage.Models;
using Microsoft.Xna.Framework;
using StardewValley.Objects;

namespace MegaStorage.Mapping
{
    public static class MappingExtensions
    {

        public static Chest ToChest(this CustomChest customChest)
        {
            var chest = new Chest(true);
            chest.items.AddRange(customChest.items);
            chest.playerChoiceColor.Value = customChest.playerChoiceColor.Value;
            return chest;
        }

        public static CustomChest ToCustomChest(this Chest chest, ChestType chestType)
        {
            var customChest = CustomChestFactory.Create(chestType);
            customChest.items.AddRange(chest.items);
            customChest.playerChoiceColor.Value = chest.playerChoiceColor.Value;
            return customChest;
        }

        public static DeserializedChest ToDeserializedChest(this CustomChest customChest, int inventoryIndex)
        {
            return new DeserializedChest
            {
                InventoryIndex = inventoryIndex,
                ChestType = customChest.ChestType
            };
        }

        public static DeserializedChest ToDeserializedChest(this CustomChest customChest, long playerId, int inventoryIndex)
        {
            return new DeserializedChest
            {
                PlayerId = playerId,
                InventoryIndex = inventoryIndex,
                ChestType = customChest.ChestType
            };
        }

        public static DeserializedChest ToDeserializedChest(this CustomChest customChest, string locationName, Vector2 position)
        {
            return new DeserializedChest
            {
                LocationName = locationName,
                PositionX = position.X,
                PositionY = position.Y,
                ChestType = customChest.ChestType
            };
        }

        public static DeserializedChest ToDeserializedChest(this CustomChest customChest, string locationName, Vector2 position, int inventoryIndex)
        {
            return new DeserializedChest
            {
                LocationName = locationName,
                PositionX = position.X,
                PositionY = position.Y,
                InventoryIndex = inventoryIndex,
                ChestType = customChest.ChestType
            };
        }

    }
}
