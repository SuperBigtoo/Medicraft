using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Medicraft.Entities;
using Medicraft.Systems.Managers;
using Medicraft.Data.Models;
using MonoGame.Extended.Sprites;

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

        public void SetupSpawner(List<EntityData> entityDatas, Dictionary<int, SpriteSheet> spriteSheets)
        {
            foreach (var entity in entityDatas)
            {
                var charId = entity.CharId;

                switch (charId)
                {
                    case 200:
                        spriteSheets.TryGetValue(200, out SpriteSheet _spriteSheetSlime);
                        AddEntity(new Slime(new AnimatedSprite(_spriteSheetSlime), entity, Vector2.One));
                        break;

                    case 201:
                        break;

                    case 202:
                        break;

                    case 203:
                        break;

                    case 204:
                        break;

                    case 205:
                        break;

                    case 206:
                        break;

                    case 300:
                        break;

                    case 301:
                        break;

                    case 302:
                        break;

                    case 303:
                        break;

                    case 304:
                        break;

                    case 305:
                        break;
                }
            }            
        }
    }
}
