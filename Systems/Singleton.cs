using Medicraft.Entites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Medicraft.Systems
{
    public class Singleton
    {
        public Player _player;

        public MouseState mousePreviose, mouseCurrent;
        public KeyboardState keyboardPreviose, keyboardCurrent;
        public Vector2 gameScreen = new Vector2(1920, 1080);
        public Vector2 _playerPosition;
        public bool IsGameActive;

        private static Singleton instance;

        private Singleton()
        {
            
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
