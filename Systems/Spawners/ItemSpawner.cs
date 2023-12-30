using Medicraft.GameObjects;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;


namespace Medicraft.Systems.Spawners
{
    public class ItemSpawner : IItemManager
    {
        public readonly List<Item> itemsInitial;

        private readonly List<Item> itemsDestroyed;
        private readonly List<Item> itemsSpawn;

        public float spawnTime;
        public bool IsSpawn;

        public ItemSpawner(float spawnTime)
        {
            this.spawnTime = spawnTime;
            IsSpawn = false;
            itemsInitial = new List<Item>();
            itemsDestroyed = new List<Item>();
            itemsSpawn = new List<Item>();
        }

        public void Initialize()
        {
            foreach (var item in itemsInitial)
            {
                Item clonedItem = item.Clone() as Item;
                ObjectManager.Instance.AddItem(clonedItem);
            }
        }

        public void Spawn()
        {
            if (IsSpawn)
            {
                foreach (var item in itemsSpawn)
                {
                    ObjectManager.Instance.AddItem(item);
                }
                IsSpawn = false;
                itemsSpawn.Clear();
            }
        }

        public T AddItem<T>(T item) where T : Item
        {
            itemsInitial.Add(item);
            return item;
        }

        public void Update(GameTime gameTime)
        {
            var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            spawnTime -= deltaSeconds;

            foreach (var item in ObjectManager.Instance.Items.Where(e => e.IsDestroyed))
            {
                itemsDestroyed.Add(item);
            }

            if (spawnTime <= 0)
            {
                if (itemsDestroyed.Count != 0)
                {
                    foreach (var itemDestroyed in itemsDestroyed)
                    {
                        foreach (var itemAdding in itemsInitial)
                        {
                            if (itemDestroyed.Id == itemAdding.Id)
                            {
                                Item clonedItem = itemAdding.Clone() as Item;
                                itemsSpawn.Add(clonedItem);
                            }
                        }
                    }
                    itemsDestroyed.Clear();
                }

                IsSpawn = true;
                spawnTime = 10f;
            }
        }
    }
}
