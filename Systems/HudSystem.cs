using Medicraft.Data.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Sprites;
using System.Linq;

namespace Medicraft.Systems
{
    public class HudSystem
    {
        private readonly BitmapFont[] _fonts;

        private Vector2 _hudPosition;

        private Texture2D _heartTexture, _herb1Texture, _herb2Texture, _drugTexture
            , _coinTexture, _pressFTexture, _insufficient;
        private AnimatedSprite _sprite;
        private Transform2 _transform;

        private float _deltaSeconds;
        private bool _nextFeed;
        private float _displayInsufficientTime;

        public HudSystem(BitmapFont[] fonts, Texture2D[] textures, AnimatedSprite sprite)
        {
            _fonts = fonts;

            _heartTexture = textures[0];
            _herb1Texture = textures[1];
            _herb2Texture = textures[2];
            _drugTexture = textures[3];
            _coinTexture = textures[4];
            _pressFTexture = textures[5];
            _insufficient = textures[6];

            _sprite = sprite;

            _nextFeed = false;
            _displayInsufficientTime = 3f;

            GameGlobals.Instance.HUDPosition = PlayerManager.Instance.Player.Position - new Vector2(720, 450);
        }

        public void Update(GameTime gameTime)
        {
            _deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (GameGlobals.Instance.CollectedItemFeed.Count != 0)
            {
                GameGlobals.Instance.DisplayFeedTime -= _deltaSeconds;

                if (GameGlobals.Instance.DisplayFeedTime <= 0)
                {
                    _nextFeed = true;
                    GameGlobals.Instance.DisplayFeedTime = GameGlobals.Instance.MaximumDisplayFeedTime;
                }
                else _nextFeed = false;
            }

            if (GameGlobals.Instance.ShowInsufficientSign)
            {
                _displayInsufficientTime -= _deltaSeconds;

                if (_displayInsufficientTime <= 0)
                {
                    GameGlobals.Instance.ShowInsufficientSign = false;
                    _displayInsufficientTime = 3f;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            var _graphicsDevice = ScreenManager.Instance.GraphicsDevice;
            _hudPosition = GameGlobals.Instance.HUDPosition;

            float addingX = GameGlobals.Instance.HUDPosition.X;
            float addingY = GameGlobals.Instance.HUDPosition.Y;

            spriteBatch.End();
            spriteBatch.Begin(
                SpriteSortMode.Deferred,
                samplerState: SamplerState.PointClamp,
                blendState: BlendState.AlphaBlend,
                transformMatrix: ScreenManager.Instance.Camera.GetTransform(_graphicsDevice.Viewport.Width
                , _graphicsDevice.Viewport.Height)
            );

            // Draw Press F
            DrawPressF(spriteBatch);

            // Draw Insufficient Sign
            DrawInsufficientSign(spriteBatch);

            // Draw Feed Items
            DrawCollectedItem(spriteBatch);

            // Draw HP mobs
            DrawHPMobs(spriteBatch);

            // Draw Damage Numbers mobs & player
            DrawDamageNumbers(spriteBatch);

            // Draw HUD Bar
            spriteBatch.FillRectangle(-720 + addingX, 0 + addingY, 2640
                , 25, Color.Black * 0.4f);
            spriteBatch.DrawString(_fonts[0], $" Mobs: {EntityManager.Instance.entities.Count}"
                , Vector2.Zero + _hudPosition, Color.White);
            spriteBatch.DrawString(_fonts[0], $"Time Spawn: {(int)EntityManager.Instance.spawnTime}"
                , new Vector2(100f, 0f) + _hudPosition, Color.White);
            spriteBatch.DrawString(_fonts[0], $"X: {(int)PlayerManager.Instance.Player.Position.X}"
                , new Vector2(240f, 0f) + _hudPosition, Color.White);
            spriteBatch.DrawString(_fonts[0], $"Y: {(int)PlayerManager.Instance.Player.Position.Y}"
                , new Vector2(320f, 0f) + _hudPosition, Color.White);

            var textString1 = "Player HP: " + PlayerManager.Instance.Player.HP;
            Vector2 textSize1 = _fonts[0].MeasureString(textString1);
            var position = new Vector2((GameGlobals.Instance.GameScreen.X - textSize1.X) / 2, 0);
            //position.X += (string.X - font.MeasureString(string).X) / 2;
            spriteBatch.Draw(_heartTexture, new Vector2(position.X - 32f, 0f) + _hudPosition, null
                , Color.White, 0f, Vector2.Zero, 0.75f, SpriteEffects.None, 0f);
            spriteBatch.DrawString(_fonts[0], textString1, new Vector2(position.X, 0f) + _hudPosition, Color.White);

            if (InventoryManager.Instance.Inventory.TryGetValue("0", out InventoryItemData value_0))
            {
                spriteBatch.Draw(_herb1Texture, new Vector2(position.X + 400f, 0f) + _hudPosition, null
                , Color.White, 0f, Vector2.Zero, 0.75f, SpriteEffects.None, 0f);
                spriteBatch.DrawString(_fonts[0], $" {value_0.Count}"
                    , new Vector2(position.X + 400f + 32f, 0f) + _hudPosition, Color.White);
            }

            if (InventoryManager.Instance.Inventory.TryGetValue("1", out InventoryItemData value_1))
            {
                spriteBatch.Draw(_herb2Texture, new Vector2(position.X + 480f, 0f) + _hudPosition, null
                , Color.White, 0f, Vector2.Zero, 0.75f, SpriteEffects.None, 0f);
                spriteBatch.DrawString(_fonts[0], $" {value_1.Count}"
                    , new Vector2(position.X + 480f + 32f, 0f) + _hudPosition, Color.White);
            }

            if (InventoryManager.Instance.Inventory.TryGetValue("2", out InventoryItemData value_2))
            {
                spriteBatch.Draw(_drugTexture, new Vector2(position.X + 560f, 0f) + _hudPosition, null
                , Color.White, 0f, Vector2.Zero, 0.75f, SpriteEffects.None, 0f);
                spriteBatch.DrawString(_fonts[0], $" {value_2.Count}"
                    , new Vector2(position.X + 560f + 32f, 0f) + _hudPosition, Color.White);
            }

            spriteBatch.Draw(_coinTexture, new Vector2(position.X + 640f, 0f) + _hudPosition, null
                , Color.White, 0f, Vector2.Zero, 0.75f, SpriteEffects.None, 0f);
            spriteBatch.DrawString(_fonts[0], $" {InventoryManager.Instance.GoldCoin}"
                , new Vector2(position.X + 640f + 32f, 0f) + _hudPosition, Color.White);
        }

        private void DrawPressF(SpriteBatch spriteBatch)
        {
            if (GameGlobals.Instance.IsDetectedGameObject)
            {
                spriteBatch.Draw(_pressFTexture, new Vector2(930f, 550f) + _hudPosition, null
                    , Color.White, 0f, Vector2.Zero, 0.75f, SpriteEffects.None, 0f);
            }
        }

        private void DrawInsufficientSign(SpriteBatch spriteBatch)
        {
            if (GameGlobals.Instance.ShowInsufficientSign)
            {
                spriteBatch.Draw(_insufficient, PlayerManager.Instance.Player.Position
                    + new Vector2(25, -((PlayerManager.Instance.Player.Sprite.TextureRegion.Height / 2) + 25))
                    , null, Color.White, 0f, Vector2.Zero, 0.40f, SpriteEffects.None, 0f);
            }
        }

        private void DrawHPMobs(SpriteBatch spriteBatch)
        {
            var entities = EntityManager.Instance.Entities;

            foreach (var entity in entities.Where(e => !e.IsDestroyed))
            {
                var entityPos = entity.Position;
                var posX = entity.Sprite.TextureRegion.Width / 2.5f;
                var posY = entity.Sprite.TextureRegion.Height;

                spriteBatch.DrawString(_fonts[0], $"{entity.HP}", entityPos - new Vector2(posX, posY + 10)
                    , Color.DarkRed, 0f, Vector2.Zero, 1.5f, SpriteEffects.None, 0f);
            }
        }

        private void DrawDamageNumbers(SpriteBatch spriteBatch)
        {
            var entities = EntityManager.Instance.Entities;

            foreach (var entity in entities.Where(e => !e.IsDestroyed))
            {
                if (entity.IsAttacked) entity.DrawDamageNumbers(spriteBatch);
            }

            if (PlayerManager.Instance.Player.IsAttacked) PlayerManager.Instance.Player.DrawDamageNumbers(spriteBatch);
        }

        private void DrawCollectedItem(SpriteBatch spriteBatch) 
        {
            if (GameGlobals.Instance.CollectedItemFeed.Count != 0)
            {
                int n;
                if (GameGlobals.Instance.CollectedItemFeed.Count >= GameGlobals.Instance.MaximumItemFeed)
                {
                    n = GameGlobals.Instance.MaximumItemFeed;
                }
                else n = GameGlobals.Instance.CollectedItemFeed.Count;

                for (int i = 0; i < n; i++)
                {
                    var referId = GameGlobals.Instance.CollectedItemFeed.ElementAt(i).ItemId;
                    var amount = GameGlobals.Instance.CollectedItemFeed.ElementAt(i).Count;
                    var addingX = GameGlobals.Instance.HUDPosition.X;
                    var addingY = GameGlobals.Instance.HUDPosition.Y;

                    spriteBatch.FillRectangle(355f + addingX, 496f + (i * 40) + addingY, 120, 28, Color.Black * 0.4f);

                    _transform = new Transform2
                    {
                        Scale = new Vector2(0.75f, 0.75f),
                        Rotation = 0f,
                        Position = new Vector2(370f, 510f + (i * 40)) + _hudPosition
                    };

                    _sprite.Play(referId.ToString());
                    _sprite.Update(_deltaSeconds);

                    spriteBatch.Draw(_sprite, _transform);

                    spriteBatch.DrawString(_fonts[2], $"{GameGlobals.Instance.ItemDatas[referId].Name} x {amount}"
                        , new Vector2(360f + 32f, 495f + (i * 40)) + _hudPosition, Color.White);
                }

                if (_nextFeed)
                {
                    if (GameGlobals.Instance.CollectedItemFeed.Count >= GameGlobals.Instance.MaximumItemFeed)
                    {
                        GameGlobals.Instance.CollectedItemFeed.RemoveRange(0, GameGlobals.Instance.MaximumItemFeed);
                    }
                    else GameGlobals.Instance.CollectedItemFeed.RemoveRange(0, GameGlobals.Instance.CollectedItemFeed.Count);
                }
            }
        }

        public static void ShowInsufficientSign()
        {
            GameGlobals.Instance.ShowInsufficientSign = true;
        }

        public static void AddFeed(int referId, int amount)
        {
            GameGlobals.Instance.CollectedItemFeed.Add(new InventoryItemData() {
                ItemId = referId,
                Count = amount
            });

            GameGlobals.Instance.DisplayFeedTime = GameGlobals.Instance.MaximumDisplayFeedTime;
        }
    }
}
