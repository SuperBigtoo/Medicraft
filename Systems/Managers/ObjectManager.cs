using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Medicraft.Systems.Spawners;
using Medicraft.GameObjects;
using Medicraft.Data.Models;
using MonoGame.Extended.Sprites;

namespace Medicraft.Systems.Managers
{
    public interface IObjectManager
    {
        T AddGameObject<T>(T gameObject) where T : GameObject;
    }

    public class ObjectManager : IObjectManager
    {
        public float SpawnTimer { private set; get; } = 0f;
        public readonly List<GameObject> gameObjects;

        private string _currMapName;
        private static ObjectManager instance;

        public IEnumerable<GameObject> GameObjects => gameObjects;
        public List<Crop> Crops { get; private set; } = [];

        private ObjectManager()
        {
            gameObjects = [];
        }

        public T AddGameObject<T>(T gameObject) where T : GameObject
        {
            gameObjects.Add(gameObject);
            return gameObject;
        }

        public Crop AddCropObject(Crop crop)
        {
            Crops.Add(crop);
            return crop;
        }

        public void InitCropObject(List<ObjectData> objectDatas)
        {
            Crops.Clear();

            foreach (var cropObject in objectDatas)
            {
                if (cropObject.Category != 5) return;

                var spriteSheets = GameGlobals.Instance.ItemsPackSprites;
                AddCropObject(new Crop(new AnimatedSprite(spriteSheets), cropObject, Vector2.One));
            }
        }

        public void Initialize(string mapName)
        {
            _currMapName = mapName;
            gameObjects.Clear();
            GameGlobals.Instance.ObjectSpawners.FirstOrDefault
                (s => s.currMapName.Equals(mapName)).Initialize();
        }

        public void Update(GameTime gameTime)
        {
            var layerDepth = 0.85f;

            // Update Object
            foreach (var gameObject in gameObjects.Where(e => !e.IsDestroyed && e.IsVisible))
            {
                layerDepth -= 0.00001f;
                gameObject.Update(gameTime, layerDepth);
            }

            // Update Crop
            foreach (var crop in Crops.Where(e => e.IsVisible))
            {
                if (ScreenManager.Instance.CurrentMap.Equals("noah_home"))
                {
                    layerDepth -= 0.00001f;
                    crop.Update(gameTime, layerDepth);
                }
                else crop.UpdateCropTimer(gameTime);
            }

            // Object Spawner
            GameGlobals.Instance.ObjectSpawners.FirstOrDefault
                (s => s.currMapName.Equals(_currMapName))?.Update(gameTime);

            if (GameGlobals.Instance.ObjectSpawners.FirstOrDefault(s => s.currMapName.Equals(_currMapName)) != null)
            {
                SpawnTimer = GameGlobals.Instance.ObjectSpawners.FirstOrDefault
                    (s => s.currMapName.Equals(_currMapName)).SpawnTimer;
            }
            else SpawnTimer = 0;

            gameObjects.RemoveAll(e => e.IsDestroyed);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var gameObject in gameObjects.Where(e => !e.IsDestroyed && e.IsVisible))
            {
                gameObject.Draw(spriteBatch);
            }

            if (ScreenManager.Instance.CurrentMap.Equals("noah_home"))
                foreach (var crop in Crops.Where(e => e.IsVisible))
                {
                    crop.Draw(spriteBatch);
                }
        }

        public void ClearGameObject()
        {
            gameObjects.Clear();
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