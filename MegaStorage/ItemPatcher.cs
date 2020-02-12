using System.Linq;
using MegaStorage.Mapping;
using MegaStorage.Models;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace MegaStorage
{
    public class ItemPatcher
    {
        private readonly IModHelper _modHelper;
        private readonly IMonitor _monitor;

        public ItemPatcher(IModHelper modHelper, IMonitor monitor)
        {
            _modHelper = modHelper;
            _monitor = monitor;
        }

        public void Start()
        {
            _modHelper.Events.Player.InventoryChanged += OnInventoryChanged;
            _modHelper.Events.World.ChestInventoryChanged += OnChestInventoryChanged;
            _modHelper.Events.World.ObjectListChanged += OnObjectListChanged;
        }

        private void OnInventoryChanged(object sender, InventoryChangedEventArgs e)
        {
            _monitor.VerboseLog("OnInventoryChanged");
            if (!e.IsLocalPlayer || e.Added.Count() != 1)
                return;

            var addedItem = e.Added.Single();
            if (addedItem is CustomChest)
                return;

            if (!CustomChestFactory.ShouldBeCustomChest(addedItem))
                return;

            _monitor.VerboseLog("OnInventoryChanged: converting");

            var index = Game1.player.Items.IndexOf(addedItem);
            Game1.player.Items[index] = addedItem.ToCustomChest();
        }

        private void OnChestInventoryChanged(object sender, ChestInventoryChangedEventArgs e)
        {
            _monitor.VerboseLog("OnChestInventoryChanged");
            if (e.Added.Count() != 1)
                return;

            var addedItem = e.Added.Single();
            if (addedItem is CustomChest)
                return;

            if (!CustomChestFactory.ShouldBeCustomChest(addedItem))
                return;

            _monitor.VerboseLog("OnChestInventoryChanged: converting");

            var index = Game1.player.Items.IndexOf(addedItem);
            Game1.player.Items[index] = addedItem.ToCustomChest();
        }

        private void OnObjectListChanged(object sender, ObjectListChangedEventArgs e)
        {
            _monitor.VerboseLog("OnObjectListChanged");
            if (e.Added.Count() != 1)
                return;

            var addedItemPosition = e.Added.Single();
            var addedItem = addedItemPosition.Value;
            if (addedItem is CustomChest)
                return;

            if (!CustomChestFactory.ShouldBeCustomChest(addedItem))
                return;

            _monitor.VerboseLog("OnObjectListChanged: converting");

            var position = addedItemPosition.Key;
            var item = e.Location.objects[position];
            e.Location.objects[position] = item.ToCustomChest();
        }

    }
}
