using Medicraft.Data.Models;
using Medicraft.GameObjects;
using Medicraft.Systems.Managers;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Sprites;
using System.Collections.Generic;
using System.Linq;

namespace Medicraft.Systems.Spawners
{
    public class ObjectSpawner : IObjectManager
    {
        public readonly SpawnerData spawnerData;
        public readonly string currMapName;

        public float SpawnTime;
        public float SpawnTimer;

        private readonly List<GameObject> _initialObjects = [];
        private readonly List<GameObject> _destroyedObjects = [];
        private readonly List<GameObject> _spawningObjects = [];

        public ObjectSpawner(SpawnerData spawnerData, string currMapName, List<ObjectData> objectDatas)
        {
            this.spawnerData = spawnerData;
            this.currMapName = currMapName;

            SetupSpawner(objectDatas);
        }

        public void Initialize()
        {
            foreach (var obj in _initialObjects)
            {
                bool isFound = false;

                foreach (var destroyedObj in _destroyedObjects)
                {
                    if (obj.Id.Equals(destroyedObj.Id))
                    {
                        isFound = true;
                        break;
                    }
                }

                if (!isFound)
                {
                    GameObject clonedObject = obj.Clone() as GameObject;
                    ObjectManager.Instance.AddGameObject(clonedObject);
                }
            }
        }

        public T AddGameObject<T>(T gameObject) where T : GameObject
        {
            _initialObjects.Add(gameObject);
            return gameObject;
        }

        public void Update(GameTime gameTime)
        {
            var mapSpawnTime = spawnerData.MapSpawnTimes.FirstOrDefault
                (m => m.SpawnerName.Equals("Object") && m.MapName.Equals(currMapName));

            SpawnTime = mapSpawnTime.SpawnTime;
            SpawnTimer = mapSpawnTime.SpawnTimer;


            foreach (var obj in ObjectManager.Instance.GameObjects.Where(e => e.IsDestroyed))
            {
                _destroyedObjects.Add(obj);
            }

            if (SpawnTimer < 0)
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

                Respawn();
            }
        }

        private void Respawn()
        {
            foreach (var obj in _spawningObjects.Where(o => o.IsRespawnable))
            {
                ObjectManager.Instance.AddGameObject(obj);
            }
            _spawningObjects.Clear();
        }

        public void SetupSpawner(List<ObjectData> objectDatas)
        {
            foreach (var gameObjectData in objectDatas)
            {
                var category = gameObjectData.Category;

                SpriteSheet spriteSheets;
                switch (category)
                {
                    case 0: // Item
                        spriteSheets = GameGlobals.Instance.ItemsPackSprites;
                        AddGameObject(new Item(new AnimatedSprite(spriteSheets), gameObjectData, Vector2.One));
                        break;

                    case 1: // QuestObject
                        spriteSheets = GameGlobals.Instance.UIBooksIconHUD;
                        break;

                    case 2: // CraftingTable
                        spriteSheets = GameGlobals.Instance.UIBooksIconHUD;
                        AddGameObject(new CraftingTable(new AnimatedSprite(spriteSheets), gameObjectData, Vector2.One));
                        break;

                    case 3: // SavingTable
                        spriteSheets = GameGlobals.Instance.UIBooksIconHUD;
                        AddGameObject(new SavingTable(new AnimatedSprite(spriteSheets), gameObjectData, Vector2.One));
                        break;

                    case 4: // WarpPoint
                        spriteSheets = GameGlobals.Instance.WarpPointSprite;
                        AddGameObject(new WarpPoint(new AnimatedSprite(spriteSheets), gameObjectData, Vector2.One));
                        break;

                    case 6: // SavingTable
                        spriteSheets = GameGlobals.Instance.UIBooksIconHUD;
                        AddGameObject(new RestPoint(new AnimatedSprite(spriteSheets), gameObjectData, Vector2.One));
                        break;
                }
            }
        }
    }
}
