using GeonBit.UI;
using GeonBit.UI.Entities;
using Medicraft.Data.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace Medicraft.Systems.Managers
{
    public class InventoryManager
    {
        public readonly int MaximunSlot;
        public int GoldCoin { private set; get; }
        public Dictionary<string, InventoryItemData> InventoryBag { private set; get; }

        // Inventory UI Variables
        // all the example panels (screens)
        readonly List<Panel> panels = [];
        // paragraph that shows the currently active entity
        public Paragraph TargetEntityShow { private set; get; }


        private static InventoryManager instance;

        private InventoryManager()
        {
            MaximunSlot = GameGlobals.Instance.MaximunInventorySlot;
            InventoryBag = [];
            GoldCoin = 0;
        }

        public void InitializeThemeAndUI(BuiltinThemes theme)
        {
            // clear previous panels
            panels.Clear();

            // create and init the UI manager
            var content = new ContentManager(GameGlobals.Instance.Content.ServiceProvider, "Content");
            UserInterface.Initialize(content, theme);
            UserInterface.Active.UseRenderTarget = true;

            // draw cursor outside the render target
            UserInterface.Active.IncludeCursorInRenderTarget = false;

            // init ui and examples
            InitExamplesAndUI();
        }

        /// <summary>
        /// Create the top bar with next / prev buttons etc, and init all UI example panels.
        /// </summary>    
        private void InitExamplesAndUI()
        {
            // will init examples only if true
            //bool initExamples = true;         

            // add exit button
            Button exitBtn = new("Exit", anchor: Anchor.BottomRight, size: new Vector2(200, -1))
            {
                OnClick = (Entity entity) =>
                {
                    //Exit();
                }
            };
            UserInterface.Active.AddEntity(exitBtn);

            // events panel for debug
            Panel eventsPanel = new(new Vector2(400, 530), PanelSkin.Simple, Anchor.CenterLeft, new Vector2(-10, 0))
            {
                Visible = false
            };

            // events log (single-time events)
            eventsPanel.AddChild(new Label("Events Log:"));
            SelectList eventsLog = new(size: new Vector2(-1, 280))
            {
                ExtraSpaceBetweenLines = -8,
                ItemsScale = 0.5f,
                Locked = true
            };
            eventsPanel.AddChild(eventsLog);

            // current events (events that happen while something is true)
            eventsPanel.AddChild(new Label("Current Events:"));
            SelectList eventsNow = new(size: new Vector2(-1, 100))
            {
                ExtraSpaceBetweenLines = -8,
                ItemsScale = 0.5f,
                Locked = true
            };
            eventsPanel.AddChild(eventsNow);

            // paragraph to show currently active panel
            TargetEntityShow = new Paragraph("test", Anchor.Auto, Color.White, scale: 0.75f);
            eventsPanel.AddChild(TargetEntityShow);

            // add the events panel
            UserInterface.Active.AddEntity(eventsPanel);

            // clear the current events after every frame they were drawn
            eventsNow.AfterDraw = (Entity entity) => {
                eventsNow.ClearItems();
            };

            // สร้าง Panel หลัก
            Panel mainPanel = new(new Vector2(1200, 650), PanelSkin.Simple, Anchor.Center);
            UserInterface.Active.AddEntity(mainPanel);

            // สร้าง Panel สำหรับฝั่งซ้าย
            Panel leftPanel = new(new Vector2(500, 600), PanelSkin.ListBackground, Anchor.TopLeft);
            mainPanel.AddChild(leftPanel);

            // สร้าง Panel สำหรับฝั่งขวา
            Panel rightPanel = new(new Vector2(600, 600), PanelSkin.ListBackground, Anchor.TopRight);
            mainPanel.AddChild(rightPanel);
            rightPanel.AddChild(new Header("IVENTORY"));
            rightPanel.AddChild(new HorizontalLine());

            for (int i = 0; i < 48; ++i)
            {
                rightPanel.AddChild(new Icon((IconType)i, Anchor.AutoInline, 1, true));
            }

            foreach (Entity childEntity in rightPanel.Children)
            {
                if (childEntity is Icon icon)
                {
                    // สร้าง Label เพื่อแสดงชื่อของไอคอน
                    Label iconLabel = new(icon.IconType.ToString(), Anchor.BottomCenter)
                    {
                        Size = new Vector2(100, -1) // กำหนดขนาดของ Label
                    };

                    icon.OnClick = (Entity entity) =>
                    {
                        // Remove all icons from the left panel
                        leftPanel.ClearChildren();

                        // Clone the clicked icon and add it to the left panel
                        Icon clonedIcon = new(icon.IconType, Anchor.AutoInline, 1, true)
                        {
                            Size = new Vector2(450, 450) // ปรับขนาดของไอคอน
                        };

                        leftPanel.AddChild(clonedIcon);
                        leftPanel.AddChild(iconLabel);
                    };
                }
            }

            var offsetX = 140;
            Button useBtn = new("Use"
                , anchor: Anchor.BottomLeft
                , size: new Vector2(200, -1)
                , offset: new Vector2(offsetX, 0));

            offsetX += 200;
            useBtn.OnClick = (Entity entity) => { };
            UserInterface.Active.AddEntity(useBtn);

            Button openBtn = new("Open"
                , anchor: Anchor.BottomLeft
                , size: new Vector2(200, -1)
                , offset: new Vector2(offsetX, 0));

            offsetX += 200;
            openBtn.OnClick = (Entity entity) => {
                mainPanel.Visible = true; 
            };
            UserInterface.Active.AddEntity(openBtn);

            Button closeButton = new("Close"
                , anchor: Anchor.BottomLeft
                , size: new Vector2(200, -1)
                , offset: new Vector2(offsetX, 0))
            {
                OnClick = (Entity entity) => { mainPanel.Visible = false; }
            };
            UserInterface.Active.AddEntity(closeButton);

            // Disable Cursor
            UserInterface.Active.ShowCursor = false;

            // once done init, clear events log
            eventsLog.ClearItems();
        }

        public void InitializeInventory(InventoryData inventoryData)
        {
            // Clear Inventory
            GoldCoin = 0;
            InventoryBag.Clear();

            // Setting Gold Coin
            GoldCoin = inventoryData.GoldCoin;

            // Setting Item in Inventory
            for (int i = 0; i < inventoryData.Inventory.Count; i++)
            {
                //if (inventoryData.Inventory.ElementAt(i) != 0)
                InventoryBag.Add(inventoryData.Inventory.ElementAt(i).ItemId.ToString(), inventoryData.Inventory.ElementAt(i));
            }
        }

        public void Update(GameTime gameTime) 
        {
            // update UI
            UserInterface.Active.Update(gameTime);

            // show currently active entity (for testing)
            TargetEntityShow.Text = "Target Entity: " + (UserInterface.Active.TargetEntity != null ? UserInterface.Active.TargetEntity.GetType().Name : "null");

        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            // draw ui
            UserInterface.Active.Draw(spriteBatch);        

            // clear buffer
            ScreenManager.Instance.GraphicsDevice.Clear(Color.CornflowerBlue);

            // finalize ui rendering
            UserInterface.Active.DrawMainRenderTarget(spriteBatch);
        }

        public void AddItem(int itemId, int quantity)
        {
            var itemData = GameGlobals.Instance.ItemsDatas.Where(i => i.ItemId.Equals(itemId));

            // Gotta Check Item id if it already has in inventory and stackable or mot
            if (InventoryBag.ContainsKey(itemId.ToString()) && itemData.ElementAt(0).Stackable)
            {
                HUDSystem.AddFeedItem(itemId, quantity);

                InventoryBag[itemId.ToString()].Count += quantity;
            }
            else
            {
                HUDSystem.AddFeedItem(itemId, quantity);

                InventoryBag.Add(itemId.ToString(), new InventoryItemData()
                {
                    ItemId = itemId,
                    Count = quantity,
                    Slot = GameGlobals.Instance.DefaultInventorySlot
                });
            }
        }

        public void AddGoldCoin(int goldCoin)
        {
            GoldCoin += goldCoin;
        }

        public static InventoryManager Instance
        {
            get
            {
                instance ??= new InventoryManager();
                return instance;
            }
        }
    }
}
