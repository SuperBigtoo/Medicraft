using Medicraft.Data.Models;
using Medicraft.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Serialization;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.Content;
using Medicraft.Entities;

namespace Medicraft.Screens
{
    internal class TestScreen : Screen
    {
        private EntityStats _slime;
        private BitmapFont _font;

        public TestScreen()
        {
            Singleton.Instance.playerPosition = new Vector2((Singleton.Instance.gameScreen.X - 70) / 2, (Singleton.Instance.gameScreen.Y - 110) / 2);
        }

        public override void LoadContent(Camera camera)
        {
            base.LoadContent(camera);

            // Test Player
            var _spriteSheet = Content.Load<SpriteSheet>("animation/MCSpriteSheet.sf", new JsonContentLoader());
            var _playerSprite = new AnimatedSprite(_spriteSheet);
            Singleton.Instance._player = EntityManager.Instance.AddEntity(new Player(_playerSprite, Singleton.Instance.playerPosition));

            // Test Load Data
            // Load Data Game from JSON file
            _slime = Content.Load<EntityStats>("data/models/slime");
            // Load bitmap font
            _font = Content.Load<BitmapFont>("fonts/Mincraft_Ten/Mincraft_Ten");
        }

        public override void UnloadContent()
        {
            base.UnloadContent();
        }

        public override void Update(GameTime gameTime)
        {
            EntityManager.Instance.Update(gameTime);
            Camera.SetPosition(Singleton.Instance._player.Position);

            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(_font, _slime.Name, new Vector2(50, 50), Color.White);
            spriteBatch.DrawString(_font, "" + _slime.HP, new Vector2(50, 80), Color.White);
            spriteBatch.DrawString(_font, "" + _slime.Atk, new Vector2(50, 110), Color.White);
            spriteBatch.DrawString(_font, "" + _slime.Def, new Vector2(50, 140), Color.White);

            EntityManager.Instance.Draw(spriteBatch);
        }
    }
}
