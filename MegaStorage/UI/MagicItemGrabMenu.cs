using System;
using System.Linq;
using MegaStorage.Models;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace MegaStorage.UI
{
    public class MagicItemGrabMenu : LargeItemGrabMenu
    {
        private int _maxRow;

        public MagicItemGrabMenu(CustomChest customChest) : base(customChest)
        {
            CurrentRow = 0;
            Refresh();
        }
        
        public override void draw(SpriteBatch b)
        {
            Draw(b);
            if (CurrentRow < _maxRow)
            {
                DownButton.draw(b);
            }
            if (CurrentRow > 0)
            {
                UpButton.draw(b);
            }
            drawMouse(b);
        }

        public override void Refresh()
        {
            ItemsToGrabMenu.actualInventory = CustomChest.items.Skip(ItemsPerRow * CurrentRow).ToList();
            _maxRow = (CustomChest.items.Count - 1) / 12 + 1 - Rows;
            if (CurrentRow > _maxRow)
                CurrentRow = _maxRow;
        }

        public override void receiveScrollWheelAction(int direction)
        {
            if (direction < 0 && CurrentRow < _maxRow)
            {
                CurrentRow++;
            }
            else if (direction > 0 && CurrentRow > 0)
            {
                CurrentRow--;
            }
            Refresh();
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, true);
            if (UpButton.containsPoint(x, y) && CurrentRow > 0)
            {
                Game1.playSound("coin");
                CurrentRow--;
                UpButton.scale = UpButton.baseScale;
            }
            if (DownButton.containsPoint(x, y) && CurrentRow < _maxRow)
            {
                Game1.playSound("coin");
                CurrentRow++;
                DownButton.scale = DownButton.baseScale;
            }
            Refresh();
        }

        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);
            UpButton.scale = UpButton.containsPoint(x, y) ? Math.Min(UpButton.scale + 0.02f, UpButton.baseScale + 0.1f) : Math.Max(UpButton.scale - 0.02f, UpButton.baseScale);
            DownButton.scale = DownButton.containsPoint(x, y) ? Math.Min(DownButton.scale + 0.02f, DownButton.baseScale + 0.1f) : Math.Max(DownButton.scale - 0.02f, DownButton.baseScale);
        }

    }
}