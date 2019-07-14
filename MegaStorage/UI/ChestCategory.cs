using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace MegaStorage.UI
{
    public class ChestCategory : ClickableComponent
    {
        private const int OffsetY = 76;

        private readonly int _index;
        private readonly string _name;
        private readonly Vector2 _spritePos;
        private readonly string[] _categoryNames;

        public ChestCategory(int index, string name, Vector2 spritePos, string[] categoryNames, int x, int y) : base(new Rectangle(x - 72, y + 36 + index * OffsetY - 64, 64, 64), name)
        {
            _index = index;
            _name = name;
            _spritePos = spritePos;
            _categoryNames = categoryNames;
        }

        public void Draw(SpriteBatch b, int x, int y)
        {
            b.Draw(Game1.mouseCursors, new Vector2(x - 72, y + 32 + _index * OffsetY + 16), new Rectangle(16, 368, 12, 16), Color.White, 4.712389f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
            b.Draw(Game1.mouseCursors, new Vector2(x - 72, y + 32 + _index * OffsetY - 16), new Rectangle(21, 368, 11, 16), Color.White, 4.712389f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
            b.Draw(Game1.mouseCursors, new Vector2(x - 52, y + 40 + _index * OffsetY - 52), new Rectangle((int)_spritePos.X, (int)_spritePos.Y,10, 11), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
        }

        public void DrawTooltip(SpriteBatch b)
        {
            IClickableMenu.drawHoverText(b, _name, Game1.smallFont);
        }

        public List<Item> Filter(IList<Item> items)
        {
            return items.Where(BelongsToCategory).ToList();
        }

        private bool BelongsToCategory(Item i)
        {
            return _categoryNames == null || _categoryNames.Contains(i.getCategoryName());
        }
    }
}
