using furyx639.Common;
using MegaStorage.Framework.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using SObject = StardewValley.Object;

namespace MegaStorage.Framework.Interface
{
    public class MegaStorageMenu : ItemGrabMenu
    {
        public static readonly Dictionary<string, Vector2> Categories = new Dictionary<string, Vector2>()
        {
            {"Crops", new Vector2(640, 80)},
            {"Seeds", new Vector2(656, 64)},
            {"Materials", new Vector2(672, 64)},
            {"Cooking", new Vector2(688, 64)},
            {"Fishing", new Vector2(640, 64)},
            {"Misc", new Vector2(672, 80)}
        };

        private const int TopHeightChange = -24;
        private const int TopBackgroundChange = 24;
        private const int MoveTop = -24;
        private const int MoveBottom = 116;
        protected const int Rows = 6;
        protected const int ItemsPerRow = 12;
        protected const int Capacity = ItemsPerRow * Rows;

        /*********
        ** Fields
        *********/
        private protected List<ClickableComponent> CategoryComponents;
        private protected ClickableTextureComponent UpArrow;
        private protected ClickableTextureComponent DownArrow;

        private readonly List<ChestCategory> _chestCategories = new List<ChestCategory>();
        private ChestCategory _hoverCategory;
        private protected ChestCategory SelectedCategory;

        private int _currentRow;
        private int _maxRow;

        private Item SourceItem => _sourceItemReflected.GetValue();
        private readonly IReflectedField<Item> _sourceItemReflected;
        private TemporaryAnimatedSprite Poof { set => _poofReflected.SetValue(value); }
        private readonly IReflectedField<TemporaryAnimatedSprite> _poofReflected;
        private behaviorOnItemSelect BehaviorFunction => _behaviorFunctionReflected.GetValue();
        private readonly IReflectedField<behaviorOnItemSelect> _behaviorFunctionReflected;
        private protected readonly CustomChest CustomChest;

        /*********
        ** Public methods
        *********/
        public MegaStorageMenu(CustomChest customChest)
            : base(
                inventory: CommonHelper.NonNull(customChest).items,
                reverseGrab: false,
                showReceivingMenu: true,
                highlightFunction: InventoryMenu.highlightAllItems,
                behaviorOnItemSelectFunction: CommonHelper.NonNull(customChest).grabItemFromInventory,
                message: null,
                behaviorOnItemGrab: CommonHelper.NonNull(customChest).grabItemFromChest,
                canBeExitedWithKey: true,
                showOrganizeButton: false,
                source: ItemGrabMenu.source_chest,
                context: customChest)
        {
            CustomChest = customChest;
            _sourceItemReflected = MegaStorageMod.Instance.Helper.Reflection.GetField<Item>(this, "sourceItem");
            _poofReflected = MegaStorageMod.Instance.Helper.Reflection.GetField<TemporaryAnimatedSprite>(this, "poof");
            _behaviorFunctionReflected = MegaStorageMod.Instance.Helper.Reflection.GetField<behaviorOnItemSelect>(this, "behaviorFunction");

            _currentRow = 0;

            ItemsToGrabMenu = new InventoryMenu(xPositionOnScreen + 32, yPositionOnScreen, false, CustomChest.items, null, Capacity, Rows);
            ItemsToGrabMenu.movePosition(0, MoveTop);
            inventory.movePosition(0, MoveBottom);

            SetupArrows();
            SetupCategories();
            SetupColorPicker();
            SetupStackButton();
            SetupOrganizeButton();
            SetupControllerSupport();
            Refresh();
        }

