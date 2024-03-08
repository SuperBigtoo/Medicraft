using GeonBit.UI;
using GeonBit.UI.Entities;
using GeonBit.UI.Utils.Forms;
using Medicraft.Data.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using System.Linq;


namespace Medicraft.Systems.Managers
{
    public class GUIManager
    {
        private static GUIManager instance;

        private float _deltaSeconds = 0;

        public int CurrentGUI { get; set; }

        public const int Hotbar = 0;
        public const int InventoryPanel = 1;
        public const int CraftingPanel = 2;
        public const int CharacterInspectPanel = 3;

        public string CurrentCraftingList { get; set; }

        public const string ThaiTraditionalMedicine = "Thai traditional medicine";
        public const string ConsumableItem = "Consumable item";
        public const string Equipment = "Equipment";

        // ui elements
        private readonly List<Panel> _mainPanels = [];

        private GUIManager()
        {
            CurrentGUI = Hotbar;
            CurrentCraftingList = ThaiTraditionalMedicine;
        }

        public void UpdateAfterChangeGUI()
        {
            // hide all panels and show current example panel
            foreach (Panel panel in _mainPanels)
            {
                panel.Visible = false;
            }

            _mainPanels[CurrentGUI].Visible = true;
        }

        public void InitializeThemeAndUI(BuiltinThemes theme)
        {

            // create and init the UI manager
            var content = new ContentManager(GameGlobals.Instance.Content.ServiceProvider, "Content");
            UserInterface.Initialize(content, theme);
            UserInterface.Active.UseRenderTarget = true;

            // draw cursor outside the render target
            UserInterface.Active.IncludeCursorInRenderTarget = false;

            // disable Cursor
            UserInterface.Active.ShowCursor = false;

            // init all ui panel
            // Hotbar = 0
            InitHotbarUI();

            // InventoryPanel = 1
            InitInventoryUI();

            // Crafting = 2
            InitCraftingUI();

            // update ui panel
            UpdateAfterChangeGUI();
        }


        /// <summary>
        /// Item Bar and Slot Item
        /// </summary>
        private void InitHotbarUI()
        {
            var panel = new Panel(new Vector2(500, 87), PanelSkin.None, Anchor.BottomCenter)
            { 
                Identifier = "hotbarItem",
                //AdjustHeightAutomatically = true,
            };
            _mainPanels.Add(panel);
            UserInterface.Active.AddEntity(panel);
        }

        public void RefreshHotbarDisplay()
        {
            var HotbarSlot = _mainPanels.ElementAt(Hotbar);
            
            HotbarSlot.ClearChildren();

            var minSlotNum = InventoryManager.HotbarSlot_1;
            var maxSlotNum = InventoryManager.HotbarSlot_8;

            var offSetX = 22f;

            // Set Item
            for (int i = minSlotNum; i <= maxSlotNum; i++)
            {
                var itemInSlot = InventoryManager.Instance.InventoryBag.Values.FirstOrDefault(item => item.Slot.Equals(i));

                if (itemInSlot != null)
                {
                    var iconItem = new Icon(IconType.None, Anchor.BottomLeft, 0.80f, false, offset: new Vector2(offSetX, 3f))
                    {
                        ItemId = itemInSlot.ItemId,
                        Count = itemInSlot.Count,
                        Slot = itemInSlot.Slot,
                        Size = new Vector2(42, 42),
                        Texture = GameGlobals.Instance.GetItemTexture(itemInSlot.ItemId),
                    };

                    iconItem.AddChild(new Label(iconItem.Count.ToString(), Anchor.BottomRight, offset: new Vector2(-25f, -33))
                    {
                        Size = new Vector2(5, 5), // กำหนดขนาดของ Label
                        Scale = 1f,
                        ClickThrough = true,
                    });

                    HotbarSlot.AddChild(iconItem);
                }
                else
                {
                    var iconNull = new Icon(IconType.None, Anchor.BottomLeft, 0.80f, false, offset: new Vector2(offSetX, 3f))
                    {
                        Locked = true,
                        Enabled = false,
                        Size = new Vector2(42, 42),
                    };

                    HotbarSlot.AddChild(iconNull);
                }

                offSetX += 47f;

                switch (i)
                {
                    case InventoryManager.HotbarSlot_1: offSetX += 5f;
                        break;
                    case InventoryManager.HotbarSlot_2: offSetX += 5.2f;
                        break;
                    case InventoryManager.HotbarSlot_3:
                    case InventoryManager.HotbarSlot_4: offSetX += 5.2f;
                        break;
                    case InventoryManager.HotbarSlot_5: 
                    case InventoryManager.HotbarSlot_6: offSetX += 5.4f;
                        break;
                    case InventoryManager.HotbarSlot_7: offSetX += 5.5f;
                        break;
                    case InventoryManager.HotbarSlot_8: offSetX += 5.6f;
                        break;
                }
            }

            foreach (var iconItem in HotbarSlot.Children.OfType<Icon>())
            {
                iconItem.OnClick = (Entity entity) =>
                {
                    InventoryManager.Instance.UseItemInHotbar(iconItem.ItemId);
                };
            }
        }


