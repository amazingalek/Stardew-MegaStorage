using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System.IO;

namespace MegaStorage.Framework.Interface
{
    public class AllCategory : ChestCategory
    {
        public AllCategory(int index, int x, int y)
            : base(index, "All", Vector2.Zero, null, x, y)
        {
            Sprite = MegaStorageMod.Instance.Helper.Content.Load<Texture2D>(Path.Combine("assets", "AllTab.png"));
        }

        protected override bool BelongsToCategory(Item i) => true;
    }
}
