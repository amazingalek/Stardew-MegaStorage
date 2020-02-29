using furyx639.Common;
using MegaStorage.Framework.Models;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using Context = StardewModdingAPI.Context;

namespace MegaStorage.Framework.Persistence
{
    internal static class SaveManager
    {
        private static readonly List<Tuple<GameLocation, Vector2, CustomChest>> CustomChests =
            new List<Tuple<GameLocation, Vector2, CustomChest>>();
        private static int _prevLength = 0;
        public static void Start()
        {
            MegaStorageMod.ModHelper.Events.GameLoop.Saving += (sender, args) => HideAndSaveCustomChests();
            MegaStorageMod.ModHelper.Events.GameLoop.Saved += (sender, args) => ReAddCustomChests();
            MegaStorageMod.ModHelper.Events.GameLoop.ReturnedToTitle += (sender, args) => HideAndSaveCustomChests();
            MegaStorageMod.ModHelper.Events.Multiplayer.PeerContextReceived += OnPeerContextReceived;
            MegaStorageMod.ModHelper.Events.Multiplayer.PeerDisconnected += OnPeerDisconnected;

            var saveAnywhereApi = MegaStorageMod.ModHelper.ModRegistry.GetApi<ISaveAnywhereApi>("Omegasis.SaveAnywhere");
            if (!(saveAnywhereApi is null))
            {
                saveAnywhereApi.BeforeSave += (sender, args) => HideAndSaveCustomChests();
                saveAnywhereApi.AfterSave += (sender, args) => ReAddCustomChests();
                saveAnywhereApi.AfterLoad += (sender, args) => LoadCustomChests();
            }

            FixLegacyOptions();
            LoadCustomChests();
        }

        private static void LoadCustomChests()
        {
            MegaStorageMod.ModMonitor.VerboseLog("SaveManager: LoadCustomChests");
            if (!Context.IsMainPlayer)
                return;

            foreach (var location in CommonHelper.GetLocations())
            {
                var customChests = location.Objects.Pairs
                    .Where(c => c.Value is Chest chest && CustomChestFactory.ShouldBeCustomChest(chest))
                    .ToDictionary(
                        c => c.Key,
                        c => c.Value.ToCustomChest());

                foreach (var customChest in customChests)
                {
                    MegaStorageMod.ModMonitor.VerboseLog($"Loading Chest at: {location.Name} {customChest.Key}");
                    location.objects[customChest.Key] = customChest.Value;
                }
            }
        }

        private static void HideAndSaveCustomChests()
        {
            MegaStorageMod.ModMonitor.VerboseLog("SaveManager: HideAndSaveCustomChests");
            CustomChests.Clear();

            foreach (var location in CommonHelper.GetLocations())
            {
                var customChests = location.Objects.Pairs
                    .Where(c => c.Value is CustomChest)
                    .Select(c => new Tuple<GameLocation, Vector2, CustomChest>(location, c.Key, (CustomChest)c.Value))
                    .ToList();

                CustomChests.AddRange(customChests);

                foreach (var customChest in customChests)
                {
                    MegaStorageMod.ModMonitor.VerboseLog($"Hiding and Saving in {customChest.Item1.Name}: {customChest.Item3.Name} ({customChest.Item2})");
                    location.objects[customChest.Item2] = customChest.Item3.ToChest();
                }
            }
        }

        private static void ReAddCustomChests()
        {
            MegaStorageMod.ModMonitor.VerboseLog("SaveManager: ReAddCustomChests");

            foreach (var customChest in CustomChests)
            {
                MegaStorageMod.ModMonitor.VerboseLog($"Re-adding in {customChest.Item1.Name}: {customChest.Item3.Name} ({customChest.Item2})");
                customChest.Item1.objects[customChest.Item2] = customChest.Item3.ToCustomChest(customChest.Item2);
            }
        }

        private static void OnPeerContextReceived(object sender, PeerContextReceivedEventArgs e)
        {
            MegaStorageMod.ModHelper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
        }