        /// <summary>
        /// Inventory Panel, Display list of items in inventory and Description
        /// </summary>
        private void InitInventoryUI()
        {
            // สร้าง Panel หลัก
            var inventoryPanel = new Panel(new Vector2(1200, 650), PanelSkin.Default, Anchor.Center)
            {
                Identifier = "invenMainPanel"
            };
            _mainPanels.Add(inventoryPanel);
            UserInterface.Active.AddEntity(inventoryPanel);

            // สร้าง Panel สำหรับฝั่งซ้าย
            var leftInvenPanel = new Panel(new Vector2(500, 600), PanelSkin.ListBackground, Anchor.TopLeft)
            {
                Identifier = "invenLeftPanel"
            };
            inventoryPanel.AddChild(leftInvenPanel);

            var invenDescriptPanel = new Panel(new Vector2(450, 225), PanelSkin.Fancy, Anchor.AutoCenter)
            {
                Identifier = "invenDescriptPanel",
                PanelOverflowBehavior = PanelOverflowBehavior.VerticalScroll
            };
            invenDescriptPanel.Scrollbar.AdjustMaxAutomatically = true;

            invenDescriptPanel.AddChild(new Label("iconLabel", Anchor.AutoCenter)
            {
                Size = new Vector2(250, 30), // กำหนดขนาดของ Label
                Scale = 1.3f,
                Identifier = "iconLabel"
            });
            invenDescriptPanel.AddChild(new Label("description", Anchor.AutoInline)
            {
                Size = new Vector2(380, 200), // กำหนดขนาดของ Label
                Scale = 1f,
                WrapWords = true,
                Identifier = "description"
            });
            invenDescriptPanel.AddChild(new LineSpace());

            leftInvenPanel.AddChild(new Icon(IconType.None, Anchor.AutoCenter, 1, false)
            {
                Size = new Vector2(300, 300),   // ปรับขนาดของไอคอน
                Locked = true,
                Identifier = "itemIcon" 
            });
            leftInvenPanel.AddChild(new LineSpace());
            leftInvenPanel.AddChild(invenDescriptPanel);

            // สร้าง Panel สำหรับฝั่งขวา
            var rightInvenPanel = new Panel(new Vector2(600, 600), PanelSkin.ListBackground, Anchor.TopRight)
            {
                Identifier = "invenRightPanel"
            };
            inventoryPanel.AddChild(rightInvenPanel);

            rightInvenPanel.AddChild(new Header("IVENTORY"));
            rightInvenPanel.AddChild(new HorizontalLine());
            rightInvenPanel.AddChild(new LineSpace());

            var listItemPanel = new Panel(new Vector2(550, 450), PanelSkin.Fancy, Anchor.AutoCenter)
            {
                PanelOverflowBehavior = PanelOverflowBehavior.VerticalScroll,
                Identifier = "listItemPanel",
            };
            listItemPanel.Scrollbar.AdjustMaxAutomatically = true;
            rightInvenPanel.AddChild(listItemPanel);

            // add close button
            var closeInventoryButton = new Button("Close", anchor: Anchor.BottomRight
                , size: new Vector2(200, -1), offset: new Vector2(64, -100))
            {
                OnClick = (Entity entity) =>
                {
                    // Closing Inventory and reset current gui panel
                    // Pause PlayScreen
                    GameGlobals.Instance.IsGamePause = !GameGlobals.Instance.IsGamePause;
                    GameGlobals.Instance.IsOpenGUI = !GameGlobals.Instance.IsOpenGUI;

                    // Toggle the IsOpenInventory flag
                    GameGlobals.Instance.IsOpenInventoryPanel = false;
                    GameGlobals.Instance.IsRefreshHotbar = false;
                    CurrentGUI = Hotbar;
                }
            };
            inventoryPanel.AddChild(closeInventoryButton);

            var offsetX = 64;

            var useInvenItemButton = new Button("Use", anchor: Anchor.BottomLeft
                , size: new Vector2(200, -1), offset: new Vector2(offsetX, -100))
            {
                Identifier = "invenUseItemButton",
                Enabled = false,
                OnClick = (Entity entity) =>
                {
                    var currItemInv = InventoryManager.Instance.InventoryItemSelected;
                    InventoryManager.Instance.UseItem(currItemInv);
                }
            };
            inventoryPanel.AddChild(useInvenItemButton);

            offsetX += 216;
            var invenSetHotbarButton = new Button("Setup Hotbar", anchor: Anchor.BottomLeft
                , size: new Vector2(200, -1), offset: new Vector2(offsetX, -100))
            {
                Identifier = "invenSetHotbarButton",
                Enabled = false,
                OnClick = (Entity entity) =>
                {
                    var currItemInv = InventoryManager.Instance.InventoryItemSelected;

                    var hotbarSetupForm = new Form([
                            new FormFieldData(FormFieldType.DropDown, "dropdown_setup_hotbar", "Hotbar Slot")
                            {                                 
                                ToolTipText = "Click to select a Hotbar Slot",
                                Choices = ["Slot 1", "Slot 2", "Slot 3", "Slot 4", "Slot 5", "Slot 6", "Slot 7", "Slot 8"] 
                            },                           
                        ], null);                 

                    GeonBit.UI.Utils.MessageBox.ShowMsgBox("Setup Hotbar Slot"
                        , "", "Done"
                        , extraEntities: [hotbarSetupForm.FormPanel]
                        , onDone: () => {

                            if (hotbarSetupForm.GetValue("dropdown_setup_hotbar") != null)
                            {
                                GeonBit.UI.Utils.MessageBox.ShowMsgBox("Setup Hotbar Successfully",
                                    $"Set '{currItemInv.GetName()}' on '{hotbarSetupForm.GetValueString("dropdown_setup_hotbar")}'"
                                );

                                var output = hotbarSetupForm.GetValueString("dropdown_setup_hotbar");
                                var slotNum = InventoryManager.Instance.GetIHotbarSlot(output);
                                InventoryManager.Instance.SetHotbarItem(currItemInv, slotNum);
                            }
                            else
                            {
                                GeonBit.UI.Utils.MessageBox.ShowMsgBox("Setup Complete",
                                    $"Ya didn't select a for '{currItemInv.GetName()}'"
                                );
                            }
                        }
                    );;   
                }
            };
            inventoryPanel.AddChild(invenSetHotbarButton);
        }

