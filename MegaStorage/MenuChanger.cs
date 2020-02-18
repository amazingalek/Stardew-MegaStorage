using MegaStorage.Framework.Interface;
using MegaStorage.Framework.Models;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace MegaStorage
{
    public class MenuChanger
    {
        public void Start()
        {
            MegaStorageMod.ModHelper.Events.Display.MenuChanged += OnMenuChanged;
        }

        private static void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            MegaStorageMod.ModMonitor.VerboseLog("New menu: " + e.NewMenu?.GetType());
            if (e.NewMenu is LargeItemGrabMenu)
                return;
            if (!(e.NewMenu is ItemGrabMenu itemGrabMenu) || !(itemGrabMenu.context is CustomChest customChest))
                return;
            Game1.activeClickableMenu = customChest.GetItemGrabMenu();
        }
    }
}