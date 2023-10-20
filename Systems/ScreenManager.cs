using Medicraft.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Medicraft.Systems
{
    public class ScreenManager
    {
        private SpriteBatch spriteBatch;
        private Screen currentScreen;
        private static ScreenManager instance;

        public ContentManager Content
        {
            private set; get;
        }

        public GraphicsDevice GraphicsDevice
        {
            private set; get;
        }

        public GameWindow Window
        {
            private set; get;
        }

        public Camera Camera
        {
            private set; get;
        }

        public enum GameScreen
        {
            TestScreen,
            SplashScreen,
            PlayScreen
        }

        public ScreenManager()
        {
            currentScreen = new TestScreen(); // TBC
        }

        public void LoadScreen(GameScreen gameScreen)
        {
            switch (gameScreen)
            {
                case GameScreen.TestScreen:
                    currentScreen = new TestScreen();
                    currentScreen.LoadContent();
                    break;
            }
        }

        public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice, GameWindow window)
        {  
            Content = new ContentManager(content.ServiceProvider, "Content");
            GraphicsDevice = graphicsDevice;
            Window = window;
            spriteBatch = new SpriteBatch(graphicsDevice);
            Camera = new Camera(graphicsDevice.Viewport);

            currentScreen.LoadContent();
        }

        public void UnloadContent()
        {
            currentScreen.UnloadContent();
        }

        public void Dispose()
        {
            currentScreen.Dispose();
        }

        public void Update(GameTime gameTime)
        {
            currentScreen.Update(gameTime);
        }

        public void Draw()
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin
            (
                SpriteSortMode.BackToFront,
                samplerState: SamplerState.PointClamp,
                blendState: BlendState.AlphaBlend,
                transformMatrix: Camera.GetTransform()
            );

            currentScreen.Draw(spriteBatch);

            spriteBatch.End();
        }

        public static ScreenManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ScreenManager();
                }
                return instance;
            }
        }
    }
}
