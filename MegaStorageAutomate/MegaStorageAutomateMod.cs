using MegaStorageAutomate.Mapping;
using Pathoschild.Stardew.Automate;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace MegaStorageAutomate
{
    public class MegaStorageAutomateMod : Mod
    {
        public static MegaStorageAutomateMod Instance { get; private set; }
        public override void Entry(IModHelper modHelper)
        {
            Monitor.VerboseLog("Entry of MegaStorageMod");
            Instance = this;
            Helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var automateApi = Helper.ModRegistry.GetApi<IAutomateAPI>("Pathoschild.Automate");
            automateApi?.AddFactory(new AutomationFactory());
        }
    }
}