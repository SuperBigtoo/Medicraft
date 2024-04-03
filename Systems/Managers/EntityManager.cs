using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Medicraft.Entities;
using Medicraft.Systems.Spawners;
using System;

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

        private readonly float _delayCompaSpawnTime = 2f;
        private float _delayCompaSpawnTimer = 0f;

        public IEnumerable<Entity> Entities => entities;

        // Scale Rendering
        const float ScreenWidthFactor = 1f;
        const float ScreenHeightFactor = 1f;
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

                if (!PlayerManager.Instance.IsCompanionDead && !PlayerManager.Instance.IsCompanionSummoned)
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
                        PlayerManager.Instance.IsCompanionSummoned = true;
                    }
                }
                else if (!PlayerManager.Instance.IsCompanionDead && PlayerManager.Instance.IsCompanionSummoned)
                {
                    PlayerManager.Instance.Companions[PlayerManager.Instance.CurrCompaIndex].Update(
                        gameTime, playerDepth, topDepth, middleDepth, bottomDepth);
                }

                // Update Mob & NPC
                if (entities.Count != 0)
                {
                    foreach (var entity in entities.Where(e => !e.IsDestroyed))
                    {
                        var playerPos = PlayerManager.Instance.Player.Position;

                        // Distance from player
                        var distanceX = MathF.Abs(entity.Position.X - playerPos.X);
                        var distanceY = MathF.Abs(entity.Position.Y - playerPos.Y);

                        // Maximum allowed distance
                        var maxDistanceX = ScreenManager.Instance.GraphicsDevice.Viewport.Width * ScreenWidthFactor;
                        var maxDistanceY = ScreenManager.Instance.GraphicsDevice.Viewport.Height * ScreenHeightFactor;

                        // Check if the entity is within the visible area
                        if (distanceX <= maxDistanceX && distanceY <= maxDistanceY)
                        {
                            playerDepth -= 0.000001f;
                            topDepth -= 0.000001f;
                            middleDepth -= 0.000001f;
                            bottomDepth -= 0.000001f;
                            entity.Update(gameTime, playerDepth, topDepth, middleDepth, bottomDepth);
                        }
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

            if (PlayerManager.Instance.IsCompanionSummoned)
                PlayerManager.Instance.Companions[PlayerManager.Instance.CurrCompaIndex].Draw(spriteBatch);

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
