using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Medicraft.Entites;

namespace Medicraft.Systems
{
    public interface IEntityManager
    {
        T AddEntity<T>(T entity) where T : Entity;
    }

    public class EntityManager : IEntityManager
    {
        public IEnumerable<Entity> Entities => Singleton.Instance._entities;

        public EntityManager() {}

        public T AddEntity<T>(T entity) where T : Entity
        {
            Singleton.Instance._entities.Add(entity);
            return entity;
        }

        public void Update(GameTime gameTime)
        {
            foreach (var entity in Singleton.Instance._entities.Where(e => !e.IsDestroyed))
            {
                entity.Update(gameTime);
            }

            Singleton.Instance._entities.RemoveAll(e => e.IsDestroyed);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var entity in Singleton.Instance._entities.Where(e => !e.IsDestroyed))
            {
                entity.Draw(spriteBatch);
            }
        }
    }
}