        // Call this after initialize Player's Inventory Data
        public void InitInventoryItemDisplay()
        {
            RefreshInvenrotyItemDisplay(false);

            var inventoryPanel = _mainPanels.ElementAt(InventoryPanel);
            var invenRightPanel = inventoryPanel.Children?.FirstOrDefault(p => p.Identifier.Equals("invenRightPanel"));
            var listItemPanel = invenRightPanel.Children?.FirstOrDefault(p => p.Identifier.Equals("listItemPanel"));
            var invenUseItemButton = inventoryPanel.Children?.FirstOrDefault(p => p.Identifier.Equals("invenUseItemButton"));
            var invenSetHotbarButton = inventoryPanel.Children?.FirstOrDefault(p => p.Identifier.Equals("invenSetHotbarButton"));

            if (listItemPanel.Children.Count != 0)
            {
                var fisrtItem = listItemPanel.Children.OfType<Icon>().ToList().ElementAt(0);

                SetInventoryItemDisplay(fisrtItem);

                // Setup Enable or Disable the button for fisrtItem
                InventoryManager.Instance.InventoryBag.TryGetValue(fisrtItem.ItemId.ToString()
                    , out InventoryItemData item);
                InventoryManager.Instance.InventoryItemSelected = item;

                if (GameGlobals.Instance.IsUsableItem(fisrtItem.ItemId))
                {
                    invenUseItemButton.Enabled = true;

                    if (!GameGlobals.Instance.GetItemCategory(fisrtItem.ItemId).Equals("Equipment"))
                    {
                        invenSetHotbarButton.Enabled = true;
                    }
                    else invenSetHotbarButton.Enabled = false;
                }
                else
                {
                    invenUseItemButton.Enabled = false;
                    invenSetHotbarButton.Enabled = true;
                }
            }
        }

