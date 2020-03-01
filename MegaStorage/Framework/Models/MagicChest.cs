using Microsoft.Xna.Framework;

namespace MegaStorage.Framework.Models
{
    public class MagicChest : CustomChest
    {
        public MagicChest(Vector2 tileLocation)
            : base(
                ChestType.MagicChest,
                tileLocation)
        {
            name = "Magic Chest";
            Capacity = int.MaxValue;
        }
    }
}
