﻿using Medicraft.Systems.Managers;
using Medicraft.Systems;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using MonoGame.Extended.Sprites;
using System.Linq;
using Medicraft.Data.Models;
using MonoGame.Extended;
using Medicraft.Systems.PathFinding;

namespace Medicraft.Entities.Companion
{
    public class Companion : Entity
    {
        public CompanionData CompanionData { get; protected set; }
        public int CompanionId { get; protected set; }

        protected float BaseCooldownNormal = 0, BaseCooldownBurst = 0, BaseCooldownPassive = 0;

        // Is Skill Cooldown
        public bool IsNormalSkillCooldown { get; protected set; }
        public bool IsBurstSkillCooldown { get; protected set; }
        public bool IsPassiveSkillCooldown { get; protected set; }

        // Time Condition Skill
        public float NormalCooldownTime { get; protected set; }
        public float BurstCooldownTime { get; protected set; }
        public float PassiveCooldownTime { get; protected set; }
        public float NormalCooldownTimer { get; protected set; }
        public float BurstCooldownTimer { get; protected set; }
        public float PassiveCooldownTimer { get; protected set; }

        public bool isCriticalAttack, isAttackMissed;

        protected float percentDamageNormalHit;
        protected float hitRateNormal, hitRateNormalSkill, hitRateBurstSkill;

        protected Companion(Vector2 scale)
        {
            stoppingNodeIndex = 2;
            knockbackForce = 40;

            var position = new Vector2(
                PlayerManager.Instance.Player.Position.X,
                PlayerManager.Instance.Player.Position.Y);

            Transform = new Transform2
            {
                Scale = scale,
                Rotation = 0f,
                Position = position
            };
        }

        protected void InitialCooldownSkill()
        {
            IsNormalSkillCooldown = false;
            IsBurstSkillCooldown = false;
            IsPassiveSkillCooldown = false;

            NormalCooldownTime = BaseCooldownNormal;
            BurstCooldownTime = BaseCooldownBurst;
            PassiveCooldownTime = BaseCooldownPassive;

            NormalCooldownTimer = NormalCooldownTime;
            BurstCooldownTimer = BurstCooldownTime;
            PassiveCooldownTimer = PassiveCooldownTime;
        }

