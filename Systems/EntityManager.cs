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
        private static EntityManager instance;

        public readonly List<Entity> _entities;

        public IEnumerable<Entity> Entities => _entities;

        private EntityManager()
        {
            _entities = new List<Entity>();
        }

        public T AddEntity<T>(T entity) where T : Entity
        {
            _entities.Add(entity);
            return entity;
        }

        public void Update(GameTime gameTime)
        {
            foreach (var entity in _entities.Where(e => !e.IsDestroyed))
            {
                entity.Update(gameTime);
            }

            _entities.RemoveAll(e => e.IsDestroyed);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var entity in _entities.Where(e => !e.IsDestroyed))
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
