using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace MegaStorage.UI
{
    public class AllCategory : ChestCategory
    {
        private const int OffsetY = 64;

        public AllCategory(int index, int x, int y) : base(index, "All", -1, null, x, y) { }

        public override void Draw(SpriteBatch b)
        {
            b.Draw(Game1.mouseCursors, new Vector2(X - 72, Y + 32 + Index * OffsetY + 16), new Rectangle(16, 368, 12, 16), Color.White, 4.712389f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
            b.Draw(Game1.mouseCursors, new Vector2(X - 72, Y + 32 + Index * OffsetY - 16), new Rectangle(21, 368, 11, 16), Color.White, 4.712389f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
            b.Draw(Game1.mouseCursors, new Vector2(X - 52, Y + 32 - 44), new Rectangle(sbyte.MaxValue, 412, 10, 11), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
        }

        public override bool BelongsToCategory(Item i)
        {
            return true;
        }

    }
}