        public override void Update(GameTime gameTime, float playerDepth, float topDepth, float middleDepth, float bottomDepth)
        {
            var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (!PlayerManager.Instance.IsCompanionDead)
            {
                // Setup PathFinding
                SetPathFindingNode(
                    (int)PlayerManager.Instance.Player.BoundingDetectCollisions.Center.X,
                    (int)PlayerManager.Instance.Player.BoundingDetectCollisions.Center.Y);

                //System.Diagnostics.Debug.WriteLine($"pathFinding : {pathFinding.GetPath().Count}");

                if (!PlayerManager.Instance.IsPlayerDead)
                {
                    // Combat Control
                    CombatControl(deltaSeconds);

                    // MovementControl
                    MovementControl(deltaSeconds);

                    // Check Aggro Player
                    CheckAggro();

                    // Update Closest Enemy Mob to Player                   
                    if (IsAggro)
                    {
                        var enemyMobs = EntityManager.Instance.Entities.Where(e => !e.IsDying &&
                            (e.EntityType == EntityTypes.Hostile || e.EntityType == EntityTypes.Boss)).ToList();

                        foreach (var mob in enemyMobs)
                            if (PlayerManager.Instance.Player.BoundingAggro.Intersects(mob.BoundingHitBox))
                            {
                                EntityManager.Instance.ClosestEnemy = FindClosestEntity(enemyMobs, this);
                                break;
                            }
                    }
                    else EntityManager.Instance.ClosestEnemy = null;
                }

                // Blinking if attacked
                HitBlinking(deltaSeconds);

                // Update layer depth
                UpdateLayerDepth(playerDepth, topDepth, middleDepth, bottomDepth);
            }
            else
            {
                if (!IsDying)
                {
                    IsDying = true;
                    CurrentAnimation = SpriteCycle + "_dying";
                    Sprite.Play(CurrentAnimation);

                    isBlinkingPlayed = false;
                    blinkingTimer = 0;
                    Sprite.Color = Color.White;
                }

                if (DyingTimer < DyingTime && IsDying)
                {
                    DyingTimer += deltaSeconds;

                    var blinkSpeed = 15f; // Adjust the speed of the blinking effect
                    var alphaMultiplier = MathF.Sin(DyingTimer * blinkSpeed);

                    // Ensure alphaMultiplier is within the valid range [0, 1]
                    alphaMultiplier = MathHelper.Clamp(alphaMultiplier, 0.25f, 2f);

                    Sprite.Color = Color.White * Math.Min(alphaMultiplier, 1f);
                }
            }

            if (PlayerManager.Instance.IsPlayerDead)
            {
                CurrentAnimation = SpriteCycle + "_idle";  // Idle
                Sprite.Play(CurrentAnimation);
            }

            // Update time conditions
            UpdateTimerConditions(deltaSeconds);

            // Ensure hp or mana doesn't exceed the maximum & minimum value
            MinimumCapacity();

            Sprite.Update(deltaSeconds);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (GameGlobals.Instance.IsShowPath)
            {
                pathFinding.Draw(spriteBatch);
            }

            spriteBatch.Draw(Sprite, Transform);

            var shadowTexture = GameGlobals.Instance.GetShadowTexture(GameGlobals.ShadowTextureName.shadow_1);

            DrawShadow(spriteBatch, shadowTexture);

            // Test Draw BoundingRec for Collision
            if (GameGlobals.Instance.IsDebugMode)
            {
                var pixelTexture = new Texture2D(ScreenManager.Instance.GraphicsDevice, 1, 1);
                pixelTexture.SetData(new Color[] { Color.White });
                spriteBatch.Draw(pixelTexture, (Rectangle)BoundingDetectCollisions, Color.Red);
            }
        }

        public override void DrawShadow(SpriteBatch spriteBatch, Texture2D shadowTexture)
        {
            var position = new Vector2(Position.X - (shadowTexture.Width * 1.2f) / 2.2f
                , BoundingDetectCollisions.Center.Y - (shadowTexture.Height * 1.2f) / 5f);

            spriteBatch.Draw(shadowTexture, position, null, Color.White
                , 0f, Vector2.Zero, 1.2f, SpriteEffects.None, Sprite.Depth + 0.0000025f);
        }

        public override Vector2 SetCombatNumDirection()
        {
            Vector2 offset = new(Position.X, Position.Y - Sprite.TextureRegion.Height * 1.5f);

            Vector2 numDirection = Position - offset;
            numDirection.Normalize();

            CombatNumVelocity = numDirection * Sprite.TextureRegion.Height / 1.5f;

            return CombatNumVelocity;
        }

