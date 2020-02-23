using MegaStorage.Framework.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;

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
        public List<Item> AllItems;
        public List<Item> VisibleItems;
        public int MaxRows;
        private readonly NetObjectList<Item> _sourceItems;
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
            CustomChest customChest = null)
            : base(xPosition, yPosition, false, customChest?.items ?? Game1.player.Items, null, capacity, rows)
        {
            _sourceItems = !(customChest is null) ? customChest.items : Game1.player.items;
            _sourceItems.OnElementChanged += OnElementChanged;
            AllItems = _sourceItems.ToList();
            RefreshItems();
        }

        public override void draw(SpriteBatch b)
        {
            // Background
            Game1.drawDialogueBox(
                xPositionOnScreen - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder,
                yPositionOnScreen - 112,
                width + (IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder) * 2,
                height + 152,
                false,
                true);

            // Draw Grid
            for (var slot = 0; slot < capacity; ++slot)
            {
                var col = slot % ItemsPerRow;
                var row = slot / ItemsPerRow;
                var pos = new Vector2(
                    xPositionOnScreen + col * (Game1.tileSize + horizontalGap),
                    yPositionOnScreen + row * (Game1.tileSize + verticalGap));

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
                    xPositionOnScreen + col * (Game1.tileSize + horizontalGap),
                    yPositionOnScreen + row * (Game1.tileSize + verticalGap));

                var currentItem = VisibleItems.ElementAt(slot);
                currentItem?.drawInMenu(
                    b,
                    pos,
                    1f,
                    1f,
                    0.865f,
                    StackDrawType.Draw,
                    Color.White,
                    false);
            }
        }

        public override void receiveScrollWheelAction(int direction)
        {
            MegaStorageMod.ModMonitor.VerboseLog("receiveScrollWheelAction");
            if (direction < 0 && _currentRow < MaxRows)
            {
                _currentRow++;
                RefreshItems();
            }
            else if (direction > 0 && _currentRow > 0)
            {
                _currentRow--;
                RefreshItems();
            }
        }

        /*********
        ** Private methods
        *********/
        private void OnElementChanged(NetList<Item, NetRef<Item>> list, int index, Item oldValue, Item newValue)
        {
            AllItems = _sourceItems.ToList();
            RefreshItems();
        }

        private void RefreshItems()
        {
            VisibleItems = (_selectedCategory?.Filter(AllItems) ?? AllItems).Skip(ItemsPerRow * _currentRow).ToList();
            MaxRows = (VisibleItems.Count - 1) / ItemsPerRow + 1 - rows;
        }
    }
}
