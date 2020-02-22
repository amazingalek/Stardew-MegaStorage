using StardewValley;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace furyx639.Common
{
    internal static class CommonHelper
    {
        /*********
        ** Public methods
        *********/
        public static IEnumerable<GameLocation> GetLocations()
        {
            return Game1.locations
                .Concat(
                    from location in Game1.locations.OfType<BuildableGameLocation>()
                    from building in location.buildings
                    where building.indoors.Value != null
                    select building.indoors.Value
                );
        }

        public static T NonNull<T>(T obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            return obj;
        }
    }
}
