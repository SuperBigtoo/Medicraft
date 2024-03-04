using GeonBit.UI;
using GeonBit.UI.Entities;
using Medicraft.Data.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Sprites;
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

        private Panel _mainPanel, _leftPanel, _rightPanel;

        private Button _useButton, openButton, closeButton;

        // paragraph that shows the currently active entity
        public Paragraph TargetEntityShow { private set; get; }

        private float _deltaSeconds = 0;

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
                    // Close Inventory
                    // Toggle the IsOpenInventory flag
                    GameGlobals.Instance.IsOpenInventory = !GameGlobals.Instance.IsOpenInventory;

                    // Pause PlayScreen
                    GameGlobals.Instance.IsGamePause = !GameGlobals.Instance.IsGamePause;
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
            _mainPanel = new(new Vector2(1200, 650), PanelSkin.Simple, Anchor.Center);
            UserInterface.Active.AddEntity(_mainPanel);

            // สร้าง Panel สำหรับฝั่งซ้าย
            _leftPanel = new(new Vector2(500, 600), PanelSkin.ListBackground, Anchor.TopLeft);
            _mainPanel.AddChild(_leftPanel);

            // สร้าง Panel สำหรับฝั่งขวา
            _rightPanel = new(new Vector2(600, 600), PanelSkin.ListBackground, Anchor.TopRight);

            _mainPanel.AddChild(_rightPanel);
            _rightPanel.AddChild(new Header("IVENTORY"));
            _rightPanel.AddChild(new HorizontalLine());        

            var offsetX = 140;
            _useButton = new("Use", anchor: Anchor.BottomLeft, size: new Vector2(200, -1), offset: new Vector2(offsetX, 50))
            {
                Visible = false,
                OnClick = (Entity entity) =>
                {
                    System.Diagnostics.Debug.WriteLine($"ItemId : {GameGlobals.Instance.SelectedItemInventory}");
                }
            };
            UserInterface.Active.AddEntity(_useButton);

            offsetX += 200;
            openButton = new("Open", anchor: Anchor.BottomLeft, size: new Vector2(200, -1), offset: new Vector2(offsetX, 100))
            {
                OnClick = (Entity entity) =>
                {
                    _mainPanel.Visible = true;
                }
            };
            UserInterface.Active.AddEntity(openButton);

            offsetX += 200;
            closeButton = new("Close", anchor: Anchor.BottomLeft, size: new Vector2(200, -1), offset: new Vector2(offsetX, 150))
            {
                OnClick = (Entity entity) => 
                {
                    _mainPanel.Visible = false;
                }
            };
            UserInterface.Active.AddEntity(closeButton);

            // Disable Cursor
            UserInterface.Active.ShowCursor = false;

            // Disable this so it won't clear out da spriteBatch
            UserInterface.Active.UseRenderTarget = false;

            // once done init, clear events log
            eventsLog.ClearItems();
        }

        public void RefreshInvenrotyItem()
        {
            if (_rightPanel.Children.OfType<Icon>().Any())
            {
                // Remove all Icon children
                foreach (var item in _rightPanel.Children.OfType<Icon>().ToList())
                {
                    _rightPanel.RemoveChild(item);
                }
            }

            // Set item
            var itemsSpriteSheet = GameGlobals.Instance.ItemsPackSprites;
            var itemSprite = new AnimatedSprite(itemsSpriteSheet);       

            foreach (var item in InventoryBag.Values)
            {
                itemSprite.Play(item.ItemId.ToString());
                itemSprite.Update(_deltaSeconds);

                var texture = itemSprite.TextureRegion.Texture;
                var bounds = itemSprite.TextureRegion.Bounds;

                // Create a new texture with new bounds
                Texture2D newTexture = new(ScreenManager.Instance.GraphicsDevice, bounds.Width, bounds.Height);

                // Get the data from the original texture
                Color[] data = new Color[bounds.Width * bounds.Height];
                texture.GetData(0, bounds, data, 0, data.Length);

                // Set the data to the new texture
                newTexture.SetData(data);

                _rightPanel.AddChild(new Icon(IconType.None, Anchor.AutoInline, 1, true)
                {
                    ItemId = item.ItemId,
                    Texture = newTexture,
                });
            }

            foreach (var icon in _rightPanel.Children.OfType<Icon>())
            {
                var itemData = GameGlobals.Instance.ItemsDatas.Where(i => i.ItemId.Equals(icon.ItemId)).ElementAt(0);

                // สร้าง Label เพื่อแสดงชื่อของไอคอน
                Label iconLabel = new(itemData.Name, Anchor.AutoCenter)
                {
                    Size = new Vector2(250, 10), // กำหนดขนาดของ Label
                    Scale = 1.5f
                };

                icon.OnClick = (Entity entity) =>
                {
                    // Remove all icons from the left panel
                    _leftPanel.ClearChildren();

                    // Clone the clicked icon and add it to the left panel
                    Icon clonedIcon = new(IconType.None, Anchor.AutoCenter, 1, true)
                    {
                        Size = new Vector2(350, 350),   // ปรับขนาดของไอคอน
                        Texture = icon.Texture
                    };

                    // Description item
                    Label description = new(itemData.Description, Anchor.AutoCenter)
                    {
                        Size = new Vector2(425, 425), // กำหนดขนาดของ Label
                        Scale = 1f,
                        WrapWords = true
                    };

                    _leftPanel.AddChild(clonedIcon);
                    _leftPanel.AddChild(iconLabel);
                    _leftPanel.AddChild(description);

                    GameGlobals.Instance.SelectedItemInventory = icon.ItemId;

                    _useButton.Visible = true;
                };
            }
        }

        // Inventory Data
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
            _deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // update UI
            UserInterface.Active.Update(gameTime);

            // show currently active entity (for testing)
            TargetEntityShow.Text = "Target Entity: " + (UserInterface.Active.TargetEntity != null ? UserInterface.Active.TargetEntity.GetType().Name : "null");
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            // draw ui
            UserInterface.Active.Draw(spriteBatch);

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

        public static bool CheckAddingItem()
        {
            return true;
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
