using Medicraft.Data;
using Medicraft.Data.Models;
using Medicraft.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.BitmapFonts;
using System.Collections.Generic;

namespace Medicraft.Systems
{
    public class ScreenManager
    {
        private static ScreenManager instance;

        private SpriteBatch _spriteBatch;
        private Screen _curScreen;  

        public Game Game { private set; get; }
        public ContentManager Content { private set; get; }
        public GraphicsDevice GraphicsDevice { private set; get; }
        public GameWindow Window { private set; get; }
        public Camera Camera { private set; get; }
        public GameScreen CurrentScreen { private set; get; }
        public enum GameScreen
        {
            TestScreen,
            SplashScreen,
            MainMenuScreen,
            PlayScreen
        }

        public ScreenManager()
        {
            _curScreen = new SplashScreen();
            CurrentScreen = GameScreen.SplashScreen;
        }

        public void Initialize(Game game)
        {
            Game = game;
            Content = game.Content;
            GraphicsDevice = game.GraphicsDevice;
            Window = game.Window;
            Camera = new Camera(GraphicsDevice.Viewport);

            _spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        public void LoadContent()
        {
            // Load GameSave
            var gameSave = JsonFileManager.LoadFlie(GameGlobals.Instance.GameSavePath);
            if (gameSave.Count != 0)
            {
                foreach (var save in gameSave)
                {
                    GameGlobals.Instance.GameSave.Add(save);
                }
            }

            // Load Item Datas
            GameGlobals.Instance.ItemsDatas = Content.Load<List<ItemData>>("data/models/items");

            // Load Character Datas
            GameGlobals.Instance.CharacterDatas = Content.Load<List<CharacterData>>("data/models/characters");

            // Load Font Bitmap
            GameGlobals.Instance.FontTA16Bit = Content.Load<BitmapFont>("fonts/TA_16_Bit/TA_16_Bit");

            _curScreen.LoadContent();
        }

        public void UnloadContent()
        {
            _curScreen?.UnloadContent();
        }

        public void Dispose()
        {
            _curScreen?.Dispose();
        }

        public void LoadScreen(GameScreen gameScreen)
        {
            UnloadContent();

            switch (gameScreen)
            {
                case GameScreen.TestScreen:
                    _curScreen = new TestScreen();
                    CurrentScreen = GameScreen.TestScreen;
                    _curScreen.LoadContent();
                    break;

                case GameScreen.SplashScreen:
                    _curScreen = new SplashScreen();
                    CurrentScreen = GameScreen.SplashScreen;
                    _curScreen.LoadContent();
                    break;
            }
        }

        public void Update(GameTime gameTime)
        {
            GameGlobals.Instance.IsGameActive = Game.IsActive;

            if (GameGlobals.Instance.IsGameActive)
            {
                if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                    Game.Exit();
            }

            _curScreen.Update(gameTime);
        }

        public void Draw()
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin
            (
                SpriteSortMode.BackToFront,
                samplerState: SamplerState.PointClamp,
                blendState: BlendState.AlphaBlend,
                transformMatrix: Camera.GetTransform(GraphicsDevice.Viewport.Width
                , GraphicsDevice.Viewport.Height)
            );

            _curScreen.Draw(_spriteBatch);

            _spriteBatch.End();
        }

        public static ScreenManager Instance
        {
            get
            {
                instance ??= new ScreenManager();
                return instance;
            }
        }
    }
}
