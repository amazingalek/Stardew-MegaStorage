using furyx639.Common;
using MegaStorage.Framework.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using SObject = StardewValley.Object;

namespace MegaStorage.Framework.Interface
{
    public class CustomItemGrabMenu : ItemGrabMenu
    {
        /*********
        ** Fields
        *********/
        public static readonly Dictionary<string, Vector2> Categories = new Dictionary<string, Vector2>()
        {
            {"All", Vector2.Zero },
            {"Crops", new Vector2(640, 80)},
            {"Seeds", new Vector2(656, 64)},
            {"Materials", new Vector2(672, 64)},
            {"Cooking", new Vector2(688, 64)},
            {"Fishing", new Vector2(640, 64)},
            {"Misc", new Vector2(672, 80)}
        };

        public const int Rows = 6;
        public const int ItemsPerRow = 12;
        public const int Capacity = ItemsPerRow * Rows;
        public const int MenuWidth = 768;
        public const int MenuHeight = 680;

        // Offsets to ItemsToGrabMenu and Inventory
        private const int XOffset = -48;
        private const int YOffset = -36;

        // Offsets to Color Picker
        private const int TopXOffset = 0;
        private const int TopYOffset = -104;

        // Offsets to Categories
        private const int LeftXOffset = -80;
        private const int LeftYOffset = -8;

        // Offsets to Color Toggle, Organize, Stack, OK, and Trash
        private const int RightXOffset = 56;
        private const int RightYOffset = -32;

        private static int TileSize => Game1.tileSize;
        private readonly CustomChest _customChest;
        private CustomInventoryMenu _itemsToGrabMenu;
        private CustomInventoryMenu _inventory;

        private TemporaryAnimatedSprite Poof { set => _poofReflected.SetValue(value); }
        private readonly IReflectedField<TemporaryAnimatedSprite> _poofReflected;
        private behaviorOnItemSelect BehaviorFunction => _behaviorFunction.GetValue();
        private readonly IReflectedField<behaviorOnItemSelect> _behaviorFunction;

        /*********
        ** Public methods
        *********/
        public CustomItemGrabMenu(CustomChest customChest)
            : base(CommonHelper.NonNull(customChest).items, customChest)
        {
            initialize(
                (Game1.viewport.Width - MenuWidth) / 2,
                (Game1.viewport.Height - MenuHeight) / 2,
                MenuWidth,
                MenuHeight);
            if (yPositionOnScreen < IClickableMenu.spaceToClearTopBorder)
            {
                yPositionOnScreen = IClickableMenu.spaceToClearTopBorder;
            }
            if (xPositionOnScreen < 0)
            {
                xPositionOnScreen = 0;
            }

            _customChest = customChest;
            _poofReflected = MegaStorageMod.Instance.Helper.Reflection.GetField<TemporaryAnimatedSprite>(this, "poof");
            _behaviorFunction = MegaStorageMod.Instance.Helper.Reflection.GetField<behaviorOnItemSelect>(this, "behaviorFunction");
            _behaviorFunction.SetValue(customChest.grabItemFromInventory);
            behaviorOnItemGrab = customChest.grabItemFromChest;
            playRightClickSound = true;
            allowRightClick = true;

            allClickableComponents = new List<ClickableComponent>();

            SetupItemsMenu();
            SetupInventoryMenu();
            SetControllerSupport();
        }
        private void SetControllerSupport()
        {
            if (Game1.options.SnappyMenus)
            {
                foreach (var cc in _itemsToGrabMenu.inventory.Where(cc => !(cc is null)))
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
                if (_inventory.inventory.Count >= 12)
                {
                    _inventory.inventory[index].upNeighborID = discreteColorPickerCC is null || _itemsToGrabMenu.inventory.Count > index
                        ? _itemsToGrabMenu.inventory.Count > index ? 53910 + index : 53910
                        : 4343;
                }

                _itemsToGrabMenu.inventory[index].upNeighborID = 4343;
            }

            for (var index = 0; index < 36; ++index)
            {
                if (_inventory.inventory.Count <= index)
                    continue;

                _inventory.inventory[index].upNeighborID = -7777;
                _inventory.inventory[index].upNeighborImmutable = true;
            }

            _inventory.inventory[11].rightNeighborID = 5948;

            trashCan.leftNeighborID = 11;

            okButton.leftNeighborID = 11;

            for (var i = 0; i < 12; i++)
            {
                var item = _inventory.inventory[i];
                if (!(item is null))
                {
                    item.upNeighborID = 53910 + 60 + i;
                }
            }

            var rightItems =
                Enumerable.Range(0, 6)
                    .Select(i => _itemsToGrabMenu.inventory.ElementAt(i * 12 + 11))
                    .ToList();

            for (var i = 0; i < rightItems.Count; ++i)
            {
                rightItems[i].rightNeighborID = i switch
                {
                    0 => colorPickerToggleButton.myID,
                    1 => colorPickerToggleButton.myID,
                    2 => colorPickerToggleButton.myID,
                    3 => organizeButton.myID,
                    4 => organizeButton.myID,
                    5 => organizeButton.myID,
                    6 => organizeButton.myID,
                    _ => organizeButton.myID
                };
            }

            colorPickerToggleButton.leftNeighborID = rightItems[2].myID;
            organizeButton.leftNeighborID = rightItems[3].myID;
            /*
            if (ModConfig.Instance.EnableCategories)
            {
                var leftItems =
                    Enumerable.Range(0, 6)
                        .Select(i => _itemsToGrabMenu.inventory.ElementAt(i * 12))
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
            */
        }
        public override void draw(SpriteBatch b)
        {
            // Background
            if (!Game1.options.showMenuBackground)
            {
                b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.5f);
            }

