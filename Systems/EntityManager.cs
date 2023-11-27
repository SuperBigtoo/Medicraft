using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Medicraft.Entities;
using Medicraft.Systems.Spawners;

namespace Medicraft.Systems
{
    public interface IEntityManager
    {
        T AddEntity<T>(T entity) where T : Entity;
    }

    public class EntityManager : IEntityManager
    {
        private static EntityManager instance;

        private MobSpawner mobSpawner;

        public float spawnTime = 0f;

        public readonly List<Entity> entities;

        public IEnumerable<Entity> Entities => entities;

        private EntityManager()
        {
            entities = new List<Entity>();
        }

        public T AddEntity<T>(T entity) where T : Entity
        {
            entities.Add(entity);
            return entity;
        }

        public void Initialize(MobSpawner mobSpawner)
        {
            entities.Clear();
            this.mobSpawner = mobSpawner;
            this.mobSpawner.Initialize();
        }

        public void Update(GameTime gameTime)
        {
            var frontDepth = 0.2f;
            var behideDepth = 0.4f;

            PlayerManager.Instance.Update(gameTime, frontDepth, behideDepth);
            var playerDepth = PlayerManager.Instance.Player.GetDepth();

            foreach (var entity in entities.Where(e => !e.IsDestroyed))
            {
                playerDepth -= 0.00001f;
                frontDepth -= 0.00001f;
                behideDepth -= 0.00001f;
                entity.Update(gameTime, playerDepth, frontDepth, behideDepth);
            }

            // MobSpawner
            mobSpawner.Update(gameTime);
            spawnTime = mobSpawner.spawnTime;

            entities.RemoveAll(e => e.IsDestroyed);

            mobSpawner.Spawn();
        }

        public void Draw(SpriteBatch spriteBatch)
        {      
            foreach (var entity in entities.Where(e => !e.IsDestroyed))
            {
                entity.Draw(spriteBatch);
            }

            PlayerManager.Instance.Player.Draw(spriteBatch);
        }

        public static EntityManager Instance
        {
            get 
            {
                if (instance == null)
                {
                    instance = new EntityManager();
                }
                return instance;
            } 
        }
    }
}