        private void SetupArrows()
        {
            UpArrow = new ClickableTextureComponent(
                new Rectangle(xPositionOnScreen + 840, yPositionOnScreen - 8, 64, 64), Game1.mouseCursors,
                Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 12), 1f)
            {
                myID = 88,
                downNeighborID = 89
            };
            DownArrow = new ClickableTextureComponent(
                new Rectangle(xPositionOnScreen + 840, yPositionOnScreen + 288, 64, 64),
                Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 11), 1f)
            {
                myID = 89,
                upNeighborID = 88
            };
        }
        private void SetupCategories()
        {
            if (!ModConfig.Instance.EnableCategories)
            {
                SelectedCategory = new AllCategory(0, xPositionOnScreen, yPositionOnScreen);
                return;
            }

            var index = 0;
            _chestCategories.Add(new AllCategory(index++, xPositionOnScreen, yPositionOnScreen));
            foreach (var category in Categories)
            {
                if (!ModConfig.Instance.Categories.TryGetValue(category.Key, out var categoryIds))
                {
                    continue;
                }
                _chestCategories.Add(new ChestCategory(
                    index++,
                    category.Key,
                    category.Value,
                    categoryIds,
                    xPositionOnScreen,
                    yPositionOnScreen));
            }
            SelectedCategory = _chestCategories.First();
        }
        private void SetupStackButton()
        {
            fillStacksButton = new ClickableTextureComponent(
                "Fill Stacks",
                new Rectangle(xPositionOnScreen + width, yPositionOnScreen + height / 3 - 86, 64, 64),
                "",
                Game1.content.LoadString("Strings\\UI:ItemGrab_FillStacks"),
                Game1.mouseCursors,
                new Rectangle(103, 469, 16, 16),
                4f)
            {
                myID = 12952,
                upNeighborID = colorPickerToggleButton != null ? 27346 : (!(specialButton is null) ? 12485 : -500),
                downNeighborID = 106,
                leftNeighborID = 53921,
                region = 15923
            };
        }
        private void SetupOrganizeButton()
        {
            organizeButton = new ClickableTextureComponent(
                "Organize",
                new Rectangle(xPositionOnScreen + width, yPositionOnScreen + height / 3 - 6, 64, 64),
                "",
                Game1.content.LoadString("Strings\\UI:ItemGrab_Organize"),
                Game1.mouseCursors,
                new Rectangle(162, 440, 16, 16),
                4f)
            {
                myID = 106,
                upNeighborID = 12952,
                downNeighborID = 5948,
                leftNeighborID = 53921,
                region = 15923
            };
        }
        private void SetupColorPicker()
        {
            chestColorPicker = new DiscreteColorPicker(
                xPositionOnScreen,
                yPositionOnScreen - 64 - IClickableMenu.borderWidth * 2,
                0,
                new Chest(true));

            chestColorPicker.colorSelection = chestColorPicker.getSelectionFromColor(CustomChest.playerChoiceColor.Value);
            CustomChest.playerChoiceColor.Value = chestColorPicker.getColorFromSelection(chestColorPicker.colorSelection);

            colorPickerToggleButton = new ClickableTextureComponent(
                new Rectangle(xPositionOnScreen + width, yPositionOnScreen + height / 3 - 166, 64, 64),
                Game1.mouseCursors,
                new Rectangle(119, 469, 16, 16),
                4f)
            {
                hoverText = Game1.content.LoadString("Strings\\UI:Toggle_ColorPicker"),
                myID = 27346,
                downNeighborID = -99998,
                leftNeighborID = 53921,
                region = 15923
            };
        }
        private void SetupControllerSupport()
        {
            if (ItemsToGrabMenu is null || inventory?.inventory is null)
            {
                return;
            }

            if (Game1.options.SnappyMenus)
            {
                ItemsToGrabMenu.populateClickableComponentList();
                foreach (var cc in ItemsToGrabMenu.inventory.Where(cc => !(cc is null)))
                {
                    cc.myID += 53910;
                    cc.upNeighborID += 53910;
                    cc.rightNeighborID += 53910;
                    cc.downNeighborID = -7777;
                    cc.leftNeighborID += 53910;
                    cc.fullyImmutable = true;
                }
            }

            for (var index = 0; index < 12; ++index)
            {
                if (inventory.inventory.Count >= 12)
                {
                    inventory.inventory[index].upNeighborID = discreteColorPickerCC is null || ItemsToGrabMenu.inventory.Count > index
                        ? ItemsToGrabMenu.inventory.Count > index ? 53910 + index : 53910
                        : 4343;
                }

                if (!(discreteColorPickerCC is null) && ItemsToGrabMenu.inventory.Count > index)
                {
                    ItemsToGrabMenu.inventory[index].upNeighborID = 4343;
                }
            }

            for (var index = 0; index < 36; ++index)
            {
                if (inventory.inventory.Count <= index)
                {
                    continue;
                }

                inventory.inventory[index].upNeighborID = -7777;
                inventory.inventory[index].upNeighborImmutable = true;
            }

            if (!(trashCan is null) && inventory.inventory.Count >= 12 && !(inventory.inventory[11] is null))
            {
                inventory.inventory[11].rightNeighborID = 5948;
            }

            if (!(trashCan is null))
            {
                trashCan.leftNeighborID = 11;
            }

            if (!(okButton is null))
            {
                okButton.leftNeighborID = 11;
            }

            for (var i = 0; i < 12; i++)
            {
                var item = inventory.inventory[i];
                if (!(item is null))
                {
                    item.upNeighborID = 53910 + 60 + i;
                }
            }

            var rightItems =
                Enumerable.Range(0, 6)
                    .Select(i => ItemsToGrabMenu.inventory.ElementAt(i * 12 + 11))
                    .ToList();

            for (var i = 0; i < rightItems.Count; ++i)
            {
                rightItems[i].rightNeighborID = i switch
                {
                    0 => UpArrow.myID,
                    1 => UpArrow.myID,
                    2 => colorPickerToggleButton.myID,
                    3 => organizeButton.myID,
                    4 => DownArrow.myID,
                    5 => DownArrow.myID,
                    6 => DownArrow.myID,
                    _ => organizeButton.myID
                };
            }

            if (!(colorPickerToggleButton is null))
            {
                colorPickerToggleButton.leftNeighborID = rightItems[2].myID;
                colorPickerToggleButton.upNeighborID = UpArrow.myID;
                UpArrow.rightNeighborID = colorPickerToggleButton.myID;
            }

            UpArrow.leftNeighborID = rightItems[0].myID;
            DownArrow.rightNeighborID = organizeButton.myID;
            DownArrow.leftNeighborID = rightItems[4].myID;
            DownArrow.downNeighborID = rightItems[5].myID;
            organizeButton.leftNeighborID = rightItems[3].myID;
            organizeButton.downNeighborID = DownArrow.myID;

            //fillStacksButton.upNeighborID = colorPickerToggleButton.myID;
            //fillStacksButton.downNeighborID = organizeButton.myID;

            if (ModConfig.Instance.EnableCategories)
            {
                var leftItems =
                    Enumerable.Range(0, 6)
                        .Select(i => ItemsToGrabMenu.inventory.ElementAt(i * 12))
                        .ToList();

                CategoryComponents =
                    Enumerable.Range(0, _chestCategories.Count)
                        .Select(i => (ClickableComponent)_chestCategories[i])
                        .ToList();

                for (var i = 0; i < CategoryComponents.Count; ++i)
                {
                    if (i > 0)
                    {
                        leftItems[i - 1].leftNeighborID = CategoryComponents[i < 4 ? i - 1 : i].myID;
                        CategoryComponents[i - 1].downNeighborID = CategoryComponents[i].myID;
                        CategoryComponents[i].upNeighborID = CategoryComponents[i - 1].myID;
                    }
                    CategoryComponents[i].myID = i + 239865;
                    CategoryComponents[i].rightNeighborID = leftItems[i < 3 ? i : i - 1].myID;
                }
            }

            populateClickableComponentList();
            snapToDefaultClickableComponent();
        }
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            var itemGrabMenu = Game1.activeClickableMenu is ItemGrabMenu
                ? (ItemGrabMenu)Game1.activeClickableMenu
                : null;

            // Held Item from Inventory
            heldItem = inventory.leftClick(x, y, heldItem, playSound);

            if (heldItem is null && showReceivingMenu)
            {
                var itemsBefore = ItemsToGrabMenu.actualInventory.ToList();
                heldItem = ItemsToGrabMenu.leftClick(x, y, heldItem, false);
                var itemsAfter = ItemsToGrabMenu.actualInventory.ToList();
                FixNulls(itemsBefore, itemsAfter);
                if (!(heldItem is null) && !(behaviorOnItemGrab is null))
                {
                    behaviorOnItemGrab(heldItem, Game1.player);
                    if (!(itemGrabMenu is null))
                    {
                        _sourceItemReflected.SetValue(SourceItem);
                        if (Game1.options.SnappyMenus)
                        {
                            itemGrabMenu.currentlySnappedComponent = currentlySnappedComponent;
                            itemGrabMenu.snapCursorToCurrentSnappedComponent();
                        }
                    }
                }

                if (heldItem is SObject obj)
                {
                    switch (obj.ParentSheetIndex)
                    {
                        case 326:
                            heldItem = null;
                            Game1.player.canUnderstandDwarves = true;
                            Poof = CreatePoof(x, y);
                            Game1.playSound("fireball");
                            break;
                        case 102:
                            heldItem = null;
                            Game1.player.foundArtifact(102, 1);
                            Poof = CreatePoof(x, y);
                            Game1.playSound("fireball");
                            break;
                        default:
                            if (Utility.IsNormalObjectAtParentSheetIndex(heldItem, 434))
                            {
                                heldItem = null;
                                exitThisMenu(false);
                                Game1.player.eatObject(obj, true);
                            }
                            else if (obj.IsRecipe)
                            {
                                var key = heldItem.Name.Substring(0,
                                    heldItem.Name.IndexOf("Recipe", StringComparison.InvariantCultureIgnoreCase) -
                                    1);
                                try
                                {
                                    if (obj.Category == -7)
                                    {
                                        Game1.player.cookingRecipes.Add(key, 0);
                                    }
                                    else
                                    {
                                        Game1.player.craftingRecipes.Add(key, 0);
                                    }

                                    Poof = CreatePoof(x, y);
                                    Game1.playSound("newRecipe");
                                }
                                catch (Exception)
                                {
                                    // ignored
                                }

                                heldItem = null;
                            }
                            break;
                    }
                }

                if (!(heldItem is null) && Game1.player.addItemToInventoryBool(heldItem))
                {
                    heldItem = null;
                    Game1.playSound("coin");
                }
            }
            else if ((reverseGrab || !(BehaviorFunction is null)) && isWithinBounds(x, y))
            {
                BehaviorFunction(heldItem, Game1.player);
                Refresh();
                if (!(itemGrabMenu is null))
                {
                    _sourceItemReflected.SetValue(SourceItem);
                    if (Game1.options.SnappyMenus)
                    {
                        itemGrabMenu.currentlySnappedComponent = currentlySnappedComponent;
                        itemGrabMenu.snapCursorToCurrentSnappedComponent();
                    }
                }
            }

            // Chest Color Picker
            if (!(chestColorPicker is null))
            {
                chestColorPicker.receiveLeftClick(x, y);
                CustomChest.playerChoiceColor.Value =
                    chestColorPicker.getColorFromSelection(chestColorPicker.colorSelection);
            }

            // Category (tab)
            if (!(_hoverCategory is null))
            {
                SelectedCategory = _hoverCategory;
                _currentRow = 0;
                Refresh();
            }

            // Up Arrow
            else if (_currentRow > 0 && UpArrow.containsPoint(x, y))
            {
                Game1.playSound("coin");
                _currentRow--;
                UpArrow.scale = UpArrow.baseScale;
                Refresh();
            }

            // Down Arrow
            else if (_currentRow < _maxRow && DownArrow.containsPoint(x, y))
            {
                Game1.playSound("coin");
                _currentRow++;
                DownArrow.scale = DownArrow.baseScale;
                Refresh();
            }

            // Organize
            else if (!(organizeButton is null) && organizeButton.containsPoint(x, y))
            {
                organizeItemsInList(CustomChest.items);
                Game1.playSound("Ship");
                Refresh();
            }

            // Fill Stacks
            else if (!(fillStacksButton is null) && fillStacksButton.containsPoint(x, y))
            {
                FillOutStacks();
                Game1.playSound("Ship");
                Refresh();
            }

            // OK Button
            else if (!(okButton is null) && okButton.containsPoint(x, y) && readyToClose())
            {
                exitThisMenu();
                if (!(Game1.currentLocation.currentEvent is null))
                {
                    ++Game1.currentLocation.currentEvent.CurrentCommand;
                }
                Game1.playSound("bigDeSelect");
            }

            // Trash Can
            else if (!(trashCan is null) && trashCan.containsPoint(x, y) && !(heldItem is null) && heldItem.canBeTrashed())
            {
                Utility.trashItem(heldItem);
                heldItem = null;
            }

            // Special Button
            else if (whichSpecialButton != -1 && !(specialButton is null) && specialButton.containsPoint(x, y))
            {
                Game1.playSound("drumkit6");
                if (whichSpecialButton == 1 && !(context is null) && context is JunimoHut hut)
                {
                    hut.noHarvest.Value = !hut.noHarvest.Value;
                    specialButton.sourceRect.X = hut.noHarvest.Value ? 124 : 108;
                }
            }

            // Color Picker Toggle
            else if (!(chestColorPicker is null) && !(colorPickerToggleButton is null) && colorPickerToggleButton.containsPoint(x, y))
            {
                Game1.player.showChestColorPicker = !Game1.player.showChestColorPicker;
                chestColorPicker.visible = Game1.player.showChestColorPicker;
                Game1.playSound("drumkit6");
            }

            // Drop Item
            else if (!(heldItem is null) && !isWithinBounds(x, y) && heldItem.canBeTrashed())
            {
                DropHeldItem();
            }
        }
        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            if (!allowRightClick)
            {
                receiveRightClickOnlyToolAttachments(x, y);
                return;
            }

            var itemGrabMenu = Game1.activeClickableMenu is ItemGrabMenu
                ? (ItemGrabMenu)Game1.activeClickableMenu
                : null;

            heldItem = inventory.rightClick(x, y, heldItem, playSound && playRightClickSound);
            if (heldItem is null && showReceivingMenu)
            {
                var itemsBefore = ItemsToGrabMenu.actualInventory.ToList();
                heldItem = ItemsToGrabMenu.rightClick(x, y, heldItem, false);
                var itemsAfter = ItemsToGrabMenu.actualInventory.ToList();
                FixNulls(itemsBefore, itemsAfter);
                if (!(heldItem is null) && !(behaviorOnItemGrab is null))
                {
                    behaviorOnItemGrab(heldItem, Game1.player);
                    if (!(itemGrabMenu is null))
                    {
                        _sourceItemReflected.SetValue(SourceItem);
                        if (Game1.options.SnappyMenus)
                        {
                            itemGrabMenu.currentlySnappedComponent = currentlySnappedComponent;
                            itemGrabMenu.snapCursorToCurrentSnappedComponent();
                        }
                    }
                }

                if (heldItem is SObject obj)
                {
                    if (obj.ParentSheetIndex == 326)
                    {
                        heldItem = null;
                        Game1.player.canUnderstandDwarves = true;
                        Poof = CreatePoof(x, y);
                        Game1.playSound("fireball");
                    }
                    else if (Utility.IsNormalObjectAtParentSheetIndex(heldItem, 434))
                    {
                        heldItem = null;
                        exitThisMenu(false);
                        Game1.player.eatObject(obj, true);
                    }
                    else if (obj.IsRecipe)
                    {
                        var key = heldItem.Name.Substring(0,
                            heldItem.Name.IndexOf("Recipe", StringComparison.InvariantCultureIgnoreCase) - 1);
                        try
                        {
                            if (obj.Category == -7)
                            {
                                Game1.player.cookingRecipes.Add(key, 0);
                            }
                            else
                            {
                                Game1.player.craftingRecipes.Add(key, 0);
                            }

                            Poof = CreatePoof(x, y);
                            Game1.playSound("newRecipe");
                        }
                        catch (Exception)
                        {
                            // ignored
                        }

                        heldItem = null;
                    }
                }
                else if (!(heldItem is null) && Game1.player.addItemToInventoryBool(heldItem))
                {
                    heldItem = null;
                    Game1.playSound("coin");
                }
            }
            else if (reverseGrab || !(BehaviorFunction is null))
            {
                BehaviorFunction(heldItem, Game1.player);
                _sourceItemReflected.SetValue(SourceItem);
                if (destroyItemOnClick)
                {
                    heldItem = null;
                }
            }
        }
        public override void receiveScrollWheelAction(int direction)
        {
            MegaStorageMod.Instance.Monitor.VerboseLog("receiveScrollWheelAction");
            if (direction < 0 && _currentRow < _maxRow)
            {
                _currentRow++;
                Refresh();
            }
            else if (direction > 0 && _currentRow > 0)
            {
                _currentRow--;
                Refresh();
            }
        }
        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);

            if (ModConfig.Instance.EnableCategories)
            {
                _hoverCategory = _chestCategories.FirstOrDefault(c => c.containsPoint(x, y));
            }

            if (!(UpArrow is null))
            {
                UpArrow.scale = UpArrow.containsPoint(x, y)
                    ? Math.Min(UpArrow.scale + 0.02f, UpArrow.baseScale + 0.1f)
                    : Math.Max(UpArrow.scale - 0.02f, UpArrow.baseScale);
            }

            if (!(DownArrow is null))
            {
                DownArrow.scale = DownArrow.containsPoint(x, y)
                    ? Math.Min(DownArrow.scale + 0.02f, DownArrow.baseScale + 0.1f)
                    : Math.Max(DownArrow.scale - 0.02f, DownArrow.baseScale);
            }
        }
        private void FixNulls(IReadOnlyList<Item> itemsBefore, IReadOnlyList<Item> itemsAfter)
        {
            for (var i = 0; i < itemsBefore.Count; i++)
            {
                var itemBefore = itemsBefore[i];
                var itemAfter = itemsAfter[i];
                if (itemBefore == null || itemAfter != null)
                {
                    continue;
                }

                var index = CustomChest.items.IndexOf(itemBefore);
                if (index > -1)
                {
                    CustomChest.items.RemoveAt(index);
                    Refresh();
                }
            }
        }
        private static TemporaryAnimatedSprite CreatePoof(int x, int y)
        {
            return new TemporaryAnimatedSprite(
                "TileSheets/animations",
                new Rectangle(0, 320, 64, 64),
                50f,
                8,
                0,
                new Vector2(x - x % 64 + 16, y - y % 64 + 16),
                false,
                false);
        }
        public override void draw(SpriteBatch b)
        {
            Draw(b);
            DrawHover(b);
            drawMouse(b);
        }
        protected void Draw(SpriteBatch b)
        {
            if (b is null)
            {
                return;
            }

            // opaque background
            b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), Color.Black * 0.5f);

            // categories (if enabled)
            foreach (var chestCategory in _chestCategories)
            {
                var xOffset = chestCategory == SelectedCategory ? 8 : 0;
                chestCategory.Draw(b, xPositionOnScreen + xOffset, yPositionOnScreen);
            }

            // top inventory
            Game1.drawDialogueBox(
                ItemsToGrabMenu.xPositionOnScreen - borderWidth - spaceToClearSideBorder,
                ItemsToGrabMenu.yPositionOnScreen - borderWidth - spaceToClearTopBorder + TopBackgroundChange,
                ItemsToGrabMenu.width + borderWidth * 2 + spaceToClearSideBorder * 2,
                ItemsToGrabMenu.height + spaceToClearTopBorder + borderWidth * 2 + TopHeightChange,
                false, true);

            // bottom inventory
            Game1.drawDialogueBox(
                xPositionOnScreen - borderWidth / 2,
                yPositionOnScreen + borderWidth + spaceToClearTopBorder + 64 + MoveBottom,
                width,
                height - (borderWidth + spaceToClearTopBorder + 192),
                false, true);

            // bottom inventory icon
            b.Draw(
                Game1.mouseCursors,
                new Vector2(xPositionOnScreen - 64, yPositionOnScreen + height / 2 + MoveBottom + 64 + 16),
                new Rectangle(16, 368, 12, 16),
                Color.White, 4.712389f, Vector2.Zero, 4f, SpriteEffects.None, 1f);

            b.Draw(
                Game1.mouseCursors,
                new Vector2(xPositionOnScreen - 64, yPositionOnScreen + height / 2 + MoveBottom + 64 - 16),
                new Rectangle(21, 368, 11, 16),
                Color.White, 4.712389f, Vector2.Zero, 4f, SpriteEffects.None, 1f);

            b.Draw(
                Game1.mouseCursors,
                new Vector2(xPositionOnScreen - 40, yPositionOnScreen + height / 2 + MoveBottom + 64 - 44),
                new Rectangle(4, 372, 8, 11),
                Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);

            // arrows (if scrolling)
            if (_currentRow < _maxRow)
            {
                DownArrow.draw(b);
            }

            if (_currentRow > 0)
            {
                UpArrow.draw(b);
            }

            // buttons
            okButton.draw(b);
            inventory.draw(b);
            ItemsToGrabMenu.draw(b);
            chestColorPicker.draw(b);
            fillStacksButton.draw(b);
            organizeButton.draw(b);

            if (!(colorPickerToggleButton is null))
            {
                colorPickerToggleButton.draw(b);
            }
            else
            {
                specialButton?.draw(b);
            }

            Game1.mouseCursorTransparency = 1f;
        }

        public override bool isWithinBounds(int x, int y)
        {
            return x >= ItemsToGrabMenu.xPositionOnScreen
                   && x <= ItemsToGrabMenu.xPositionOnScreen + ItemsToGrabMenu.width
                   && y >= ItemsToGrabMenu.yPositionOnScreen
                   && y <= inventory.yPositionOnScreen + inventory.height;
        }
        protected void DrawHover(SpriteBatch b)
        {
            _hoverCategory?.DrawTooltip(b);

            if (!(hoverText is null) && hoveredItem is null)
            {
                drawHoverText(b, hoverText, Game1.smallFont);
            }

            if (!(hoveredItem is null))
            {
                drawToolTip(b, hoveredItem.getDescription(), hoveredItem.DisplayName, hoveredItem, !(heldItem is null));
            }
            else
            {
                drawToolTip(b, ItemsToGrabMenu.descriptionText, ItemsToGrabMenu.descriptionTitle, hoveredItem, !(heldItem is null));
            }

            heldItem?.drawInMenu(b, new Vector2(Game1.getOldMouseX() + 8, Game1.getOldMouseY() + 8), 1f);
        }
        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            // TBD
        }
        protected internal void Refresh()
        {
            var filteredItems = SelectedCategory.Filter(CustomChest.items);
            ItemsToGrabMenu.actualInventory = filteredItems.Skip(ItemsPerRow * _currentRow).ToList();
            _maxRow = (filteredItems.Count - 1) / 12 + 1 - Rows;
            if (_currentRow > _maxRow)
            {
                _currentRow = _maxRow;
            }
        }
    }
}