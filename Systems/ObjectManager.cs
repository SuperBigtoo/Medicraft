using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Medicraft.Systems.Spawners;
using Medicraft.GameObjects;
using OpenRpg.Items;

namespace Medicraft.Systems
{
    public interface IObjectManager
    {
        T AddGameObject<T>(T gameObject) where T : GameObject;
    }

    public interface IItemManager
    {
        T AddItem<T>(T item) where T : Item;
    }

    public class ObjectManager : IObjectManager, IItemManager
    {
        private static ObjectManager instance;

        private ItemSpawner itemSpawner;

        public float spawnTime = 0f;

        public readonly List<GameObject> gameObjects;
        public readonly List<Item> items;

        public IEnumerable<GameObject> GameObjects => gameObjects;
        public IEnumerable<Item> Items => items;

        private ObjectManager()
        {
            gameObjects = new List<GameObject>();
            items = new List<Item>();          
        }

        public T AddGameObject<T>(T gameObject) where T : GameObject
        {
            gameObjects.Add(gameObject);
            return gameObject;
        }

        public T AddItem<T>(T item) where T : Item
        {
            items.Add(item);
            return item;
        }

        public void Initialize(ItemSpawner itemSpawner)
        {
            gameObjects.Clear();
            items.Clear();
            this.itemSpawner = itemSpawner;
            this.itemSpawner.Initialize();
        }

        public void Update(GameTime gameTime)
        {
            var layerDepth = 0.8f;

            foreach (var item in items.Where(e => !e.IsDestroyed))
            {
                layerDepth -= 0.00001f;
                item.Update(gameTime, layerDepth);
            }

            foreach (var gameObject in gameObjects.Where(e => !e.IsDestroyed))
            {
                layerDepth -= 0.00001f;
                gameObject.Update(gameTime, layerDepth);
            }

            // ItemSpawner
            itemSpawner.Update(gameTime);
            spawnTime = itemSpawner.spawnTime;

            items.RemoveAll(e => e.IsDestroyed);

            itemSpawner.Spawn();
        }

        public void Draw(SpriteBatch spriteBatch)
        {           
            foreach (var item in items.Where(e => !e.IsDestroyed))
            {
                item.Draw(spriteBatch);
            }

            foreach (var gameObject in gameObjects.Where(e => !e.IsDestroyed))
            {
                gameObject.Draw(spriteBatch);
            }
        }

        public static ObjectManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ObjectManager();
                }
                return instance;
            }
        }
    }
}
