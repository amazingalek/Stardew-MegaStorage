using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace MegaStorage.UI
{
    public class ChestCategory : ClickableComponent
    {
        private const int OffsetY = 64;

        private readonly int _index;
        private readonly string _name;
        private readonly string[] _categoryNames;
        private readonly int _x;
        private readonly int _y;

        public ChestCategory(int index, string name, int itemId, string[] categoryNames, int x, int y) : base(new Rectangle(0, 0, 0, 0), new Object(itemId, 1))
        {
            _index = index;
            _name = name;
            _categoryNames = categoryNames;
            _x = x;
            _y = y;
        }

        public void Draw(SpriteBatch b)
        {
            b.Draw(Game1.mouseCursors, new Vector2(_x - 72, _y + 32 + _index * OffsetY + 16), new Rectangle(16, 368, 12, 16), Color.White, 4.712389f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
            b.Draw(Game1.mouseCursors, new Vector2(_x - 72, _y + 32 + _index * OffsetY - 16), new Rectangle(21, 368, 11, 16), Color.White, 4.712389f, Vector2.Zero, 4f, SpriteEffects.None, 1f);

            if (_categoryNames == null)
            {
                b.Draw(Game1.mouseCursors, new Vector2(_x - 52, _y + 32 - 44), new Rectangle(sbyte.MaxValue, 412, 10, 11), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
            }
            else
            {
                b.Draw(Game1.objectSpriteSheet, new Vector2(_x - 64, _y + 32 + _index * OffsetY - 52), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, item.ParentSheetIndex, 16, 16), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
            }
        }

        public bool BelongsToCategory(Item i)
        {
            return _categoryNames == null || _categoryNames.Contains(i.getCategoryName());
        }

        public void DrawTooltip()
        {
        }

    }
}