        public void RefreshInvenrotyItemDisplay(bool isClearLeftPanel)
        {
            var inventoryPanel = _mainPanels.ElementAt(InventoryPanel);          
            var invenRightPanel = inventoryPanel.Children?.FirstOrDefault(p => p.Identifier.Equals("invenRightPanel"));
            var invenLeftPanel = inventoryPanel.Children?.FirstOrDefault(p => p.Identifier.Equals("invenLeftPanel"));
            var listItemPanel = invenRightPanel.Children?.FirstOrDefault(p => p.Identifier.Equals("listItemPanel"));
            var invenDescriptPanel = invenLeftPanel.Children?.FirstOrDefault(p => p.Identifier.Equals("invenDescriptPanel"));
            var invenUseItemButton = inventoryPanel.Children?.FirstOrDefault(p => p.Identifier.Equals("invenUseItemButton"));
            var invenSetHotbarButton = inventoryPanel.Children?.FirstOrDefault(p => p.Identifier.Equals("invenSetHotbarButton"));

            // Clear inventory display
            listItemPanel.ClearChildren();

            // if true den set invisible to all entity in invenLeftPanel
            if (isClearLeftPanel)
                foreach (var itemLeftPanel in invenLeftPanel.Children)
                    itemLeftPanel.Visible = false;

            //// Set item
            foreach (var item in InventoryManager.Instance.InventoryBag.Values)
            {
                // Item Icon
                var iconItem = new Icon(IconType.None, Anchor.AutoInline, 1, true)
                {
                    ItemId = item.ItemId,
                    Count = item.Count,
                    Slot = item.Slot,
                    Texture = GameGlobals.Instance.GetItemTexture(item.ItemId),
                };

                // Item Count
                iconItem.AddChild(new Label(iconItem.Count.ToString(), Anchor.BottomRight, offset: new Vector2(-22, -35))
                {
                    Size = new Vector2(10, 10),
                    Scale = 1f,
                    ClickThrough = true,
                });

                listItemPanel.AddChild(iconItem);
            }

            foreach (var iconItem in listItemPanel.Children.OfType<Icon>())
            {
                iconItem.OnClick = (Entity entity) =>
                {
                    SetInventoryItemDisplay(iconItem);

                    foreach (var itemLeftPanel in invenLeftPanel.Children)
                        itemLeftPanel.Visible = true;

                    // Enable or Disable the button for each item icon
                    InventoryManager.Instance.InventoryBag.TryGetValue(iconItem.ItemId.ToString()
                        , out InventoryItemData item);

                    InventoryManager.Instance.InventoryItemSelected = item;

                    if (GameGlobals.Instance.IsUsableItem(iconItem.ItemId))
                    {
                        invenUseItemButton.Enabled = true;

                        if (!GameGlobals.Instance.GetItemCategory(iconItem.ItemId).Equals("Equipment"))
                        {
                            invenSetHotbarButton.Enabled = true;
                        }
                        else invenSetHotbarButton.Enabled = false;
                    }
                    else
                    {
                        invenUseItemButton.Enabled = false;
                        invenSetHotbarButton.Enabled = true;
                    }
       
                };
            }
        }

        private void SetInventoryItemDisplay(Icon icon)
        {
            var itemData = GameGlobals.Instance.ItemsDatas.FirstOrDefault(i => i.ItemId.Equals(icon.ItemId));

            var inventoryPanel = _mainPanels.ElementAt(InventoryPanel);
            var invenLeftPanel = inventoryPanel.Children?.FirstOrDefault(p => p.Identifier.Equals("invenLeftPanel"));
            var invenDescriptPanel = invenLeftPanel.Children?.FirstOrDefault(p => p.Identifier.Equals("invenDescriptPanel"));

            // Clone the clicked icon and add it to the left panel
            var itemIcon = invenLeftPanel.Children.OfType<Icon>().FirstOrDefault
                (i => i.Identifier.Equals("itemIcon"));
            itemIcon.Texture = icon.Texture;

            // สร้าง Label เพื่อแสดงชื่อของไอคอน
            var iconLabel = invenDescriptPanel.Children.OfType<Label>().FirstOrDefault
                (i => i.Identifier.Equals("iconLabel"));
            iconLabel.Text = itemData.Name;

            // Description item
            var description = invenDescriptPanel.Children.OfType<Label>().FirstOrDefault
                (i => i.Identifier.Equals("description"));
            description.Text = itemData.Description;                 
        }


