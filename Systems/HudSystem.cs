﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using System.Linq;

namespace Medicraft.Systems
{
    public class HudSystem
    {
        private readonly BitmapFont _font1, _font2;

        private Vector2 _hudPosition;

        private Texture2D _heartTexture, _herb1Texture, _herb2Texture, _drugTexture
            , _coinTexture, _pressFTexture, _insufficient;

        private bool _nextFeed;

        private float _displayInsufficientTime;

        public HudSystem(BitmapFont font1, BitmapFont font2, Texture2D[] texture)
        {
            _font1 = font1;
            _font2 = font2;
            _heartTexture = texture[0];
            _herb1Texture = texture[1];
            _herb2Texture = texture[2];
            _drugTexture = texture[3];
            _coinTexture = texture[4];
            _pressFTexture = texture[5];
            _insufficient = texture[6];

            _nextFeed = false;
            _displayInsufficientTime = 3f;

            GameGlobals.Instance.HUDPosition = PlayerManager.Instance.Player.Position - new Vector2(720, 450);
        }

        public void Update(GameTime gameTime)
        {
            var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (GameGlobals.Instance.ItemsFeed.Count != 0)
            {
                GameGlobals.Instance.DisplayFeedTime -= deltaSeconds;

                if (GameGlobals.Instance.DisplayFeedTime <= 0)
                {
                    _nextFeed = true;
                    GameGlobals.Instance.DisplayFeedTime = 6f;
                }
                else _nextFeed = false;
            }

            if (GameGlobals.Instance.ShowInsufficientSign)
            {
                _displayInsufficientTime -= deltaSeconds;

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

            // Draw HUD Bar
            spriteBatch.FillRectangle(0 + addingX, 0 + addingY, GameGlobals.Instance.GameScreen.X
                , 25, Color.Black * 0.4f);
            spriteBatch.DrawString(_font1, $" Mobs: {EntityManager.Instance.entities.Count}"
                , Vector2.Zero + _hudPosition, Color.White);
            spriteBatch.DrawString(_font1, $"Time Spawn: {(int)EntityManager.Instance.spawnTime}"
                , new Vector2(100f, 0f) + _hudPosition, Color.White);
            spriteBatch.DrawString(_font1, $"X: {(int)PlayerManager.Instance.Player.Position.X}"
                , new Vector2(240f, 0f) + _hudPosition, Color.White);
            spriteBatch.DrawString(_font1, $"Y: {(int)PlayerManager.Instance.Player.Position.Y}"
                , new Vector2(320f, 0f) + _hudPosition, Color.White);

            var textString1 = "Player HP: " + PlayerManager.Instance.Player.HP;
            Vector2 textSize1 = _font1.MeasureString(textString1);
            var position = new Vector2((GameGlobals.Instance.GameScreen.X - textSize1.X) / 2, 0);
            //position.X += (string.X - font.MeasureString(string).X) / 2;
            spriteBatch.Draw(_heartTexture, new Vector2(position.X - 32f, 0f) + _hudPosition, null
                , Color.White, 0f, Vector2.Zero, 0.75f, SpriteEffects.None, 0f);
            spriteBatch.DrawString(_font1, textString1, new Vector2(position.X, 0f) + _hudPosition, Color.White);

            spriteBatch.Draw(_herb1Texture, new Vector2(position.X + 400f, 0f) + _hudPosition, null
                , Color.White, 0f, Vector2.Zero, 0.75f, SpriteEffects.None, 0f);
            spriteBatch.DrawString(_font1, $" {PlayerManager.Instance.Inventory["herb_1"]}"
                , new Vector2(position.X + 400f + 32f, 0f) + _hudPosition, Color.White);

            spriteBatch.Draw(_herb2Texture, new Vector2(position.X + 480f, 0f) + _hudPosition, null
                , Color.White, 0f, Vector2.Zero, 0.75f, SpriteEffects.None, 0f);
            spriteBatch.DrawString(_font1, $" {PlayerManager.Instance.Inventory["herb_2"]}"
                , new Vector2(position.X + 480f + 32f, 0f) + _hudPosition, Color.White);

            spriteBatch.Draw(_drugTexture, new Vector2(position.X + 560f, 0f) + _hudPosition, null
                , Color.White, 0f, Vector2.Zero, 0.75f, SpriteEffects.None, 0f);
            spriteBatch.DrawString(_font1, $" {PlayerManager.Instance.Inventory["drug"]}"
                , new Vector2(position.X + 560f + 32f, 0f) + _hudPosition, Color.White);

            spriteBatch.Draw(_coinTexture, new Vector2(position.X + 640f, 0f) + _hudPosition, null
                , Color.White, 0f, Vector2.Zero, 0.75f, SpriteEffects.None, 0f);
            spriteBatch.DrawString(_font1, $" {PlayerManager.Instance.Coin}"
                , new Vector2(position.X + 640f + 32f, 0f) + _hudPosition, Color.White);
        }

        private void DrawPressF(SpriteBatch spriteBatch)
        {
            if (GameGlobals.Instance.IsDetectedItem)
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

                spriteBatch.DrawString(_font1, $"{entity.HP}", entityPos - new Vector2(posX, posY + 10)
                    , Color.DarkRed, 0f, Vector2.Zero, 1.5f, SpriteEffects.None, 0f);
            }
        }

        private void DrawCollectedItem(SpriteBatch spriteBatch) 
        {
            if (GameGlobals.Instance.ItemsFeed.Count != 0)
            {
                int n;
                if (GameGlobals.Instance.ItemsFeed.Count >= 6)
                {
                    n = 6;
                }
                else
                {
                    n = GameGlobals.Instance.ItemsFeed.Count;
                }

                for (int i = 0; i < n; i++)
                {
                    var Name = GameGlobals.Instance.ItemsFeed[i];
                    Texture2D texture = null;

                    switch (Name)
                    {
                        case "herb_1":
                            texture = _herb1Texture;
                            break;

                        case "herb_2":
                            texture = _herb2Texture;
                            break;

                        case "drug":
                            texture = _drugTexture;
                            break;
                    }

                    float addingX = GameGlobals.Instance.HUDPosition.X;
                    float addingY = GameGlobals.Instance.HUDPosition.Y;

                    spriteBatch.FillRectangle(355f + addingX, 498f + (i * 40) + addingY, 110, 26, Color.Black * 0.4f);

                    spriteBatch.Draw(texture, new Vector2(360f, 500f + (i * 40)) + _hudPosition, null
                        , Color.White, 0f, Vector2.Zero, 0.75f, SpriteEffects.None, 0f);

                    spriteBatch.DrawString(_font1, $"{Name} x 1", new Vector2(360f + 32f, 500f + (i * 40))
                        + _hudPosition, Color.White);
                }

                if (_nextFeed)
                {
                    if (GameGlobals.Instance.ItemsFeed.Count >= 6)
                    {
                        GameGlobals.Instance.ItemsFeed.RemoveRange(0, 6);
                    }
                    else
                    {
                        GameGlobals.Instance.ItemsFeed.RemoveRange(0, GameGlobals.Instance.ItemsFeed.Count);
                    }
                }
            }
        }

        public static void ShowInsufficientSign()
        {
            GameGlobals.Instance.ShowInsufficientSign = true;
        }

        public static void AddFeed(string itemName)
        {
            GameGlobals.Instance.ItemsFeed.Add(itemName);
            GameGlobals.Instance.DisplayFeedTime = 6f;
        }
    }
}
