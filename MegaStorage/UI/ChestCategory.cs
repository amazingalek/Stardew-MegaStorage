using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace MegaStorage.UI
{
    public class ChestCategory : ClickableComponent
    {
        protected const int OffsetY = 64;

        protected readonly int Index;
        private readonly string _name;
        private readonly int _itemId;
        private readonly string[] _categoryNames;
        protected readonly int X;
        protected readonly int Y;

        public ChestCategory(int index, string name, int itemId, string[] categoryNames, int x, int y) : base(new Rectangle(x - 72, y + 32 + index * OffsetY - 64, 64, 64), name)
        {
            Index = index;
            _name = name;
            _itemId = itemId;
            _categoryNames = categoryNames;
            X = x;
            Y = y;
        }

        public virtual void Draw(SpriteBatch b)
        {
            b.Draw(Game1.mouseCursors, new Vector2(X - 72, Y + 32 + Index * OffsetY + 16), new Rectangle(16, 368, 12, 16), Color.White, 4.712389f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
            b.Draw(Game1.mouseCursors, new Vector2(X - 72, Y + 32 + Index * OffsetY - 16), new Rectangle(21, 368, 11, 16), Color.White, 4.712389f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
            b.Draw(Game1.objectSpriteSheet, new Vector2(X - 64, Y + 32 + Index * OffsetY - 52), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, _itemId, 16, 16), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
        }

        public virtual bool BelongsToCategory(Item i)
        {
            return _categoryNames.Contains(i.getCategoryName());
        }

        public void DrawTooltip(SpriteBatch b)
        {
            IClickableMenu.drawHoverText(b, _name, Game1.smallFont);
        }

    }
}
