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
    public class CraftingManager
    {
        private static CraftingManager instance;

        public Dictionary<string, InventoryItemData> InventoryBag { private set; get; }

        // Inventory UI Variables
        // all the example panels (screens)
        readonly List<Panel> panels = [];

        private Panel _mainPanel, _leftPanel, _rightPanel;

        private Panel _descriptionPanel, _listItemPanel;

        private Button _useButton, _openButton, _closeButton;

        private float _deltaSeconds = 0;

        private CraftingManager() { }

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

            // Disable Cursor
            UserInterface.Active.ShowCursor = false;

            // init ui and examples
            InitExamplesAndUI();
        }

        /// <summary>
        /// Create the top bar with next / prev buttons etc, and init all UI example panels.
        /// </summary>    
        private void InitExamplesAndUI()
        {
            // สร้าง Panel หลัก
            _mainPanel = new(new Vector2(1200, 650), PanelSkin.Simple, Anchor.Center);
            UserInterface.Active.AddEntity(_mainPanel);

            // สร้าง Panel สำหรับฝั่งซ้าย
            _leftPanel = new(new Vector2(500, 600), PanelSkin.ListBackground, Anchor.TopLeft);
            _mainPanel.AddChild(_leftPanel);

            _descriptionPanel = new(new Vector2(450, 225), PanelSkin.Fancy, Anchor.AutoCenter)
            {
                PanelOverflowBehavior = PanelOverflowBehavior.VerticalScroll
            };
            _descriptionPanel.Scrollbar.AdjustMaxAutomatically = true;

            // สร้าง Panel สำหรับฝั่งขวา
            _rightPanel = new(new Vector2(600, 600), PanelSkin.ListBackground, Anchor.TopRight);
            _mainPanel.AddChild(_rightPanel);

            _rightPanel.AddChild(new Header("IVENTORY"));
            _rightPanel.AddChild(new HorizontalLine());
            _rightPanel.AddChild(new LineSpace());

            _listItemPanel = new(new Vector2(550, 450), PanelSkin.Fancy, Anchor.AutoCenter)
            {
                PanelOverflowBehavior = PanelOverflowBehavior.VerticalScroll
            };
            _listItemPanel.Scrollbar.AdjustMaxAutomatically = true;

            _rightPanel.AddChild(_listItemPanel);

            // add exit button
            Button exitBtn = new("Exit", anchor: Anchor.BottomRight, size: new Vector2(200, -1), offset: new Vector2(0, 50))
            {
                OnClick = (Entity entity) =>
                {
                    // Close Inventory
                    // Toggle the IsOpenInventory flag
                    GameGlobals.Instance.IsOpenInventoryPanel = !GameGlobals.Instance.IsOpenInventoryPanel;

                    // Pause PlayScreen
                    GameGlobals.Instance.IsGamePause = !GameGlobals.Instance.IsGamePause;
                }
            };
            UserInterface.Active.AddEntity(exitBtn);

            var offsetX = 140;
            _useButton = new("Use", anchor: Anchor.BottomLeft, size: new Vector2(200, -1), offset: new Vector2(offsetX, 50))
            {
                Enabled = false,
                OnClick = (Entity entity) =>
                {
                    // crafting
                }
            };
            UserInterface.Active.AddEntity(_useButton);

            offsetX += 200;
            _openButton = new("Open", anchor: Anchor.BottomLeft, size: new Vector2(200, -1), offset: new Vector2(offsetX, 50))
            {
                OnClick = (Entity entity) =>
                {
                    _mainPanel.Visible = true;
                }
            };
            UserInterface.Active.AddEntity(_openButton);

            offsetX += 200;
            _closeButton = new("Close", anchor: Anchor.BottomLeft, size: new Vector2(200, -1), offset: new Vector2(offsetX, 50))
            {
                OnClick = (Entity entity) =>
                {
                    _mainPanel.Visible = false;
                }
            };
            UserInterface.Active.AddEntity(_closeButton);

            InitInventoryItemDisplay();
        }

        private void InitInventoryItemDisplay()
        {
            RefreshInvenrotyItemDisplay(false);

            var fisrtItem = _listItemPanel.Children.OfType<Icon>().ToList().ElementAt(0);

            SetLeftPanelDisplay(fisrtItem);

            // Setup Enable or Disable the button for fisrtItem
            InventoryBag.TryGetValue(fisrtItem.ItemId.ToString(), out InventoryItemData item);
            //GameGlobals.Instance.CurrentInventoryItemSelect = item;
            if (GameGlobals.Instance.IsUsableItem(fisrtItem.ItemId))
            {
                _useButton.Enabled = true;
            }
            else _useButton.Enabled = false;
        }

        public void SetLeftPanelDisplay(Icon icon)
        {
            var itemData = GameGlobals.Instance.ItemsDatas.FirstOrDefault(i => i.ItemId.Equals(icon.ItemId));

            // Clone the clicked icon and add it to the left panel
            Icon clonedIcon = new(IconType.None, Anchor.AutoCenter, 1, true)
            {
                Size = new Vector2(300, 300),   // ปรับขนาดของไอคอน
                Texture = icon.Texture,
                Locked = true,
            };

            // สร้าง Label เพื่อแสดงชื่อของไอคอน
            Label iconLabel = new(itemData.Name, Anchor.AutoCenter)
            {
                Size = new Vector2(250, 30), // กำหนดขนาดของ Label
                Scale = 1.3f
            };

            // Description item
            Label description = new(itemData.Description, Anchor.AutoInline)
            {
                Size = new Vector2(380, 200), // กำหนดขนาดของ Label
                Scale = 1f,
                WrapWords = true
            };

            _leftPanel.AddChild(clonedIcon);
            _leftPanel.AddChild(new LineSpace());
            _leftPanel.AddChild(_descriptionPanel);

            //_descriptionPanel.AddChild(new LineSpace());
            _descriptionPanel.AddChild(iconLabel);
            _descriptionPanel.AddChild(description);
            _descriptionPanel.AddChild(new LineSpace());
        }

        public void RefreshInvenrotyItemDisplay(bool isClearLeftPanel)
        {
            // Clear inventory display
            _listItemPanel.ClearChildren();

            if (isClearLeftPanel) _leftPanel.ClearChildren();

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

                var itemDisplay = new Icon(IconType.None, Anchor.AutoInline, 1, true)
                {
                    ItemId = item.ItemId,
                    Count = item.Count,
                    Slot = item.Slot,
                    Texture = newTexture,
                };

                itemDisplay.AddChild(new Label(itemDisplay.Count.ToString(), Anchor.BottomRight, offset: new Vector2(-22, -35))
                {
                    Size = new Vector2(10, 10), // กำหนดขนาดของ Label
                    Scale = 1f
                });

                _listItemPanel.AddChild(itemDisplay);
            }

            foreach (var icon in _listItemPanel.Children.OfType<Icon>())
            {
                icon.OnClick = (Entity entity) =>
                {
                    // Remove all icons from the left panel
                    _leftPanel.ClearChildren();
                    _descriptionPanel.ClearChildren();

                    SetLeftPanelDisplay(icon);

                    // Enable or Disable the button for each item icon
                    InventoryBag.TryGetValue(icon.ItemId.ToString(), out InventoryItemData item);
                    //GameGlobals.Instance.CurrentInventoryItemSelect = item;
                    if (GameGlobals.Instance.IsUsableItem(icon.ItemId))
                    {
                        _useButton.Enabled = true;
                    }
                    else _useButton.Enabled = false;
                };
            }
        }

        public void Update(GameTime gameTime)
        {
            _deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // update UI
            UserInterface.Active.Update(gameTime);    
        }

        public void DrawUI(SpriteBatch spriteBatch)
        {
            // draw ui
            UserInterface.Active.Draw(spriteBatch);
        }

        public void DrawMainRenderTarget(SpriteBatch spriteBatch)
        {
            // finalize ui rendering
            UserInterface.Active.DrawMainRenderTarget(spriteBatch);
        }

        public static CraftingManager Instance
        {
            get
            {
                instance ??= new CraftingManager();
                return instance;
            }
        }
    }
}
