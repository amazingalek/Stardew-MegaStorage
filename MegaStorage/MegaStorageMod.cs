using System;
using System.IO;
using MegaStorage.Models;
using MegaStorage.Persistence;
using MegaStorageAutomate;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace MegaStorage
{
    public class MegaStorageMod : Mod
    {
        public static MegaStorageMod Instance { get; private set; }
        public static int LargeChestId { get; private set; }
        public static int MagicChestId { get; private set; }
        private IJsonAssetsApi _jsonAssetsApi;

        public override void Entry(IModHelper modHelper)
        {
            Monitor.VerboseLog("Entry of MegaStorageMod");
            Instance = this;

            Helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            Helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            _jsonAssetsApi = Helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");
            if (_jsonAssetsApi is null)
            {
                Monitor.Log("JsonAssets is needed to load Mega Storage chests", LogLevel.Error);
                return;
            }
            _jsonAssetsApi.LoadAssets(Path.Combine(Helper.DirectoryPath, "assets", "JsonAssets"));
            _jsonAssetsApi.IdsAssigned += OnIdsAssigned;
        }

        private void OnIdsAssigned(object sender, EventArgs e)
        {
            LargeChestId = _jsonAssetsApi.GetBigCraftableId("Large Chest");
            MagicChestId = _jsonAssetsApi.GetBigCraftableId("Magic Chest");
            Monitor.VerboseLog($"Large Chest ID is {LargeChestId}.");
            Monitor.VerboseLog($"Magic Chest ID is {MagicChestId}.");
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            var convenientChestsApi = Helper.ModRegistry.GetApi<IConvenientChestsApi>("aEnigma.ConvenientChests");

            var spritePatcher = new SpritePatcher(Helper, Monitor);
            var itemPatcher = new ItemPatcher(Helper, Monitor);
            var menuChanger = new MenuChanger(Helper, Monitor);
            var saveManager = new SaveManager(Helper, Monitor,
                new FarmhandMonitor(Helper, Monitor),
                new InventorySaver(Helper, Monitor, convenientChestsApi),
                new FarmhandInventorySaver(Helper, Monitor, convenientChestsApi),
                new LocationSaver(Helper, Monitor, convenientChestsApi),
                new LocationInventorySaver(Helper, Monitor, convenientChestsApi));

            Helper.ReadConfig<ModConfig>();
            Helper.Content.AssetEditors.Add(spritePatcher);
            itemPatcher.Start();
            saveManager.Start();
            menuChanger.Start();

            if (!(convenientChestsApi is null))
                ModConfig.Instance.EnableCategories = false;
        }
    }
}