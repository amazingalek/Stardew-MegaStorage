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
        public static readonly IDictionary<Tuple<GameLocation, Vector2>, CustomChest> PlacedChests =
            new Dictionary<Tuple<GameLocation, Vector2>, CustomChest>();
        private static int _prevLength;
        public static void Start()
        {
            MegaStorageMod.ModHelper.Events.GameLoop.Saving += (sender, args) => HideAndSaveCustomChests();
            MegaStorageMod.ModHelper.Events.GameLoop.Saved += (sender, args) => ReAddCustomChests();
            MegaStorageMod.ModHelper.Events.GameLoop.ReturnedToTitle += (sender, args) => HideAndSaveCustomChests();
            MegaStorageMod.ModHelper.Events.GameLoop.SaveLoaded += (sender, args) => FixLegacyOptions();
            MegaStorageMod.ModHelper.Events.GameLoop.SaveLoaded += (sender, args) => LoadCustomChests();
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
            if (!Context.IsMainPlayer || !Context.IsWorldReady)
                return;

            MegaStorageMod.ModMonitor.VerboseLog("SaveManager: LoadCustomChests");
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
                    PlacedChests.Add(new Tuple<GameLocation, Vector2>(location, customChest.Key), customChest.Value);
                }
            }
        }

        private static void HideAndSaveCustomChests()
        {
            if (!Context.IsWorldReady)
                return;

            MegaStorageMod.ModMonitor.VerboseLog("SaveManager: HideAndSaveCustomChests");
            foreach (var placedChest in PlacedChests)
            {
                var location = placedChest.Key.Item1;
                var pos = placedChest.Key.Item2;
                var customChest = placedChest.Value;
                MegaStorageMod.ModMonitor.VerboseLog($"Hiding and Saving in {location.Name}: {customChest.Name} ({pos})");
                location.objects[pos] = customChest.ToChest();
            }
        }

        private static void ReAddCustomChests()
        {
            if (!Context.IsWorldReady)
                return;

            MegaStorageMod.ModMonitor.VerboseLog("SaveManager: ReAddCustomChests");
            foreach (var placedChest in PlacedChests)
            {
                var location = placedChest.Key.Item1;
                var pos = placedChest.Key.Item2;
                var customChest = placedChest.Value;
                MegaStorageMod.ModMonitor.VerboseLog($"Hiding and Saving in {location.Name}: {customChest.Name} ({pos})");
                location.objects[pos] = customChest;
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
            if (currentLength <= _prevLength)
                return;

            MegaStorageMod.ModHelper.Events.GameLoop.UpdateTicked -= OnUpdateTicked;
            _prevLength = currentLength;
            FixLegacyOptions();
        }

        private static void FixLegacyOptions()
        {
            MegaStorageMod.ModMonitor.VerboseLog("SaveManager: FixLegacyOptions");
            if (!Context.IsMainPlayer)
                return;

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
            player.Items[index] = chest.ToObject(chestType);
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