            _itemsToGrabMenu.draw(b);
            _inventory.draw(b);
            chestColorPicker.draw(b);

            // Inventory Icon
            b.Draw(Game1.mouseCursors,
                new Vector2(_inventory.xPositionOnScreen - 80, _inventory.yPositionOnScreen + 124),
                new Rectangle(16, 368, 12, 16),
                Color.White,
                4.712389f,
                Vector2.Zero,
                Game1.pixelZoom,
                SpriteEffects.None,
                1f);
            b.Draw(Game1.mouseCursors,
                new Vector2(_inventory.xPositionOnScreen - 80, _inventory.yPositionOnScreen + 92),
                new Rectangle(21, 368, 11, 16),
                Color.White,
                4.712389f,
                Vector2.Zero,
                Game1.pixelZoom,
                SpriteEffects.None,
                1f);
            b.Draw(Game1.mouseCursors,
                new Vector2(_inventory.xPositionOnScreen - 56, _inventory.yPositionOnScreen + 64),
                new Rectangle(4, 372, 8, 11),
                Color.White,
                0.0f,
                Vector2.Zero,
                Game1.pixelZoom,
                SpriteEffects.None,
                1f);

            //poof?.draw(b, true);

            foreach (var clickableComponent in allClickableComponents.OfType<ClickableTextureComponent>())
            {
                switch (clickableComponent.name)
                {
                    case "trashCan":
                        clickableComponent.draw(b);
                        b.Draw(
                            Game1.mouseCursors,
                            new Vector2(clickableComponent.bounds.X + 60, clickableComponent.bounds.Y + 40),
                            new Rectangle(564 + Game1.player.trashCanLevel * 18, 129, 18, 10),
                            Color.White,
                            trashCanLidRotation,
                            new Vector2(16f, 10f),
                            Game1.pixelZoom,
                            SpriteEffects.None,
                            0.86f);
                        break;
                    default:
                        if (clickableComponent is ChestCategory chestCategory)
                        {
                            chestCategory.Draw(b, chestCategory.Equals(_itemsToGrabMenu.SelectedCategory));
                        }
                        else
                        {
                            clickableComponent.draw(b);
                        }
                        break;
                }
            }

            if (!(hoveredItem is null))
            {
                // Hover Item
                IClickableMenu.drawToolTip(
                    b,
                    hoveredItem.getDescription(),
                    hoveredItem.DisplayName,
                    hoveredItem,
                    !(heldItem is null));
            }
            else if (!(hoverText is null) && hoverAmount > 0)
            {
                // Hover Text w/Amount
                IClickableMenu.drawToolTip(
                    b,
                    hoverText,
                    "",
                    null,
                    true,
                    moneyAmountToShowAtBottom: hoverAmount);
            }
            else if (!(hoverText is null))
            {
                // Hover Text
                IClickableMenu.drawHoverText(b, hoverText, Game1.smallFont);
            }

            // Held Item
            heldItem?.drawInMenu(b, new Vector2(Game1.getOldMouseX() + 8, Game1.getOldMouseY() + 8), 1f);

