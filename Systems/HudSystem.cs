using Microsoft.Xna.Framework;
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
            _hudPosition = Singleton.Instance.addingHudPos;
        }

        public void DrawTest(SpriteBatch spriteBatch)
        {
            _hudPosition = Singleton.Instance.addingHudPos;

            float addingX = Singleton.Instance.addingHudPos.X;
            float addingY = Singleton.Instance.addingHudPos.Y;

            spriteBatch.End();
            spriteBatch.Begin(
                SpriteSortMode.Deferred,
                samplerState: SamplerState.PointClamp,
                blendState: BlendState.AlphaBlend,
                transformMatrix: ScreenManager.Instance.Camera.GetTransform()
            );

            // Draw HUD
            spriteBatch.FillRectangle(0 + addingX, 0 + addingY, Singleton.Instance.gameScreen.X
                , 20, Color.Black * 0.4f);
            spriteBatch.DrawString(_font, $" Mobs: {EntityManager.Instance.entities.Count}"
                , Vector2.Zero + _hudPosition, Color.White);
            spriteBatch.DrawString(_font, $"X: {(int)PlayerManager.Instance.GetPlayer().Position.X}"
                , new Vector2(120f, 0f) + _hudPosition, Color.White);
            spriteBatch.DrawString(_font, $"Y: {(int)PlayerManager.Instance.GetPlayer().Position.Y}"
                , new Vector2(240f, 0f) + _hudPosition, Color.White);
        }
    }
}
