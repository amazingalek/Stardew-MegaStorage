using System;
using StardewValley.Objects;

namespace MegaStorage
{
    public interface IConvenientChestsAPI
    {
        void CopyChestData(Chest source, Chest target);
    }
}
