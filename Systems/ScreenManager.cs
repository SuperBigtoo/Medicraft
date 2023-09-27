using Medicraft.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Medicraft.Systems
{
    public class ScreenManager
    {
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
            currentScreen = new TestScreen();
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

        public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice, Camera camera)
        {
            Content = new ContentManager(content.ServiceProvider, "Content");
            GraphicsDevice = graphicsDevice;
            Camera = camera;

            currentScreen.LoadContent();
        }

        public void UnloadContent()
        {
            currentScreen.UnloadContent();
        }

        public void Update(GameTime gameTime)
        {
            currentScreen.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            currentScreen.Draw(spriteBatch);
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
