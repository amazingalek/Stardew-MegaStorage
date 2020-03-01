using MegaStorage.Framework.Models;
using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using SObject = StardewValley.Object;

namespace MegaStorage.Framework
{
    public static class CustomChestFactory
    {
        private static IDictionary<ChestType, int> _customChests;

        public static IDictionary<ChestType, int> CustomChests =>
            _customChests ??= new Dictionary<ChestType, int>
            {
                {ChestType.LargeChest, MegaStorageMod.LargeChestId},
                {ChestType.MagicChest, MegaStorageMod.MagicChestId},
                {ChestType.SuperMagicChest, MegaStorageMod.SuperMagicChestId}
            };

        public static bool ShouldBeCustomChest(Item item) =>
            item is SObject obj
            && obj.bigCraftable.Value
            && CustomChests.Any(x => x.Value == obj.ParentSheetIndex);

        public static CustomChest Create(ChestType chestType, Vector2 tileLocation) =>
            chestType switch
            {
                ChestType.LargeChest => new LargeChest(tileLocation),
                ChestType.MagicChest => new MagicChest(tileLocation),
                ChestType.SuperMagicChest => new SuperMagicChest(tileLocation),
                _ => throw new InvalidOperationException("Invalid ChestType")
            };

    }
}