        /// <summary>
        /// Crafting Panel, Display list of craftable items that play unlocked
        /// </summary>
        private void InitCraftingUI()
        {
            // สร้าง Panel หลัก
            var craftingPanel = new Panel(new Vector2(1400, 800), PanelSkin.Default, Anchor.Center)
            {
                Visible = true,
                Identifier = "craftingMainPanel"
            };
            _mainPanels.Add(craftingPanel);
            UserInterface.Active.AddEntity(craftingPanel);

            // สร้าง Panel สำหรับฝั่งซ้าย
            var leftCraftingPanel = new Panel(new Vector2(650, 660), PanelSkin.ListBackground, Anchor.TopLeft)
            {
                Identifier = "leftCraftingPanel"
            };
            craftingPanel.AddChild(leftCraftingPanel);

            var craftingDescriptPanel = new Panel(new Vector2(550, 285), PanelSkin.Fancy, Anchor.AutoCenter)
            {
                Identifier = "craftingDescriptPanel",
                PanelOverflowBehavior = PanelOverflowBehavior.VerticalScroll
            };
            craftingDescriptPanel.Scrollbar.AdjustMaxAutomatically = true;

            craftingDescriptPanel.AddChild(new Label("iconLabel", Anchor.AutoCenter)
            {
                Size = new Vector2(250, 30), // กำหนดขนาดของ Label
                Scale = 1.3f,
                Identifier = "iconLabel"
            });
            craftingDescriptPanel.AddChild(new Label("description", Anchor.AutoInline)
            {
                Size = new Vector2(380, 200), // กำหนดขนาดของ Label
                Scale = 1f,
                WrapWords = true,
                Identifier = "description"
            });
            craftingDescriptPanel.AddChild(new LineSpace());

            leftCraftingPanel.AddChild(new Icon(IconType.None, Anchor.AutoCenter, 1, false)
            {
                Size = new Vector2(300, 300),   // ปรับขนาดของไอคอน
                Locked = true,
                Identifier = "itemIcon"
            });
            leftCraftingPanel.AddChild(new LineSpace());
            leftCraftingPanel.AddChild(craftingDescriptPanel);

            // สร้าง Panel สำหรับฝั่งขวา
            var rightCraftingPanel = new Panel(new Vector2(650, 660), PanelSkin.ListBackground, Anchor.TopRight)
            {
                Identifier = "rightCraftingPanel"
            };
            craftingPanel.AddChild(rightCraftingPanel);

            rightCraftingPanel.AddChild(new Header("CRAFTING ITEM LIST"));
            rightCraftingPanel.AddChild(new HorizontalLine());
            rightCraftingPanel.AddChild(new LineSpace());

            var listCraftableItemPanel = new Panel(new Vector2(550, 530), PanelSkin.Fancy, Anchor.AutoCenter)
            {
                PanelOverflowBehavior = PanelOverflowBehavior.VerticalScroll,
                Identifier = "listCraftableItemPanel",
            };
            listCraftableItemPanel.Scrollbar.AdjustMaxAutomatically = true;
            rightCraftingPanel.AddChild(listCraftableItemPanel);

            // Crafting Selected Item
            // Panel
            var craftingSelectItemPanel = new Panel(new Vector2(1400, 800), PanelSkin.Default, Anchor.Center)
            {
                Identifier = "craftingSelectItemPanel",
                Visible = false,
            };
            UserInterface.Active.AddEntity(craftingSelectItemPanel);

            var leftCraftingSelectItemPanel = new Panel(new Vector2(650, 660), PanelSkin.ListBackground, Anchor.TopLeft)
            {
                Identifier = "leftCraftingSelectItemPanel"
            };
            craftingSelectItemPanel.AddChild(leftCraftingSelectItemPanel);

            leftCraftingSelectItemPanel.AddChild(new Icon(IconType.None, Anchor.AutoCenter, 1, false)
            {
                Size = new Vector2(300, 300),   // ปรับขนาดของไอคอน
                Locked = true,
                Identifier = "itemIcon"
            });
            leftCraftingSelectItemPanel.AddChild(new LineSpace(1));

            var quantitySelectorPanel = new Panel(new Vector2(500, 200), PanelSkin.None, Anchor.BottomCenter)
            {
                Identifier = "quantitySelectorPanel"
            };

            var quantitySlider = new Slider(1, 10, SliderSkin.Fancy, Anchor.AutoCenter)
            {
                Identifier = "quantitySlider"
            };

            var quantityLabel = new Label("Quantity: " + quantitySlider.Min, Anchor.AutoCenter)
            {
                Identifier = "quantityLabel",
                Scale = 1.2f,
            };
            quantitySlider.OnValueChange = (Entity entity) =>
            {
                quantityLabel.Text = "Quantity: " + quantitySlider.Value;
            };

            quantitySelectorPanel.AddChild(quantityLabel);
            quantitySelectorPanel.AddChild(quantitySlider);

            leftCraftingSelectItemPanel.AddChild(quantitySelectorPanel);

            var rightCraftingSelectPanel = new Panel(new Vector2(650, 660), PanelSkin.None, Anchor.TopRight)
            {
                Identifier = "rightCraftingSelectPanel"
            };
            craftingSelectItemPanel.AddChild(rightCraftingSelectPanel);

            rightCraftingSelectPanel.AddChild(new Header("INGREDINES"));
            rightCraftingSelectPanel.AddChild(new HorizontalLine());
            rightCraftingSelectPanel.AddChild(new LineSpace());

            var ingredientList = new SelectList(new Vector2(550, 530), Anchor.AutoCenter, skin: PanelSkin.ListBackground)
            {
                Identifier = "ingredientList",
                Padding = new Vector2(40, 16),
                LockSelection = true
            };         

            rightCraftingSelectPanel.AddChild(ingredientList);

            var closeCraftingSelectedButton = new Button("Back", anchor: Anchor.BottomRight
                , size: new Vector2(200, -1), offset: new Vector2(64, 10))
            {
                Identifier = "closeCraftingSelectButton",
                OnClick = (Entity entity) =>
                {
                    craftingPanel.Visible = !craftingPanel.Visible;
                    craftingSelectItemPanel.Visible = !craftingSelectItemPanel.Visible;

                    quantitySlider.Value = 1;
                }
            };
            craftingSelectItemPanel.AddChild(closeCraftingSelectedButton);

            var craftingSelectedButton = new Button("Craft", anchor: Anchor.BottomLeft
                , size: new Vector2(250, -1), offset: new Vector2(200, 10))
            {
                Enabled = false,
                Identifier = "craftingSelectedButton",
                OnClick = (Entity entity) =>
                {
                    var itemCraftingSelected = CraftingManager.Instance.CraftingItemSelected;
                    var itemQuantity = quantitySlider.Value;
                    var craftableCount = CraftingManager.Instance.GetCraftableNumber(itemCraftingSelected.ItemId);

                    if (craftableCount != 0 && itemQuantity <= craftableCount)
                    {
                        for (int i = 0; i < itemQuantity; i++)
                            CraftingManager.Instance.CraftingItem(itemCraftingSelected.ItemId);
                    }
                    else return;

                    GeonBit.UI.Utils.MessageBox.ShowMsgBox("Crafting Item", "Item created successfully");

                    // Reset craft button
                    var craftingSelectItemPanel = UserInterface.Active.Root.Children.FirstOrDefault
                        (i => i.Identifier.Equals("craftingSelectItemPanel"));
                    var craftingSelectedButton = craftingSelectItemPanel.Children.OfType<Button>().FirstOrDefault
                        (i => i.Identifier.Equals("craftingSelectedButton"));

                    var maxQuantity = CraftingManager.Instance.GetCraftableNumber(itemCraftingSelected.ItemId);

                    if (maxQuantity > 0)
                    {
                        quantitySlider.Max = maxQuantity;
                        quantitySlider.Enabled = true;
                        craftingSelectedButton.Enabled = true;
                    }
                    else
                    {
                        quantitySlider.Max = 1;
                        quantitySlider.Enabled = false;
                        craftingSelectedButton.Enabled = false;
                    }
                }
            };
            craftingSelectItemPanel.AddChild(craftingSelectedButton);

            // CraftingPanel
            // add close button
            var closeCraftingButton = new Button("Close", anchor: Anchor.BottomRight
                , size: new Vector2(200, -1), offset: new Vector2(64, 10))
            {
                Identifier = "closeCraftingButton",
                OnClick = (Entity entity) =>
                {
                    // Closing Inventory and reset current gui panel
                    // Pause PlayScreen
                    GameGlobals.Instance.IsGamePause = !GameGlobals.Instance.IsGamePause;
                    GameGlobals.Instance.IsOpenGUI = !GameGlobals.Instance.IsOpenGUI;

                    // Toggle the IsOpenCraftingPanel flag
                    GameGlobals.Instance.IsOpenCraftingPanel = false;
                    GameGlobals.Instance.IsRefreshHotbar = false;
                    CurrentGUI = Hotbar;
                }
            };
            craftingPanel.AddChild(closeCraftingButton);

            var craftingItemButton = new Button("Craft", anchor: Anchor.BottomLeft
                , size: new Vector2(250, -1), offset: new Vector2(200, 10))
            {
                Identifier = "craftingItemButton",
                Enabled = false,
                OnClick = (Entity entity) =>
                {
                    // Set display crafting item selected & indredients
                    var itemCraftingSelected = CraftingManager.Instance.CraftingItemSelected;
                    SetCraftingItemSelectedDisplay(itemCraftingSelected);
                    SetItemIngredientDisplay(itemCraftingSelected);

                    // Set craftable quantity
                    var maxQuantity = CraftingManager.Instance.GetCraftableNumber(itemCraftingSelected.ItemId);

                    if (maxQuantity > 0)
                    {
                        quantitySlider.Max = maxQuantity;
                        quantitySlider.Enabled = true;
                        craftingSelectedButton.Enabled = true;
                    }
                    else
                    {
                        quantitySlider.Max = 1;
                        quantitySlider.Enabled = false;
                        craftingSelectedButton.Enabled = false;
                    }

                    craftingPanel.Visible = !craftingPanel.Visible;
                    craftingSelectItemPanel.Visible = !craftingSelectItemPanel.Visible;
                }
            };
            craftingPanel.AddChild(craftingItemButton);
        }

