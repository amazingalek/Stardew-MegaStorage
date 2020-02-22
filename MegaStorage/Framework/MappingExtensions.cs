using MegaStorage.Framework.Models;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace MegaStorage.Framework
{
    public static class MappingExtensions
    {
        public static Item ToObject(this CustomChest customChest)
        {
            if (customChest is null)
            {
                return null;
            }

            return new SObject(Vector2.Zero, customChest.ParentSheetIndex)
            {
                Stack = customChest.Stack
            };
        }

        public static Item ToObject(this Item item, ChestType chestType)
        {
            if (!(item is SObject))
            {
                return item;
            }

            var parentSheetIndex = chestType switch
            {
                ChestType.LargeChest => MegaStorageMod.LargeChestId,
                ChestType.MagicChest => MegaStorageMod.MagicChestId,
                ChestType.SuperMagicChest => MegaStorageMod.SuperMagicChestId,
                _ => 0
            };

            return new SObject(Vector2.Zero, parentSheetIndex)
            {
                Stack = item.Stack
            };
        }
        public static Chest ToChest(this CustomChest customChest)
        {
            if (customChest is null)
            {
                return null;
            }

            var chest = new Chest(true)
            {
                name = customChest.name,
                Stack = customChest.Stack
            };

            chest.items.AddRange(customChest.items);
            chest.playerChoiceColor.Value = customChest.playerChoiceColor.Value;

            return chest;
        }

        public static CustomChest ToCustomChest(this Chest chest, ChestType chestType) =>
            chest.ToCustomChest(chestType, Vector2.Zero);
        public static CustomChest ToCustomChest(this Chest chest, ChestType chestType, Vector2 tileLocation)
        {
            if (chest is null)
            {
                return null;
            }

            var customChest = CustomChestFactory.Create(chestType, tileLocation);
            customChest.items.AddRange(chest.items);
            customChest.playerChoiceColor.Value = chest.playerChoiceColor.Value;
            customChest.name = chest.name;
            customChest.Stack = chest.Stack;

            return customChest;
        }

        public static DeserializedChest ToDeserializedChest(this CustomChest customChest, int inventoryIndex)
        {
            return customChest is null
                ? null
                : new DeserializedChest
                {
                    InventoryIndex = inventoryIndex,
                    ChestType = customChest.ChestType,
                    Name = customChest.name
                };
        }

        public static DeserializedChest ToDeserializedChest(this CustomChest customChest, string locationName, Vector2 position)
        {
            return customChest is null
                ? null
                : new DeserializedChest
                {
                    LocationName = locationName,
                    PositionX = position.X,
                    PositionY = position.Y,
                    ChestType = customChest.ChestType,
                    Name = customChest.name
                };
        }

        public static DeserializedChest ToDeserializedChest(this CustomChest customChest, long playerId, int inventoryIndex)
        {
            var deserializedChest = customChest.ToDeserializedChest(inventoryIndex);
            deserializedChest.PlayerId = playerId;
            return deserializedChest;
        }

        public static DeserializedChest ToDeserializedChest(this CustomChest customChest, string locationName, Vector2 position, int inventoryIndex)
        {
            var deserializedChest = customChest.ToDeserializedChest(locationName, position);
            deserializedChest.InventoryIndex = inventoryIndex;
            return deserializedChest;
        }

        public static CustomChest ToCustomChest(this Item item) => item.ToCustomChest(Vector2.Zero);
        public static CustomChest ToCustomChest(this Item item, Vector2 tileLocation)
        {
            if (item is null)
            {
                return null;
            }

            var customChest = CustomChestFactory.Create(item.ParentSheetIndex, tileLocation);
            customChest.name = item.Name;
            customChest.Stack = item.Stack;
            return customChest;
        }

    }
}
