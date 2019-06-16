using MegaStorage.Persistence;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace MegaStorage
{
    public class MegaStorageMod : Mod
    {
        public static IModHelper ModHelper;
        public static IMonitor Logger;
        public static IReflectionHelper Reflection;

        public override void Entry(IModHelper modHelper)
        {
            Monitor.VerboseLog("Entry of MegaStorageMod");
            ModHelper = modHelper;
            Logger = Monitor;
            Reflection = modHelper.Reflection;
            modHelper.Events.GameLoop.GameLaunched += OnGameLaunched;
            modHelper.Content.AssetLoaders.Add(new SpritePatcher(modHelper, Monitor));
            new SaveManager(Helper, Monitor, new ISaver[]
            {
                new InventorySaver(Helper, Monitor),
                new FarmhandInventorySaver(Helper, Monitor),
                new LocationSaver(Helper, Monitor),
                new LocationInventorySaver(Helper, Monitor)
            }).Start();
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            new ItemPatcher(Helper, Monitor).Patch();
        }

    }
}
