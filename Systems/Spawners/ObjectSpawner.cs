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
            foreach (var obj in initialObjects)
            {
                GameObject clonedObject = obj.Clone() as GameObject;
                ObjectManager.Instance.AddGameObject(clonedObject);
            }
        }

        public void Spawn()
        {
            if (IsSpawn)
            {
                foreach (var obj in spawningObjects)
                {
                    ObjectManager.Instance.AddGameObject(obj);
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

            foreach (var obj in ObjectManager.Instance.GameObjects.Where(e => e.IsDestroyed))
            {
                destroyedObjects.Add(obj);
            }

            if (spawnTime <= 0)
            {
                if (destroyedObjects.Count != 0)
                {
                    var clonedEntities = destroyedObjects.Join(initialObjects,
                            destroyedEntity => destroyedEntity.Id,
                            initialEntity => initialEntity.Id,
                            (destroyedEntity, initialEntity) => initialEntity.Clone() as GameObject).ToList();

                    spawningObjects.AddRange(clonedEntities);
                    destroyedObjects.Clear();
                }

                IsSpawn = true;
                spawnTime = initialSpawnTime;
            }
        }
    }
}
