using MegaStorage.Framework.Models;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MegaStorage.Framework.Persistence
{
    internal class StateManager
    {
        /*********
        ** Fields
        *********/
        public static event EventHandler PlayerAdded;
        public static event EventHandler PlayerRemoved;

        internal static readonly IDictionary<Tuple<GameLocation, Vector2>, CustomChest> PlacedChests =
            new Dictionary<Tuple<GameLocation, Vector2>, CustomChest>();
        internal static CustomChest MainChest
        {
            get =>
                _mainChest ??= PlacedChests
                    .Single(c =>
                        c.Key.Item1.NameOrUniqueName.Equals(_deserializedChest.LocationName,
                            StringComparison.InvariantCultureIgnoreCase) &&
                        c.Key.Item2.Equals(new Vector2(_deserializedChest.PositionX, _deserializedChest.PositionY)))
                    .Value;
            set
            {
                if (!Context.IsMainPlayer || !Context.IsWorldReady)
                    return;
                _mainChest = value;
                _deserializedChest = _mainChest.ToDeserializedChest(Game1.player.currentLocation.NameOrUniqueName);
            }
        }

        private static CustomChest _mainChest;
        private static DeserializedChest _deserializedChest;
        private static int _prevLength;

        /*********
        ** Public methods
        *********/
        public static void Start()
        {
            MegaStorageMod.ModHelper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            MegaStorageMod.ModHelper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            MegaStorageMod.ModHelper.Events.Multiplayer.ModMessageReceived += OnModMessageReceived;
        }

        public static void SendMainChestMessage()
        {
            if (!Context.IsMainPlayer || !Context.IsWorldReady)
                return;
            var playerIDs = MegaStorageMod.ModHelper.Multiplayer
                .GetConnectedPlayers()
                .Where(p => !p.IsHost)
                .Select(p => p.PlayerID)
                .ToArray();
            MegaStorageMod.ModHelper.Multiplayer.SendMessage(
                _deserializedChest,
                "MainChest",
                new[] { MegaStorageMod.Instance.ModManifest.UniqueID },
                playerIDs);
        }


        /*********
        ** Private methods
        *********/
        private static void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            if (!Context.IsMainPlayer)
                return;

            _deserializedChest = MegaStorageMod.ModHelper.Data.ReadSaveData<DeserializedChest>(SaveManager.SaveDataKey);
        }

        private static void OnUpdateTicked(object sender, EventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            var currentLength = Game1.otherFarmers.Count;
            if (currentLength > _prevLength)
            {
                PlayerRemoved?.Invoke(null, null);
            }
            else if (currentLength < _prevLength)
            {
                PlayerAdded?.Invoke(null, null);
            }
            _prevLength = currentLength;
        }

        private static void OnModMessageReceived(object sender, ModMessageReceivedEventArgs e)
        {
            if (Context.IsMainPlayer || !Context.IsWorldReady || e.FromModID != MegaStorageMod.Instance.ModManifest.UniqueID || e.Type != "MainChest")
                return;
            _deserializedChest = e.ReadAs<DeserializedChest>();
            _mainChest = null;
        }
    }
}
