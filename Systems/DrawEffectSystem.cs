using Medicraft.Data.Models;
using Medicraft.Systems.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Particles;
using MonoGame.Extended.Particles.Modifiers.Containers;
using MonoGame.Extended.Particles.Modifiers.Interpolators;
using MonoGame.Extended.Particles.Modifiers;
using MonoGame.Extended.Particles.Profiles;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.TextureAtlases;
using System;
using System.Linq;
using MonoGame.Extended.SceneGraphs;

namespace Medicraft.Systems
{
    public class DrawEffectSystem
    {
        private readonly SpriteSheet _hitSpriteSheet;
        private readonly SpriteSheet _hitSkillSpriteSheet;
        private readonly SpriteSheet _bossSpriteSheet;
        private readonly SpriteSheet _statesSpriteSheet;

        public static readonly Texture2D ParticleTexture = new(ScreenManager.Instance.GraphicsDevice, 1, 1);

        private float _deltaSeconds;

        public DrawEffectSystem()
        {
            ParticleTexture.SetData(new[] { Color.White });

            _hitSpriteSheet = GameGlobals.Instance.HitSpriteEffect;
            _hitSkillSpriteSheet = GameGlobals.Instance.HitSkillSpriteEffect;
            _bossSpriteSheet = GameGlobals.Instance.BossSpriteEffect;
            _statesSpriteSheet = GameGlobals.Instance.StatesSpriteEffect;
        }

        public static ParticleEffect SetItemParticleEffect(Vector2 itemPos)
        {
            TextureRegion2D textureRegion = new(ParticleTexture);

            var particleEffect = new ParticleEffect()
            {
                Position = new Vector2(itemPos.X, itemPos.Y + 5f),
                Emitters = [
                    new ParticleEmitter(textureRegion, 175, TimeSpan.FromSeconds(1.2)
                            , Profile.Circle(18, Profile.CircleRadiation.In))
                        {
                            Parameters = new ParticleReleaseParameters
                            {
                                Speed = new Range<float>(0f, 12f),
                                Quantity = 2,
                                Rotation = new Range<float>(-0.5f, 0.5f),
                                Scale = new Range<float>(1f, 3.2f),
                            },
                            Modifiers =
                            {
                                new AgeModifier
                                {
                                    Interpolators =
                                    {
                                        new ColorInterpolator
                                        {
                                            StartValue = Color.Khaki.ToHsl(),
                                            EndValue = Color.LightGoldenrodYellow.ToHsl()
                                        }
                                    }
                                },
                                new RotationModifier
                                {
                                    RotationRate = 5f
                                },
                                new DragModifier
                                {
                                    Density = 0.7f, DragCoefficient = 1f
                                },
                                new LinearGravityModifier
                                {
                                    Direction = new Vector2(0,-1),
                                    Strength = 70f
                                }
                            }
                        }]
            };

            return particleEffect;
        }

        public static void Dispose()
        {
            ParticleTexture?.Dispose();

            var itemObject = ObjectManager.Instance.GameObjects;

            foreach (var item in itemObject)
            {
                item.ParticleEffect?.Dispose();
            }
        }

        public void Update(GameTime gameTime)
        {
            _deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            var itemObject = ObjectManager.Instance.GameObjects;

            foreach (var item in itemObject)
            {
                foreach (var particleEmitter in item.ParticleEffect.Emitters)
                {
                    particleEmitter.LayerDepth = item.Sprite.Depth - 0.00001f;
                    particleEmitter.Parameters.Opacity = new Range<float>(0.15f, 0.6f); 
                }

                item.ParticleEffect.Update(_deltaSeconds);
            }
        }

        public void Draw(SpriteBatch spriteBatch) 
        {
            // All Entity that initialized in Entities from EntityManager
            DrawAttackedEffectToMob(spriteBatch);

            DrawBossSkillActivatedEffect(spriteBatch);

            DrawAttackedEffectToPlayer(spriteBatch);

            DrawPlayerSkillEffect(spriteBatch);

            DrawCompanionAttackedEffect(spriteBatch);

            DrawCompanionSkillActivatedEffect(spriteBatch);

            DrawItemParticle(spriteBatch);
        }

