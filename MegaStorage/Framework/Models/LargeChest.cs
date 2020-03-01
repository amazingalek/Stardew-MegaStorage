using Microsoft.Xna.Framework;

namespace MegaStorage.Framework.Models
{
    public class LargeChest : CustomChest
    {
        public override int Capacity => 72;
        public override bool EnableCategories => ModConfig.Instance.LargeChest.EnableCategories;
        public override bool EnableRemoteStorage => false;

        public LargeChest(Vector2 tileLocation)
            : base(
                ChestType.LargeChest,
                tileLocation)
        {
            name = "Large Chest";
        }
    }
}
