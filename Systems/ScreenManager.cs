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
        private SpriteBatch spriteBatch;
        private Screen screen;

        private static ScreenManager instance;

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
            screen = new SplashScreen();
            CurrentScreen = GameScreen.SplashScreen;
        }

        public void LoadScreen(GameScreen gameScreen)
        {
            UnloadContent();

            switch (gameScreen)
            {
                case GameScreen.TestScreen:
                    screen = new TestScreen();
                    CurrentScreen = GameScreen.TestScreen;
                    screen.LoadContent();
                    break;

                case GameScreen.SplashScreen:
                    screen = new SplashScreen();
                    CurrentScreen = GameScreen.SplashScreen;
                    screen.LoadContent();
                    break;
            }
        }

        public void LoadContent(Game game, ContentManager content)
        {  
            Game = game;
            Content = new ContentManager(content.ServiceProvider, "Content");
            GraphicsDevice = game.GraphicsDevice;
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Camera = new Camera(GraphicsDevice.Viewport);
            Window = game.Window;

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

            screen.LoadContent();
        }

        public void UnloadContent()
        {
            screen?.UnloadContent();
        }

        public void Dispose()
        {
            screen?.Dispose();
        }

        public void Update(GameTime gameTime)
        {
            GameGlobals.Instance.IsGameActive = Game.IsActive;

            if (GameGlobals.Instance.IsGameActive)
            {
                if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                    Game.Exit();
            }

            screen.Update(gameTime);
        }

        public void Draw()
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin
            (
                SpriteSortMode.BackToFront,
                samplerState: SamplerState.PointClamp,
                blendState: BlendState.AlphaBlend,
                transformMatrix: Camera.GetTransform(GraphicsDevice.Viewport.Width
                , GraphicsDevice.Viewport.Height)
            );

            screen.Draw(spriteBatch);

            spriteBatch.End();
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
