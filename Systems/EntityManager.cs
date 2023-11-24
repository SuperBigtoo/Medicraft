using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Medicraft.Entities;

namespace Medicraft.Systems
{
    public interface IEntityManager
    {
        T AddEntity<T>(T entity) where T : Entity;
    }

    public class EntityManager : IEntityManager
    {
        private static EntityManager instance;

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

        public void Update(GameTime gameTime)
        {
            var frontDepth = 0.3f;
            var behideDepth = 0.7f;

            PlayerManager.Instance.Update(gameTime, frontDepth, behideDepth);
            var playerDepth = PlayerManager.Instance.Player.GetPlayerDepth();

            foreach (var entity in entities.Where(e => !e.IsDestroyed))
            {
                playerDepth -= 0.00001f;
                frontDepth -= 0.00001f;
                behideDepth -= 0.00001f;
                entity.Update(gameTime, playerDepth, frontDepth, behideDepth);
            }

            entities.RemoveAll(e => e.IsDestroyed);
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
