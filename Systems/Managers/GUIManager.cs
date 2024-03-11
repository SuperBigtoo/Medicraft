using GeonBit.UI;
using GeonBit.UI.Entities;
using GeonBit.UI.Utils.Forms;
using Medicraft.Data.Models;
using Medicraft.GameObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using MonoGame.Extended.Sprites;
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
        public const int InspectPanel = 3;

        public string CurrentCraftingList { get; set; }

        public const string ThaiTraditionalMedicine = "Thai traditional medicine";
        public const string ConsumableItem = "Consumable item";
        public const string Equipment = "Equipment";

        public bool IsCharacterTabSelected { get; set; } = false;
        public bool IsShowConfirmBox { get; set; } = false;
        public AnimatedSprite PlayerSprite { get; set; }

        // Skill selected
        private string _noahSkillSelected; 

        // UI elements
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

            // Inventory = 1
            InitInventoryUI();

            // Crafting = 2
            InitCraftingUI();

            // Inspect = 3
            InitInspectUI();

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
            var inventoryPanel = new Panel(new Vector2(1200, 650), PanelSkin.Fancy, Anchor.Center)
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
                Scale = 1.25f,
                Identifier = "iconLabel"
            });
            invenDescriptPanel.AddChild(new Paragraph("description", Anchor.AutoInline)
            {
                //Size = new Vector2(380, 200), // กำหนดขนาดของ Label
                Scale = 1f,
                WrapWords = true,
                Identifier = "description"
            });
            invenDescriptPanel.AddChild(new LineSpace(1));

            leftInvenPanel.AddChild(new Icon(IconType.None, Anchor.AutoCenter, 1, false)
            {
                Size = new Vector2(300, 300),   // ปรับขนาดของไอคอน
                Locked = true,
                Identifier = "itemIcon" 
            });
            leftInvenPanel.AddChild(new LineSpace(1));
            leftInvenPanel.AddChild(invenDescriptPanel);

            // สร้าง Panel สำหรับฝั่งขวา
            var rightInvenPanel = new Panel(new Vector2(600, 600), PanelSkin.None, Anchor.TopRight)
            {
                Identifier = "invenRightPanel"
            };
            inventoryPanel.AddChild(rightInvenPanel);

            rightInvenPanel.AddChild(new Header("IVENTORY"));
            rightInvenPanel.AddChild(new LineSpace(1));

            var listItemPanel = new Panel(new Vector2(550, 450), PanelSkin.ListBackground, Anchor.AutoCenter)
            {
                PanelOverflowBehavior = PanelOverflowBehavior.VerticalScroll,
                Identifier = "listItemPanel",
                Padding = new Vector2(15, 15)
            };
            listItemPanel.Scrollbar.AdjustMaxAutomatically = true;
            rightInvenPanel.AddChild(listItemPanel);

            // add close button
            var closeInventoryButton = new Button("Close", anchor: Anchor.BottomRight
                , size: new Vector2(200, -1), offset: new Vector2(64, -100))
            {
                Identifier = "closeInventoryButton",
                Skin = ButtonSkin.Fancy,
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
                Skin = ButtonSkin.Fancy
            };
            inventoryPanel.AddChild(useInvenItemButton);         

            offsetX += 216;
            var invenSetHotbarButton = new Button("Setup Hotbar", anchor: Anchor.BottomLeft
                , size: new Vector2(200, -1), offset: new Vector2(offsetX, -100))
            {
                Identifier = "invenSetHotbarButton",
                Enabled = false,
                Skin = ButtonSkin.Fancy 
            };
            inventoryPanel.AddChild(invenSetHotbarButton);

            // Set on Click use item in inventory
            useInvenItemButton.OnClick = (Entity entity) =>
            {
                var currItemInv = InventoryManager.Instance.InventoryItemSelected;

                string notifyText = string.Empty;
                if (currItemInv.GetCategory().Equals("Equipment"))
                {
                    notifyText = $"Do you wanna equip '{currItemInv.GetName()}'?";
                }
                else notifyText = $"Do you wanna use '{currItemInv.GetName()}'?";

                GeonBit.UI.Utils.MessageBox.ShowMsgBox("Use Item?"
                    , notifyText
                    , new GeonBit.UI.Utils.MessageBox.MsgBoxOption[]
                    {
                            new("Ok", () =>
                            {
                                // Use selected item from inventory
                                InventoryManager.Instance.UseItem(currItemInv);

                                InventoryManager.Instance.InventoryItemSelected = null;
                                useInvenItemButton.Enabled = false;
                                invenSetHotbarButton.Enabled = false;
                                return true;
                            }),
                            new("Cancel", () =>
                            {
                                return true;
                            })
                    });
            };

            // Set on Click set Hotbar item
            invenSetHotbarButton.OnClick = (Entity entity) =>
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
                    , onDone: () =>
                    {
                        if (hotbarSetupForm.GetValue("dropdown_setup_hotbar") != null)
                        {
                            GeonBit.UI.Utils.MessageBox.ShowMsgBox("Setup Hotbar Successfully"
                                , $"Set '{currItemInv.GetName()}' on '{hotbarSetupForm.GetValueString("dropdown_setup_hotbar")}'");

                            var output = hotbarSetupForm.GetValueString("dropdown_setup_hotbar");
                            var slotNum = InventoryManager.Instance.GetIHotbarSlot(output);
                            InventoryManager.Instance.SetHotbarItem(currItemInv, slotNum);
                        }
                        else
                        {
                            GeonBit.UI.Utils.MessageBox.ShowMsgBox("Setup Complete", $"Ya didn't select a for '{currItemInv.GetName()}'");
                            GUIManager.Instance.RefreshInvenrotyItemDisplay(true);
                        }

                        InventoryManager.Instance.InventoryItemSelected = null;
                        useInvenItemButton.Enabled = false;
                        invenSetHotbarButton.Enabled = false;
                    });
            };
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
                    else
                    {
                        // In case selected item is Equipment
                        invenSetHotbarButton.Enabled = false;

                        if (!item.Slot.Equals(GameGlobals.Instance.DefaultInventorySlot))
                            invenUseItemButton.Enabled = false;
                    }
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
                        else
                        {
                            // In case selected item is Equipment
                            invenSetHotbarButton.Enabled = false;

                            if (!item.Slot.Equals(GameGlobals.Instance.DefaultInventorySlot))
                                invenUseItemButton.Enabled = false;
                        }
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
            var description = invenDescriptPanel.Children.OfType<Paragraph>().FirstOrDefault
                (i => i.Identifier.Equals("description"));
            description.Text = itemData.Description;                 
        }


        /// <summary>
        /// Crafting Panel, Display list of craftable items that play unlocked
        /// </summary>
        private void InitCraftingUI()
        {
            // สร้าง Panel หลัก
            var craftingPanel = new Panel(new Vector2(1400, 710), PanelSkin.Fancy, Anchor.Center)
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
            craftingDescriptPanel.AddChild(new Paragraph("description", Anchor.AutoInline)
            {
                //Size = new Vector2(380, 200), // กำหนดขนาดของ Label
                Scale = 1f,
                WrapWords = true,
                Identifier = "description"
            });
            craftingDescriptPanel.AddChild(new LineSpace(1));

            leftCraftingPanel.AddChild(new Icon(IconType.None, Anchor.AutoCenter, 1, false)
            {
                Size = new Vector2(300, 300),   // ปรับขนาดของไอคอน
                Locked = true,
                Identifier = "itemIcon"
            });
            leftCraftingPanel.AddChild(new LineSpace(1));
            leftCraftingPanel.AddChild(craftingDescriptPanel);

            // สร้าง Panel สำหรับฝั่งขวา
            var rightCraftingPanel = new Panel(new Vector2(650, 660), PanelSkin.None, Anchor.TopRight)
            {
                Identifier = "rightCraftingPanel"
            };
            craftingPanel.AddChild(rightCraftingPanel);

            rightCraftingPanel.AddChild(new Header("CRAFTING ITEM LIST"));
            rightCraftingPanel.AddChild(new LineSpace(1));

            var listCraftableItemPanel = new Panel(new Vector2(550, 530), PanelSkin.ListBackground, Anchor.AutoCenter)
            {
                PanelOverflowBehavior = PanelOverflowBehavior.VerticalScroll,
                Identifier = "listCraftableItemPanel",
                Padding = new Vector2(15, 15)
            };
            listCraftableItemPanel.Scrollbar.AdjustMaxAutomatically = true;
            rightCraftingPanel.AddChild(listCraftableItemPanel);

            // Crafting Selected Item
            // Panel
            var craftingSelectItemPanel = new Panel(new Vector2(1400, 710), PanelSkin.Default, Anchor.Center)
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
            rightCraftingSelectPanel.AddChild(new LineSpace(1));

            var ingredientList = new SelectList(new Vector2(550, 530), Anchor.AutoCenter, skin: PanelSkin.ListBackground)
            {
                Identifier = "ingredientList",
                Padding = new Vector2(40, 16),
                LockSelection = true
            };         

            rightCraftingSelectPanel.AddChild(ingredientList);

            var closeCraftingSelectedButton = new Button("Back", anchor: Anchor.BottomRight
                , size: new Vector2(200, -1), offset: new Vector2(64, -100))
            {
                Identifier = "closeCraftingSelectButton",
                Skin = ButtonSkin.Fancy,
                OnClick = (Entity entity) =>
                {
                    craftingPanel.Visible = !craftingPanel.Visible;
                    craftingSelectItemPanel.Visible = !craftingSelectItemPanel.Visible;

                    quantitySlider.Value = 1;
                }
            };
            craftingSelectItemPanel.AddChild(closeCraftingSelectedButton);

            var craftingSelectedButton = new Button("Craft", anchor: Anchor.BottomLeft
                , size: new Vector2(250, -1), offset: new Vector2(200, -100))
            {
                Enabled = false,
                Identifier = "craftingSelectedButton",
                Skin = ButtonSkin.Fancy,
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
                , size: new Vector2(200, -1), offset: new Vector2(64, -100))
            {
                Identifier = "closeCraftingButton",
                Skin = ButtonSkin.Fancy,
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
                , size: new Vector2(250, -1), offset: new Vector2(200, -100))
            {
                Identifier = "craftingItemButton",
                Skin = ButtonSkin.Fancy,
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
            var description = craftingDescriptPanel.Children.OfType<Paragraph>().FirstOrDefault
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

        public void InitInspectUI()
        {
            // Main Panel
            var inspectPanel = new Panel(new Vector2(900, 710), PanelSkin.None, Anchor.Center)
            {
                Visible = true,
                Identifier = "inspectPanel"
            };
            _mainPanels.Add(inspectPanel);
            UserInterface.Active.AddEntity(inspectPanel);

            // create panel tabs
            var inspectTabs = new PanelTabs()
            {
                Identifier = "inspectTabs",
                BackgroundSkin = PanelSkin.Fancy
            };
            inspectPanel.AddChild(inspectTabs);

            // Add tab: Character Inspect
            {            
                TabData characterTab = inspectTabs.AddTab("Character");

                // Set true if character tab is clicked
                characterTab.button.OnClick = (Entity entity) =>
                {
                    RefreshInspectCharacterDisply();
                    ClearSkillDescription();
                    
                    IsCharacterTabSelected = true;
                };

                // Left Panel
                var leftPanel = new Panel(new Vector2(450, 670), PanelSkin.None, Anchor.TopLeft)
                {
                    Identifier = "leftPanel"
                };
                characterTab.panel.AddChild(leftPanel);

                // Display Character
                var displayCharacterPanel = new Panel(new Vector2(425, 400), PanelSkin.ListBackground, Anchor.TopCenter)
                {
                    Identifier = "displayCharacterPanel"
                };
                leftPanel.AddChild(displayCharacterPanel);

                displayCharacterPanel.AddChild(new Header()
                {                
                    Identifier = "characterNameHeader",
                    Scale = 1.5f,
                    Anchor = Anchor.TopCenter
                });

                // Left Equipment Panel
                var leftEquipmentPanel = new Panel(new Vector2(70, 210), PanelSkin.None, Anchor.CenterLeft)
                {
                    Identifier = "leftEquipmentPanel",
                };
                displayCharacterPanel.AddChild(leftEquipmentPanel);

                // Add Slot Equipment in Left Panel
                for (int i = 0; i < 3; i++)
                {
                    leftEquipmentPanel.AddChild(new Icon(IconType.None, Anchor.AutoInline, 1, true)
                    {
                        Identifier = "Slot_" + i,
                        Offset = new Vector2(-16, 0)
                    });
                }

                // Right Equipment Panel
                var rightEquipmentPanel = new Panel(new Vector2(70, 210), PanelSkin.None, Anchor.CenterRight)
                {
                    Identifier = "rightEquipmentPanel"
                };
                displayCharacterPanel.AddChild(rightEquipmentPanel);

                // Add Slot Equipment in Right Panel
                for (int i = 3; i < 6; i++)
                {
                    rightEquipmentPanel.AddChild(new Icon(IconType.None, Anchor.AutoInline, 1, true)
                    {
                        Identifier = "Slot_" + i,
                        Offset = new Vector2(-16, 0)
                    });
                }

                // Skill Icon Panel
                var skillIconPanel = new Panel(new Vector2(220, 60), PanelSkin.None, Anchor.BottomCenter)
                {
                    Identifier = "skillIconPanel"
                };
                displayCharacterPanel.AddChild(skillIconPanel); 

                leftPanel.AddChild(new LineSpace(1));
                var selectedSkillPanel = new Panel(new Vector2(820, 210), PanelSkin.ListBackground, Anchor.AutoCenter)
                {
                    Identifier = "selectedSkillPanel",
                    Offset = new Vector2((800 / 4) - 5, 0f)
                };
                leftPanel.AddChild(selectedSkillPanel);

                var descripLeftSkill = new Paragraph(@"", Anchor.CenterLeft, Color.White, 0.9f, new Vector2(280, 200))
                {
                    Identifier = "descripLeftSkill",
                    WrapWords = true
                };
                selectedSkillPanel.AddChild(descripLeftSkill);

                var upSkillButton = new Button("> Lv.Up >", ButtonSkin.Alternative, size: new Vector2(140, 40))
                {
                    Identifier = "upSkillButton",
                    ToolTipText = "Use a 'Skill Point' to up level skill",
                    Enabled = false,
                    Anchor = Anchor.Center,                  
                };
                selectedSkillPanel.AddChild(upSkillButton);

                var descripRightSkill = new Paragraph(@"", Anchor.CenterRight, Color.White, 0.9f, new Vector2(280, 200))
                {
                    Identifier = "descripRightSkill",
                    WrapWords = true
                };
                selectedSkillPanel.AddChild(descripRightSkill);

                var normalSkillIcon = new Icon(IconType.None, Anchor.AutoInlineNoBreak, 1, true)
                {
                    Identifier = "normalSkillIcon",
                    ToolTipText = "Normal Skill",
                    Offset = new Vector2(0, 0)
                };
                skillIconPanel.AddChild(normalSkillIcon);
                normalSkillIcon.AddChild(new Label($"", Anchor.BottomRight, offset: new Vector2(-22, -35))
                {
                    Identifier = "normalSkillLevel",
                    Size = new Vector2(10, 10),
                    Scale = 1f,
                    ClickThrough = true,
                });
                var burstSkillIcon = new Icon(IconType.None, Anchor.AutoInlineNoBreak, 1, true)
                {
                    Identifier = "burstSkillIcon",
                    ToolTipText = "Burst Skill",
                    Offset = new Vector2(0, 0)
                };
                skillIconPanel.AddChild(burstSkillIcon);
                burstSkillIcon.AddChild(new Label($"", Anchor.BottomRight, offset: new Vector2(-22, -35))
                {
                    Identifier = "burstSkillLevel",
                    Size = new Vector2(10, 10),
                    Scale = 1f,
                    ClickThrough = true,
                });
                var passiveSkillIcon = new Icon(IconType.None, Anchor.AutoInlineNoBreak, 1, true)
                {
                    Identifier = "passiveSkillIcon",
                    ToolTipText = "Passive Skill",
                    Offset = new Vector2(0, 0)
                };
                skillIconPanel.AddChild(passiveSkillIcon);
                passiveSkillIcon.AddChild(new Label($"", Anchor.BottomRight, offset: new Vector2(-22, -35))
                {
                    Identifier = "passiveSkillLevel",
                    Size = new Vector2(10, 10),
                    Scale = 1f,
                    ClickThrough = true,
                });

                normalSkillIcon.OnClick = (Entity entity) =>
                {
                    entity.Enabled = false;
                    burstSkillIcon.Enabled = true;
                    passiveSkillIcon.Enabled = true;

                    _noahSkillSelected = "I've got the Scent!";

                    var descNormal = GameGlobals.Instance.SkillDescriptionDatas.Where(s => s.Name.Equals(_noahSkillSelected)).ToList();
                    var levelNormal = PlayerManager.Instance.Player.PlayerData.Abilities.NormalSkillLevel;

                    // Refesh skill description
                    RefreshSkillDescription(descNormal, levelNormal);
                };

                burstSkillIcon.OnClick = (Entity entity) =>
                {
                    entity.Enabled = false;
                    normalSkillIcon.Enabled = true;
                    passiveSkillIcon.Enabled = true;

                    _noahSkillSelected = "Noah Strike";

                    var descBurst = GameGlobals.Instance.SkillDescriptionDatas.Where(s => s.Name.Equals(_noahSkillSelected)).ToList();
                    var levelBurst = PlayerManager.Instance.Player.PlayerData.Abilities.BurstSkillLevel;

                    // Refesh skill description
                    RefreshSkillDescription(descBurst, levelBurst);
                };

                passiveSkillIcon.OnClick = (Entity entity) =>
                {
                    entity.Enabled = false;
                    normalSkillIcon.Enabled = true;
                    burstSkillIcon.Enabled = true;

                    _noahSkillSelected = "Survivalist";

                    var descPassive = GameGlobals.Instance.SkillDescriptionDatas.Where(s => s.Name.Equals(_noahSkillSelected)).ToList();
                    var levelPassive = PlayerManager.Instance.Player.PlayerData.Abilities.PassiveSkillLevel;

                    // Refesh skill description
                    RefreshSkillDescription(descPassive, levelPassive);
                };

                upSkillButton.OnClick = (Entity entity) =>
                {
                    IsShowConfirmBox = true;
                    if (PlayerManager.Instance.Player.PlayerData.SkillPoint > 0)
                    {
                        GeonBit.UI.Utils.MessageBox.ShowMsgBox("Up Level Skill"
                            , $"Do you want to level up the skill '{_noahSkillSelected}'?"
                            , new GeonBit.UI.Utils.MessageBox.MsgBoxOption[]
                            {
                                new("Ok", () =>
                                {
                                    // Do up level skill
                                    var isUpLevelSucc = PlayerManager.Instance.Player.UpSkillLevle(_noahSkillSelected);

                                    if (isUpLevelSucc)
                                    {
                                        GeonBit.UI.Utils.MessageBox.ShowMsgBox("LEVEL UP!!", $"'{_noahSkillSelected}' was successfully up level."
                                            , onDone: () =>
                                            {                                                                                       
                                                // Refesh skill description
                                                var descSkill = GameGlobals.Instance.SkillDescriptionDatas.Where(s => s.Name.Equals(_noahSkillSelected)).ToList();
                                                int level = 1;

                                                if (normalSkillIcon.Enabled == false)
                                                {
                                                    level = PlayerManager.Instance.Player.PlayerData.Abilities.NormalSkillLevel;
                                                }
                                                else if (burstSkillIcon.Enabled == false)
                                                {
                                                    level = PlayerManager.Instance.Player.PlayerData.Abilities.BurstSkillLevel;
                                                }
                                                else if (passiveSkillIcon.Enabled == false)
                                                {
                                                    level = PlayerManager.Instance.Player.PlayerData.Abilities.PassiveSkillLevel;
                                                }

                                                RefreshSkillDescription(descSkill, level);
                                                IsShowConfirmBox = false;
                                            });
                                        return true;
                                    }
                                    else
                                    {
                                        int capLevel = 0;
                                        switch (_noahSkillSelected)
                                        {
                                            case "I've got the Scent!":
                                                capLevel = PlayerManager.Instance.Player.PlayerData.Abilities.NormalSkillLevel == 3? 11 : 21;
                                                break;

                                            case "Noah Strike":
                                                capLevel = PlayerManager.Instance.Player.PlayerData.Abilities.BurstSkillLevel == 3? 11 : 21;
                                                break;

                                            case "Survivalist":
                                                capLevel = PlayerManager.Instance.Player.PlayerData.Abilities.PassiveSkillLevel == 3? 11 : 21;
                                                break;
                                        }

                                        GeonBit.UI.Utils.MessageBox.ShowMsgBox("ERROR!!", $"Character need to reach level {capLevel} to level up skills."
                                            , onDone: () => 
                                            {
                                                IsShowConfirmBox = false;                                                      
                                            });
                                        return true;
                                    }                                       
                                }),
                                new("Cancel", () =>
                                {
                                    IsShowConfirmBox = false;
                                    return true;
                                })
                            });
                    }
                    else
                    {
                        GeonBit.UI.Utils.MessageBox.ShowMsgBox("Not Enough Skill Point!", "It seems that ya don't have enough 'Skill Point' for this huhh?, go get some level up."
                            , onDone: () => { IsShowConfirmBox = false; } );
                    }
                };

                // Right Panel
                var rightPanel = new Panel(new Vector2(375, 600), PanelSkin.None, Anchor.TopRight)
                {
                    Identifier = "rightPanel"
                };
                characterTab.panel.AddChild(rightPanel);

                var statsPanel = new Panel(new Vector2(350, 400), PanelSkin.ListBackground, Anchor.TopCenter)
                {
                    Identifier = "statsPanel"
                };
                rightPanel.AddChild(statsPanel);

                statsPanel.AddChild(new Button("Level", ButtonSkin.Fancy, size: new Vector2(325, 30))
                {
                    Identifier = "levelHeader",
                    Locked = true,
                    Anchor = Anchor.TopCenter
                });

                // Player Level
                statsPanel.AddChild(new Label()
                {
                    Identifier = "playerLevel",
                    Scale = 1.5f,
                    Anchor = Anchor.AutoCenter
                });

                statsPanel.AddChild(new Button("Attributes", ButtonSkin.Fancy, size: new Vector2(325, 30))
                {
                    Identifier = "attributesHeader",
                    Locked = true,
                    Anchor = Anchor.AutoCenter
                });

                // ATK
                var statsATKPanel = new Panel(new Vector2(300, 15), PanelSkin.None, Anchor.AutoCenter)
                {
                    Identifier = "statsATKPanel",
                };
                statsPanel.AddChild(statsATKPanel);
                statsATKPanel.AddChild(new Label()
                {
                    Text = "ATK:",
                    Scale = 0.9f,
                    Anchor = Anchor.CenterLeft
                });
                statsATKPanel.AddChild(new Label()
                {
                    Identifier = "statsATK",
                    Scale = 0.9f,
                    Anchor = Anchor.CenterRight
                });
                
                // HP
                var statsHPPanel = new Panel(new Vector2(300, 15), PanelSkin.None, Anchor.AutoCenter)
                {
                    Identifier = "statsHPPanel",
                };
                statsPanel.AddChild(statsHPPanel);
                statsHPPanel.AddChild(new Label()
                {
                    Text = "HP:",
                    Scale = 0.9f,
                    Anchor = Anchor.CenterLeft
                });
                statsHPPanel.AddChild(new Label()
                {
                    Identifier = "statsHP",
                    Scale = 0.9f,
                    Anchor = Anchor.CenterRight
                });

                // Mana
                var statsManaPanel = new Panel(new Vector2(300, 15), PanelSkin.None, Anchor.AutoCenter)
                {
                    Identifier = "statsManaPanel",
                };
                statsPanel.AddChild(statsManaPanel);
                statsManaPanel.AddChild(new Label()
                {
                    Text = "Mana:",
                    Scale = 0.9f,
                    Anchor = Anchor.CenterLeft
                });
                statsManaPanel.AddChild(new Label()
                {
                    Identifier = "statsMana",
                    Scale = 0.9f,
                    Anchor = Anchor.CenterRight
                });

                // ManaRegen
                var statsManaRegenPanel = new Panel(new Vector2(300, 15), PanelSkin.None, Anchor.AutoCenter)
                {
                    Identifier = "statsManaRegenPanel",
                };
                statsPanel.AddChild(statsManaRegenPanel);
                statsManaRegenPanel.AddChild(new Label()
                {
                    Text = "Mana Regen:",
                    Scale = 0.9f,
                    Anchor = Anchor.CenterLeft
                });
                statsManaRegenPanel.AddChild(new Label()
                {
                    Identifier = "statsManaRegen",
                    Scale = 0.9f,
                    Anchor = Anchor.CenterRight
                });

                // DEF
                var statsDEFPanel = new Panel(new Vector2(300, 15), PanelSkin.None, Anchor.AutoCenter)
                {
                    Identifier = "statsDEFPanel",
                };
                statsPanel.AddChild(statsDEFPanel);
                statsDEFPanel.AddChild(new Label()
                {
                    Text = "DEF:",
                    Scale = 0.9f,
                    Anchor = Anchor.CenterLeft
                });
                statsDEFPanel.AddChild(new Label()
                {
                    Identifier = "statsDEF",
                    Scale = 0.9f,
                    Anchor = Anchor.CenterRight
                });

                // Crit
                var statsCritPanel = new Panel(new Vector2(300, 15), PanelSkin.None, Anchor.AutoCenter)
                {
                    Identifier = "statsCritPanel",
                };
                statsPanel.AddChild(statsCritPanel);
                statsCritPanel.AddChild(new Label()
                {
                    Text = "Crit:",
                    Scale = 0.9f,
                    Anchor = Anchor.CenterLeft
                });
                statsCritPanel.AddChild(new Label()
                {
                    Identifier = "statsCrit",
                    Scale = 0.9f,
                    Anchor = Anchor.CenterRight
                });

                // CritDMG
                var statsCritDMGPanel = new Panel(new Vector2(300, 15), PanelSkin.None, Anchor.AutoCenter)
                {
                    Identifier = "statsCritDMGPanel",
                };
                statsPanel.AddChild(statsCritDMGPanel);
                statsCritDMGPanel.AddChild(new Label()
                {
                    Text = "CritDMG:",
                    Scale = 0.9f,
                    Anchor = Anchor.CenterLeft
                });
                statsCritDMGPanel.AddChild(new Label()
                {
                    Identifier = "statsCritDMG",
                    Scale = 0.9f,
                    Anchor = Anchor.CenterRight
                });

                // Evasion
                var statsEvasionPanel = new Panel(new Vector2(300, 15), PanelSkin.None, Anchor.AutoCenter)
                {
                    Identifier = "statsEvasionPanel",
                };
                statsPanel.AddChild(statsEvasionPanel);
                statsEvasionPanel.AddChild(new Label()
                {
                    Text = "Evasion:",
                    Scale = 0.9f,
                    Anchor = Anchor.CenterLeft
                });
                statsEvasionPanel.AddChild(new Label()
                {
                    Identifier = "statsEvasion",
                    Scale = 0.9f,
                    Anchor = Anchor.CenterRight
                });

                // Speed
                var statsSpeedPanel = new Panel(new Vector2(300, 15), PanelSkin.None, Anchor.AutoCenter)
                {
                    Identifier = "statsSpeedPanel",
                };
                statsPanel.AddChild(statsSpeedPanel);
                statsSpeedPanel.AddChild(new Label()
                {
                    Text = "Movement Speed:",
                    Scale = 0.9f,
                    Anchor = Anchor.CenterLeft
                });
                statsSpeedPanel.AddChild(new Label()
                {
                    Identifier = "statsSpeed",
                    Scale = 0.9f,
                    Anchor = Anchor.CenterRight
                });
            }

            // Add tab: Progression
            {
                TabData progressionTab = inspectTabs.AddTab("Progression");

                progressionTab.button.OnClick = (Entity entity) =>
                {
                    RefreshMedicineProgresstion();

                    IsCharacterTabSelected = false;
                };

                progressionTab.panel.AddChild(new Header("Thai Traditional Medicine")
                {
                    Anchor = Anchor.TopCenter
                });
                progressionTab.panel.AddChild(new LineSpace(1) { Anchor = Anchor.AutoCenter });

                // left Panel
                var leftPanel = new Panel(new Vector2(625, 600), PanelSkin.ListBackground, Anchor.BottomLeft)
                {
                    PanelOverflowBehavior = PanelOverflowBehavior.VerticalScroll,
                    Identifier = "leftPanel"
                };
                leftPanel.Scrollbar.AdjustMaxAutomatically = true;
                progressionTab.panel.AddChild(leftPanel);

                leftPanel.AddChild(new Icon(IconType.None, Anchor.AutoCenter, 1, false)
                {
                    Size = new Vector2(300, 300),   // ปรับขนาดของไอคอน
                    Locked = true,
                    Identifier = "itemIcon"
                });
                leftPanel.AddChild(new LineSpace(1));
               
                // Thai medicine name
                leftPanel.AddChild(new Label("Name", Anchor.AutoCenter)
                {
                    Scale = 1.3f,
                    Identifier = "Name"
                });
                leftPanel.AddChild(new HorizontalLine());

                // Group
                leftPanel.AddChild(new Label("กลุ่มยา", Anchor.AutoInline) { Scale = 1.1f });
                leftPanel.AddChild(new Paragraph("Group", Anchor.AutoInline)
                {
                    Scale = 1f,
                    WrapWords = true,
                    Identifier = "Group"
                });
                leftPanel.AddChild(new LineSpace(1));

                // Characteristics
                leftPanel.AddChild(new Label("ลักษณะยา", Anchor.AutoInline) { Scale = 1.1f });
                leftPanel.AddChild(new Paragraph("Characteristics", Anchor.AutoInline)
                {
                    Scale = 1f,
                    WrapWords = true,
                    Identifier = "Characteristics"
                });
                leftPanel.AddChild(new LineSpace(1));

                // Recipe
                leftPanel.AddChild(new Label("สูตรตำรับ", Anchor.AutoInline) { Scale = 1.1f });
                leftPanel.AddChild(new Paragraph("Recipe", Anchor.AutoInline)
                {
                    Scale = 1f,
                    WrapWords = true,
                    Identifier = "Recipe"
                });
                leftPanel.AddChild(new LineSpace(1));

                // Instructions
                leftPanel.AddChild(new Label("คำแนะนำ", Anchor.AutoInline) { Scale = 1.1f });
                leftPanel.AddChild(new Paragraph("Instructions", Anchor.AutoInline)
                {
                    Scale = 1f,
                    WrapWords = true,
                    Identifier = "Instructions"
                });
                leftPanel.AddChild(new LineSpace(1));

                // HowtoUse
                leftPanel.AddChild(new Label("ขนาดและวิธีใช้", Anchor.AutoInline) { Scale = 1.1f });
                leftPanel.AddChild(new Paragraph("HowtoUse", Anchor.AutoInline)
                {
                    Scale = 1f,
                    WrapWords = true,
                    Identifier = "HowtoUse"
                });
                leftPanel.AddChild(new LineSpace(1));

                // Warning
                leftPanel.AddChild(new Label("คำเตือน", Anchor.AutoInline) { Scale = 1.1f });
                leftPanel.AddChild(new Paragraph("Warning", Anchor.AutoInline)
                {
                    Scale = 1f,
                    WrapWords = true,
                    Identifier = "Warning"
                });
                leftPanel.AddChild(new LineSpace(1));

                var listCraftableMedicine = new Panel(new Vector2(185, 600), PanelSkin.ListBackground, Anchor.BottomRight)
                {
                    PanelOverflowBehavior = PanelOverflowBehavior.VerticalScroll,
                    Identifier = "listCraftableMedicine",
                    Padding = new Vector2(18, 18)
                };
                listCraftableMedicine.Scrollbar.AdjustMaxAutomatically = true;
                progressionTab.panel.AddChild(listCraftableMedicine);
            }
        }

        public void RefreshMedicineProgresstion()
        {
            var inspectPanel = _mainPanels.ElementAt(InspectPanel);
            var inspectTabs = inspectPanel.Children.OfType<PanelTabs>().FirstOrDefault(t => t.Identifier.Equals("inspectTabs"));
            inspectTabs.SelectTab("Progression");
            var currentTabPanel = inspectTabs.ActiveTab.panel;
            var leftPanel = currentTabPanel.Children.FirstOrDefault(p => p.Identifier.Equals("leftPanel"));
            var listCraftableMedicine = currentTabPanel.Children.FirstOrDefault(p => p.Identifier.Equals("listCraftableMedicine"));

            // Clear craftable item display
            listCraftableMedicine.ClearChildren();

            List<CraftableItemData> craftableItemList = CraftingManager.Instance.CraftableMedicineItem;

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

                listCraftableMedicine.AddChild(iconItem);

                iconItem.OnClick = (Entity entity) =>
                {
                    SetMedicineDescriptionDisplay(iconItem);
                };
            }
        }

        private void SetMedicineDescriptionDisplay(Icon icon)
        {
            var itemData = GameGlobals.Instance.MedicineDescriptionDatas.FirstOrDefault(i => i.ItemId.Equals(icon.ItemId));

            var inspectPanel = _mainPanels.ElementAt(InspectPanel);
            var inspectTabs = inspectPanel.Children.OfType<PanelTabs>().FirstOrDefault(t => t.Identifier.Equals("inspectTabs"));
            inspectTabs.SelectTab("Progression");
            var currentTabPanel = inspectTabs.ActiveTab.panel;
            var leftPanel = currentTabPanel.Children.FirstOrDefault(p => p.Identifier.Equals("leftPanel"));

            var itemIcon = leftPanel.Children.OfType<Icon>().FirstOrDefault
                (i => i.Identifier.Equals("itemIcon"));
            itemIcon.Texture = icon.Texture;

            // Thai medicine name
            var Name = leftPanel.Children.OfType<Label>().FirstOrDefault
                (i => i.Identifier.Equals("Name"));
            Name.Text = itemData.Name;

            // Group
            var Group = leftPanel.Children.OfType<Paragraph>().FirstOrDefault
                (i => i.Identifier.Equals("Group"));
            Group.Text = itemData.Group;

            // Characteristics
            var Characteristics = leftPanel.Children.OfType<Paragraph>().FirstOrDefault
                (i => i.Identifier.Equals("Characteristics"));
            Characteristics.Text = itemData.Characteristics;

            // Recipe
            var Recipe = leftPanel.Children.OfType<Paragraph>().FirstOrDefault
                (i => i.Identifier.Equals("Recipe"));
            Recipe.Text = itemData.Recipe;

            // Instructions
            var Instructions = leftPanel.Children.OfType<Paragraph>().FirstOrDefault
                (i => i.Identifier.Equals("Instructions"));
            Instructions.Text = itemData.Instructions;

            // HowtoUse
            var HowtoUse = leftPanel.Children.OfType<Paragraph>().FirstOrDefault
                (i => i.Identifier.Equals("HowtoUse"));
            HowtoUse.Text = itemData.HowtoUse;

            // Warning
            var Warning = leftPanel.Children.OfType<Paragraph>().FirstOrDefault
                (i => i.Identifier.Equals("Warning"));
            Warning.Text = itemData.Warning;
        }

        public void RefreshInspectCharacterDisply()
        {
            var inspectPanel = _mainPanels.ElementAt(InspectPanel);
            var inspectTabs = inspectPanel.Children.OfType<PanelTabs>().FirstOrDefault(t => t.Identifier.Equals("inspectTabs"));
            inspectTabs.SelectTab("Character");
            var currentTabPanel = inspectTabs.ActiveTab.panel;
            var leftPanel = currentTabPanel.Children.FirstOrDefault(p => p.Identifier.Equals("leftPanel"));
            var displayCharacterPanel = leftPanel.Children.FirstOrDefault(p => p.Identifier.Equals("displayCharacterPanel"));
            var leftEquipmentPanel = displayCharacterPanel.Children.FirstOrDefault(p => p.Identifier.Equals("leftEquipmentPanel"));
            var rightEquipmentPanel = displayCharacterPanel.Children.FirstOrDefault(p => p.Identifier.Equals("rightEquipmentPanel"));          

            // Set Main Character Name
            var characterNameHeader = displayCharacterPanel.Children.OfType<Header>().FirstOrDefault
                (h => h.Identifier.Equals("characterNameHeader"));
            characterNameHeader.Text = PlayerManager.Instance.Player.Name;

            // Clear Equipment Slot
            leftEquipmentPanel.ClearChildren();
            rightEquipmentPanel.ClearChildren();

            // Equipment Slot
            for (int i = 0; i < 6; i++)
            {
                var itemEquipmentData = InventoryManager.Instance.InventoryBag.Values.FirstOrDefault(e => e.Slot.Equals(i));
                Icon iconEquipmentSlot;

                if (itemEquipmentData != null)
                {
                    iconEquipmentSlot = new Icon(IconType.None, Anchor.AutoInline, 1, true)
                    {
                        Identifier = "Slot_" + i,
                        ItemId = itemEquipmentData.ItemId,
                        Slot = itemEquipmentData.Slot,
                        Texture = GameGlobals.Instance.GetItemTexture(itemEquipmentData.ItemId),
                        ToolTipText = "Click to unequip",
                        Locked = false,
                        Offset = new Vector2(-16, 0),
                    };

                    iconEquipmentSlot.OnClick = (Entity entity) =>
                    {
                        IsShowConfirmBox = true;

                        InventoryManager.Instance.InventoryBag.TryGetValue(iconEquipmentSlot.ItemId.ToString()
                            , out InventoryItemData item);

                        GeonBit.UI.Utils.MessageBox.ShowMsgBox("Use Item?"
                            , $"Do ya wanna unequip '{item.GetName()}'"
                            , new GeonBit.UI.Utils.MessageBox.MsgBoxOption[]
                            {
                                    new("Ok", () =>
                                    {
                                        // Use selected item from inventory
                                        InventoryManager.Instance.UnEquip(item);

                                        RefreshInspectCharacterDisply();

                                        IsShowConfirmBox = false;
                                        return true;
                                    }),
                                    new("Cancel", () =>
                                    {
                                        IsShowConfirmBox = false;
                                        return true;
                                    })
                            });
                    };
                }
                else
                {
                    iconEquipmentSlot = new Icon(IconType.None, Anchor.AutoInline, 1, true)
                    {
                        Identifier = "Slot_" + i,
                        Locked = true,
                        Offset = new Vector2(-16, 0)
                    };
                }

                if (i < 3)
                {
                    // Add Equipment slot in Left Panel
                    leftEquipmentPanel.AddChild(iconEquipmentSlot);
                }
                else
                {
                    // Add Equipment slot in Right Panel 
                    rightEquipmentPanel.AddChild(iconEquipmentSlot);
                }
            }         

            var skillIconPanel = displayCharacterPanel.Children.FirstOrDefault(p => p.Identifier.Equals("skillIconPanel"));

            // Normal Skill icon
            var normalSkillIcon = skillIconPanel.Children.OfType<Icon>().FirstOrDefault
                        (e => e.Identifier.Equals("normalSkillIcon"));
            normalSkillIcon.Texture = GameGlobals.Instance.GetItemTexture(400);
            normalSkillIcon.Count = PlayerManager.Instance.Player.PlayerData.Abilities.NormalSkillLevel;
            var normalSkillLevel = normalSkillIcon.Children.OfType<Label>().FirstOrDefault
                        (e => e.Identifier.Equals("normalSkillLevel"));
            normalSkillLevel.Text = $"+{normalSkillIcon.Count}";

            // Burst Skill Icon
            var burstSkillIcon = skillIconPanel.Children.OfType<Icon>().FirstOrDefault
                        (e => e.Identifier.Equals("burstSkillIcon"));
            burstSkillIcon.Texture = GameGlobals.Instance.GetItemTexture(400);
            burstSkillIcon.Count = PlayerManager.Instance.Player.PlayerData.Abilities.BurstSkillLevel;
            var burstSkillLevel = burstSkillIcon.Children.OfType<Label>().FirstOrDefault
                        (e => e.Identifier.Equals("burstSkillLevel"));
            burstSkillLevel.Text = $"+{burstSkillIcon.Count}";

            // Passive Skill Icon
            var passiveSkillIcon = skillIconPanel.Children.OfType<Icon>().FirstOrDefault
                        (e => e.Identifier.Equals("passiveSkillIcon"));
            passiveSkillIcon.Texture = GameGlobals.Instance.GetItemTexture(400);
            passiveSkillIcon.Count = PlayerManager.Instance.Player.PlayerData.Abilities.PassiveSkillLevel;
            var passiveSkillLevel = passiveSkillIcon.Children.OfType<Label>().FirstOrDefault
                        (e => e.Identifier.Equals("passiveSkillLevel"));
            passiveSkillLevel.Text = $"+{passiveSkillIcon.Count}";

            var rightPanel = currentTabPanel.Children.FirstOrDefault(p => p.Identifier.Equals("rightPanel"));
            var statsPanel = rightPanel.Children.FirstOrDefault(p => p.Identifier.Equals("statsPanel"));

            // Player Level
            var playerLevel = statsPanel.Children.OfType<Label>().FirstOrDefault
                        (e => e.Identifier.Equals("playerLevel"));
            playerLevel.Text = PlayerManager.Instance.Player.Level.ToString();

            // ATK
            var statsATKPanel = statsPanel.Children.FirstOrDefault(p => p.Identifier.Equals("statsATKPanel"));
            var statsATK = statsATKPanel.Children.OfType<Label>().FirstOrDefault
                        (e => e.Identifier.Equals("statsATK"));
            statsATK.Text = PlayerManager.Instance.Player.ATK.ToString();

            // HP
            var statsHPPanel = statsPanel.Children.FirstOrDefault(p => p.Identifier.Equals("statsHPPanel"));
            var statsHP = statsHPPanel.Children.OfType<Label>().FirstOrDefault
                        (e => e.Identifier.Equals("statsHP"));
            statsHP.Text = PlayerManager.Instance.Player.MaxHP.ToString();

            // Mana
            var statsManaPanel = statsPanel.Children.FirstOrDefault(p => p.Identifier.Equals("statsManaPanel"));
            var statsMana = statsManaPanel.Children.OfType<Label>().FirstOrDefault
                        (e => e.Identifier.Equals("statsMana"));
            statsMana.Text = ((int)PlayerManager.Instance.Player.MaxMana).ToString();

            // ManaRegen
            var statsManaRegenPanel = statsPanel.Children.FirstOrDefault(p => p.Identifier.Equals("statsManaRegenPanel"));
            var statsManaRegen = statsManaRegenPanel.Children.OfType<Label>().FirstOrDefault
                        (e => e.Identifier.Equals("statsManaRegen"));
            statsManaRegen.Text = PlayerManager.Instance.Player.ManaRegenRate.ToString("F2") + "/s";

            // DEF
            var statsDEFPanel = statsPanel.Children.FirstOrDefault(p => p.Identifier.Equals("statsDEFPanel"));
            var statsDEF = statsDEFPanel.Children.OfType<Label>().FirstOrDefault
                        (e => e.Identifier.Equals("statsDEF"));
            statsDEF.Text = (PlayerManager.Instance.Player.DEF * 100f).ToString("F2") + "%";

            // Crit
            var statsCritPanel = statsPanel.Children.FirstOrDefault(p => p.Identifier.Equals("statsCritPanel"));
            var statsCrit = statsCritPanel.Children.OfType<Label>().FirstOrDefault
                        (e => e.Identifier.Equals("statsCrit"));
            statsCrit.Text = (PlayerManager.Instance.Player.Crit * 100f).ToString("F2") + "%";

            // CritDMG
            var statsCritDMGPanel = statsPanel.Children.FirstOrDefault(p => p.Identifier.Equals("statsCritDMGPanel"));
            var statsCritDMG = statsCritDMGPanel.Children.OfType<Label>().FirstOrDefault
                        (e => e.Identifier.Equals("statsCritDMG"));
            statsCritDMG.Text = (PlayerManager.Instance.Player.CritDMG * 100f).ToString("F2") + "%";

            // Evasion
            var statsEvasionPanel = statsPanel.Children.FirstOrDefault(p => p.Identifier.Equals("statsEvasionPanel"));
            var statsEvasion = statsEvasionPanel.Children.OfType<Label>().FirstOrDefault
                        (e => e.Identifier.Equals("statsEvasion"));
            statsEvasion.Text = (PlayerManager.Instance.Player.Evasion * 100f).ToString("F2") + "%";

            // Speed
            var statsSpeedPanel = statsPanel.Children.FirstOrDefault(p => p.Identifier.Equals("statsSpeedPanel"));
            var statsSpeed = statsSpeedPanel.Children.OfType<Label>().FirstOrDefault
                        (e => e.Identifier.Equals("statsSpeed"));
            statsSpeed.Text = PlayerManager.Instance.Player.Speed.ToString();
        }

        private void RefreshSkillDescription(List<SkillDescriptionData> descSkill, int level)
        {
            var inspectPanel = _mainPanels.ElementAt(InspectPanel);
            var inspectTabs = inspectPanel.Children.OfType<PanelTabs>().FirstOrDefault(t => t.Identifier.Equals("inspectTabs"));
            var currentTabPanel = inspectTabs.ActiveTab.panel;
            var leftPanel = currentTabPanel.Children.FirstOrDefault(t => t.Identifier.Equals("leftPanel"));
            var selectedSkillPanel = leftPanel.Children.FirstOrDefault(t => t.Identifier.Equals("selectedSkillPanel"));
            var descripLeftSkill = selectedSkillPanel.Children.OfType<Paragraph>().FirstOrDefault(t => t.Identifier.Equals("descripLeftSkill"));
            var upSkillButton = selectedSkillPanel.Children.FirstOrDefault(t => t.Identifier.Equals("upSkillButton"));
            var descripRightSkill = selectedSkillPanel.Children.OfType<Paragraph>().FirstOrDefault(t => t.Identifier.Equals("descripRightSkill"));
          
            if (level < 10)
            {
                descripLeftSkill.Text = $"Lv.{level} : {_noahSkillSelected}\n"
                    + descSkill.FirstOrDefault(s => s.Level.Equals(level)).Description;
                upSkillButton.Enabled = true;
                descripRightSkill.Text = $"Lv.{level + 1} : {_noahSkillSelected}\n"
                    + descSkill.FirstOrDefault(s => s.Level.Equals(level + 1)).Description;
            }
            else
            {
                descripLeftSkill.Text = $"Lv.{level} : {_noahSkillSelected}\n"
                    + descSkill.FirstOrDefault(s => s.Level.Equals(level)).Description;
                upSkillButton.Enabled = false;
                descripRightSkill.Text = "Skill level has reached maximum.";
            }
        }

        public void ClearSkillDescription()
        {
            IsCharacterTabSelected = false;

            var inspectPanel = _mainPanels.ElementAt(InspectPanel);
            var inspectTabs = inspectPanel.Children.OfType<PanelTabs>().FirstOrDefault(t => t.Identifier.Equals("inspectTabs"));
            inspectTabs.SelectTab("Character");

            var currentTabPanel = inspectTabs.ActiveTab.panel;
            var leftPanel = currentTabPanel.Children.FirstOrDefault(t => t.Identifier.Equals("leftPanel"));
            var selectedSkillPanel = leftPanel.Children.FirstOrDefault(t => t.Identifier.Equals("selectedSkillPanel"));
            var descripLeftSkill = selectedSkillPanel.Children.OfType<Paragraph>().FirstOrDefault(t => t.Identifier.Equals("descripLeftSkill"));
            var upSkillButton = selectedSkillPanel.Children.FirstOrDefault(t => t.Identifier.Equals("upSkillButton"));
            var descripRightSkill = selectedSkillPanel.Children.OfType<Paragraph>().FirstOrDefault(t => t.Identifier.Equals("descripRightSkill"));

            descripLeftSkill.Text = "";
            upSkillButton.Enabled = false;
            descripRightSkill.Text = "";
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
