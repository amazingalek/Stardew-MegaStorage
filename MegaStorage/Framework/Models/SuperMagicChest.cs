using Microsoft.Xna.Framework;

namespace MegaStorage.Framework.Models
{
    public class SuperMagicChest : CustomChest
    {
        public override int Capacity => int.MaxValue;
        public override bool EnableCategories => ModConfig.Instance.SuperMagicChest.EnableCategories;
        public override bool EnableRemoteStorage => true;

        public SuperMagicChest(Vector2 tileLocation)
            : base(
                ChestType.SuperMagicChest,
                tileLocation)
        {
            name = "Super Magic Chest";
        }
    }
}
