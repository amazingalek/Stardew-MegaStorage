using MegaStorage.Framework.Persistence;
using Microsoft.Xna.Framework;

namespace MegaStorage.Framework.Models
{
    public class SuperMagicChest : CustomChest
    {
        public override int Capacity => SaveManager.MainChest == this ? int.MaxValue : 0;
        public SuperMagicChest(Vector2 tileLocation)
            : base(
                ChestType.SuperMagicChest,
                tileLocation)
        {
            name = "Super Magic Chest";
            EnableRemoteStorage = true;
        }
    }
}
