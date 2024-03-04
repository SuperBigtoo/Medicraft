﻿using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Medicraft.Systems.Spawners;
using Medicraft.GameObjects;

namespace Medicraft.Systems.Managers
{
    public interface IObjectManager
    {
        T AddGameObject<T>(T gameObject) where T : GameObject;
    }

    public class ObjectManager : IObjectManager
    {
        public float SpawnTime = 0f;

        public readonly List<GameObject> gameObjects;

        private ObjectSpawner _objectSpawner;

        private static ObjectManager instance;

        public IEnumerable<GameObject> GameObjects => gameObjects;

        private ObjectManager()
        {
            gameObjects = [];
        }

        public T AddGameObject<T>(T gameObject) where T : GameObject
        {
            gameObjects.Add(gameObject);
            return gameObject;
        }

        public void Initialize(ObjectSpawner objectSpawner)
        {
            gameObjects.Clear();
            _objectSpawner = objectSpawner;
            _objectSpawner.Initialize();
        }

        public void Update(GameTime gameTime)
        {
            var layerDepth = 0.85f;

            foreach (var gameObject in gameObjects.Where(e => !e.IsDestroyed))
            {
                layerDepth -= 0.00001f;
                gameObject.Update(gameTime, layerDepth);
            }

            // Object Spawner
            _objectSpawner.Update(gameTime);
            SpawnTime = _objectSpawner.SpawnTime;

            gameObjects.RemoveAll(e => e.IsDestroyed);

            _objectSpawner.Spawn();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var gameObject in gameObjects.Where(e => !e.IsDestroyed))
            {
                gameObject.Draw(spriteBatch);
            }
        }

        public static ObjectManager Instance
        {
            get
            {
                instance ??= new ObjectManager();
                return instance;
            }
        }
    }
}