        private void DrawAttackedEffectToMob(SpriteBatch spriteBatch)
        {
            var entities = EntityManager.Instance.Entities;

            foreach (var entity in entities.Where(e => !e.IsDestroyed))
            {
                var combatLog = entity.CombatLogs;

                if (combatLog.Count != 0)
                {
                    foreach (var log in combatLog.Where(l => l.ElapsedTime < 1f
                        && (l.Action == CombatNumberData.ActionType.Attack
                            || l.Action == CombatNumberData.ActionType.CriticalAttack
                            || l.Action == CombatNumberData.ActionType.Missed)))
                    {                     
                        if (log.EffectName != null)
                        {
                            if (!log.IsEffectPlayed)
                            {
                                log.AnimatedSprite = new AnimatedSprite(_hitSpriteSheet)
                                {
                                    Depth = entity.Sprite.Depth - 0.00001f    // Make de effect draw on entity
                                };

                                log.AnimatedSprite.Play(log.EffectName);
                                log.IsEffectPlayed = true;
                            }

                            var _transform = new Transform2
                            {
                                Scale = new Vector2(3f, 3f),
                                Rotation = 0f,
                                Position = entity.Position
                            };

                            log.AnimatedSprite.Update(_deltaSeconds);

                            var cycleEffect = _hitSpriteSheet.Cycles.Where(c => c.Key.Equals(log.EffectName)).ElementAt(0);
                            var duration = cycleEffect.Value.FrameDuration * cycleEffect.Value.Frames.Capacity;

                            if (log.ElapsedTime < duration)
                            {
                                spriteBatch.Draw(log.AnimatedSprite, _transform);
                            }
                        }
                    }
                }
            }
        }

        private void DrawBossSkillActivatedEffect(SpriteBatch spriteBatch)
        {
            
        }

        private void DrawAttackedEffectToPlayer(SpriteBatch spriteBatch)
        {
            var combatLog = PlayerManager.Instance.Player.CombatLogs;

            if (combatLog.Count != 0)
            {
                foreach (var log in combatLog.Where(l => l.ElapsedTime < 1f
                    && (l.Action == CombatNumberData.ActionType.Attack
                        || l.Action == CombatNumberData.ActionType.CriticalAttack
                        || l.Action == CombatNumberData.ActionType.Missed)))
                {                  
                    if (log.EffectName != null)
                    {
                        if (!log.IsEffectPlayed)
                        {
                            log.AnimatedSprite = new AnimatedSprite(_hitSpriteSheet)
                            {
                                Depth = PlayerManager.Instance.Player.Sprite.Depth - 0.00001f
                            };

                            log.AnimatedSprite.Play(log.EffectName);
                            log.IsEffectPlayed = true;
                        }

                        var position = PlayerManager.Instance.Player.Position;

                        var _transform = new Transform2
                        {
                            Scale = new Vector2(3f, 3f),
                            Rotation = 0f,
                            Position = new Vector2(position.X, position.Y + 40f)
                        };

                        log.AnimatedSprite.Update(_deltaSeconds);

                        var cycleEffect = _hitSpriteSheet.Cycles.Where(c => c.Key.Equals(log.EffectName)).ElementAt(0);
                        var duration = cycleEffect.Value.FrameDuration * cycleEffect.Value.Frames.Capacity;

                        if (log.ElapsedTime < duration)
                        {
                            spriteBatch.Draw(log.AnimatedSprite, _transform);
                        }
                    } 
                }
            }
        }

        private void DrawPlayerSkillEffect(SpriteBatch spriteBatch)
        {
            var combatLog = PlayerManager.Instance.Player.CombatLogs;

            if (combatLog.Count != 0)
            {
                foreach (var log in combatLog.Where(l => l.ElapsedTime < 1f
                    && (l.Action == CombatNumberData.ActionType.Buff
                        || l.Action == CombatNumberData.ActionType.Recovery)))
                {
                    if (log.EffectName != null)
                    {
                        if (!log.IsEffectPlayed)
                        {
                            log.AnimatedSprite = new AnimatedSprite(_hitSkillSpriteSheet)
                            {
                                Depth = PlayerManager.Instance.Player.Sprite.Depth - 0.00001f
                            };

                            log.AnimatedSprite.Play(log.EffectName);
                            log.IsEffectPlayed = true;
                        }

                        var position = PlayerManager.Instance.Player.Position;

                        var _transform = new Transform2
                        {
                            Scale = new Vector2(1f, 1f),
                            Rotation = 0f,
                            Position = new Vector2(position.X, position.Y + 40f)
                        };

                         log.AnimatedSprite.Update(_deltaSeconds);

                        var cycleEffect = _hitSkillSpriteSheet.Cycles.Where(c => c.Key.Equals(log.EffectName)).ElementAt(0);
                        var duration = cycleEffect.Value.FrameDuration * cycleEffect.Value.Frames.Capacity;

                        if (log.ElapsedTime < duration)
                        {
                            spriteBatch.Draw(log.AnimatedSprite, _transform);
                        }
                    }
                }
            }
        }

        private void DrawCompanionAttackedEffect(SpriteBatch spriteBatch)
        {

        }

        private void DrawCompanionSkillActivatedEffect(SpriteBatch spriteBatch)
        {

        }

        private static void DrawItemParticle(SpriteBatch spriteBatch)
        {
            var itemObject = ObjectManager.Instance.GameObjects;

            foreach (var item in itemObject)
            {
                spriteBatch.Draw(item.ParticleEffect);
            }   
        }
    }
}
