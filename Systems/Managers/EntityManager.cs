using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Medicraft.Entities;
using Medicraft.Systems.Spawners;

namespace Medicraft.Systems.Managers
{
    public interface IEntityManager
    {
        T AddEntity<T>(T entity) where T : Entity;
    }

    public class EntityManager : IEntityManager
    {
        public float SpawnTime = 0f;
        public readonly List<Entity> entities;

        private MobSpawner _mobSpawner;
        private static EntityManager instance;

        private readonly float _delayCompaSpawnTime = 3f;
        private float _delayCompaSpawnTimer = 0f;

        public IEnumerable<Entity> Entities => entities;

        public Entity ClosestEnemy { get; set; }

        private EntityManager()
        {
            entities = [];
        }

        public T AddEntity<T>(T entity) where T : Entity
        {
            entities.Add(entity);
            return entity;
        }

        public void Initialize(MobSpawner mobSpawner)
        {
            entities.Clear();
            _mobSpawner = mobSpawner;
            _mobSpawner.Initialize();
        }

        public void Update(GameTime gameTime)
        {
            var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            var topDepth = GameGlobals.Instance.TopEntityDepth;
            var middleDepth = GameGlobals.Instance.MiddleEntityDepth;
            var bottomDepth = GameGlobals.Instance.BottomEntityDepth;

            if (!GameGlobals.Instance.IsOpenGUI)
            {
                // Update Playable Character: Player and Companion
                PlayerManager.Instance.Update(gameTime);
                var playerDepth = PlayerManager.Instance.Player.GetDepth();

                if (!PlayerManager.Instance.IsCompanionSpawned)
                {
                    if (_delayCompaSpawnTimer < _delayCompaSpawnTime)
                    {
                        _delayCompaSpawnTimer += deltaSeconds;
                    }
                    else
                    {
                        // Summon current companion
                        PlayerManager.Instance.SummonCurrentCompanion();

                        _delayCompaSpawnTimer = 0;
                        PlayerManager.Instance.IsCompanionSpawned = true;
                    }
                }
                else
                {
                    PlayerManager.Instance.Companions[PlayerManager.Instance.CurrentCompanionIndex].Update(
                        gameTime, playerDepth, topDepth, middleDepth, bottomDepth
                    );
                }

                // Update Mob & NPC
                if (entities.Count != 0)
                {
                    foreach (var entity in entities.Where(e => !e.IsDestroyed))
                    {
                        playerDepth -= 0.000001f;
                        topDepth -= 0.000001f;
                        middleDepth -= 0.000001f;
                        bottomDepth -= 0.000001f;
                        entity.Update(gameTime, playerDepth, topDepth, middleDepth, bottomDepth);
                    }
                }

                // Update Status Effect
                StatusEffectManager.Instance.Update(gameTime);

                // Mob Spawner
                _mobSpawner?.Update(gameTime);

                if (_mobSpawner != null)
                    SpawnTime = _mobSpawner.SpawnTimer;

                entities.RemoveAll(e => e.IsDestroyed);

                _mobSpawner?.Spawn();
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var entity in entities.Where(e => !e.IsDestroyed))
                entity.Draw(spriteBatch);          

            if (PlayerManager.Instance.IsCompanionSpawned)
                PlayerManager.Instance.Companions[PlayerManager.Instance.CurrentCompanionIndex].Draw(spriteBatch);

            PlayerManager.Instance.Player.Draw(spriteBatch);
        }

        public void ClearEntity()
        {
            entities.Clear();
            _mobSpawner ??= null;
        }

        public int EntityCount()
        {
            return entities.Count;
        }

        public static EntityManager Instance
        {
            get
            {
                instance ??= new EntityManager();
                return instance;
            }
        }
    }
}
