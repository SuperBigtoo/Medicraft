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
        private PlayerStats _playerStats;
        private EntityStats _slimeStats;
        private BitmapFont _font;

        public TestScreen()
        {
            Singleton.Instance.playerPosition = new Vector2((Singleton.Instance.gameScreen.X - 70) / 2, (Singleton.Instance.gameScreen.Y - 110) / 2);
        }

        public override void LoadContent(Camera camera)
        {
            base.LoadContent(camera);

            // Test Load Data
            // Load Data Game from JSON file
            _playerStats = Content.Load<PlayerStats>("data/models/player_stats");
            _slimeStats = Content.Load<EntityStats>("data/models/slime");

            // Load bitmap font
            _font = Content.Load<BitmapFont>("fonts/Mincraft_Ten/Mincraft_Ten");

            // Adding Player to EntityList
            var _playerAnimation = Content.Load<SpriteSheet>("animation/MCSpriteSheet.sf", new JsonContentLoader());
            var _playerSprite = new AnimatedSprite(_playerAnimation);
            Singleton.Instance._player = EntityManager.Instance.AddEntity(new Player(_playerSprite
                ,Singleton.Instance.playerPosition, _playerStats));

            // Adding Slime to EntityList
            var _slimeAnimation = Content.Load<SpriteSheet>("animation/Slime_Green.sf", new JsonContentLoader());
            var _slimeSprite = new AnimatedSprite(_slimeAnimation);
            Vector2 _slimePos = new Vector2(((Singleton.Instance.gameScreen.X - 48) / 2) - 250, (Singleton.Instance.gameScreen.Y - 48) / 2);
            EntityManager.Instance.AddEntity(new Slime(_slimeSprite, _slimePos, _slimeStats));
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
            spriteBatch.DrawString(_font, _slimeStats.Name, new Vector2(50, 50), Color.White);
            spriteBatch.DrawString(_font, "" + _slimeStats.HP, new Vector2(50, 80), Color.White);
            spriteBatch.DrawString(_font, "" + _slimeStats.ATK, new Vector2(50, 110), Color.White);
            spriteBatch.DrawString(_font, "" + _slimeStats.DEF_Percent, new Vector2(50, 140), Color.White);
            spriteBatch.DrawString(_font, "" + _slimeStats.Speed, new Vector2(50, 170), Color.White);
            spriteBatch.DrawString(_font, "" + _slimeStats.Evasion, new Vector2(50, 200), Color.White);

            EntityManager.Instance.Draw(spriteBatch);
        }
    }
}
