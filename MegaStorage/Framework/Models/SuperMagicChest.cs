using MegaStorage.Framework.Persistence;
using Microsoft.Xna.Framework;

namespace MegaStorage.Framework.Models
{
    public class SuperMagicChest : CustomChest
    {
        public SuperMagicChest(Vector2 tileLocation)
            : base(
                ChestType.SuperMagicChest,
                tileLocation)
        {
            name = "Super Magic Chest";
            Capacity = int.MaxValue;
            EnableRemoteStorage = true;

            if (SaveManager.MainChest is null)
                SaveManager.MainChest = this;
        }
    }
}
