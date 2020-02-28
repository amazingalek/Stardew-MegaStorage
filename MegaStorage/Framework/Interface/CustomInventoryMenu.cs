using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using furyx639.Common;
using StardewValley.Objects;

namespace MegaStorage.Framework.Interface
{
    internal class CustomInventoryMenu : InventoryMenu
    {
        /*********
        ** Fields
        *********/
        public ChestCategory SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                RefreshItems();
            }
        }

        public int MaxRows;
        public IList<Item> VisibleItems;

        // Padding for Items Grid
        private const int XPadding = 24;
        private const int YPadding = 12;

        private protected ClickableTextureComponent UpArrow;
        private protected ClickableTextureComponent DownArrow;
        private ChestCategory _selectedCategory;
        private int _currentRow;
        private int ItemsPerRow => capacity / rows;

        /*********
        ** Public methods
        *********/
        public CustomInventoryMenu(
            int xPosition,
            int yPosition,
            int capacity = -1,
            int rows = 3,
            Chest chest = null)
            : base(xPosition, yPosition, false, chest?.items ?? Game1.player.Items, InventoryMenu.highlightAllItems, capacity, rows)
        {
            width = (Game1.tileSize + horizontalGap) * ItemsPerRow + XPadding * 2;
            height = (Game1.tileSize + verticalGap) * rows + YPadding * 2;

            // Up Arrow
            UpArrow = new ClickableTextureComponent(
                "upArrow",
                new Rectangle(
                    xPositionOnScreen + width - Game1.tileSize / 2 + 8,
                    yPositionOnScreen,
                    Game1.tileSize, Game1.tileSize),
                "",
                "",
                Game1.mouseCursors,
                Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 12),
                1f)
            {
                myID = 88,
                downNeighborID = 89,
                visible = _currentRow > 0
            };

            // Down Arrow
            DownArrow = new ClickableTextureComponent(
                "downArrow",
                new Rectangle(xPositionOnScreen + width - Game1.tileSize / 2 + 8,
                    yPositionOnScreen + height - Game1.tileSize,
                    Game1.tileSize, Game1.tileSize),
                "",
                "",
                Game1.mouseCursors,
                Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 11),
                1f)
            {
                myID = 89,
                upNeighborID = 88,
                visible = _currentRow <= MaxRows - rows
            };

            RefreshItems();
        }

        public override void draw(SpriteBatch b)
        {
            // Draw Dialogue Box
            CommonHelper.DrawDialogueBox(b, xPositionOnScreen, yPositionOnScreen, width, height);

            // Draw Grid
            for (var slot = 0; slot < capacity; ++slot)
            {
                var col = slot % ItemsPerRow;
                var row = slot / ItemsPerRow;
                var pos = new Vector2(
                    xPositionOnScreen + col * (Game1.tileSize + horizontalGap) + XPadding,
                    yPositionOnScreen + row * (Game1.tileSize + verticalGap) + YPadding);

                b.Draw(
                    Game1.menuTexture,
                    pos,
                    Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10),
                    Color.White,
                    0.0f,
                    Vector2.Zero,
                    1f,
                    SpriteEffects.None,
                    0.5f);

                if (showGrayedOutSlots && slot >= Game1.player.MaxItems)
                {
                    b.Draw(
                        Game1.menuTexture,
                        pos,
                        Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 57),
                        Color.White * 0.5f,
                        0.0f,
                        Vector2.Zero,
                        1f,
                        SpriteEffects.None,
                        0.5f);
                }
            }

            // Draw Items
            for (var slot = 0; slot < Math.Min(capacity, VisibleItems.Count); ++slot)
            {
                var col = slot % ItemsPerRow;
                var row = slot / ItemsPerRow;
                var pos = new Vector2(
                    xPositionOnScreen + col * (Game1.tileSize + horizontalGap) + XPadding,
                    yPositionOnScreen + row * (Game1.tileSize + verticalGap) + YPadding);

                var currentItem = VisibleItems.ElementAt(slot);
                if (currentItem is null || currentItem.Stack == 0)
                    continue;
                currentItem.drawInMenu(
                    b,
                    pos,
                    1f,
                    1f,
                    0.865f,
                    StackDrawType.Draw,
                    Color.White,
                    false);
            }

            UpArrow.draw(b);
            DownArrow.draw(b);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (UpArrow.containsPoint(x, y))
                ScrollUp();
            if (DownArrow.containsPoint(x, y))
                ScrollDown();
        }

        public override void receiveScrollWheelAction(int direction)
        {
            MegaStorageMod.ModMonitor.VerboseLog("receiveScrollWheelAction");
            if (direction < 0)
            {
                ScrollDown();
            }
            else if (direction > 0)
            {
                ScrollUp();
            }
        }

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            UpArrow.bounds.X = xPositionOnScreen + width - Game1.tileSize / 2 + 8;
            UpArrow.bounds.Y = yPositionOnScreen;
            DownArrow.bounds.X = xPositionOnScreen + width - Game1.tileSize / 2 + 8;
            DownArrow.bounds.Y = yPositionOnScreen + height - 80;
        }

        public void ScrollDown()
        {
            if (_currentRow > MaxRows - rows)
                return;
            _currentRow++;
            RefreshItems();
        }

        public void ScrollUp()
        {
            if (_currentRow <= 0)
                return;
            _currentRow--;
            RefreshItems();
        }

        /*********
        ** Private methods
        *********/
        public void RefreshItems()
        {
            VisibleItems = (_selectedCategory?.Filter(actualInventory) ?? actualInventory)
                .Skip(ItemsPerRow * _currentRow)
                .ToList();
            MaxRows = VisibleItems.Count / ItemsPerRow + 1;
            UpArrow.visible = _currentRow > 0;
            DownArrow.visible = _currentRow < MaxRows - rows;
            inventory.Clear();
            for (var slot = 0; slot < capacity; ++slot)
            {
                var col = slot % ItemsPerRow;
                var row = slot / ItemsPerRow;
                var index = actualInventory.Count;
                if (slot < VisibleItems.Count)
                {
                    for (index = 0; index < actualInventory.Count; ++index)
                    {
                        if (actualInventory[index] == VisibleItems[slot])
                            break;
                    }
                }
                inventory.Add(new ClickableComponent(
                    new Rectangle(
                        xPositionOnScreen + col * (Game1.tileSize + horizontalGap) + XPadding,
                        yPositionOnScreen + row * (Game1.tileSize + verticalGap) + YPadding,
                        Game1.tileSize,
                        Game1.tileSize),
                    index.ToString(CultureInfo.InvariantCulture))
                {
                    myID = slot,
                    leftNeighborID = col != 0 ? slot - 1 : 107,
                    rightNeighborID = (slot + 1) % ItemsPerRow != 0 ? slot + 1 : 106,
                    downNeighborID = slot >= this.actualInventory.Count - this.capacity / rows ? 102 : slot + ItemsPerRow,
                    upNeighborID = slot < capacity / rows ? 12340 + slot : slot - capacity / rows,
                    region = 9000,
                    upNeighborImmutable = true,
                    downNeighborImmutable = true,
                    leftNeighborImmutable = true,
                    rightNeighborImmutable = true
                });
            }
        }
    }
}