        private static void OnPeerDisconnected(object sender, PeerDisconnectedEventArgs e)
        {
            _prevLength = Game1.otherFarmers.Count;
        }

        private static void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            var currentLength = Game1.otherFarmers.Count;
            if (currentLength > _prevLength)
            {
                FixLegacyOptions();
                _prevLength = currentLength;
            }
            MegaStorageMod.ModHelper.Events.GameLoop.UpdateTicked -= OnUpdateTicked;
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
                {"LocationNiceChests", FixLocation},
                {"FarmhandInventoryNiceChests", FixFarmhandInventory},
                {"LocationInventoryNiceChests", FixLocationInventory}
            };

            foreach (var legacySaver in legacySavers)
            {
                var saveData = MegaStorageMod.ModHelper.Data.ReadSaveData<SaveData>(legacySaver.Key);
                if (saveData is null)
                    continue;
                foreach (var deserializedChest in saveData.DeserializedChests)
                {
                    legacySaver.Value.Invoke(deserializedChest);
                }
            }
        }

        /// <summary>
        /// Reverts all Custom Chests in player inventory back to a regular Object
        /// </summary>
        /// <param name="deserializedChest">Save data for custom chest</param>
        private static void FixInventory(DeserializedChest deserializedChest)
        {
            var index = deserializedChest.InventoryIndex;
            var chestType = deserializedChest.ChestType;

            // Only revert chests in expected position
            if (!(Game1.player.Items[index] is Chest chest))
                return;
            Game1.player.Items[index] = chest.ToObject(chestType);
        }

        /// <summary>
        /// Reverts all Custom Chests in farmhand inventory back to a regular Object
        /// </summary>
        /// <param name="deserializedChest">Save data for custom chest</param>
        private static void FixFarmhandInventory(DeserializedChest deserializedChest)
        {
            var playerId = deserializedChest.PlayerId;
            var index = deserializedChest.InventoryIndex;
            var chestType = deserializedChest.ChestType;

            // Only revert chests in expected position
            if (!Game1.otherFarmers.ContainsKey(playerId))
                return;
            var player = Game1.otherFarmers.Single(x => x.Key == playerId).Value;
            if (!(player.Items[index] is Chest chest))
                return;
            player.Items[index] = player.Items[index].ToObject(chestType);
        }

        /// <summary>
        /// Updates all placed Custom Chests with correct ParentSheetIndex
        /// </summary>
        /// <param name="deserializedChest">Save data for custom chest</param>
        private static void FixLocation(DeserializedChest deserializedChest)
        {
            var locationName = deserializedChest.LocationName;
            var pos = new Vector2(deserializedChest.PositionX, deserializedChest.PositionY);
            var chestType = deserializedChest.ChestType;

            var location = CommonHelper.GetLocations()
                .Single(l => (l.uniqueName?.Value ?? l.Name) == locationName);
            if (location is null
                || !location.objects.ContainsKey(pos)
                || !(location.objects[pos] is Chest chest))
            {
                return;
            }

            chest.ParentSheetIndex = CustomChestFactory.CustomChests[chestType];
        }

        /// <summary>
        /// Reverts all Custom Chests in placed chests back to a regular Object
        /// </summary>
        /// <param name="deserializedChest">Save data for custom chest</param>
        private static void FixLocationInventory(DeserializedChest deserializedChest)
        {
            var locationName = deserializedChest.LocationName;
            var pos = new Vector2(deserializedChest.PositionX, deserializedChest.PositionY);
            var index = deserializedChest.InventoryIndex;
            var chestType = deserializedChest.ChestType;

            var location = CommonHelper.GetLocations()
                .Single(l => (l.uniqueName?.Value ?? l.Name) == locationName);
            if (location is null
                || !location.objects.ContainsKey(pos)
                || !(location.objects[pos] is Chest chest)
                || !(chest.items[index] is Chest customChest))
            {
                return;
            }

            chest.items[index] = customChest.items[index].ToObject(chestType);
        }
    }
}
