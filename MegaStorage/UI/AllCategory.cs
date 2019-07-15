using Microsoft.Xna.Framework;
using StardewValley;

namespace MegaStorage.UI
{
    public class AllCategory : ChestCategory
    {
        public AllCategory(int index, string name, Vector2 spritePos, int x, int y) : base(index, name, spritePos, null, x, y)
        {
        }

        protected override bool BelongsToCategory(Item i)
        {
            return true;
        }
    }
}
