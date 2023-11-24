﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;

namespace Medicraft.Systems
{
    public class HudSystem
    {
        private readonly BitmapFont _font;
        private Vector2 _hudPosition;

        public HudSystem(BitmapFont font)
        {
            _font = font;
            GameGlobals.Instance.HUDPosition = PlayerManager.Instance.Player.Position - new Vector2(720, 450);
        }

        public void DrawTest(SpriteBatch spriteBatch)
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

            // Draw HUD
            spriteBatch.FillRectangle(0 + addingX, 0 + addingY, GameGlobals.Instance.GameScreen.X
                , 20, Color.Black * 0.4f);
            spriteBatch.DrawString(_font, $" Mobs: {EntityManager.Instance.entities.Count}"
                , Vector2.Zero + _hudPosition, Color.White);
            spriteBatch.DrawString(_font, $"X: {(int)PlayerManager.Instance.Player.Position.X}"
                , new Vector2(120f, 0f) + _hudPosition, Color.White);
            spriteBatch.DrawString(_font, $"Y: {(int)PlayerManager.Instance.Player.Position.Y}"
                , new Vector2(240f, 0f) + _hudPosition, Color.White);
        }
    }
}