        public void InitCraftableItemDisplay()
        {
            RefreshCraftableItemDisplay(CurrentCraftingList);

            var craftingPanel = _mainPanels.ElementAt(CraftingPanel);
            var rightCraftingPanel = craftingPanel.Children?.FirstOrDefault(e => e.Identifier.Equals("rightCraftingPanel"));
            var listCraftableItemPanel = rightCraftingPanel.Children?.FirstOrDefault(e => e.Identifier.Equals("listCraftableItemPanel"));
            var craftingItemButton = craftingPanel.Children?.FirstOrDefault(e => e.Identifier.Equals("craftingItemButton"));
            var leftCraftingPanel = craftingPanel.Children?.FirstOrDefault(p => p.Identifier.Equals("leftCraftingPanel"));

            List<CraftableItemData> craftableItemList = [];

            switch (CurrentCraftingList)
            {
                case "Thai traditional medicine":
                    craftableItemList = CraftingManager.Instance.CraftableMedicineItem;
                    break;

                case "Consumable item":
                    craftableItemList = CraftingManager.Instance.CraftableFoodItem;
                    break;

                case "Equipment":
                    craftableItemList = CraftingManager.Instance.CraftableEquipmentItem;
                    break;
            }

            if (listCraftableItemPanel.Children.Count != 0)
            {
                var fisrtItem = listCraftableItemPanel.Children.OfType<Icon>().ToList().ElementAt(0);

                SetCraftableItemDisplay(fisrtItem);

                var craftingItemselected = craftableItemList.FirstOrDefault(i => i.ItemId.Equals(fisrtItem.ItemId));
                CraftingManager.Instance.CraftingItemSelected = craftingItemselected;

                // Enable or Disable the button for each item icon
                if (fisrtItem.Enabled) craftingItemButton.Enabled = true;
            }
        }

