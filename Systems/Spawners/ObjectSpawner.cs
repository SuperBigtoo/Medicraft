using Medicraft.GameObjects;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;


namespace Medicraft.Systems.Spawners
{
    public class ObjectSpawner : IObjectManager
    {
        public readonly List<GameObject> initialObjects;

        private readonly List<GameObject> destroyedObjects;
        private readonly List<GameObject> spawningObjects;

        public float initialSpawnTime;
        public float spawnTime;
        public bool IsSpawn;

        public ObjectSpawner(float initialSpawnTime)
        {
            this.initialSpawnTime = initialSpawnTime;
            spawnTime = initialSpawnTime;
            IsSpawn = false;
            initialObjects = new List<GameObject>();
            destroyedObjects = new List<GameObject>();
            spawningObjects = new List<GameObject>();
        }

        public void Initialize()
        {
            foreach (var o in initialObjects)
            {
                GameObject clonedObject = o.Clone() as GameObject;
                ObjectManager.Instance.AddGameObject(clonedObject);
            }
        }

        public void Spawn()
        {
            if (IsSpawn)
            {
                foreach (var o in spawningObjects)
                {
                    ObjectManager.Instance.AddGameObject(o);
                }
                IsSpawn = false;
                spawningObjects.Clear();
            }
        }

        public T AddGameObject<T>(T gameObject) where T : GameObject
        {
            initialObjects.Add(gameObject);
            return gameObject;
        }

        public void Update(GameTime gameTime)
        {
            var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            spawnTime -= deltaSeconds;

            foreach (var o in ObjectManager.Instance.GameObjects.Where(e => e.IsDestroyed))
            {
                destroyedObjects.Add(o);
            }

            if (spawnTime <= 0)
            {
                if (destroyedObjects.Count != 0)
                {
                    foreach (var destroyedObj in destroyedObjects)
                    {
                        foreach (var initialObj in initialObjects)
                        {
                            if (destroyedObj.Id == initialObj.Id)
                            {
                                GameObject clonedObject = initialObj.Clone() as GameObject;
                                spawningObjects.Add(clonedObject);
                            }
                        }
                    }
                    destroyedObjects.Clear();
                }

                IsSpawn = true;
                spawnTime = initialSpawnTime;
            }
        }
    }
}
