using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using System.Collections.Generic;
using System.Linq;

namespace MegaStorage.Framework.Interface
{
    public class ChestCategory : ClickableTextureComponent
    {
        private const int XOffset = 8;
        private readonly IList<int> _categoryIds;
        private readonly int _x;
        public ChestCategory(int index, string name, Vector2 spritePos, int x, int y, IList<int> categoryIds)
            : this(index, name, spritePos, Game1.mouseCursors, x, y, categoryIds) { }
        public ChestCategory(int index, string name, Vector2 spritePos, Texture2D sprite, int x, int y, IList<int> categoryIds)
            : base(
                name,
                new Rectangle(x, y, Game1.tileSize, Game1.tileSize),
                "",
                "",
                sprite,
                new Rectangle((int)spritePos.X, (int)spritePos.Y, 16, 16),
                Game1.pixelZoom)
        {
            _x = x;
            _categoryIds = categoryIds;
        }
        public void Draw(SpriteBatch b, bool selected)
        {
            bounds.X = _x + (selected ? XOffset : 0);
            base.draw(b);
        }
        public List<Item> Filter(IList<Item> items) => items.Where(BelongsToCategory).ToList();
        protected virtual bool BelongsToCategory(Item i) => !(i is null) && _categoryIds.Contains(i.Category);
    }
}