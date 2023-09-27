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
            PlayerManager.Instance.player.Update(gameTime);

            foreach (var entity in entities.Where(e => !e.IsDestroyed))
            {
                entity.Update(gameTime);
            }

            entities.RemoveAll(e => e.IsDestroyed);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            PlayerManager.Instance.player.Draw(spriteBatch);

            foreach (var entity in entities.Where(e => !e.IsDestroyed))
            {
                entity.Draw(spriteBatch);
            }
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
