using MegaStorage.Models;
using MegaStorage.Persistence;
using MegaStorage.UI;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

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
            modHelper.ReadConfig<Config>();
            new SaveManager(Helper, Monitor, new ISaver[]
            {
                new InventorySaver(Helper, Monitor),
                new FarmhandInventorySaver(Helper, Monitor),
                new LocationSaver(Helper, Monitor),
                new LocationInventorySaver(Helper, Monitor)
            }).Start();
            modHelper.Events.Display.MenuChanged += OnMenuChanged;
        }

        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            Monitor.Log("New menu: " + e.NewMenu?.GetType());
            if (e.NewMenu is LargeItemGrabMenu)
                return;
            if (e.NewMenu is ItemGrabMenu itemGrabMenu && itemGrabMenu.context is NiceChest niceChest)
                Game1.activeClickableMenu = niceChest.CreateItemGrabMenu();
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            Helper.Content.AssetEditors.Add(new SpritePatcher(Helper, Monitor));
            new ItemPatcher(Helper, Monitor).Start();
        }

    }
}