        public void RefreshCraftableItemDisplay(string craftableType)
        {
            var craftingPanel = _mainPanels.ElementAt(CraftingPanel);
            var rightCraftingPanel = craftingPanel.Children?.FirstOrDefault(e => e.Identifier.Equals("rightCraftingPanel"));
            var listCraftableItemPanel = rightCraftingPanel.Children?.FirstOrDefault(e => e.Identifier.Equals("listCraftableItemPanel"));
            var craftingItemButton = craftingPanel.Children?.FirstOrDefault(e => e.Identifier.Equals("craftingItemButton"));
            var leftCraftingPanel = craftingPanel.Children?.FirstOrDefault(p => p.Identifier.Equals("leftCraftingPanel"));

            craftingItemButton.Enabled = false;

            // Clear craftable item display
            listCraftableItemPanel.ClearChildren();

            List<CraftableItemData> craftableItemList = [];

            switch (craftableType)
            {
                case "Thai traditional medicine":
                    craftableItemList = CraftingManager.Instance.CraftableMedicineItem;
                    break;

                case "Consumable item":
                    craftableItemList = CraftingManager.Instance.CraftableFoodItem;
                    break;

                case "Equipment":
                    craftableItemList = CraftingManager.Instance.CraftableEquipmentItem;
                    break;
            }

            // Set item
            foreach (var item in craftableItemList)
            {
                // Item Icon
                var iconItem = new Icon(IconType.None, Anchor.AutoInline, 1, true)
                {
                    Enabled = item.IsCraftable,
                    ItemId = item.ItemId,
                    Texture = GameGlobals.Instance.GetItemTexture(item.ItemId),
                };

                listCraftableItemPanel.AddChild(iconItem);
            }

            foreach (var iconItem in listCraftableItemPanel.Children.OfType<Icon>())
            {
                iconItem.OnClick = (Entity entity) =>
                {
                    SetCraftableItemDisplay(iconItem);

                    var craftingItemselected = craftableItemList.FirstOrDefault(i => i.ItemId.Equals(iconItem.ItemId));
                    CraftingManager.Instance.CraftingItemSelected = craftingItemselected;              

                    // Enable or Disable the button for each item icon
                    if (iconItem.Enabled) craftingItemButton.Enabled = true;
                };
            }
        }

