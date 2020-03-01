using Microsoft.Xna.Framework;

namespace MegaStorage.Framework.Models
{
    public class MagicChest : CustomChest
    {
        public override int Capacity => int.MaxValue;
        public override bool EnableCategories => ModConfig.Instance.MagicChest.EnableCategories;
        public override bool EnableRemoteStorage => false;

        public MagicChest(Vector2 tileLocation)
            : base(
                ChestType.MagicChest,
                tileLocation)
        {
            name = "Magic Chest";
        }
    }
}
