using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Medicraft.Entities;

namespace Medicraft.Systems.Spawners
{
    public class MobSpawner : IEntityManager
    {
        public readonly List<Entity> entitiesInitial;

        private readonly List<Entity> entitiesDestroyed;
        private readonly List<Entity> entitiesSpawn;

        public float spawnTime;
        public bool IsSpawn;

        public MobSpawner(float spawnTime) 
        {
            this.spawnTime = spawnTime;
            IsSpawn = false;
            entitiesInitial = new List<Entity>();
            entitiesDestroyed = new List<Entity>();
            entitiesSpawn = new List<Entity>();
        }

        public void Initialize()
        {
            foreach (var entity in entitiesInitial)
            {
                Entity clonedEntity = entity.Clone() as Entity;
                EntityManager.Instance.AddEntity(clonedEntity);
            }
        }

        public void Spawn()
        {
            if (IsSpawn)
            {
                foreach (var entity in entitiesSpawn)
                {
                    EntityManager.Instance.AddEntity(entity);
                }

                IsSpawn = false;
                entitiesSpawn.Clear();
            }
        }

        public T AddEntity<T>(T entity) where T : Entity
        {
            entitiesInitial.Add(entity);
            return entity;
        }

        public void Update(GameTime gameTime)
        {
            var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            spawnTime -= deltaSeconds;

            foreach (var entity in EntityManager.Instance.entities.Where(e => e.IsDestroyed))
            {
                //System.Diagnostics.Debug.WriteLine($"Add Mob ID: {entity.Id} to entitiesDestroyed");
                entitiesDestroyed.Add(entity);
            }

            if (spawnTime <= 0)
            {
                //System.Diagnostics.Debug.WriteLine($"Destroyed Mob: {entitiesDestroyed.Count}");
                if (entitiesDestroyed.Count != 0)
                {
                    foreach (var entityDestroyed in entitiesDestroyed)
                    {
                        foreach (var entityAdding in entitiesInitial)
                        {
                            if (entityDestroyed.Id == entityAdding.Id)
                            {
                                Entity clonedEntity = entityAdding.Clone() as Entity;
                                entitiesSpawn.Add(clonedEntity);
                            }
                        }
                    }

                    entitiesDestroyed.Clear();
                }

                IsSpawn = true;
                spawnTime = 10f;
            }
        }
    }
}
