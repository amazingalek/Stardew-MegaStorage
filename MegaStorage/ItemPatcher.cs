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
            _modHelper.Events.World.ObjectListChanged += OnObjectListChanged;
            _modHelper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            foreach (var customChest in CustomChestFactory.CustomChests)
            {
                Register(customChest);
            }
        }

        private void Register(CustomChest customChest)
        {
            _monitor.VerboseLog($"Registering {customChest.Config.Name} ({customChest.Config.Id})");
            Game1.bigCraftablesInformation[customChest.Config.Id] = customChest.BigCraftableInfo;
            CraftingRecipe.craftingRecipes[customChest.Config.Name] = customChest.RecipeString;
            Game1.player.craftingRecipes[customChest.Config.Name] = 0;
        }

        private void OnInventoryChanged(object sender, InventoryChangedEventArgs e)
        {
            _monitor.VerboseLog("OnInventoryChanged");
            if (!e.IsLocalPlayer || !e.Added.Any())
                return;

            var addedCustomChestsToConvert = e.Added.Where(CustomChestFactory.ShouldBeCustomChest).ToList();
            if (!addedCustomChestsToConvert.Any())
                return;

            _monitor.VerboseLog("OnInventoryChanged: converting");
            
            var addedCustomChestToConvert = addedCustomChestsToConvert.First();
            var customChestToAdd = addedCustomChestToConvert.ToCustomChest();
            customChestToAdd.Stack = addedCustomChestsToConvert.Count;

            var index = Game1.player.Items.IndexOf(addedCustomChestToConvert);
            Game1.player.Items[index] = customChestToAdd;
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
