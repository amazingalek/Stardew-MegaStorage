using furyx639.Common;
using MegaStorage.Framework.Interface.Widgets;
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
        public const int MenuWidth = 768;
        public const int MenuHeight = 680;

        public static readonly Dictionary<string, Rectangle> Categories = new Dictionary<string, Rectangle>()
        {
            {"All", Rectangle.Empty},
            {"Crops", new Rectangle(640, 80, 16, 16)},
            {"Seeds", new Rectangle(656, 64, 16, 16)},
            {"Materials", new Rectangle(672, 64, 16, 16)},
            {"Cooking", new Rectangle(688, 64, 16, 16)},
            {"Fishing", new Rectangle(640, 64, 16, 16)},
            {"Misc", new Rectangle(672, 80, 16, 16)}
        };
        internal Rectangle GetItemsToGrabMenuBounds => _itemsToGrabMenu.Bounds;
        internal Rectangle GetInventoryBounds => _inventory.Bounds;
        internal Vector2 GetItemsToGrabMenuDimensions => _itemsToGrabMenu.Dimensions;
        internal Vector2 GetInventoryDimensions => _inventory.Dimensions;
        internal Vector2 GetItemsToGrabMenuPosition => _itemsToGrabMenu.Position;
        internal Vector2 GetInventoryPosition => _inventory.Position;
        // Offsets to ItemsToGrabMenu and Inventory
        private static readonly Vector2 Offset = new Vector2(-48, -36);

        // Offsets to Color Picker
        private static readonly Vector2 TopOffset = new Vector2(0, -104);

        // Offsets to Categories
        private static readonly Vector2 LeftOffset = new Vector2(-80, -8);

        // Offsets to Color Toggle, Organize, Stack, OK, and Trash
        private static readonly Vector2 RightOffset = new Vector2(56, -32);

        private readonly CustomChest _customChest;
        private CustomInventoryMenu _itemsToGrabMenu;
        private CustomInventoryMenu _inventory;

        private TemporaryAnimatedSprite Poof
        {
            set => _poofReflected.SetValue(value);
        }

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
                yPositionOnScreen = IClickableMenu.spaceToClearTopBorder;
            if (xPositionOnScreen < 0)
                xPositionOnScreen = 0;

            _customChest = customChest;
            _poofReflected = MegaStorageMod.Instance.Helper.Reflection.GetField<TemporaryAnimatedSprite>(this, "poof");
            _behaviorFunction =
                MegaStorageMod.Instance.Helper.Reflection.GetField<behaviorOnItemSelect>(this, "behaviorFunction");
            _behaviorFunction.SetValue(customChest.grabItemFromInventory);
            behaviorOnItemGrab = customChest.grabItemFromChest;
            playRightClickSound = true;
            allowRightClick = true;

            allClickableComponents = new List<ClickableComponent>();

            SetupItemsMenu();
            SetupInventoryMenu();
            SetControllerSupport();
        }

        public override void draw(SpriteBatch b)
        {
            if (b is null)
                return;

            // Background
            if (!Game1.options.showMenuBackground)
            {
                b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.5f);
            }

            _itemsToGrabMenu.draw(b);
            _inventory.draw(b);
            chestColorPicker.draw(b);

            // Inventory Icon
            CommonHelper.DrawInventoryIcon(b, _inventory.xPositionOnScreen - 80, _inventory.yPositionOnScreen + 64);

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
            _customChest.playerChoiceColor.Value =
                chestColorPicker.getColorFromSelection(chestColorPicker.colorSelection);

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

            foreach (var clickableComponent in allClickableComponents
                .OfType<CustomClickableTextureComponent>()
                .Where(c => c.containsPoint(x, y) && !(c.LeftClickAction is null)))
            {
                clickableComponent.LeftClickAction(clickableComponent);
            }

            _itemsToGrabMenu.receiveLeftClick(x, y, playSound);
            _inventory.receiveLeftClick(x, y, playSound);
            RefreshItems();
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

            foreach (var clickableComponent in allClickableComponents
                .OfType<CustomClickableTextureComponent>()
                .Where(c => c.containsPoint(x, y) && !(c.RightClickAction is null)))
            {
                clickableComponent.RightClickAction(clickableComponent);
            }

            _itemsToGrabMenu.receiveRightClick(x, y, playSound && playRightClickSound);
            _inventory.receiveRightClick(x, y, playSound && playRightClickSound);
            RefreshItems();
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

                ((Chest)chestColorPicker.itemToDrawColored).playerChoiceColor.Value =
                    chestColorPicker.getColorFromSelection(chestColorPicker.colorSelection);
                _customChest.playerChoiceColor.Value =
                    chestColorPicker.getColorFromSelection(chestColorPicker.colorSelection);
            }
            else if (_itemsToGrabMenu.isWithinBounds(mouseX, mouseY))
            {
                _itemsToGrabMenu.receiveScrollWheelAction(direction);
            }
            else
            {
                foreach (var clickableComponent in allClickableComponents
                    .OfType<CustomClickableTextureComponent>()
                    .Where(c => c.containsPoint(mouseX, mouseY) && !(c.ScrollAction is null)))
                {
                    clickableComponent.ScrollAction(direction, clickableComponent);
                }
            }
        }

        public override void performHoverAction(int x, int y)
        {
            hoveredItem = _inventory.hover(x, y, heldItem) ?? _itemsToGrabMenu.hover(x, y, heldItem);
            hoverText = _inventory.hoverText ?? _itemsToGrabMenu.hoverText;
            hoverAmount = 0;
            chestColorPicker.performHoverAction(x, y);

            // Hover Text
            foreach (var clickableComponent in allClickableComponents
                .OfType<CustomClickableTextureComponent>()
                .Where(c => !(c.hoverText is null) && c.containsPoint(x, y)))
            {
                hoverText = clickableComponent.hoverText;
            }

            // Hover Action
            foreach (var clickableComponent in allClickableComponents
                .OfType<CustomClickableTextureComponent>()
                .Where(c => !(c.HoverAction is null)))
            {
                clickableComponent.HoverAction(x, y, clickableComponent);
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
                yPositionOnScreen = IClickableMenu.spaceToClearTopBorder;
            if (xPositionOnScreen < 0)
                xPositionOnScreen = 0;

            _itemsToGrabMenu.GameWindowSizeChanged();
            _inventory.GameWindowSizeChanged();

            chestColorPicker.xPositionOnScreen = _itemsToGrabMenu.xPositionOnScreen + (int)TopOffset.X;
            chestColorPicker.yPositionOnScreen = _itemsToGrabMenu.yPositionOnScreen + (int)TopOffset.Y;

            foreach (var clickableComponent in allClickableComponents.OfType<CustomClickableTextureComponent>())
            {
                clickableComponent.GameWindowSizeChanged();
            }
        }

        private CustomChestEventArgs CustomChestEventArgs => new CustomChestEventArgs()
        {
            VisibleItems = _itemsToGrabMenu.VisibleItems,
            AllItems = _itemsToGrabMenu.actualInventory,
            CurrentCategory = _itemsToGrabMenu.SelectedCategory.name,
            HeldItem = heldItem
        };

        internal void RefreshItems()
        {
            _itemsToGrabMenu.RefreshItems();
            _inventory.RefreshItems();
            MegaStorageApi.InvokeVisibleItemsRefreshed(this, CustomChestEventArgs);
        }

        internal void ClickColorPickerToggleButton(CustomClickableTextureComponent clickableComponent = null)
        {
            Game1.player.showChestColorPicker = !Game1.player.showChestColorPicker;
            chestColorPicker.visible = Game1.player.showChestColorPicker;
            Game1.playSound("drumkit6");
            MegaStorageApi.InvokeColorPickerToggleButtonClicked(this, CustomChestEventArgs);
        }

        internal void ClickFillStacksButton(CustomClickableTextureComponent clickableComponent = null)
        {
            MegaStorageApi.InvokeBeforeFillStacksButtonClicked(this, CustomChestEventArgs);
            FillOutStacks();
            Game1.player.Items = _inventory.actualInventory;
            Game1.playSound("Ship");
            MegaStorageApi.InvokeAfterFillStacksButtonClicked(this, CustomChestEventArgs);
        }

        internal void ClickOrganizeButton(CustomClickableTextureComponent clickableComponent = null)
        {
            MegaStorageApi.InvokeBeforeOrganizeButtonClicked(this, CustomChestEventArgs);
            organizeItemsInList(_itemsToGrabMenu.actualInventory);
            Game1.playSound("Ship");
            MegaStorageApi.InvokeAfterOrganizeButtonClicked(this, CustomChestEventArgs);
        }

        internal void ClickOkButton(CustomClickableTextureComponent clickableComponent = null)
        {
            MegaStorageApi.InvokeBeforeOkButtonClicked(this, CustomChestEventArgs);
            exitThisMenu();
            if (!(Game1.currentLocation.currentEvent is null))
                ++Game1.currentLocation.currentEvent.CurrentCommand;
            Game1.playSound("bigDeSelect");
            MegaStorageApi.InvokeAfterOkButtonClicked(this, CustomChestEventArgs);
        }

        internal void ClickTrashCan(CustomClickableTextureComponent clickableComponent = null)
        {
            MegaStorageApi.InvokeBeforeTrashCanClicked(this, CustomChestEventArgs);
            if (heldItem is null)
                return;
            Utility.trashItem(heldItem);
            heldItem = null;
            MegaStorageApi.InvokeAfterTrashCanClicked(this, CustomChestEventArgs);
        }

        internal void ClickCategoryButton(string categoryName)
        {
            var clickableComponent = allClickableComponents
                .OfType<ChestCategory>()
                .First(c => c.name.Equals(categoryName, StringComparison.InvariantCultureIgnoreCase));
            ClickCategoryButton(clickableComponent);
        }

        internal void ClickCategoryButton(CustomClickableTextureComponent clickableComponent)
        {
            MegaStorageApi.InvokeBeforeCategoryChanged(this, CustomChestEventArgs);
            if (clickableComponent is ChestCategory chestCategory)
                _itemsToGrabMenu.SelectedCategory = chestCategory;
            MegaStorageApi.InvokeAfterCategoryChanged(this, CustomChestEventArgs);
        }
        internal void ScrollCategory(int direction, CustomClickableTextureComponent clickableComponent = null)
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
            MegaStorageApi.InvokeBeforeCategoryChanged(this, CustomChestEventArgs);
            if (direction < 0 && !(nextCategory is null))
            {
                _itemsToGrabMenu.SelectedCategory = nextCategory;
            }
            else if (direction > 0 && !(beforeCategory is null))
            {
                _itemsToGrabMenu.SelectedCategory = beforeCategory;
            }
            MegaStorageApi.InvokeAfterCategoryChanged(this, CustomChestEventArgs);
        }

        internal void HoverZoom(int x, int y, CustomClickableTextureComponent clickableComponent)
        {
            clickableComponent.scale = clickableComponent.containsPoint(x, y)
                ? Math.Min(1.1f, clickableComponent.scale + 0.05f)
                : Math.Max(1f, clickableComponent.scale - 0.05f);
        }

        internal void HoverPixelZoom(int x, int y, CustomClickableTextureComponent clickableComponent)
        {
            clickableComponent.scale = clickableComponent.containsPoint(x, y)
                ? Math.Min(Game1.pixelZoom * 1.1f, clickableComponent.scale + 0.05f)
                : Math.Max(Game1.pixelZoom * 1f, clickableComponent.scale - 0.05f);
        }

        internal void HoverTrashCan(int x, int y, CustomClickableTextureComponent clickableComponent)
        {
            if (!clickableComponent.containsPoint(x, y))
            {
                trashCanLidRotation = Math.Max(trashCanLidRotation - (float)Math.PI / 48f, 0.0f);
                return;
            }

            if (trashCanLidRotation <= 0f)
                Game1.playSound("trashcanlid");
            trashCanLidRotation = Math.Min(trashCanLidRotation + (float)Math.PI / 48f, 1.570796f);

            if (heldItem is null || Utility.getTrashReclamationPrice(heldItem, Game1.player) <= 0)
                return;
            hoverText = Game1.content.LoadString("Strings\\UI:TrashCanSale");
            hoverAmount = Utility.getTrashReclamationPrice(heldItem, Game1.player);
        }

        /*********
        ** Private methods
        *********/
        private void SetupItemsMenu()
        {
            _itemsToGrabMenu = new CustomInventoryMenu(this, Offset, _customChest);
            ItemsToGrabMenu = _itemsToGrabMenu;

            // Color Picker
            chestColorPicker = new DiscreteColorPicker(
                _itemsToGrabMenu.xPositionOnScreen + (int)TopOffset.X,
                _itemsToGrabMenu.yPositionOnScreen + (int)TopOffset.Y,
                0,
                new Chest(true));
            chestColorPicker.colorSelection =
                chestColorPicker.getSelectionFromColor(_customChest.playerChoiceColor.Value);
            ((Chest)chestColorPicker.itemToDrawColored).playerChoiceColor.Value =
                chestColorPicker.getColorFromSelection(chestColorPicker.colorSelection);

            // Color Picker Toggle
            colorPickerToggleButton = new CustomClickableTextureComponent(
                "colorPickerToggleButton",
                _itemsToGrabMenu,
                RightOffset + _itemsToGrabMenu.Dimensions * new Vector2(1, 1f / 4f),
                Game1.mouseCursors,
                new Rectangle(119, 469, 16, 16),
                Game1.content.LoadString("Strings\\UI:Toggle_ColorPicker"))
            {
                myID = 27346,
                downNeighborID = -99998,
                leftNeighborID = 53921,
                region = 15923,
                LeftClickAction = ClickColorPickerToggleButton
            };

            // Stack
            fillStacksButton = new CustomClickableTextureComponent(
                "fillStacksButton",
                _itemsToGrabMenu,
                RightOffset + _itemsToGrabMenu.Dimensions * new Vector2(1, 2f / 4f),
                Game1.mouseCursors,
                new Rectangle(103, 469, 16, 16),
                Game1.content.LoadString("Strings\\UI:ItemGrab_FillStacks"))
            {
                myID = 12952,
                upNeighborID = 27346,
                downNeighborID = 106,
                leftNeighborID = 53921,
                region = 15923,
                LeftClickAction = ClickFillStacksButton,
                HoverAction = HoverPixelZoom
            };

            // Organize
            organizeButton = new CustomClickableTextureComponent(
                "organizeButton",
                _itemsToGrabMenu,
                RightOffset + _itemsToGrabMenu.Dimensions * new Vector2(1, 3f / 4f),
                Game1.mouseCursors,
                new Rectangle(162, 440, 16, 16),
                Game1.content.LoadString("Strings\\UI:ItemGrab_Organize"))
            {
                myID = 106,
                upNeighborID = 12952,
                downNeighborID = 5948,
                leftNeighborID = 53921,
                region = 15923,
                LeftClickAction = ClickOrganizeButton,
                HoverAction = HoverPixelZoom
            };

            allClickableComponents.Add(colorPickerToggleButton);
            allClickableComponents.Add(fillStacksButton);
            allClickableComponents.Add(organizeButton);

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
                            _itemsToGrabMenu,
                            LeftOffset + new Vector2(0, index * 60),
                            category.Value)
                        {
                            LeftClickAction = ClickCategoryButton,
                            ScrollAction = ScrollCategory
                        });
                        break;
                    case "Misc":
                        allClickableComponents.Add(new MiscCategory(
                            category.Key,
                            _itemsToGrabMenu,
                            LeftOffset + new Vector2(0, index * 60),
                            category.Value,
                            categoryIds)
                        {
                            LeftClickAction = ClickCategoryButton,
                            ScrollAction = ScrollCategory
                        });
                        break;
                    default:
                        allClickableComponents.Add(new ChestCategory(
                            category.Key,
                            _itemsToGrabMenu,
                            LeftOffset + new Vector2(0, index * 60),
                            category.Value,
                            categoryIds)
                        {
                            LeftClickAction = ClickCategoryButton,
                            ScrollAction = ScrollCategory
                        });
                        break;
                }
                index++;
            }
            _itemsToGrabMenu.SelectedCategory = allClickableComponents.OfType<ChestCategory>().First();
        }
        private void SetupInventoryMenu()
        {
            _inventory = new CustomInventoryMenu(this, new Vector2(0, _itemsToGrabMenu.height + 80) + Offset)
            {
                showGrayedOutSlots = true
            };
            inventory = _inventory;

            // OK Button
            okButton = new CustomClickableTextureComponent(
                "okButton",
                _inventory,
                new Vector2(_inventory.width + RightOffset.X, 140),
                Game1.mouseCursors,
                Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46),
                scale: 1f)
            {
                myID = 4857,
                upNeighborID = 5948,
                leftNeighborID = 11,
                LeftClickAction = ClickOkButton,
                HoverAction = HoverZoom
            };

            // Trash Can
            trashCan = new CustomClickableTextureComponent(
                "trashCan",
                _inventory,
                new Vector2(_inventory.width + RightOffset.X, 4),
                Game1.mouseCursors,
                new Rectangle(564 + Game1.player.trashCanLevel * 18, 102, 18, 26),
                width: Game1.tileSize,
                height: 104)
            {
                myID = 106,
                downNeighborID = 4857,
                leftNeighborID = 11,
                upNeighborID = 106,
                LeftClickAction = ClickTrashCan,
                HoverAction = HoverTrashCan
            };

            // Add Invisible Drop Item Button?

            allClickableComponents.Add(okButton);
            allClickableComponents.Add(trashCan);
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
        private static TemporaryAnimatedSprite CreatePoof(int x, int y) => new TemporaryAnimatedSprite(
            "TileSheets/animations",
            new Rectangle(0, 320, Game1.tileSize, Game1.tileSize),
            50f,
            8,
            0,
            new Vector2(x - x % Game1.tileSize + 16, y - y % Game1.tileSize + 16),
            false,
            false);
    }
}