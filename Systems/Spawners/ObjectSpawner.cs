using Medicraft.Data.Models;
using Medicraft.Entities;
using Medicraft.GameObjects;
using Medicraft.Systems.Managers;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Sprites;
using System.Collections.Generic;
using System.Linq;


namespace Medicraft.Systems.Spawners
{
    public class ObjectSpawner(float spawnTime, float spawnTimer) : IObjectManager
    {
        public float SpawnTime = spawnTime;
        public float SpawnTimer = spawnTimer;
        public bool IsSpawn = false;

        private readonly List<GameObject> _initialObjects = [];
        private readonly List<GameObject> _destroyedObjects = [];
        private readonly List<GameObject> _spawningObjects = [];

        public void Initialize()
        {
            foreach (var obj in _initialObjects)
            {
                GameObject clonedObject = obj.Clone() as GameObject;
                ObjectManager.Instance.AddGameObject(clonedObject);
            }
        }

        public T AddGameObject<T>(T gameObject) where T : GameObject
        {
            _initialObjects.Add(gameObject);
            return gameObject;
        }

        public void Update(GameTime gameTime)
        {
            var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            SpawnTimer -= deltaSeconds;

            foreach (var obj in ObjectManager.Instance.GameObjects.Where(e => e.IsDestroyed))
            {
                _destroyedObjects.Add(obj);
            }

            if (SpawnTimer <= 0)
            {
                if (_destroyedObjects.Count != 0)
                {
                    var clonedObjects = _destroyedObjects.Join(_initialObjects,
                            destroyedObject => destroyedObject.Id,
                            initialObject => initialObject.Id,
                            (destroyedObject, initialObject) => initialObject.Clone() as GameObject).ToList();

                    _spawningObjects.AddRange(clonedObjects);
                    _destroyedObjects.Clear();
                }

                IsSpawn = true;
                SpawnTimer = SpawnTime;
            }
        }

        public void Spawn()
        {
            if (IsSpawn)
            {
                foreach (var obj in _spawningObjects.Where(o => o.IsRespawnable))
                {
                    ObjectManager.Instance.AddGameObject(obj);
                }

                IsSpawn = false;
                _spawningObjects.Clear();
            }
        }

        public void SetupSpawner(List<ObjectData> objectDatas)
        {
            var spriteSheets = GameGlobals.Instance.ItemsPackSprites;

            foreach (var gameObjectData in objectDatas)
            {
                var category = gameObjectData.Category;

                switch (category)
                {
                    case 0:
                        AddGameObject(new Item(new AnimatedSprite(spriteSheets), gameObjectData, Vector2.One));
                        break;

                    case 1:
                        break;

                    case 2:
                        break;

                    case 3:
                        break;

                    case 4:
                        break;
                }
            }
        }
    }
}
