using GeonBit.UI;
using GeonBit.UI.Entities;
using Medicraft.Data.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Sprites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medicraft.Systems.Managers
{
    public class GUIManager
    {
        private static GUIManager instance;

        private float _deltaSeconds = 0;

        public const int ItemBar = 0;
        public const int InventoryPanel = 1;
        public const int CharacterInspectPanel = 2;
        public const int CraftingTablePanel = 3;

        public readonly List<Panel> Panels = [];

        public int CurrentGUI { get; set; }

        private GUIManager()
        {
            CurrentGUI = ItemBar;
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
            // ItemBar = 0
            InitItemBarUI();

            // InventoryPanel = 1
            InitInventoryUI();

            // update ui panel
            UpdateAfterChangeGUI();
        }


        /// <summary>
        /// Item Bar and Slot Item
        /// </summary>
        public void InitItemBarUI()
        {
            var panel = new Panel(new Vector2(443, 86), PanelSkin.None, Anchor.BottomCenter);
            Panels.Add(panel);
            UserInterface.Active.AddEntity(panel);
        }

        public void RefreshItemBarDisplay()
        {
            var itemBarPanel = Panels.ElementAt(ItemBar);
            
            if (itemBarPanel.Children.Count > 0) itemBarPanel.ClearChildren();

            var minSlotNum = InventoryManager.ItemBarSlot_1;
            var maxSlotNum = InventoryManager.ItemBarSlot_8;

            var itemInSlot = InventoryManager.Instance.EquipmentAndBarItemSlot?.Where(
                i => i != null && i.Slot >= minSlotNum && i.Slot <= maxSlotNum);

            var offSetX = 16;

            // Set Item
            foreach (var item in itemInSlot)
            {
                var iconItem = new Icon(IconType.None, Anchor.BottomLeft, 0.1f, offset: new Vector2(-10, 0))
                {
                    ItemId = item.ItemId,
                    Count = item.Count,
                    Slot = item.Slot,
                    Texture = GameGlobals.Instance.GetItemTexture(item.ItemId),
                };

                iconItem.AddChild(new Label(iconItem.Count.ToString(), Anchor.BottomRight, offset: new Vector2(-17f, -33))
                {
                    Size = new Vector2(5, 5), // กำหนดขนาดของ Label
                    Scale = 1f,
                    ClickThrough = true,
                });

                itemBarPanel.AddChild(iconItem);

                offSetX += 5;
            }

            foreach (var iconItem in itemBarPanel.Children.OfType<Icon>())
            {
                iconItem.OnClick = (Entity entity) =>
                {
                    InventoryManager.Instance.UseItemInSlotBar(iconItem.ItemId);
                };
            }
        }


        /// <summary>
        /// Inventory Panel, Display list of items in inventory and Description
        /// </summary>
        public void InitInventoryUI()
        {
            // สร้าง Panel หลัก
            var mainInvenPanel = new Panel(new Vector2(1200, 650), PanelSkin.Simple, Anchor.Center)
            {
                Identifier = "invenMainPanel"
            };
            Panels.Add(mainInvenPanel);
            UserInterface.Active.AddEntity(mainInvenPanel);

            // สร้าง Panel สำหรับฝั่งซ้าย
            var leftInvenPanel = new Panel(new Vector2(500, 600), PanelSkin.ListBackground, Anchor.TopLeft)
            {
                Identifier = "invenLeftPanel"
            };
            mainInvenPanel.AddChild(leftInvenPanel);

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
            mainInvenPanel.AddChild(rightInvenPanel);

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
                    GameGlobals.Instance.IsRefreshItemBar = false;
                    CurrentGUI = ItemBar;
                }
            };
            mainInvenPanel.AddChild(exitBtn);

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
            mainInvenPanel.AddChild(useInvenItemButton);

            offsetX += 216;
            var setItemBarButton = new Button("Set", anchor: Anchor.BottomLeft
                , size: new Vector2(200, -1), offset: new Vector2(offsetX, -100))
            {
                Identifier = "invenSetItemButton",
                Enabled = false,
                OnClick = (Entity entity) =>
                {
                    var currItemInv = InventoryManager.Instance.SelectedInventoryItem;
                    var slotNum = InventoryManager.Instance.SetItemBarSlot(6);
                    InventoryManager.Instance.SetEquipmentOrItemBarSlot(currItemInv, slotNum);
                }
            };
            mainInvenPanel.AddChild(setItemBarButton);

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

            if (listItemPanel.Children.Count != 0)
            {
                var fisrtItem = listItemPanel.Children.OfType<Icon>().ToList().ElementAt(0);

                SetLeftPanelDisplay(fisrtItem);

                // Setup Enable or Disable the button for fisrtItem
                InventoryManager.Instance.InventoryBag.TryGetValue(fisrtItem.ItemId.ToString(), out InventoryItemData item);
                InventoryManager.Instance.SelectedInventoryItem = item;

                if (GameGlobals.Instance.IsUsableItem(fisrtItem.ItemId))
                {
                    invenUseItemButton.Enabled = true;
                }
                else invenUseItemButton.Enabled = false;
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
 
            // Clear inventory display
            listItemPanel.ClearChildren();

            // if true den set invisible to all entity in invenLeftPanel
            if (isClearLeftPanel)
            {
                foreach (var entity in invenLeftPanel.Children)
                {
                    entity.Visible = false;
                }
            }

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
                    {
                        itemLeftPanel.Visible = true;
                    }

                    // Enable or Disable the button for each item icon
                    InventoryManager.Instance.InventoryBag.TryGetValue(iconItem.ItemId.ToString(), out InventoryItemData item);
                    InventoryManager.Instance.SelectedInventoryItem = item;

                    if (GameGlobals.Instance.IsUsableItem(iconItem.ItemId))
                    {
                        invenUseItemButton.Enabled = true;
                    }
                    else invenUseItemButton.Enabled = false;
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