        protected override void MovementControl(float deltaSeconds)
        {
            var walkSpeed = deltaSeconds * Speed;
            prevPos = Position;

            // Check Object Collsion
            CheckCollision();

            // Play Sprite: Idle 
            if (!IsAttacking && !IsMoving)
            {
                CurrentAnimation = SpriteCycle + "_idle";
                Sprite.Play(CurrentAnimation);
            }

            // Check movement according to PathFinding
            if (ScreenManager.Instance.IsScreenLoaded && !IsStunning
                && ((!IsKnockback && !IsAttacking) || (!IsAttacked && !IsAttacking)))
            {
                if (pathFinding != null || pathFinding.GetPath().Count != 0)
                {
                    if (currentNodeIndex < pathFinding.GetPath().Count - stoppingNodeIndex)
                    {
                        // Calculate direction to the next node
                        var direction = new Vector2(
                            pathFinding.GetPath()[currentNodeIndex + 1].Col - pathFinding.GetPath()[currentNodeIndex].Col,
                            pathFinding.GetPath()[currentNodeIndex + 1].Row - pathFinding.GetPath()[currentNodeIndex].Row);
                        direction.Normalize();

                        // Move the character towards the next node
                        Position += direction * walkSpeed;
                        IsMoving = true;

                        // Check Object Collsion
                        CheckCollision();

                        // Check Animation
                        if (direction.Y < 0)
                        {
                            CurrentAnimation = SpriteCycle + "_walking_up";     // Up
                        }
                        if (direction.Y > 0)
                        {
                            CurrentAnimation = SpriteCycle + "_walking_down";     // Down
                        }
                        if (direction.X < 0)
                        {
                            CurrentAnimation = SpriteCycle + "_walking_left";     // Left
                        }
                        if (direction.X > 0)
                        {
                            CurrentAnimation = SpriteCycle + "_walking_right";     // Right
                        }

                        Sprite.Play(CurrentAnimation);

                        // Check if the character has passed the next node
                        var tileSize = GameGlobals.Instance.TILE_SIZE;
                        var nextNodePosition = new Vector2(
                            pathFinding.GetPath()[currentNodeIndex + 1].Col * tileSize + tileSize / 2,
                            pathFinding.GetPath()[currentNodeIndex + 1].Row * tileSize + tileSize / 2);

                        var boundingCenter = new Vector2(
                            BoundingDetectCollisions.Center.X,
                            BoundingDetectCollisions.Center.Y);
                        if ((boundingCenter - nextNodePosition).Length() < tileSize * stoppingNodeIndex + tileSize / 4)
                        {
                            currentNodeIndex++; // Increase currentNodeIndex
                        }

                        //System.Diagnostics.Debug.WriteLine($"currentNodeIndex : {currentNodeIndex} | {pathFinding.GetPath().Count}");
                        //System.Diagnostics.Debug.WriteLine($"Position : {Position}");
                        //System.Diagnostics.Debug.WriteLine($"nextNodePosition : {nextNodePosition}");
                        //System.Diagnostics.Debug.WriteLine($"Length : {(Position - nextNodePosition).Length()}");
                    }
                    else IsMoving = false;
                }
            }
            else IsMoving = false;
        }

        protected override void CombatControl(float deltaSeconds)
        {
            // Normal Hit
            foreach (var entity in EntityManager.Instance.Entities.Where(e => !e.IsDestroyed
                && e.EntityType == EntityTypes.Hostile || e.EntityType == EntityTypes.Boss))
            {
                if (BoundingDetectEntity.Intersects(entity.BoundingHitBox)
                    && ScreenManager.Instance.IsScreenLoaded && !IsAttacking && !IsStunning)
                {
                    CurrentAnimation = SpriteCycle + "_attacking_normal_hit";
                    Sprite.Play(CurrentAnimation);

                    IsAttacking = true;
                    ActionTimer = attackSpeed;
                    cooldownAttackTimer = cooldownAttack;
                }
            }  

            if (IsAttacking)
            {
                // Delay attacking
                if (ActionTimer > 0)
                {
                    ActionTimer -= deltaSeconds;
                }
                else
                {
                    if (!isAttackCooldown)
                    {
                        CheckAttackDetection(ATK, percentDamageNormalHit, false, 0f, NormalHitEffectAttacked);
                        isAttackCooldown = true;
                    }
                    else
                    {
                        if (cooldownAttackTimer > 0)
                        {
                            cooldownAttackTimer -= deltaSeconds;
                        }
                        else
                        {
                            IsAttacking = false;
                            isAttackCooldown = false;
                        }
                    }
                }
            }

            //// Normal Skill
            //if (keyboardCur.IsKeyDown(Keys.E) && !IsAttacking && !IsNormalSkillCooldown && Mana >= NormalSkillCost)
            //{
            //    CurrentAnimation = SpriteCycle + "_attacking_normal_skill";
            //    Sprite.Play(CurrentAnimation);

            //    IsAttacking = true;
            //    IsNormalSkillCooldown = true;
            //    ActionTimer = _hitRateNormalSkill;

            //    // Do normal skill & effect of Sets Item
            //    NormalSkillControl(PlayerData.Abilities.NormalSkillLevel);

            //    CombatNumCase = 4;
            //    var combatNumVelocity = SetCombatNumDirection();
            //    AddCombatLogNumbers(Name, "POWER UP", CombatNumCase, combatNumVelocity, NormalSkillEffectActivated);
            //}

            //// Burst Skill
            //if (keyboardCur.IsKeyDown(Keys.Q) && !IsAttacking && !IsBurstSkillCooldown && Mana >= BurstSkillCost)
            //{
            //    CurrentAnimation = SpriteCycle + "_attacking_burst_skill";
            //    Sprite.Play(CurrentAnimation);

            //    IsAttacking = true;
            //    IsBurstSkillCooldown = true;
            //    ActionTimer = _hitRateBurstSkill;

            //    // Do burst skill & effect of Sets Item
            //    BurstSkillControl(PlayerData.Abilities.BurstSkillLevel);
            //}
        }

