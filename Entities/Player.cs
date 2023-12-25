﻿using Medicraft.Data.Models;
using Medicraft.Items;
using Medicraft.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Sprites;
using System;
using System.Linq;

namespace Medicraft.Entities
{
    public class Player : Entity
    {
        private readonly PlayerStats _playerStats;

        private Vector2 _initHudPos, _initCamPos;

        private float _normalHitSpeed, _burstSkillSpeed, _knockbackForce;

        public Player(AnimatedSprite sprite, PlayerStats playerStats)
        {
            _playerStats = playerStats;
            
            // Initial stats
            Id = playerStats.Id;
            Name = playerStats.Name;
            Level = playerStats.Level;
            EXP = playerStats.EXP;
            ATK = playerStats.ATK;
            HP = playerStats.HP;
            DEF_Percent = (float)playerStats.DEF_Percent;
            Crit_Percent = (float)playerStats.Crit_Percent;
            CritDMG_Percent = (float)playerStats.CritDMG_Percent;
            Speed = playerStats.Speed;
            Evasion = (float)playerStats.Evasion;

            Sprite = sprite;

            _normalHitSpeed = 0.40f;    // Stat
            _burstSkillSpeed = 0.70f;   // Stat
            _knockbackForce = 50f;      // Stat

            var position = new Vector2((float)playerStats.Position[0], (float)playerStats.Position[1]);
            Transform = new Transform2
            {
                Scale = Vector2.One,
                Rotation = 0f,
                Position = position
            };

            BoundingCollisionX = 20;
            BoundingCollisionY = 2.60;
            BoundingDetectCollisions = new Rectangle((int)((int)Position.X - sprite.TextureRegion.Width / BoundingCollisionX)
                , (int)((int)Position.Y + sprite.TextureRegion.Height / BoundingCollisionY)
                , (int)(sprite.TextureRegion.Width / 8), sprite.TextureRegion.Height / 8);

            BoundingHitBox = new CircleF(Position + new Vector2(0f, 32f), 40f);

            BoundingDetection = new CircleF(Position + new Vector2(0f, 32f), 80f);

            BoundingCollection = new CircleF(Position + new Vector2(0f, 64f), 30f);

            Sprite.Depth = 0.3f;
            Sprite.Play("idle");
        }

        // Update Player
        public override void Update(GameTime gameTime, KeyboardState keyboardCur, KeyboardState keyboardPrev
            , MouseState mouseCur, MouseState mousePrev, float depthFrontTile, float depthBehideTile)
        {
            var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Movement Control
            MovementControl(deltaSeconds, keyboardCur);

            // Update layer depth
            UpdateLayerDepth(depthFrontTile, depthBehideTile);

            // Combat Control
            CombatControl(deltaSeconds, keyboardCur, keyboardPrev, mouseCur, mousePrev);

            // Collect Item
            CheckInteraction(keyboardCur, keyboardPrev);

            Sprite.Play(CurrentAnimation);
            Sprite.Update(deltaSeconds);
        }

        // Draw Player
        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Sprite, Transform);

