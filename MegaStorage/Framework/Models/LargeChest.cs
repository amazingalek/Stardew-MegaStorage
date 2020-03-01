using Microsoft.Xna.Framework;

namespace MegaStorage.Framework.Models
{
    public class LargeChest : CustomChest
    {
        public LargeChest(Vector2 tileLocation)
            : base(
                ChestType.LargeChest,
                tileLocation)
        {
            name = "Large Chest";
            Capacity = 72;
        }
    }
}
