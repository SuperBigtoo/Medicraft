using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Medicraft.Entities;
using Medicraft.Systems.Managers;
using Medicraft.Data.Models;
using MonoGame.Extended.Sprites;
using Medicraft.Entities.Mobs.Monster;
using Medicraft.Entities.Mobs.Friendly;
using Medicraft.Entities.Mobs.Boss;

namespace Medicraft.Systems.Spawners
{
    public class MobSpawner : IEntityManager
    {
        public readonly SpawnerData spawnerData;
        public readonly string currMapName;

        public float SpawnTime;
        public float SpawnTimer;

        private readonly List<Entity> _initialEntities = [];
        private readonly List<Entity> _destroyedEntities = [];
        private readonly List<Entity> _spawningEntities = [];

        public MobSpawner(SpawnerData spawnerData, string currMapName, List<EntityData> entityDatas
            , Dictionary<int, SpriteSheet> spriteSheets)
        {
            this.spawnerData = spawnerData;
            this.currMapName = currMapName;

            SetupSpawner(entityDatas, spriteSheets);
        }

        public void Initialize()
        {
            foreach (var entity in _initialEntities)
            {
                bool isFound = false;

                foreach (var destroyedEntity in _destroyedEntities)
                {
                    if (entity.Id.Equals(destroyedEntity.Id))
                    {
                        isFound = true;
                        break;
                    }
                }

                if (!isFound)
                {
                    Entity clonedEntity = entity.Clone() as Entity;
                    EntityManager.Instance.AddEntity(clonedEntity);
                }
            }
        }

        public T AddEntity<T>(T entity) where T : Entity
        {
            _initialEntities.Add(entity);
            return entity;
        }

        public void Update(GameTime gameTime)
        {
            var mapSpawnTime = spawnerData.MapSpawnTimes.FirstOrDefault
                (m => m.SpawnerName.Equals("Entity") && m.MapName.Equals(currMapName));

            SpawnTime = mapSpawnTime.SpawnTime;
            SpawnTimer = mapSpawnTime.SpawnTimer;


            foreach (var entity in EntityManager.Instance.Entities.Where(e => e.IsDestroyed))
            {
                _destroyedEntities.Add(entity);
            }

            if (SpawnTimer < 0)
            {
                if (_destroyedEntities.Count != 0)
                {
                    var clonedEntities = _destroyedEntities.Join(_initialEntities,
                            destroyedEntity => destroyedEntity.Id,
                            initialEntity => initialEntity.Id,
                            (destroyedEntity, initialEntity) => initialEntity.Clone() as Entity).ToList();

                    _spawningEntities.AddRange(clonedEntities.Where(e => e.EntityType != Entity.EntityTypes.Boss));
                    _destroyedEntities.RemoveAll(e => e.EntityType != Entity.EntityTypes.Boss);
                }

                Respawn();
            }

            // Check Respawn for Boss
            if (!spawnerData.IsBossDead)
            {
                foreach (var entityBoss in _destroyedEntities.Where
                    (e => e.IsRespawnable && e.EntityType == Entity.EntityTypes.Boss))
                {
                    Entity clonedEntity = entityBoss.Clone() as Entity;
                    EntityManager.Instance.AddEntity(clonedEntity);           
                }
                _destroyedEntities.RemoveAll(e => e.EntityType == Entity.EntityTypes.Boss);
            }
        }

        private void Respawn()
        {
            foreach (var entity in _spawningEntities.Where
                    (e => e.IsRespawnable && e.EntityType != Entity.EntityTypes.Boss))
            {
                EntityManager.Instance.AddEntity(entity);
            }
            _spawningEntities.RemoveAll(e => e.EntityType != Entity.EntityTypes.Boss);
        }

        public void SetupSpawner(List<EntityData> entityDatas, Dictionary<int, SpriteSheet> spriteSheets)
        {
            System.Diagnostics.Debug.WriteLine($"SetupSpawner");

            foreach (var entityData in entityDatas)
            {
                spriteSheets.TryGetValue(entityData.CharId, out SpriteSheet spriteSheet);

                switch (entityData.CharId)
                {
                    case 100:                    
                        AddEntity(new Cat(new AnimatedSprite(spriteSheet), entityData, Vector2.One));
                        break;

                    case 101:
                        AddEntity(new Vendor(new AnimatedSprite(spriteSheet), entityData, Vector2.One));
                        break;

                    // Civilian01_NordlingenTown -> Civilian24_NordlingenTown
                    case 102:
                    case 103:
                    case 104:
                    case 105:
                    case 106:
                    case 107:
                    case 108:
                    case 109:
                    case 110:
                    case 111:
                    case 112:
                    case 113:
                    case 114:
                    case 115:
                    case 116:
                    case 117:
                    case 118:
                    case 119:
                    case 120:
                    case 121:
                    case 122:
                    case 123:
                    case 124:
                    case 125:
                        AddEntity(new Civilian(new AnimatedSprite(spriteSheet), entityData, Vector2.One));
                        break;

                    case 200:
                        AddEntity(new Slime(new AnimatedSprite(spriteSheet), entityData, Vector2.One));
                        break;

                    case 201:
                        AddEntity(new Goblin(new AnimatedSprite(spriteSheet), entityData, Vector2.One));
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
                        AddEntity(new Minotaur(new AnimatedSprite(spriteSheet), entityData, Vector2.One));
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