            // Test Draw BoundingRec for Collision
            if (GameGlobals.Instance.IsShowDetectBox)
            {
                var pixelTexture = new Texture2D(ScreenManager.Instance.GraphicsDevice, 1, 1);
                pixelTexture.SetData(new Color[] { Color.White });
                spriteBatch.Draw(pixelTexture, BoundingDetectCollisions, Color.Red);
            }
        }

        // Movement
        private void MovementControl(float deltaSeconds, KeyboardState keyboardState)
        {
            var walkSpeed = deltaSeconds * Speed;
            Velocity = Vector2.Zero;
            InitPos = Position;
            _initHudPos = GameGlobals.Instance.HUDPosition;
            _initCamPos = GameGlobals.Instance.AddingCameraPos;                                             

            if (!IsAttacking)
            {
                if (keyboardState.IsKeyDown(Keys.W) || keyboardState.IsKeyDown(Keys.Up))
                {
                    CurrentAnimation = "walking_up";
                    IsMoving = true;   
                    Velocity -= Vector2.UnitY;
                }

                if (keyboardState.IsKeyDown(Keys.S) || keyboardState.IsKeyDown(Keys.Down))
                {
                    CurrentAnimation = "walking_down";
                    IsMoving = true;
                    Velocity += Vector2.UnitY;
                }

                if (keyboardState.IsKeyDown(Keys.A) || keyboardState.IsKeyDown(Keys.Left))
                {
                    CurrentAnimation = "walking_left";
                    IsMoving = true;
                    Velocity -= Vector2.UnitX;
                }

                if (keyboardState.IsKeyDown(Keys.D) || keyboardState.IsKeyDown(Keys.Right))
                {
                    CurrentAnimation = "walking_right";
                    IsMoving = true;
                    Velocity += Vector2.UnitX;

                    //Position += new Vector2(walkSpeed, 0);
                    //Singleton.Instance.addingHudPos += new Vector2(walkSpeed, 0);
                    //Singleton.Instance.addingCameraPos += new Vector2(walkSpeed, 0);
                }

                var isPlayerMoving = Velocity != Vector2.Zero;
                if (isPlayerMoving)
                {
                    Velocity.Normalize();
                    Position += Velocity * walkSpeed;
                    GameGlobals.Instance.HUDPosition += Velocity * walkSpeed;
                    GameGlobals.Instance.AddingCameraPos += Velocity * walkSpeed;
                }
                else IsMoving = false;

                if (!IsMoving)
                {
                    CurrentAnimation = "idle";
                }

                // Detect Object Collsion
                var ObjectOnTile = GameGlobals.Instance.CollistionObject;
                foreach (var rect in ObjectOnTile)
                {
                    if (rect.Intersects(BoundingDetectCollisions))
                    {   
                        IsDetectCollistionObject = true;
                        Position = InitPos;
                        GameGlobals.Instance.HUDPosition = _initHudPos;
                        GameGlobals.Instance.AddingCameraPos = _initCamPos;
                    }
                    else
                    {
                        IsDetectCollistionObject = false;
                    }
                }

                // Update Camera Position
                ScreenManager.Instance.Camera.Update(deltaSeconds);
            }
        }

        // Combat
        private void CombatControl(float deltaSeconds, KeyboardState keyboardCur, KeyboardState keyboardPrev
            , MouseState mouseCur, MouseState mousePrev)
        {
            // Normal Hit
            if (mouseCur.LeftButton == ButtonState.Pressed
                && mousePrev.LeftButton == ButtonState.Released && !IsAttacking)
            {
                CurrentAnimation = "attacking_normal_hit";

                IsAttacking = true;
                AttackingTime = _normalHitSpeed;

                CheckAttackDetection(ATK, 1f);
            }

            // Burst Skill
            if (keyboardCur.IsKeyDown(Keys.Q) && !IsAttacking)
            {
                CurrentAnimation = "attacking_burst_skill";

                IsAttacking = true;
                AttackingTime = _burstSkillSpeed;

                CheckAttackDetection(ATK, 1f);
            }

            // Check attack timing
            if (AttackingTime > 0)
            {
                AttackingTime -= deltaSeconds;
            }
            else IsAttacking = false;
        }

        private void CheckAttackDetection(int ATK, float PercentATK)
        {
            if (EntityManager.Instance.entities.Count != 0) 
            {
                foreach (var entity in EntityManager.Instance.entities.Where(e => !e.IsDestroyed))
                {
                    if (BoundingDetection.Intersects(entity.BoundingHitBox))
                    {
                        if (!entity.IsDying)
                        {
                            entity.HP -= TotalDamage(ATK, PercentATK, entity.DEF_Percent);

                            Vector2 knockBackDirection = entity.Position - Position;
                            knockBackDirection.Normalize();

                            entity.Velocity = knockBackDirection * _knockbackForce;
                            entity.IsKnockback = true;
                            entity.StunTime = 0.2f;

                            if (entity.HP <= 0)
                            {
                                entity.DyingTime = 1.3f;
                                entity.IsDying = true;
                            }
                        }
                    }
                }
            }
        }

        private int TotalDamage(int ATK, float PercentATK, float PercentDEF)
        {
            // Default case
            int totalDamage = (int)(ATK * PercentATK);

            // Check crit chance
            var random = new Random();
            int critChance = random.Next(1, 101);

            if (critChance <= Crit_Percent * 100)
            {
                totalDamage += (int)(totalDamage * CritDMG_Percent);
            }

            // Calculate DEF
            totalDamage -= (int)(totalDamage * PercentDEF);

            return totalDamage;
        }

        private void CheckInteraction(KeyboardState keyboardCur, KeyboardState keyboardPrev)
        {
            GameGlobals.Instance.IsDetectedItem = false;

            // Check Item Dectection
            if (ItemManager.Instance.items.Count != 0)
            {
                var items = ItemManager.Instance.items;
                foreach (var item in items)
                {
                    if (BoundingCollection.Intersects(item.BoundingCollection))
                    {
                        GameGlobals.Instance.IsDetectedItem = true;
                        break;
                    }
                }
            }

            // Check Table Craft
            var TableCraft = GameGlobals.Instance.TableCraft;
            foreach (var obj in TableCraft)
            {
                if (BoundingDetectCollisions.Intersects(obj))
                {
                    GameGlobals.Instance.IsDetectedItem = true;
                    break;
                }
            }

            // Check Interaction
            if (keyboardCur.IsKeyUp(Keys.F) && keyboardPrev.IsKeyDown(Keys.F))
            {
                if (GameGlobals.Instance.IsDetectedItem)
                {
                    CheckItemDetection();
                    CheckTableCraftDetection();
                }
            }
        }

        private void CheckItemDetection()
        {
            if (ItemManager.Instance.items.Count != 0)
            {
                foreach (var item in ItemManager.Instance.items)
                {
                    if (BoundingCollection.Intersects(item.BoundingCollection))
                    {
                        if (!item.IsCollected)
                        {
                            item.IsCollected = true;
                        }
                    }
                }
            }
        }

        private void CheckTableCraftDetection()
        {
            if (GameGlobals.Instance.TableCraft.Count != 0)
            {
                var TableCraft = GameGlobals.Instance.TableCraft;
                foreach (var obj in TableCraft)
                {
                    if (BoundingDetectCollisions.Intersects(obj))
                    {
                        if (PlayerManager.Instance.Inventory["herb_1"] >= 1
                            && PlayerManager.Instance.Inventory["herb_2"] >= 1)
                        {
                            HudSystem.AddFeed("drug");

                            if (PlayerManager.Instance.Inventory.ContainsKey("drug"))
                            {
                                PlayerManager.Instance.Inventory["drug"] += 1;
                                PlayerManager.Instance.Inventory["herb_1"] -= 1;
                                PlayerManager.Instance.Inventory["herb_2"] -= 1;
                            }
                        }
                        else
                        {
                            HudSystem.ShowInsufficientSign();
                        }

                        break;
                    }
                }
            }
        }

        private void UpdateLayerDepth(float depthFrontTile, float depthBehideTile)
        {
            // Detect for LayerDepth
            Sprite.Depth = depthFrontTile; // Default depth

            var ObjectOnLayer1 = GameGlobals.Instance.ObjectOnLayer1;
            foreach (var obj in ObjectOnLayer1)
            {
                if (obj.Intersects(BoundingDetectCollisions))
                {
                    Sprite.Depth = depthBehideTile;
                    break; // Exit the loop as soon as an intersection is found
                }
            }

            var ObjectOnLayer2 = GameGlobals.Instance.ObjectOnLayer2;
            foreach (var obj in ObjectOnLayer2)
            {
                if (obj.Intersects(BoundingDetectCollisions))
                {
                    Sprite.Depth = depthBehideTile + 0.2f;
                    break; // Exit the loop as soon as an intersection is found
                }
            }
        }

        public PlayerStats GetStats()
        {
            return _playerStats;
        }

        public float GetDepth()
        {
            return Sprite.Depth;
        }
    }
}
