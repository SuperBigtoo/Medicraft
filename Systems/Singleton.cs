using Medicraft.Entites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Medicraft.Systems
{
    public class Singleton
    {
        public Player _player;
        public readonly List<Entity> _entities;

        public MouseState mousePreviose, mouseCurrent;
        public KeyboardState keyboardPreviose, keyboardCurrent;
        public Vector2 gameScreen = new Vector2(1280, 720);
        public Vector2 _playerPosition;
        public bool IsGameActive;

        private static Singleton instance;

        private Singleton()
        {
            _entities = new List<Entity>();
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
