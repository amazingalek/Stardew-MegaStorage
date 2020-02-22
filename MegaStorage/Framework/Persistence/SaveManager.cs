using System.Collections.Generic;

namespace MegaStorage.Framework.Persistence
{
    internal class SaveManager
    {
        public static readonly IList<ISaver> Savers = new List<ISaver>();
        public static void Start(FarmhandMonitor farmhandMonitor)
        {
            MegaStorageMod.ModHelper.Events.GameLoop.Saving += (sender, args) => HideAndSaveCustomChests();
            MegaStorageMod.ModHelper.Events.GameLoop.Saved += (sender, args) => ReAddCustomChests();
            MegaStorageMod.ModHelper.Events.GameLoop.ReturnedToTitle += (sender, args) => HideAndSaveCustomChests();
            MegaStorageMod.ModHelper.Events.Multiplayer.PeerContextReceived += (sender, args) => HideAndSaveCustomChests();

            var saveAnywhereApi = MegaStorageMod.ModHelper.ModRegistry.GetApi<ISaveAnywhereApi>("Omegasis.SaveAnywhere");
            if (!(saveAnywhereApi is null))
            {
                saveAnywhereApi.BeforeSave += (sender, args) => HideAndSaveCustomChests();
                saveAnywhereApi.AfterSave += (sender, args) => ReAddCustomChests();
                saveAnywhereApi.AfterLoad += (sender, args) => LoadCustomChests();
            }

            if (!(farmhandMonitor is null))
            {
                farmhandMonitor.Start();
                farmhandMonitor.OnPlayerAdded += ReAddCustomChests;
                farmhandMonitor.OnPlayerRemoved += ReAddCustomChests;
            }

            LoadCustomChests();
        }

        private static void LoadCustomChests()
        {
            MegaStorageMod.ModMonitor.VerboseLog("LoadCustomChests");
            foreach (var saver in Savers)
            {
                saver.LoadCustomChests();
            }
        }

        private static void ReAddCustomChests()
        {
            MegaStorageMod.ModMonitor.VerboseLog("ReAddCustomChests");
            foreach (var saver in Savers)
            {
                saver.ReAddCustomChests();
            }
        }

        private static void HideAndSaveCustomChests()
        {
            MegaStorageMod.ModMonitor.VerboseLog("HideAndSaveCustomChests");
            foreach (var saver in Savers)
            {
                saver.HideAndSaveCustomChests();
            }
        }

    }
}
