using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Medicraft.Entities;
using System;
using static Medicraft.Systems.GameGlobals;

namespace Medicraft.Systems.Managers
{
    public interface IEntityManager
    {
        T AddEntity<T>(T entity) where T : Entity;
    }

    public class EntityManager : IEntityManager
    {
        public float SpawnTimer { private set; get; } = 0f;
        public readonly List<Entity> entities;

        private static EntityManager instance;

        private readonly float _delayCompaSpawnTime = 2f;
        private float _delayCompaSpawnTimer = 0f;
        private string _currMapName;

        public IEnumerable<Entity> Entities => entities;

        // Scale Rendering
        const float ScreenWidthFactor = 1f;
        const float ScreenHeightFactor = 1f;
        public Entity ClosestEnemyToCompanion { get; set; }
        public Entity Boss { get; set; }

        private EntityManager()
        {
            entities = [];
        }

        public T AddEntity<T>(T entity) where T : Entity
        {
            entities.Add(entity);
            return entity;
        }

        public void Initialize(string mapName)
        {
            _currMapName = mapName;
            entities.Clear();
            GameGlobals.Instance.MobSpawners.FirstOrDefault
                (s => s.currMapName.Equals(mapName)).Initialize();
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

                if (PlayerManager.Instance.Companions.Count != 0)
                {
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
                }

                // Update Mob & NPC
                if (entities.Count != 0 && !ScreenManager.Instance.IsTransitioning)
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
                GameGlobals.Instance.MobSpawners.FirstOrDefault
                    (s => s.currMapName.Equals(_currMapName))?.Update(gameTime);

                if (GameGlobals.Instance.MobSpawners.FirstOrDefault(s => s.currMapName.Equals(_currMapName)) != null)
                {
                    SpawnTimer = GameGlobals.Instance.MobSpawners.FirstOrDefault
                        (s => s.currMapName.Equals(_currMapName)).SpawnTimer;
                }
                else SpawnTimer = 0;

                entities.RemoveAll(e => e.IsDestroyed);
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

        public void ClearAggroMobs()
        {
            if (entities.Count == 0) return;

            foreach (var entity in entities.Where(e => !e.IsDestroyed))
            {
                entity.IsAggro = false;
                entity.AggroTimer = 0f;
            }
        }

        public void ClearEntity()
        {
            entities.Clear();
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
