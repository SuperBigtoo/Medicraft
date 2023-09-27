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
            float addingX = _hudPosition.X;
            float addingY = _hudPosition.Y;

            spriteBatch.FillRectangle(0 + addingX, 0 + addingY, 1440, 20, Color.Black * 0.4f);
            spriteBatch.DrawString(_font, $"entities: {EntityManager.Instance.entities.Count}"
                , (Vector2.Zero + _hudPosition), Color.White);
            spriteBatch.DrawString(_font, $"X: {PlayerManager.Instance.player.Position.X}"
                , (new Vector2(120f, 0f) + _hudPosition), Color.White);
            spriteBatch.DrawString(_font, $"Y: {PlayerManager.Instance.player.Position.Y}"
                , (new Vector2(240f, 0f) + _hudPosition), Color.White);
        }
    }
}
