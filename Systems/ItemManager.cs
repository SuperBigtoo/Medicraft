using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Medicraft.Systems.Spawners;
using Medicraft.Items;

namespace Medicraft.Systems
{
    public interface IItemManager
    {
        T AddItem<T>(T item) where T : Item;
    }

    public class ItemManager : IItemManager
    {
        private static ItemManager instance;

        private ItemSpawner itemSpawner;

        public float spawnTime = 0f;

        public readonly List<Item> items;

        public IEnumerable<Item> Items => items;

        private ItemManager()
        {
            items = new List<Item>();
        }

        public T AddItem<T>(T item) where T : Item
        {
            items.Add(item);
            return item;
        }

        public void Initialize(ItemSpawner itemSpawner)
        {
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
        }

        public static ItemManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ItemManager();
                }
                return instance;
            }
        }
    }
}
