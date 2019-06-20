using System.Threading.Tasks;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace MegaStorage.Persistence
{
    public class SaveManager
    {
        private readonly IModHelper _modHelper;
        private readonly IMonitor _monitor;
        private readonly ISaver[] _savers;

        public SaveManager(IModHelper modHelper, IMonitor monitor, ISaver[] savers)
        {
            _modHelper = modHelper;
            _monitor = monitor;
            _savers = savers;
        }

        public void Start()
        {
            _modHelper.Events.GameLoop.SaveLoaded += (sender, args) => LoadNiceChests();
            _modHelper.Events.GameLoop.Saving += (sender, args) => HideAndSaveNiceChests();
            _modHelper.Events.GameLoop.Saved += (sender, args) => ReAddNiceChests();
            _modHelper.Events.GameLoop.ReturnedToTitle += (sender, args) => HideAndSaveNiceChests();

            _modHelper.Events.Multiplayer.PeerContextReceived += OnPeerContextReceived;
            _modHelper.Events.Multiplayer.PeerDisconnected += OnPeerDisconnected;
        }

        private void LoadNiceChests()
        {
            _monitor.VerboseLog("LoadNiceChests");
            foreach (var saver in _savers)
            {
                saver.LoadNiceChests();
            }
        }

        private void ReAddNiceChests()
        {
            _monitor.VerboseLog("ReAddNiceChests");
            foreach (var saver in _savers)
            {
                saver.ReAddNiceChests();
            }
        }

        private void HideAndSaveNiceChests()
        {
            _monitor.VerboseLog("HideAndSaveNiceChests");
            foreach (var saver in _savers)
            {
                saver.HideAndSaveNiceChests();
            }
        }

        private async void OnPeerContextReceived(object sender, PeerContextReceivedEventArgs e)
        {
            _monitor.VerboseLog("OnPeerContextReceived");
            HideAndSaveNiceChests();
            await Task.Delay(1000); // hack :-(
            ReAddNiceChests();
        }

        private async void OnPeerDisconnected(object sender, PeerDisconnectedEventArgs e)
        {
            _monitor.VerboseLog("OnPeerDisconnected");
            await Task.Delay(1000); // hack :-(
            ReAddNiceChests();
        }

    }
}
