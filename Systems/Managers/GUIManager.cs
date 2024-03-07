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

        public const int Hotbar = 0;
        public const int InventoryPanel = 1;
        public const int CharacterInspectPanel = 2;
        public const int CraftingTablePanel = 3;

        public readonly List<Panel> Panels = [];

        public int CurrentGUI { get; set; }

        private GUIManager()
        {
            CurrentGUI = Hotbar;
        }

        public void Update(GameTime gameTime)
        {
            _deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public void UpdateAfterChangeGUI()
        {
            // hide all panels and show current example panel
            foreach (Panel panel in Panels)
            {
                panel.Visible = false;
            }

            Panels[CurrentGUI].Visible = true;
        }

        public void InitializeThemeAndUI(BuiltinThemes theme)
        {
            // clear previous panels
            Panels.Clear();

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

            // update ui panel
            UpdateAfterChangeGUI();
        }


        /// <summary>
        /// Item Bar and Slot Item
        /// </summary>
        public void InitHotbarUI()
        {
            var panel = new Panel(new Vector2(500, 87), PanelSkin.None, Anchor.BottomCenter)
            { 
                Identifier = "hotbarItem",
                //AdjustHeightAutomatically = true,
            };
            Panels.Add(panel);
            UserInterface.Active.AddEntity(panel);
        }

        public void RefreshHotbarDisplay()
        {
            var HotbarSlot = Panels.ElementAt(Hotbar);
            
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
        public void InitInventoryUI()
        {
            // สร้าง Panel หลัก
            var InventoryPanel = new Panel(new Vector2(1200, 650), PanelSkin.Simple, Anchor.Center)
            {
                Identifier = "invenMainPanel"
            };
            Panels.Add(InventoryPanel);
            UserInterface.Active.AddEntity(InventoryPanel);

            // สร้าง Panel สำหรับฝั่งซ้าย
            var leftInvenPanel = new Panel(new Vector2(500, 600), PanelSkin.ListBackground, Anchor.TopLeft)
            {
                Identifier = "invenLeftPanel"
            };
            InventoryPanel.AddChild(leftInvenPanel);

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
            InventoryPanel.AddChild(rightInvenPanel);

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

            // add exit button
            Button exitBtn = new("Close", anchor: Anchor.BottomRight
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
            InventoryPanel.AddChild(exitBtn);

            var offsetX = 64;

            var useInvenItemButton = new Button("Use", anchor: Anchor.BottomLeft
                , size: new Vector2(200, -1), offset: new Vector2(offsetX, -100))
            {
                Identifier = "invenUseItemButton",
                Enabled = false,
                OnClick = (Entity entity) =>
                {
                    var currItemInv = InventoryManager.Instance.SelectedInventoryItem;
                    InventoryManager.Instance.UseItem(currItemInv);
                }
            };
            InventoryPanel.AddChild(useInvenItemButton);

            offsetX += 216;
            var invenSetHotbarButton = new Button("Setup Hotbar", anchor: Anchor.BottomLeft
                , size: new Vector2(200, -1), offset: new Vector2(offsetX, -100))
            {
                Identifier = "invenSetHotbarButton",
                Enabled = false,
                OnClick = (Entity entity) =>
                {
                    var currItemInv = InventoryManager.Instance.SelectedInventoryItem;

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
                                GeonBit.UI.Utils.MessageBox.ShowMsgBox("Setup Complete",
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
            InventoryPanel.AddChild(invenSetHotbarButton);

            //offsetX += 216;
            //_closeButton = new("Close", anchor: Anchor.BottomLeft, size: new Vector2(200, -1)
            //  , offset: new Vector2(offsetX, -150))
            //{
            //    OnClick = (Entity entity) =>
            //    {
            //        _mainPanel.Visible = false;                  
            //    }
            //};
            //UserInterface.Active.AddEntity(_closeButton);
        }

        // Call this after initialize Player's Inventory Data
        public void InitInventoryItemDisplay()
        {
            RefreshInvenrotyItemDisplay(false);

            var InventoryPanel = Panels.ElementAt(GUIManager.InventoryPanel);
            var invenRightPanel = InventoryPanel.Children?.FirstOrDefault(p => p.Identifier.Equals("invenRightPanel"));
            var listItemPanel = invenRightPanel.Children?.FirstOrDefault(p => p.Identifier.Equals("listItemPanel"));
            var invenUseItemButton = InventoryPanel.Children?.FirstOrDefault(p => p.Identifier.Equals("invenUseItemButton"));
            var invenSetHotbarButton = InventoryPanel.Children?.FirstOrDefault(p => p.Identifier.Equals("invenSetHotbarButton"));

            if (listItemPanel.Children.Count != 0)
            {
                var fisrtItem = listItemPanel.Children.OfType<Icon>().ToList().ElementAt(0);

                SetLeftPanelDisplay(fisrtItem);

                // Setup Enable or Disable the button for fisrtItem
                InventoryManager.Instance.InventoryBag.TryGetValue(fisrtItem.ItemId.ToString()
                    , out InventoryItemData item);
                InventoryManager.Instance.SelectedInventoryItem = item;

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
            var InventoryPanel = Panels.ElementAt(GUIManager.InventoryPanel);          
            var invenRightPanel = InventoryPanel.Children?.FirstOrDefault(p => p.Identifier.Equals("invenRightPanel"));
            var invenLeftPanel = InventoryPanel.Children?.FirstOrDefault(p => p.Identifier.Equals("invenLeftPanel"));
            var listItemPanel = invenRightPanel.Children?.FirstOrDefault(p => p.Identifier.Equals("listItemPanel"));
            var invenDescriptPanel = invenLeftPanel.Children?.FirstOrDefault(p => p.Identifier.Equals("invenDescriptPanel"));
            var invenUseItemButton = InventoryPanel.Children?.FirstOrDefault(p => p.Identifier.Equals("invenUseItemButton"));
            var invenSetHotbarButton = InventoryPanel.Children?.FirstOrDefault(p => p.Identifier.Equals("invenSetHotbarButton"));

            // Clear inventory display
            listItemPanel.ClearChildren();

            // if true den set invisible to all entity in invenLeftPanel
            if (isClearLeftPanel)
                foreach (var entity in invenLeftPanel.Children)
                    entity.Visible = false;

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
                    SetLeftPanelDisplay(iconItem);

                    foreach (var itemLeftPanel in invenLeftPanel.Children)
                        itemLeftPanel.Visible = true;

                    // Enable or Disable the button for each item icon
                    InventoryManager.Instance.InventoryBag.TryGetValue(iconItem.ItemId.ToString()
                        , out InventoryItemData item);
                    InventoryManager.Instance.SelectedInventoryItem = item;

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

        public void SetLeftPanelDisplay(Icon icon)
        {
            var itemData = GameGlobals.Instance.ItemsDatas.FirstOrDefault(i => i.ItemId.Equals(icon.ItemId));

            var InventoryPanel = Panels.ElementAt(GUIManager.InventoryPanel);
            var invenLeftPanel = InventoryPanel.Children?.FirstOrDefault(p => p.Identifier.Equals("invenLeftPanel"));
            var invenDescriptPanel = invenLeftPanel.Children?.FirstOrDefault(p => p.Identifier.Equals("invenDescriptPanel"));

            // Clone the clicked icon and add it to the left panel
            var itemIcon = invenLeftPanel.Children.OfType<Icon>().FirstOrDefault(i => i.Identifier.Equals("itemIcon"));
            itemIcon.Texture = icon.Texture;

            // สร้าง Label เพื่อแสดงชื่อของไอคอน
            var iconLabel = invenDescriptPanel.Children.OfType<Label>().FirstOrDefault(i => i.Identifier.Equals("iconLabel"));
            iconLabel.Text = itemData.Name;

            // Description item
            var description = invenDescriptPanel.Children.OfType<Label>().FirstOrDefault(i => i.Identifier.Equals("description"));
            description.Text = itemData.Description;                 
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
