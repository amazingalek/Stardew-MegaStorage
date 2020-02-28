using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using Object = StardewValley.Object;

namespace MegaStorage.Framework.Interface.Widgets
{
    public class MiscCategory : ChestCategory
    {
        public MiscCategory(string name, Vector2 spritePos, int x, int y, IList<int> categoryIds)
            : base(name, spritePos, x, y, categoryIds) { }

        protected override bool BelongsToCategory(Item i)
        {
            if (i is null || string.IsNullOrWhiteSpace(i.getCategoryName()))
            {
                return true;
            }

            if (i is Object obj && !(obj.Type is null) && obj.Type.Equals("Arch", StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            return i switch
            {
                Tool _ => true,
                Boots _ => true,
                Ring _ => true,
                Furniture _ => true,
                _ => base.BelongsToCategory(i)
            };
        }
    }
}
