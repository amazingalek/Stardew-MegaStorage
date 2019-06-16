using MegaStorage.Persistence;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace MegaStorage
{
    public class MegaStorageMod : Mod, IAssetLoader
    {
        public static IModHelper ModHelper;
        public static IMonitor Logger;
        public static IReflectionHelper Reflection;

        private Texture2D _patchedSpriteSheet;

        public override void Entry(IModHelper modHelper)
        {
            Monitor.VerboseLog("Entry of MegaStorageMod");
            ModHelper = modHelper;
            Logger = Monitor;
            Reflection = modHelper.Reflection;
            modHelper.Events.GameLoop.GameLaunched += OnGameLaunched;
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

        public bool CanLoad<T>(IAssetInfo asset) => asset.AssetNameEquals(SpritePatcher.SpriteSheetName);
        public T Load<T>(IAssetInfo asset)
        {
            if (_patchedSpriteSheet == null)
            {
                _patchedSpriteSheet = new SpritePatcher(Helper, Monitor).Patch();
            }
            return (T)(object)_patchedSpriteSheet;
        }

    }
}
