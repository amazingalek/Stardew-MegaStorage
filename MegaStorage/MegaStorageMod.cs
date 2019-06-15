using MegaStorage.Persistence;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace MegaStorage
{
    public class MegaStorageMod : Mod, IAssetLoader
    {
        public static IModHelper ModHelper;
        public static IMonitor Logger;
        public static IReflectionHelper Reflection;

        private SpritePatcher _spritePatcher;

        public override void Entry(IModHelper modHelper)
        {
            Monitor.VerboseLog("Entry of MegaStorageMod");
            ModHelper = modHelper;
            Logger = Monitor;
            Reflection = modHelper.Reflection;
            _spritePatcher = new SpritePatcher(Helper, Monitor);
            _spritePatcher.Patch();
            modHelper.Events.GameLoop.GameLaunched += OnGameLaunched;
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            new SaveManager(Helper, Monitor, new ISaver[]
            {
                new InventorySaver(Helper, Monitor),
                new FarmhandInventorySaver(Helper, Monitor),
                new LocationSaver(Helper, Monitor),
                new LocationInventorySaver(Helper, Monitor)
            }).Start();
            new ItemPatcher(Helper, Monitor).Patch();
        }

        public bool CanLoad<T>(IAssetInfo asset)
        {
            return asset.AssetNameEquals("TileSheets/Craftables");
        }

        public T Load<T>(IAssetInfo asset)
        {
            return (T)(object)_spritePatcher.PatchedSpriteSheet;
        }
    }
}
