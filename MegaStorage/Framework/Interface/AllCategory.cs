using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System.IO;

namespace MegaStorage.Framework.Interface
{
    public class AllCategory : ChestCategory
    {
        public AllCategory(string name, Vector2 spritePos, int x, int y)
            : base(
                name,
                spritePos,
                MegaStorageMod.Instance.Helper.Content.Load<Texture2D>(Path.Combine("assets", "AllTab.png")),
                x, y, null) { }
        protected override bool BelongsToCategory(Item i) => true;
    }
}
