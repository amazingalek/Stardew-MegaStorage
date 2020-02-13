using System;
using MegaStorageAutomate.Mapping;
using Pathoschild.Stardew.Automate;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace MegaStorageAutomate
{
    public class MegaStorageAutomateMod : Mod
    {
        public static MegaStorageAutomateMod Instance { get; private set; }
        public static int LargeChestId { get; private set; }
        public static int MagicChestId { get; private set; }
        private IJsonAssetsApi _jsonAssetsApi;

        public override void Entry(IModHelper modHelper)
        {
            Monitor.VerboseLog("Entry of MegaStorageMod");
            Instance = this;
            Helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            _jsonAssetsApi = Helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");
            if (_jsonAssetsApi is null)
                return;
            _jsonAssetsApi.IdsAssigned += OnIdsAssigned;

            var automateApi = Helper.ModRegistry.GetApi<IAutomateAPI>("Pathoschild.Automate");
            automateApi?.AddFactory(new AutomationFactory());
        }

        private void OnIdsAssigned(object sender, EventArgs e)
        {
            LargeChestId = _jsonAssetsApi.GetBigCraftableId("Large Chest");
            MagicChestId = _jsonAssetsApi.GetBigCraftableId("Magic Chest");
            Monitor.VerboseLog($"Large Chest ID is {LargeChestId}.");
            Monitor.VerboseLog($"Magic Chest ID is {MagicChestId}.");
        }
    }
}