        // Check Attack
        protected void CheckAttackDetection(float atk, float percentHit, bool isUndodgeable, float stunTime, string effectAttacked)
        {
            foreach (var entity in EntityManager.Instance.Entities.Where(e => !e.IsDestroyed
                && e.EntityType == EntityTypes.Hostile || e.EntityType == EntityTypes.Boss))
            {
                if (entity.BoundingHitBox.Intersects(BoundingDetectEntity))
                {
                    if (!entity.IsDying)
                    {
                        var totalDamage = TotalDamage(atk, percentHit, entity.DEF, entity.Evasion, isUndodgeable);

                        var combatNumVelocity = entity.SetCombatNumDirection();
                        entity.AddCombatLogNumbers(Name, ((int)totalDamage).ToString()
                            , CombatNumCase, combatNumVelocity, effectAttacked);

                        // In case the Attack doesn't Missed
                        if (CombatNumCase != 3)
                        {
                            // Mob Attacked
                            entity.IsAttacked = true;

                            entity.HP -= totalDamage;

                            if (entity.IsKnockbackable)
                            {
                                var knockBackDirection = (entity.Position - new Vector2(0, 50)) - Position;
                                knockBackDirection.Normalize();
                                entity.Velocity = knockBackDirection * knockbackForce;

                                entity.IsKnockback = true;
                                entity.KnockbackedTimer = 0.2f;
                            }

                            if (stunTime > 0f && entity.IsStunable)
                            {
                                entity.IsStunning = true;
                                entity.StunningTimer = stunTime;
                            }
                        }

                        if (entity.HP <= 0)
                        {
                            entity.IsDying = true;
                        }
                    }
                }
            }
        }

        protected float TotalDamage(float ATK, float HitPercent, float DefPercent, float EvasionPercent, bool IsUndodgeable)
        {
            var random = new Random();

            // Default total damage
            float totalDamage = ATK * HitPercent;

            // Check evasion
            int evaChance = random.Next(1, 101);
            if (evaChance <= EvasionPercent * 100 && !IsUndodgeable)
            {
                // if Attack Missed
                totalDamage = 0;
                CombatNumCase = 3;
                isAttackMissed = true;
            }
            else
            {
                // if Attacked
                isAttackMissed = false;

                // Check crit chance           
                int critChance = random.Next(1, 101);
                if (critChance <= Crit * 100)
                {
                    totalDamage += (int)(totalDamage * CritDMG);
                    CombatNumCase = 1;
                    isCriticalAttack = true;
                }
                else
                {
                    CombatNumCase = 0;
                    isCriticalAttack = false;
                }

                // Calculate DEF
                totalDamage -= (int)(totalDamage * DefPercent);
            }

            return totalDamage;
        }

        public void SetPathFinding(AStar pathFinding)
        {
            this.pathFinding = pathFinding;
        }
    }
}
