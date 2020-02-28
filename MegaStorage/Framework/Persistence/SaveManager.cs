using furyx639.Common;
using MegaStorage.Framework.Models;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MegaStorage.Framework.Persistence
{
    internal static class SaveManager
    {
        private const string SaveDataKey = "LocationNiceChests";

        private static readonly Dictionary<GameLocation, Dictionary<Vector2, CustomChest>> LocationCustomChests = new Dictionary<GameLocation, Dictionary<Vector2, CustomChest>>();
        public static void Start(FarmhandMonitor farmhandMonitor)
        {
            MegaStorageMod.ModHelper.Events.GameLoop.Saving += (sender, args) => HideAndSaveCustomChests();
            MegaStorageMod.ModHelper.Events.GameLoop.Saved += (sender, args) => ReAddCustomChests();
            MegaStorageMod.ModHelper.Events.GameLoop.ReturnedToTitle += (sender, args) => HideAndSaveCustomChests();
            MegaStorageMod.ModHelper.Events.Multiplayer.PeerContextReceived += (sender, args) => HideAndSaveCustomChests();

            var saveAnywhereApi = MegaStorageMod.ModHelper.ModRegistry.GetApi<ISaveAnywhereApi>("Omegasis.SaveAnywhere");
            if (!(saveAnywhereApi is null))
            {
                saveAnywhereApi.BeforeSave += (sender, args) => HideAndSaveCustomChests();
                saveAnywhereApi.AfterSave += (sender, args) => ReAddCustomChests();
                saveAnywhereApi.AfterLoad += (sender, args) => LoadCustomChests();
            }

            if (!(farmhandMonitor is null))
            {
                farmhandMonitor.Start();
                farmhandMonitor.OnPlayerAdded += ReAddCustomChests;
                farmhandMonitor.OnPlayerRemoved += ReAddCustomChests;
                farmhandMonitor.OnPlayerAdded += FixLegacyOptions;
                farmhandMonitor.OnPlayerRemoved += FixLegacyOptions;
            }

            LoadCustomChests();
            FixLegacyOptions();
        }

        private static void LoadCustomChests()
        {
            MegaStorageMod.ModMonitor.VerboseLog("SaveManager: LoadCustomChests");
            if (!Context.IsMainPlayer)
            {
                MegaStorageMod.ModMonitor.VerboseLog("Not main player!");
                return;
            }
            var saveData = MegaStorageMod.ModHelper.Data.ReadSaveData<SaveData>(SaveDataKey);
            if (saveData == null)
            {
                MegaStorageMod.ModMonitor.VerboseLog("Nothing to load");
                return;
            }
            foreach (var location in CommonHelper.GetLocations())
            {
                var locationName = location.uniqueName?.Value ?? location.Name;
                var customChestsInLocation = saveData.DeserializedChests.Where(x => x.LocationName == locationName);
                foreach (var deserializedChest in customChestsInLocation)
                {
                    var position = new Vector2(deserializedChest.PositionX, deserializedChest.PositionY);
                    if (!location.objects.ContainsKey(position))
                    {
                        MegaStorageMod.ModMonitor.VerboseLog("WARNING! Expected chest at position: " + position);
                        continue;
                    }
                    var chest = (Chest)location.objects[position];
                    var customChest = chest.ToCustomChest(deserializedChest.ChestType, position);
                    MegaStorageMod.ModMonitor.VerboseLog($"Loading: {deserializedChest}");
                    MegaStorageMod.ConvenientChests?.CopyChestData(chest, customChest);
                    location.objects[position] = customChest;
                }
            }
        }

        private static void HideAndSaveCustomChests()
        {
            MegaStorageMod.ModMonitor.VerboseLog("SaveManager: HideAndSaveCustomChests");
            LocationCustomChests.Clear();
            var deserializedChests = new List<DeserializedChest>();
            foreach (var location in CommonHelper.GetLocations())
            {
                var customChestPositions = location.objects.Pairs
                    .Where(x => x.Value is CustomChest)
                    .ToDictionary(pair => pair.Key,
                        pair => (CustomChest)pair.Value);

                if (!customChestPositions.Any())
                {
                    continue;
                }

                var locationName = location.uniqueName?.Value ?? location.Name;
                LocationCustomChests.Add(location, customChestPositions);

                foreach (var customChestPosition in customChestPositions)
                {
                    var position = customChestPosition.Key;
                    var customChest = customChestPosition.Value;
                    var chest = customChest.ToChest();
                    MegaStorageMod.ConvenientChests?.CopyChestData(customChest, chest);
                    location.objects[position] = chest;
                    var deserializedChest = customChest.ToDeserializedChest(locationName, position);
                    MegaStorageMod.ModMonitor.VerboseLog($"Hiding and saving in {locationName}: {deserializedChest}");
                    deserializedChests.Add(deserializedChest);
                }
            }
            if (!Context.IsMainPlayer)
            {
                MegaStorageMod.ModMonitor.VerboseLog("Not main player!");
                return;
            }

            var saveData = new SaveData
            {
                DeserializedChests = deserializedChests
            };
            MegaStorageMod.ModHelper.Data.WriteSaveData(SaveDataKey, saveData);
        }

        private static void ReAddCustomChests()
        {
            MegaStorageMod.ModMonitor.VerboseLog("SaveManager: ReAddCustomChests");
            if (LocationCustomChests == null)
            {
                MegaStorageMod.ModMonitor.VerboseLog("Nothing to re-add");
                return;
            }

            foreach (var customChestLocations in LocationCustomChests)
            {
                var location = customChestLocations.Key;
                var customChestPositions = customChestLocations.Value;

                foreach (var customChestPosition in customChestPositions)
                {
                    var position = customChestPosition.Key;
                    var customChest = customChestPosition.Value;
                    var locationName = location.uniqueName.Value ?? location.Name;
                    MegaStorageMod.ModMonitor.VerboseLog($"Re-adding in {locationName}: {customChest.Name} ({position})");
                    location.objects[position] = customChest;
                }
            }
        }

        private static void FixLegacyOptions()
        {
            MegaStorageMod.ModMonitor.VerboseLog("SaveManager: FixLegacyOptions");
            if (!Context.IsMainPlayer)
            {
                MegaStorageMod.ModMonitor.VerboseLog("Not main player!");
                return;
            }

            var legacySavers = new Dictionary<string, Action<DeserializedChest>>()
            {
                {"InventoryNiceChests", FixInventory},
                {"FarmhandInventoryNiceChests", FixFarmhandInventory},
                {"LocationInventoryNiceChests", FixLocationInventory}
            };

            foreach (var legacySaver in legacySavers)
            {
                var saveData = MegaStorageMod.ModHelper.Data.ReadSaveData<SaveData>(legacySaver.Key);
                if (saveData is null)
                {
                    MegaStorageMod.ModMonitor.VerboseLog($"Nothing to load for {legacySaver.Key}");
                    continue;
                }

                foreach (var deserializedChest in saveData.DeserializedChests)
                {
                    legacySaver.Value.Invoke(deserializedChest);
                }
            }
        }

        private static void FixInventory(DeserializedChest deserializedChest)
        {
            var index = deserializedChest.InventoryIndex;
            var sObject = Game1.player.Items[index].ToObject(deserializedChest.ChestType);
            Game1.player.Items[index] = sObject;
        }

        private static void FixFarmhandInventory(DeserializedChest deserializedChest)
        {
            var playerId = deserializedChest.PlayerId;
            if (!Game1.otherFarmers.ContainsKey(playerId))
            {
                MegaStorageMod.ModMonitor.VerboseLog($"Other player isn't loaded: {playerId}");
                return;
            }
            var player = Game1.otherFarmers.Single(x => x.Key == playerId).Value;
            var index = deserializedChest.InventoryIndex;
            var sObject = player.Items[index].ToObject(deserializedChest.ChestType);
            player.Items[index] = sObject;
        }

        private static void FixLocationInventory(DeserializedChest deserializedChest)
        {
            foreach (var location in CommonHelper.GetLocations())
            {
                var pos = new Vector2(deserializedChest.PositionX, deserializedChest.PositionY);
                if (!location.objects.ContainsKey(pos))
                {
                    MegaStorageMod.ModMonitor.VerboseLog("WARNING! Expected chest at position: " + pos);
                    return;
                }
                var chest = (Chest)location.objects[pos];
                var index = deserializedChest.InventoryIndex;
                var sObject = chest.items[index].ToObject(deserializedChest.ChestType);
                chest.items[index] = sObject;
            }
        }
    }
}