            // Game Cursor
            Game1.mouseCursorTransparency = 1f;
            drawMouse(b);
        }
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            heldItem = _inventory.leftClick(x, y, heldItem, playSound);

            chestColorPicker.receiveLeftClick(x, y);
            _customChest.playerChoiceColor.Value = chestColorPicker.getColorFromSelection(chestColorPicker.colorSelection);
            
            if (heldItem is null)
            {
                heldItem = _itemsToGrabMenu.leftClick(x, y, heldItem, false);
                if (!(heldItem is null))
                {
                    behaviorOnItemGrab(heldItem, Game1.player);
                    if (Game1.options.SnappyMenus)
                        snapCursorToCurrentSnappedComponent();
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
                                    heldItem.Name.IndexOf("Recipe",
                                        StringComparison.InvariantCultureIgnoreCase) -
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
                                catch (Exception e)
                                {
                                    Console.WriteLine(e);
                                    throw;
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
            else if (isWithinBounds(x, y))
            {
                BehaviorFunction(heldItem, Game1.player);
            }

            foreach (var clickableComponent in allClickableComponents.Where(c => c.containsPoint(x, y)))
            {
                switch (clickableComponent.name)
                {

                    case "colorPickerToggleButton":
                        Game1.player.showChestColorPicker = !Game1.player.showChestColorPicker;
                        chestColorPicker.visible = Game1.player.showChestColorPicker;
                        Game1.playSound("drumkit6");
                        break;
                    case "fillStacksButton":
                        FillOutStacks();
                        Game1.player.Items = _inventory.actualInventory;
                        Game1.playSound("Ship");
                        break;
                    case "organizeButton":
                        organizeItemsInList(_itemsToGrabMenu.actualInventory);
                        Game1.playSound("Ship");
                        break;
                    case "okButton":
                        exitThisMenu();
                        if (!(Game1.currentLocation.currentEvent is null))
                            ++Game1.currentLocation.currentEvent.CurrentCommand;
                        Game1.playSound("bigDeSelect");
                        break;
                    case "trashCan":
                        if (!(heldItem is null))
                        {
                            Utility.trashItem(heldItem);
                            heldItem = null;
                        }
                        break;
                    default:
                        if (clickableComponent is ChestCategory chestCategory)
                        {
                            _itemsToGrabMenu.SelectedCategory = chestCategory;
                        }
                        break;
                }
            }
            _itemsToGrabMenu.receiveLeftClick(x, y, playSound);
            _inventory.receiveLeftClick(x, y, playSound);
            _itemsToGrabMenu.RefreshItems();
            _inventory.RefreshItems();
        }
        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            if (!allowRightClick)
            {
                heldItem = _inventory.rightClick(x, y, heldItem, playSound && playRightClickSound, true);
                return;
            }

            heldItem = _inventory.rightClick(x, y, heldItem, playSound && playRightClickSound);
            if (heldItem is null)
            {
                heldItem = _itemsToGrabMenu.rightClick(x, y, heldItem, false);
                if (!(heldItem is null))
                {
                    behaviorOnItemGrab(heldItem, Game1.player);
                    if (Game1.options.SnappyMenus)
                        snapCursorToCurrentSnappedComponent();
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
                                    heldItem.Name.IndexOf("Recipe",
                                        StringComparison.InvariantCultureIgnoreCase) -
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
                                catch (Exception e)
                                {
                                    Console.WriteLine(e);
                                    throw;
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
            else if (isWithinBounds(x, y))
            {
                BehaviorFunction(heldItem, Game1.player);
            }
            _itemsToGrabMenu.receiveRightClick(x, y, playSound && playRightClickSound);
            _inventory.receiveRightClick(x, y, playSound && playRightClickSound);
            _itemsToGrabMenu.RefreshItems();
            _inventory.RefreshItems();
        }
        public override void receiveScrollWheelAction(int direction)
        {
            var mouseX = Game1.getOldMouseX();
            var mouseY = Game1.getOldMouseY();
            if (chestColorPicker.isWithinBounds(mouseX, mouseY))
            {
                if (direction < 0 && chestColorPicker.colorSelection < chestColorPicker.totalColors - 1)
                {
                    chestColorPicker.colorSelection++;
                }
                else if (direction > 0 && chestColorPicker.colorSelection > 0)
                {
                    chestColorPicker.colorSelection--;
                }
                ((Chest)chestColorPicker.itemToDrawColored).playerChoiceColor.Value = chestColorPicker.getColorFromSelection(chestColorPicker.colorSelection);
                _customChest.playerChoiceColor.Value = chestColorPicker.getColorFromSelection(chestColorPicker.colorSelection);
            }
            else if (_itemsToGrabMenu.isWithinBounds(mouseX, mouseY))
            {
                _itemsToGrabMenu.receiveScrollWheelAction(direction);
            }
            else if (allClickableComponents.OfType<ChestCategory>().Any(c => c.containsPoint(mouseX, mouseY)))
            {
                ChestCategory savedCategory = null;
                ChestCategory beforeCategory = null;
                ChestCategory nextCategory = null;
                foreach (var currentCategory in allClickableComponents.OfType<ChestCategory>())
                {
                    if (savedCategory == _itemsToGrabMenu.SelectedCategory)
                    {
                        nextCategory = currentCategory;
                        break;
                    }
                    else
                    {
                        beforeCategory = savedCategory;
                    }
                    savedCategory = currentCategory;
                }

                if (direction < 0 && !(nextCategory is null))
                {
                    _itemsToGrabMenu.SelectedCategory = nextCategory;
                }
                else if (direction > 0 && !(beforeCategory is null))
                {
                    _itemsToGrabMenu.SelectedCategory = beforeCategory;
                }
            }
        }
        public override void performHoverAction(int x, int y)
        {
            hoveredItem = _inventory.hover(x, y, heldItem) ?? _itemsToGrabMenu.hover(x, y, heldItem);
            hoverText = _inventory.hoverText ?? _itemsToGrabMenu.hoverText;
            hoverAmount = 0;
            chestColorPicker.performHoverAction(x, y);
            foreach (var clickableComponent in allClickableComponents.OfType<ClickableTextureComponent>())
            {
                if (!(clickableComponent.hoverText is null) && clickableComponent.containsPoint(x, y))
                {
                    hoverText = clickableComponent.hoverText;
                }

                switch (clickableComponent.name)
                {
                    case "fillStacksButton":
                    case "organizeButton":
                        clickableComponent.scale = clickableComponent.containsPoint(x, y)
                            ? Math.Min(Game1.pixelZoom * 1.1f, clickableComponent.scale + 0.05f)
                            : Math.Max(Game1.pixelZoom, clickableComponent.scale - 0.05f);
                        break;
                    case "okButton":
                        clickableComponent.scale = clickableComponent.containsPoint(x, y)
                            ? Math.Min(1.1f, clickableComponent.scale + 0.05f)
                            : Math.Max(1f, clickableComponent.scale - 0.05f);
                        break;
                    case "trashCan":
                        if (clickableComponent.containsPoint(x, y))
                        {
                            if (trashCanLidRotation <= 0f)
                            {
                                Game1.playSound("trashcanlid");
                            }

                            trashCanLidRotation = Math.Min(trashCanLidRotation + (float)Math.PI / 48f, 1.570796f);

                            if (!(heldItem is null) && Utility.getTrashReclamationPrice(heldItem, Game1.player) > 0)
                            {
                                hoverText = Game1.content.LoadString("Strings\\UI:TrashCanSale");
                                hoverAmount = Utility.getTrashReclamationPrice(heldItem, Game1.player);
                            }
                        }
                        else
                        {
                            trashCanLidRotation = Math.Max(trashCanLidRotation - (float)Math.PI / 48f, 0.0f);
                        }
                        break;
                }
            }
        }
        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            initialize(
                (Game1.viewport.Width - MenuWidth) / 2,
                (Game1.viewport.Height - MenuHeight) / 2,
                MenuWidth,
                MenuHeight);
            if (yPositionOnScreen < IClickableMenu.spaceToClearTopBorder)
            {
                yPositionOnScreen = IClickableMenu.spaceToClearTopBorder;
            }
            if (xPositionOnScreen < 0)
            {
                xPositionOnScreen = 0;
            }
            _itemsToGrabMenu.xPositionOnScreen = xPositionOnScreen + XOffset;
            _itemsToGrabMenu.yPositionOnScreen = yPositionOnScreen + YOffset;
            _inventory.xPositionOnScreen = _itemsToGrabMenu.xPositionOnScreen;
            _inventory.yPositionOnScreen = _itemsToGrabMenu.yPositionOnScreen + _itemsToGrabMenu.height + 80;
            okButton.bounds.X = xPositionOnScreen + _itemsToGrabMenu.width + RightXOffset;
            okButton.bounds.Y = _inventory.yPositionOnScreen + 140;
            trashCan.bounds.X = xPositionOnScreen + _itemsToGrabMenu.width  + RightXOffset;
            trashCan.bounds.Y = _inventory.yPositionOnScreen + 4;
            chestColorPicker.xPositionOnScreen = _itemsToGrabMenu.xPositionOnScreen + TopXOffset;
            chestColorPicker.yPositionOnScreen = _itemsToGrabMenu.yPositionOnScreen + TopYOffset;
            colorPickerToggleButton.bounds.X = _itemsToGrabMenu.xPositionOnScreen + _itemsToGrabMenu.width + RightXOffset;
            colorPickerToggleButton.bounds.Y = _itemsToGrabMenu.yPositionOnScreen + _itemsToGrabMenu.height / 4  + RightYOffset;
            fillStacksButton.bounds.X = xPositionOnScreen + _itemsToGrabMenu.width + RightXOffset;
            fillStacksButton.bounds.Y = _itemsToGrabMenu.yPositionOnScreen + _itemsToGrabMenu.height * 2 / 4  + RightYOffset;
            organizeButton.bounds.X = xPositionOnScreen + _itemsToGrabMenu.width + RightXOffset;
            organizeButton.bounds.Y = _itemsToGrabMenu.yPositionOnScreen + _itemsToGrabMenu.height * 3 / 4  + RightYOffset;
            var index = 0;
            foreach (var chestCategory in Categories.Select(category => allClickableComponents
                .OfType<ChestCategory>()
                .First(c => c.name == category.Key)))
            {
                chestCategory.xPosition = _itemsToGrabMenu.xPositionOnScreen + LeftXOffset;
                chestCategory.yPosition = _itemsToGrabMenu.yPositionOnScreen + index * 60 + LeftYOffset;
                index++;
            }
            _itemsToGrabMenu.gameWindowSizeChanged(oldBounds, newBounds);
            _inventory.gameWindowSizeChanged(oldBounds, newBounds);
        }

        /*********
        ** Private methods
        *********/
        private void SetupItemsMenu()
        {
            _itemsToGrabMenu = new CustomInventoryMenu(
                xPositionOnScreen + XOffset,
                yPositionOnScreen + YOffset,
                Capacity,
                Rows,
                _customChest);
            ItemsToGrabMenu = _itemsToGrabMenu;

            // Color Picker
            chestColorPicker = new DiscreteColorPicker(
                _itemsToGrabMenu.xPositionOnScreen + TopXOffset,
                _itemsToGrabMenu.yPositionOnScreen + TopYOffset,
                0,
                new Chest(true));
            chestColorPicker.colorSelection =
                chestColorPicker.getSelectionFromColor(_customChest.playerChoiceColor.Value);
            ((Chest)chestColorPicker.itemToDrawColored).playerChoiceColor.Value =
                chestColorPicker.getColorFromSelection(chestColorPicker.colorSelection);
            
            // Color Picker Toggle
            colorPickerToggleButton = new ClickableTextureComponent(
                "colorPickerToggleButton",
                new Rectangle(_itemsToGrabMenu.xPositionOnScreen + _itemsToGrabMenu.width + RightXOffset,
                    _itemsToGrabMenu.yPositionOnScreen + _itemsToGrabMenu.height / 4 + RightYOffset,
                    TileSize, TileSize),
                "",
                "",
                Game1.mouseCursors,
                new Rectangle(119, 469, 16, 16),
                Game1.pixelZoom)
            {
                hoverText = Game1.content.LoadString("Strings\\UI:Toggle_ColorPicker"),
                myID = 27346,
                downNeighborID = -99998,
                leftNeighborID = 53921,
                region = 15923
            };

            // Stack
            fillStacksButton = new ClickableTextureComponent(
                "fillStacksButton",
                new Rectangle(_itemsToGrabMenu.xPositionOnScreen + _itemsToGrabMenu.width + RightXOffset,
                    _itemsToGrabMenu.yPositionOnScreen + _itemsToGrabMenu.height * 2 / 4 + RightYOffset,
                    TileSize, TileSize),
                "",
                Game1.content.LoadString("Strings\\UI:ItemGrab_FillStacks"),
                Game1.mouseCursors,
                new Rectangle(103, 469, 16, 16),
                Game1.pixelZoom)
            {
                myID = 12952,
                upNeighborID = 27346,
                downNeighborID = 106,
                leftNeighborID = 53921,
                region = 15923
            };

            // Organize
            organizeButton = new ClickableTextureComponent(
                "organizeButton",
                new Rectangle(_itemsToGrabMenu.xPositionOnScreen + _itemsToGrabMenu.width + RightXOffset,
                    _itemsToGrabMenu.yPositionOnScreen + _itemsToGrabMenu.height * 3 / 4 + RightYOffset,
                    TileSize, TileSize),
                "",
                Game1.content.LoadString("Strings\\UI:ItemGrab_Organize"),
                Game1.mouseCursors,
                new Rectangle(162, 440, 16, 16),
                Game1.pixelZoom)
            {
                myID = 106,
                upNeighborID = 12952,
                downNeighborID = 5948,
                leftNeighborID = 53921,
                region = 15923
            };

            // Categories
            var index = 0;
            foreach (var category in Categories)
            {
                if (!ModConfig.Instance.Categories.TryGetValue(category.Key, out var categoryIds) &&
                    !category.Key.Equals("All", StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }
                switch (category.Key)
                {
                    case "All":
                        allClickableComponents.Add(new AllCategory(
                            category.Key,
                            category.Value,
                            _itemsToGrabMenu.xPositionOnScreen + LeftXOffset,
                            _itemsToGrabMenu.yPositionOnScreen + index * 60 + LeftYOffset));
                        break;
                    case "Misc":
                        allClickableComponents.Add(new MiscCategory(
                            category.Key,
                            category.Value,
                            _itemsToGrabMenu.xPositionOnScreen + LeftXOffset,
                            _itemsToGrabMenu.yPositionOnScreen + index * 60 + LeftYOffset,
                            categoryIds));
                        break;
                    default:
                        allClickableComponents.Add(new ChestCategory(
                            category.Key,
                            category.Value,
                            _itemsToGrabMenu.xPositionOnScreen + LeftXOffset,
                            _itemsToGrabMenu.yPositionOnScreen + index * 60 + LeftYOffset,
                            categoryIds));
                        break;
                }
                index++;
            }
            _itemsToGrabMenu.SelectedCategory = allClickableComponents.OfType<ChestCategory>().First();

            allClickableComponents.Add(colorPickerToggleButton);
            allClickableComponents.Add(fillStacksButton);
            allClickableComponents.Add(organizeButton);
        }
        private void SetupInventoryMenu()
        {
            _inventory = new CustomInventoryMenu(
                _itemsToGrabMenu.xPositionOnScreen,
                _itemsToGrabMenu.yPositionOnScreen + _itemsToGrabMenu.height + 80)
            {
                showGrayedOutSlots = true
            };
            inventory = _inventory;

            // OK Button
            okButton = new ClickableTextureComponent(
                "okButton",
                new Rectangle(
                    _inventory.xPositionOnScreen + _inventory.width + RightXOffset,
                    _inventory.yPositionOnScreen + 140,
                    TileSize, TileSize),
                "",
                "",
                Game1.mouseCursors,
                Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46),
                1f)
            {
                myID = 4857,
                upNeighborID = 5948,
                leftNeighborID = 11
            };

            // Trash Can
            trashCan = new ClickableTextureComponent(
                "trashCan",
                new Rectangle(
                    _inventory.xPositionOnScreen + _inventory.width + RightXOffset,
                    _inventory.yPositionOnScreen + 4,
                    TileSize, 104),
                "",
                "",
                Game1.mouseCursors,
                new Rectangle(564 + Game1.player.trashCanLevel * 18, 102, 18, 26),
                Game1.pixelZoom)
            {
                myID = 106,
                downNeighborID = 4857,
                leftNeighborID = 11,
                upNeighborID = 106
            };

            // Add Invisible Drop Item Button?

            allClickableComponents.Add(okButton);
            allClickableComponents.Add(trashCan);
        }
        private static TemporaryAnimatedSprite CreatePoof(int x, int y) => new TemporaryAnimatedSprite(
            "TileSheets/animations",
            new Rectangle(0, 320, TileSize, TileSize),
            50f,
            8,
            0,
            new Vector2(x - x % TileSize + 16, y - y % TileSize + 16),
            false,
            false);
    }
}