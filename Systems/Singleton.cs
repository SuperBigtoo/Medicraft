using Medicraft.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Medicraft.Systems
{
    public class Singleton
    {
        public Player player;

        public MouseState mousePreviose, mouseCurrent;
        public KeyboardState keyboardPreviose, keyboardCurrent;
        public Vector2 gameScreen;
        public Vector2 playerPosition;
        public bool IsGameActive;

        private static Singleton instance;

        private Singleton()
        {
            gameScreen = new Vector2(1440, 900);
        }

        public static Singleton Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Singleton();
                }
                return instance;
            }
        }
    }
}
