using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Medicraft.Entities;
using Medicraft.Systems.Managers;
using Medicraft.Data.Models;
using MonoGame.Extended.Sprites;
using Medicraft.Entities.Mobs.Monster;
using Medicraft.Entities.Mobs.Friendly;

namespace Medicraft.Systems.Spawners
{
    public class MobSpawner(float spawnTime, float spawnTimer) : IEntityManager
    {
        public float SpawnTime = spawnTime;
        public float SpawnTimer = spawnTimer;
        public bool IsSpawn = false;

        private readonly List<Entity> _initialEntities = [];
        private readonly List<Entity> _destroyedEntities = [];
        private readonly List<Entity> _spawningEntities = [];

        public void Initialize()
        {
            foreach (var entity in _initialEntities)
            {
                if (entity.EntityType == Entity.EntityTypes.Boss)
                {
                    Entity clonedBoss = entity.Clone() as Entity;

                    switch (ScreenManager.Instance.CurrentMap)
                    {
                        case "Test":                           
                            if (GameGlobals.Instance.IsBossTestDead)
                            {                             
                                _spawningEntities.Add(clonedBoss);
                            }
                            else EntityManager.Instance.AddEntity(clonedBoss);
                            break;

                        case "dungeon_1":
                            if (GameGlobals.Instance.IsBossOneDead)
                            {
                                _spawningEntities.Add(clonedBoss);
                            }
                            else EntityManager.Instance.AddEntity(clonedBoss);
                            break;
                    }
                }
                else
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
            var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            SpawnTimer -= deltaSeconds;

            foreach (var entity in EntityManager.Instance.Entities.Where(e => e.IsDestroyed))
            {
                //System.Diagnostics.Debug.WriteLine($"Add Mob ID: {entity.Id} to entitiesDestroyed");
                _destroyedEntities.Add(entity);
            }

            if (SpawnTimer <= 0)
            {
                if (_destroyedEntities.Count != 0)
                {
                    var clonedEntities = _destroyedEntities.Join(_initialEntities,
                            destroyedEntity => destroyedEntity.Id,
                            initialEntity => initialEntity.Id,
                            (destroyedEntity, initialEntity) => initialEntity.Clone() as Entity).ToList();

                    _spawningEntities.AddRange(clonedEntities);
                    _destroyedEntities.Clear();
                }

                IsSpawn = true;
                SpawnTimer = SpawnTime;
            }
        }

        public void Spawn()
        {
            if (IsSpawn)
            {
                foreach (var entity in _spawningEntities.Where(e => e.IsRespawnable && e.EntityType != Entity.EntityTypes.Boss))
                {
                    EntityManager.Instance.AddEntity(entity);
                }

                IsSpawn = false;
                _spawningEntities.RemoveAll(entity => entity.EntityType != Entity.EntityTypes.Boss);
            }

            foreach (var entityBoss in _spawningEntities.Where(e => e.IsRespawnable && e.EntityType == Entity.EntityTypes.Boss))
            {
                bool isBossDead = false;

                switch (ScreenManager.Instance.CurrentMap)
                {
                    case "Test":
                        isBossDead = GameGlobals.Instance.IsBossTestDead;
                        break;

                    case "dungeon_1":
                        isBossDead = GameGlobals.Instance.IsBossOneDead;
                        break;
                }

                if (!isBossDead)
                {
                    EntityManager.Instance.AddEntity(entityBoss);
                    _spawningEntities.Remove(entityBoss);
                }
            }
        }

        public void SetupSpawner(List<EntityData> entityDatas, Dictionary<int, SpriteSheet> spriteSheets)
        {
            foreach (var entityData in entityDatas)
            {
                switch (entityData.CharId)
                {
                    case 100:
                        spriteSheets.TryGetValue(100, out SpriteSheet spriteCat);
                        AddEntity(new Cat(new AnimatedSprite(spriteCat), entityData, Vector2.One));
                        break;

                    case 101:
                        spriteSheets.TryGetValue(101, out SpriteSheet spriteBlackSmite);
                        AddEntity(new Vendor(new AnimatedSprite(spriteBlackSmite), entityData, Vector2.One));
                        break;

                    case 102: // Civilian01_NordlingenTown
                        spriteSheets.TryGetValue(102, out SpriteSheet spriteCivilian01);
                        AddEntity(new Civilian(new AnimatedSprite(spriteCivilian01), entityData, Vector2.One));
                        break;

                    case 200:
                        spriteSheets.TryGetValue(200, out SpriteSheet spriteSlime);
                        AddEntity(new Slime(new AnimatedSprite(spriteSlime), entityData, Vector2.One));
                        break;

                    case 201:
                        spriteSheets.TryGetValue(201, out SpriteSheet spriteGoblin);
                        AddEntity(new Goblin(new AnimatedSprite(spriteGoblin), entityData, Vector2.One));
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
