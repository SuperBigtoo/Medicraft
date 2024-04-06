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

        public void Dispose()
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
            // Attacked Effect
            DrawAttackedEffectToMob(spriteBatch);
            DrawAttackedEffectToPlayer(spriteBatch);
            DrawAttackedEffectToCompanion(spriteBatch);

            // Ability and Status Effect
            DrawBossEffects(spriteBatch);
            DrawMobStatusEffect(spriteBatch);      
            DrawPlayerAbilityAndStatusEffects(spriteBatch);
            DrawCompanionAbilityAndStatusEffect(spriteBatch);

            // Object Particle
            DrawObjectParticle(spriteBatch);
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

                            var transform = new Transform2
                            {
                                Scale = new Vector2(3f, 3f),
                                Rotation = 0f,
                                Position = entity.Position
                            };

                            log.AnimatedSprite.Update(_deltaSeconds);

                            var cycleEffect = _hitSpriteSheet.Cycles.FirstOrDefault(c => c.Key.Equals(log.EffectName));
                            var duration = cycleEffect.Value.FrameDuration * cycleEffect.Value.Frames.Capacity;

                            if (log.ElapsedTime < duration)
                            {
                                spriteBatch.Draw(log.AnimatedSprite, transform);
                            }
                        }
                    }
                }
            }
        }

        private void DrawBossEffects(SpriteBatch spriteBatch)
        {
            
        }

        private void DrawMobStatusEffect(SpriteBatch spriteBatch)
        {
            var entities = EntityManager.Instance.Entities;

            foreach (var entity in entities.Where(e => !e.IsDestroyed && !e.IsDying))
            {
                var combatLog = entity.CombatLogs;

                if (combatLog.Count != 0)
                {
                    foreach (var log in combatLog.Where(l => l.ElapsedTime < 1f
                        && (l.Action == CombatNumberData.ActionType.Buff
                            || l.Action == CombatNumberData.ActionType.Debuff
                            || l.Action == CombatNumberData.ActionType.Recovery)))
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

                            var transform = new Transform2
                            {
                                Scale = new Vector2(3f, 3f),
                                Rotation = 0f,
                                Position = entity.Position
                            };

                            log.AnimatedSprite.Update(_deltaSeconds);

                            var cycleEffect = _hitSpriteSheet.Cycles.FirstOrDefault(c => c.Key.Equals(log.EffectName));
                            var duration = cycleEffect.Value.FrameDuration * cycleEffect.Value.Frames.Capacity;

                            if (log.ElapsedTime < duration)
                            {
                                spriteBatch.Draw(log.AnimatedSprite, transform);
                            }
                        }
                    }
                }

                // Stun
                if (entity.IsStunning)
                {
                    if (!entity.IsStunningEffectDraw)
                    {
                        entity.StatesSprite = new AnimatedSprite(_statesSpriteSheet)
                        {
                            Depth = entity.Sprite.Depth - 0.0000025f
                        };

                        entity.StatesSprite.Play("effect_stun");

                        entity.IsStunningEffectDraw = true;
                    }

                    if (entity.StunningTimer > 0)
                    {
                        var transform = new Transform2
                        {
                            Scale = new Vector2(1f, 1f),
                            Rotation = 0f,
                            Position = new Vector2(entity.Position.X + entity.Sprite.TextureRegion.Width / 5
                                , entity.Position.Y - entity.Sprite.TextureRegion.Height / 5)
                        };

                        entity.StatesSprite.Update(_deltaSeconds);

                        spriteBatch.Draw(entity.StatesSprite, transform);
                    }
                }
                else if (entity.IsAggro)
                {
                    if (!entity.IsAggroEffectDraw)
                    {
                        entity.StatesSprite = new AnimatedSprite(_statesSpriteSheet)
                        {
                            Depth = entity.Sprite.Depth - 0.0000025f
                        };

                        entity.StatesSprite.Play("effect_taunt&aggro");

                        entity.IsAggroEffectDraw = true;
                        entity.AggroDrawEffectTimer = 3f;
                    }

                    if (entity.AggroDrawEffectTimer > 0)
                    {
                        var transform = new Transform2
                        {
                            Scale = new Vector2(1f, 1f),
                            Rotation = 0f,
                            Position = new Vector2(entity.Position.X + entity.Sprite.TextureRegion.Width / 5
                            , entity.Position.Y - entity.Sprite.TextureRegion.Height / 5)
                        };

                        entity.StatesSprite.Update(_deltaSeconds);

                        spriteBatch.Draw(entity.StatesSprite, transform);
                    }
                }

                // Effect on Mob
                entity.BuffSprite ??= new AnimatedSprite(_statesSpriteSheet)
                {
                    Depth = entity.Sprite.Depth - 0.0000026f
                };

                entity.DebuffSprite ??= new AnimatedSprite(_statesSpriteSheet)
                {
                    Depth = entity.Sprite.Depth - 0.0000027f
                };

                // Buff
                if (entity.IsBuffOn)
                {
                    entity.BuffSprite.Play("effect_buff&shock");

                    var transform = new Transform2
                    {
                        Scale = new Vector2(1.3f, 1.3f),
                        Rotation = 0f,
                        Position = entity.Position
                    };

                    entity.BuffSprite.Update(_deltaSeconds);

                    spriteBatch.Draw(entity.BuffSprite, transform);
                }

                // Debuff
                if (entity.IsDebuffOn)
                {
                    entity.DebuffSprite.Play("effect_debuff");

                    var transform = new Transform2
                    {
                        Scale = new Vector2(1.3f, 1.3f),
                        Rotation = 0f,
                        Position = entity.Position
                    };

                    entity.DebuffSprite.Update(_deltaSeconds);

                    spriteBatch.Draw(entity.DebuffSprite, transform);
                }
            }
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

                        var transform = new Transform2
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
                            spriteBatch.Draw(log.AnimatedSprite, transform);
                        }
                    } 
                }
            }
        }

        private void DrawPlayerAbilityAndStatusEffects(SpriteBatch spriteBatch)
        {
            // Activated Effect
            var combatLog = PlayerManager.Instance.Player.CombatLogs;
            if (combatLog.Count != 0)
            {
                foreach (var log in combatLog.Where(l => l.ElapsedTime < 1f
                    && (l.Action == CombatNumberData.ActionType.Buff
                        || l.Action == CombatNumberData.ActionType.Debuff
                        || l.Action == CombatNumberData.ActionType.Recovery)))
                {
                    if (log.EffectName != null)
                    {
                        if (!log.IsEffectPlayed)
                        {
                            log.AnimatedSprite = new AnimatedSprite(_hitSkillSpriteSheet)
                            {
                                Depth = PlayerManager.Instance.Player.Sprite.Depth - 0.000005f
                            };

                            log.AnimatedSprite.Play(log.EffectName);
                            log.IsEffectPlayed = true;
                        }

                        var position = PlayerManager.Instance.Player.Position;
                        var transform = new Transform2
                        {
                            Scale = new Vector2(1f, 1f),
                            Rotation = 0f,
                            Position = new Vector2(position.X, position.Y + 40f)
                        };

                        log.AnimatedSprite.Update(_deltaSeconds);

                        var cycleEffect = _hitSkillSpriteSheet.Cycles.FirstOrDefault(c => c.Key.Equals(log.EffectName));
                        var duration = cycleEffect.Value.FrameDuration * cycleEffect.Value.Frames.Capacity;

                        if (log.ElapsedTime < duration)
                        {
                            spriteBatch.Draw(log.AnimatedSprite, transform);
                        }
                    }
                }
            }   

            PlayerManager.Instance.Player.StatesSprite ??= new AnimatedSprite(_statesSpriteSheet)
            {
                Depth = PlayerManager.Instance.Player.Sprite.Depth - 0.0000025f
            };

            // Stun
            if (PlayerManager.Instance.Player.IsStunning)
            {
                if (!PlayerManager.Instance.Player.IsStunningEffectDraw)
                {
                    PlayerManager.Instance.Player.StatesSprite.Play("effect_stun");

                    PlayerManager.Instance.Player.IsStunningEffectDraw = true;
                }

                if (PlayerManager.Instance.Player.StunningTimer > 0)
                {
                    var transform = new Transform2
                    {
                        Scale = new Vector2(1.25f, 1.25f),
                        Rotation = 0f,
                        Position = PlayerManager.Instance.Player.Position
                    };

                    PlayerManager.Instance.Player.StatesSprite.Update(_deltaSeconds);

                    spriteBatch.Draw(PlayerManager.Instance.Player.StatesSprite, transform);
                }
            }

            // Passive Skill
            if (PlayerManager.Instance.Player.IsPassiveSkillActivate)
            {
                if (PlayerManager.Instance.Player.PassiveActivatedTimer
                    > PlayerManager.Instance.Player.PassiveActivatedTime - 0.5f)
                {
                    PlayerManager.Instance.Player.StatesSprite.Play("effect_low_hp");
                }

                if (PlayerManager.Instance.Player.PassiveActivatedTimer > 0)
                {
                    var transform = new Transform2
                    {
                        Scale = new Vector2(1.5f, 1.5f),
                        Rotation = 0f,
                        Position = new Vector2(PlayerManager.Instance.Player.Position.X
                            , PlayerManager.Instance.Player.Position.Y - 32f)
                    };

                    PlayerManager.Instance.Player.StatesSprite.Update(_deltaSeconds);

                    spriteBatch.Draw(PlayerManager.Instance.Player.StatesSprite, transform);
                }
            }

            // Effect on Player
            PlayerManager.Instance.Player.BuffSprite ??= new AnimatedSprite(_statesSpriteSheet)
            {
                Depth = PlayerManager.Instance.Player.Sprite.Depth - 0.0000026f
            };

            PlayerManager.Instance.Player.DebuffSprite ??= new AnimatedSprite(_statesSpriteSheet)
            {
                Depth = PlayerManager.Instance.Player.Sprite.Depth - 0.0000027f
            };

            // Buff
            if (PlayerManager.Instance.Player.IsBuffOn || PlayerManager.Instance.Player.IsNormalSkillActivate)
            {
                PlayerManager.Instance.Player.BuffSprite.Play("effect_buff&shock");

                var transform = new Transform2
                {
                    Scale = new Vector2(1.6f, 1.5f),
                    Rotation = 0f,
                    Position = PlayerManager.Instance.Player.Position
                };

                PlayerManager.Instance.Player.BuffSprite.Update(_deltaSeconds);

                spriteBatch.Draw(PlayerManager.Instance.Player.BuffSprite, transform);
            }

            // Debuff
            if (PlayerManager.Instance.Player.IsDebuffOn)
            {
                PlayerManager.Instance.Player.DebuffSprite.Play("effect_debuff");

                var transform = new Transform2
                {
                    Scale = new Vector2(1.6f, 1.5f),
                    Rotation = 0f,
                    Position = PlayerManager.Instance.Player.Position
                };

                PlayerManager.Instance.Player.DebuffSprite.Update(_deltaSeconds);

                spriteBatch.Draw(PlayerManager.Instance.Player.DebuffSprite, transform);
            }
        }

        private void DrawAttackedEffectToCompanion(SpriteBatch spriteBatch)
        {
            if (PlayerManager.Instance.Companions.Count == 0) return;

            var companion = PlayerManager.Instance.Companions[PlayerManager.Instance.CurrCompaIndex];
            var combatLog = companion.CombatLogs;

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
                                Depth = companion.Sprite.Depth - 0.00001f
                            };

                            log.AnimatedSprite.Play(log.EffectName);
                            log.IsEffectPlayed = true;
                        }

                        var position = companion.Position;
                        var transform = new Transform2
                        {
                            Scale = new Vector2(3f, 3f),
                            Rotation = 0f,
                            Position = new Vector2(position.X, position.Y)
                        };

                        log.AnimatedSprite.Update(_deltaSeconds);

                        var cycleEffect = _hitSpriteSheet.Cycles.FirstOrDefault(c => c.Key.Equals(log.EffectName));
                        var duration = cycleEffect.Value.FrameDuration * cycleEffect.Value.Frames.Capacity;

                        if (log.ElapsedTime < duration)
                        {
                            spriteBatch.Draw(log.AnimatedSprite, transform);
                        }
                    }
                }
            }
        }

        private void DrawCompanionAbilityAndStatusEffect(SpriteBatch spriteBatch)
        {
            if (PlayerManager.Instance.Companions.Count == 0) return;

            var companion = PlayerManager.Instance.Companions[PlayerManager.Instance.CurrCompaIndex];
            var combatLog = companion.CombatLogs;

            // Activated Effect
            if (combatLog.Count != 0)
            {
                foreach (var log in combatLog.Where(l => l.ElapsedTime < 1f
                    && (l.Action == CombatNumberData.ActionType.Buff
                        || l.Action == CombatNumberData.ActionType.Debuff
                        || l.Action == CombatNumberData.ActionType.Recovery)))
                {
                    if (log.EffectName != null)
                    {
                        if (!log.IsEffectPlayed)
                        {
                            log.AnimatedSprite = new AnimatedSprite(_hitSkillSpriteSheet)
                            {
                                Depth = companion.Sprite.Depth - 0.000005f
                            };

                            log.AnimatedSprite.Play(log.EffectName);
                            log.IsEffectPlayed = true;
                        }

                        var position = companion.Position;

                        var transform = new Transform2
                        {
                            Scale = new Vector2(1f, 1f),
                            Rotation = 0f,
                            Position = new Vector2(position.X, position.Y)
                        };

                        log.AnimatedSprite.Update(_deltaSeconds);

                        var cycleEffect = _hitSkillSpriteSheet.Cycles.FirstOrDefault(c => c.Key.Equals(log.EffectName));
                        var duration = cycleEffect.Value.FrameDuration * cycleEffect.Value.Frames.Capacity;

                        if (log.ElapsedTime < duration)
                        {
                            spriteBatch.Draw(log.AnimatedSprite, transform);
                        }
                    }
                }
            }

            // Stun
            if (companion.IsStunning)
            {
                if (!companion.IsStunningEffectDraw)
                {
                    companion.StatesSprite = new AnimatedSprite(_statesSpriteSheet)
                    {
                        Depth = companion.Sprite.Depth - 0.0000025f
                    };

                    companion.StatesSprite.Play("effect_stun");

                    companion.IsStunningEffectDraw = true;
                }

                if (companion.StunningTimer > 0)
                {
                    var transform = new Transform2
                    {
                        Scale = new Vector2(1f, 1f),
                        Rotation = 0f,
                        Position = new Vector2(companion.Position.X + companion.Sprite.TextureRegion.Width / 5
                            , companion.Position.Y - companion.Sprite.TextureRegion.Height / 5)
                    };

                    companion.StatesSprite.Update(_deltaSeconds);

                    spriteBatch.Draw(companion.StatesSprite, transform);
                }
            }
            else if (companion.IsAggro)
            {
                if (!companion.IsAggroEffectDraw)
                {
                    companion.StatesSprite = new AnimatedSprite(_statesSpriteSheet)
                    {
                        Depth = companion.Sprite.Depth - 0.0000025f
                    };

                    companion.StatesSprite.Play("effect_taunt&aggro");

                    companion.IsAggroEffectDraw = true;
                    companion.AggroDrawEffectTimer = 3f;
                }

                if (companion.AggroDrawEffectTimer > 0)
                {
                    var transform = new Transform2
                    {
                        Scale = new Vector2(1f, 1f),
                        Rotation = 0f,
                        Position = new Vector2(companion.Position.X + companion.Sprite.TextureRegion.Width / 5
                        , companion.Position.Y - companion.Sprite.TextureRegion.Height / 5)
                    };

                    companion.StatesSprite.Update(_deltaSeconds);

                    spriteBatch.Draw(companion.StatesSprite, transform);
                }
            }

            // Effect on Companion
            companion.BuffSprite ??= new AnimatedSprite(_statesSpriteSheet)
            {
                Depth = companion.Sprite.Depth - 0.0000026f
            };

            companion.DebuffSprite ??= new AnimatedSprite(_statesSpriteSheet)
            {
                Depth = companion.Sprite.Depth - 0.0000027f
            };

            // Buff
            if (companion.IsBuffOn)
            {
                companion.BuffSprite.Play("effect_buff&shock");

                var transform = new Transform2
                {
                    Scale = new Vector2(1.3f, 1.3f),
                    Rotation = 0f,
                    Position = companion.Position
                };

                companion.BuffSprite.Update(_deltaSeconds);

                spriteBatch.Draw(companion.BuffSprite, transform);
            }

            // Debuff
            if (companion.IsDebuffOn)
            {
                companion.DebuffSprite.Play("effect_debuff");

                var transform = new Transform2
                {
                    Scale = new Vector2(1.3f, 1.3f),
                    Rotation = 0f,
                    Position = companion.Position
                };

                companion.DebuffSprite.Update(_deltaSeconds);

                spriteBatch.Draw(companion.DebuffSprite, transform);
            }
        }

        private static void DrawObjectParticle(SpriteBatch spriteBatch)
        {
            var gameObject = ObjectManager.Instance.GameObjects;

            if (gameObject != null || gameObject.Any())
                foreach (var item in gameObject)
                    spriteBatch.Draw(item.ParticleEffect);
        }
    }
}
