using Microsoft.Xna.Framework.Graphics;
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
        private static ObjectManager instance;

        private ObjectSpawner objectSpawner;

        public float spawnTime = 0f;

        public readonly List<GameObject> gameObjects;

        public IEnumerable<GameObject> GameObjects => gameObjects;

        private ObjectManager()
        {
            gameObjects = new List<GameObject>();
        }

        public T AddGameObject<T>(T gameObject) where T : GameObject
        {
            gameObjects.Add(gameObject);
            return gameObject;
        }

        public void Initialize(ObjectSpawner objectSpawner)
        {
            gameObjects.Clear();
            this.objectSpawner = objectSpawner;
            this.objectSpawner.Initialize();
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
            objectSpawner.Update(gameTime);
            spawnTime = objectSpawner.spawnTime;

            gameObjects.RemoveAll(e => e.IsDestroyed);

            objectSpawner.Spawn();
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