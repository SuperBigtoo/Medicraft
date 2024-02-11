using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Medicraft.Entities;

namespace Medicraft.Systems.Spawners
{
    public class MobSpawner : IEntityManager
    {
        public readonly List<Entity> initialEntities;

        private readonly List<Entity> destroyedEntities;
        private readonly List<Entity> spawningEntities;

        public float initialSpawnTime;
        public float spawnTime;
        public bool IsSpawn;

        public MobSpawner(float initialSpawnTime) 
        {
            this.initialSpawnTime = initialSpawnTime;
            spawnTime = initialSpawnTime;
            IsSpawn = false;
            initialEntities = new List<Entity>();
            destroyedEntities = new List<Entity>();
            spawningEntities = new List<Entity>();
        }

        public void Initialize()
        {
            foreach (var entity in initialEntities)
            {
                Entity clonedEntity = entity.Clone() as Entity;
                EntityManager.Instance.AddEntity(clonedEntity);
            }
        }

        public void Spawn()
        {
            if (IsSpawn)
            {
                foreach (var entity in spawningEntities)
                {
                    EntityManager.Instance.AddEntity(entity);
                }

                IsSpawn = false;
                spawningEntities.Clear();
            }
        }

        public T AddEntity<T>(T entity) where T : Entity
        {
            initialEntities.Add(entity);
            return entity;
        }

        public void Update(GameTime gameTime)
        {
            var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            spawnTime -= deltaSeconds;

            foreach (var entity in EntityManager.Instance.Entities.Where(e => e.IsDestroyed))
            {
                //System.Diagnostics.Debug.WriteLine($"Add Mob ID: {entity.Id} to entitiesDestroyed");
                destroyedEntities.Add(entity);
            }

            //if (spawnTime <= 0)
            //{
            //    //System.Diagnostics.Debug.WriteLine($"Destroyed Mob: {entitiesDestroyed.Count}");
            //    if (destroyedEntities.Count != 0)
            //    {
            //        foreach (var destroyedEntity in destroyedEntities)
            //        {
            //            foreach (var initialEntity in initialEntities)
            //            {
            //                if (destroyedEntity.Id == initialEntity.Id)
            //                {
            //                    Entity clonedEntity = initialEntity.Clone() as Entity;
            //                    spawningEntities.Add(clonedEntity);
            //                }
            //            }
            //        }

            //        destroyedEntities.Clear();
            //    }

            //    IsSpawn = true;
            //    spawnTime = initialSpawnTime;
            //}

            if (spawnTime <= 0)
            {
                if (destroyedEntities.Count != 0)
                {
                    var clonedEntities = destroyedEntities.Join(initialEntities,
                            destroyedEntity => destroyedEntity.Id,
                            initialEntity => initialEntity.Id,
                            (destroyedEntity, initialEntity) => initialEntity.Clone() as Entity).ToList();

                    spawningEntities.AddRange(clonedEntities);
                    destroyedEntities.Clear();
                }

                IsSpawn = true;
                spawnTime = initialSpawnTime;
            }
        }
    }
}