        private void SetCraftableItemDisplay(Icon icon)
        {
            var itemData = GameGlobals.Instance.ItemsDatas.FirstOrDefault(i => i.ItemId.Equals(icon.ItemId));

            var craftingPanel = _mainPanels.ElementAt(CraftingPanel);
            var leftCraftingPanel = craftingPanel.Children?.FirstOrDefault(p => p.Identifier.Equals("leftCraftingPanel"));
            var craftingDescriptPanel = leftCraftingPanel.Children?.FirstOrDefault(p => p.Identifier.Equals("craftingDescriptPanel"));

            // Clone the clicked icon and add it to the left panel
            var itemIcon = leftCraftingPanel.Children.OfType<Icon>().FirstOrDefault
                (i => i.Identifier.Equals("itemIcon"));
            itemIcon.Texture = icon.Texture;

            // สร้าง Label เพื่อแสดงชื่อของไอคอน
            var iconLabel = craftingDescriptPanel.Children.OfType<Label>().FirstOrDefault
                (i => i.Identifier.Equals("iconLabel"));
            iconLabel.Text = itemData.Name;

            // Description item
            var description = craftingDescriptPanel.Children.OfType<Label>().FirstOrDefault
                (i => i.Identifier.Equals("description"));
            description.Text = itemData.Description;
        }

        private void SetCraftingItemSelectedDisplay(CraftableItemData itemCrafting)
        {            
            var craftingSelectItemPanel = UserInterface.Active.Root.Children.FirstOrDefault
                (i => i.Identifier.Equals("craftingSelectItemPanel"));
            var leftCraftingSelectItemPanel = craftingSelectItemPanel.Children.FirstOrDefault
                (i => i.Identifier.Equals("leftCraftingSelectItemPanel"));

            var itemIcon = leftCraftingSelectItemPanel.Children.OfType<Icon>().FirstOrDefault
                (i => i.Identifier.Equals("itemIcon"));
            itemIcon.Texture = GameGlobals.Instance.GetItemTexture(itemCrafting.ItemId);
        }

        private void SetItemIngredientDisplay(CraftableItemData itemCrafting)
        {
            var recipeData = GameGlobals.Instance.CraftingRecipeDatas.FirstOrDefault
                (i => i.RecipeId.Equals(itemCrafting.ItemId));

            var craftingSelectItemPanel = UserInterface.Active.Root.Children.FirstOrDefault
                (i => i.Identifier.Equals("craftingSelectItemPanel"));
            var rightCraftingSelectPanel = craftingSelectItemPanel.Children.FirstOrDefault
                (i => i.Identifier.Equals("rightCraftingSelectPanel"));
            var ingredientList = rightCraftingSelectPanel.Children.OfType<SelectList>().FirstOrDefault
                (i => i.Identifier.Equals("ingredientList"));

            ingredientList.ClearItems();

            foreach (var ingredient in recipeData.Ingredients)
            {
                // Add ingredient of selected crafting item
                string ingLabel = "";

                var isItemFound = InventoryManager.Instance.InventoryBag.TryGetValue(
                    ingredient.ItemId.ToString(), out InventoryItemData itemData);

                if (isItemFound)
                {
                    if (itemData.Count < ingredient.Quantity)
                    {
                        var count = itemData.Count - ingredient.Quantity;
                        ingLabel = ingredient.Name + " x " + ingredient.Quantity + " (-" + count + ")";
                    }
                    else ingLabel = ingredient.Name + " x " + ingredient.Quantity + " (" + itemData.Count + ")";
                }
                else ingLabel = ingredient.Name + " x " + ingredient.Quantity + " (-" + ingredient.Quantity + ")";

                ingredientList.AddItem(ingLabel);
                ingredientList.IconsScale *= 1f;
                ingredientList.SetIcon(GameGlobals.Instance.GetItemTexture(ingredient.ItemId), ingLabel);
            }
        }      

        public static GUIManager Instance
        {
            get
            {
                instance ??= new GUIManager();
                return instance;
            }
        }
    }
}
