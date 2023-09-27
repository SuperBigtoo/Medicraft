using Medicraft.Data.Models;
using Medicraft.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Serialization;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.Content;
using Medicraft.Entities;
using Medicraft.Data;
using System;
using Microsoft.Xna.Framework.Input;

namespace Medicraft.Screens
{
    public class TestScreen : Screen
    {
        private HudSystem _hudSystem;

        private PlayerStats _playerStats;
        private EntityStats _slimeStats;
        private BitmapFont _fontMinecraft, _fontSensation;

        public TestScreen()
        {
        }

        public override void LoadContent()
        {
            base.LoadContent();

            // Test Load Data
            var _playerPos = Vector2.Zero;
            if (Singleton.Instance.gameSave.Count != 0)
            {
                var gameSave = Singleton.Instance.gameSave[Singleton.Instance.gameSaveIdex];
                _playerStats = gameSave.PlayerStats;

                Singleton.Instance.cameraPosition = new Vector2((float)gameSave.Camera_Position[0]
                    , (float)gameSave.Camera_Position[1]);

                Singleton.Instance.addingHudPos = new Vector2((float)gameSave.HUD_Position[0]
                    , (float)gameSave.HUD_Position[1]);
            }
            else
            {
                _playerStats = Content.Load<PlayerStats>("data/models/player_stats");
            }

            // Load Data Game from JSON file
            _slimeStats = Content.Load<EntityStats>("data/models/slime");

            // Load bitmap font
            _fontMinecraft = Content.Load<BitmapFont>("fonts/Mincraft_Ten/Mincraft_Ten");
            _fontSensation = Content.Load<BitmapFont>("fonts/Sensation/Sensation");

            // Adding Player to EntityList
            var _playerAnimation = Content.Load<SpriteSheet>("animation/MCSpriteSheet.sf", new JsonContentLoader());
            var _playerSprite = new AnimatedSprite(_playerAnimation);
            PlayerManager.Instance.player = new Player(_playerSprite, _playerStats);

            // Adding Slime to EntityList
            var _slimeAnimation = Content.Load<SpriteSheet>("animation/Slime_Green.sf", new JsonContentLoader());
            var _slimeSprite = new AnimatedSprite(_slimeAnimation);
            Vector2 _slimeScale = new Vector2(2.0f, 1.5f);
            EntityManager.Instance.AddEntity(new Slime(_slimeSprite, _slimeStats, _slimeScale));

            // Adding HUD
            _hudSystem = new HudSystem(_fontSensation);
        }

        public override void UnloadContent()
        {
            base.UnloadContent();
        }

        public override void Update(GameTime gameTime)
        {
            if (Singleton.Instance.IsGameActive)
            {
                Singleton.Instance.keyboardPreviose = Singleton.Instance.keyboardCurrent;
                Singleton.Instance.keyboardCurrent = Keyboard.GetState();

                EntityManager.Instance.Update(gameTime);
                Camera.SetPosition(Singleton.Instance.cameraPosition + Singleton.Instance.addingCameraPos);

                if (Singleton.Instance.keyboardCurrent.IsKeyDown(Keys.M))
                {
                    JsonFileManager.SaveGame();
                }
            }

            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(_fontMinecraft, $"Name: {_slimeStats.Name}", new Vector2(50, 50), Color.White);
            spriteBatch.DrawString(_fontMinecraft, $"HP: {_slimeStats.HP}", new Vector2(50, 80), Color.White);
            spriteBatch.DrawString(_fontMinecraft, $"ATK: {_slimeStats.ATK}", new Vector2(50, 110), Color.White);
            spriteBatch.DrawString(_fontMinecraft, $"DEF: {_slimeStats.DEF_Percent}", new Vector2(50, 140), Color.White);
            spriteBatch.DrawString(_fontMinecraft, $"Speed: {_slimeStats.Speed}", new Vector2(50, 170), Color.White);
            spriteBatch.DrawString(_fontMinecraft, $"Evasion: {_slimeStats.Evasion}", new Vector2(50, 200), Color.White);

            EntityManager.Instance.Draw(spriteBatch);

            _hudSystem.DrawTest(spriteBatch);
        }
    }
}
