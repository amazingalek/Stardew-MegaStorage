using MegaStorage.Models;
using MegaStorage.Persistence;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace MegaStorage
{
    public class MegaStorageMod : Mod
    {
        public static MegaStorageMod Instance;

        private SpritePatcher _spritePatcher;
        private ItemPatcher _itemPatcher;
        private MenuChanger _menuChanger;
        private SaveManager _saveManager;

        public override void Entry(IModHelper modHelper)
        {
            Monitor.VerboseLog("Entry of MegaStorageMod");
            Instance = this;

            var convenientChestsApi = Helper.ModRegistry.GetApi<IConvenientChestsApi>("aEnigma.ConvenientChests");
            
            _spritePatcher = new SpritePatcher(Helper, Monitor);
            _itemPatcher = new ItemPatcher(Helper, Monitor);
            _menuChanger = new MenuChanger(Helper, Monitor);
            _saveManager = new SaveManager(Helper, Monitor,
                new FarmhandMonitor(Helper, Monitor),
                new InventorySaver(Helper, Monitor, convenientChestsApi),
                new FarmhandInventorySaver(Helper, Monitor, convenientChestsApi),
                new LocationSaver(Helper, Monitor, convenientChestsApi),
                new LocationInventorySaver(Helper, Monitor, convenientChestsApi));

            Helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            Helper.ReadConfig<ModConfig>();
            Helper.Content.AssetEditors.Add(_spritePatcher);
            _itemPatcher.Start();
            _saveManager.Start();
            _menuChanger.Start();
        }

    }
}
