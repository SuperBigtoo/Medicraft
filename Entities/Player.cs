using Medicraft.Data.Models;
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
        private readonly PlayerStats _basePlayerStats;

        private Vector2 _initHudPos, _initCamPos;

        private readonly float _normalHitSpeed, _burstSkillSpeed, _knockbackForce;

        public Player(AnimatedSprite sprite, PlayerStats basePlayerStats)
        {
            _basePlayerStats = basePlayerStats;
            
            // Initial stats
            InitializeStats();

            Sprite = sprite;

            _normalHitSpeed = 0.40f;    // Stat
            _burstSkillSpeed = 0.70f;   // Stat
            _knockbackForce = 50f;      // Stat

            var position = new Vector2((float)basePlayerStats.Position[0], (float)basePlayerStats.Position[1]);
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

        private void InitializeStats()
        {
            Id = _basePlayerStats.CharId;
            Name = _basePlayerStats.Name;
            Level = _basePlayerStats.Level;
            EXP = _basePlayerStats.EXP;
            ATK = _basePlayerStats.ATK;
            HP = _basePlayerStats.HP;
            DEF_Percent = (float)_basePlayerStats.DEF_Percent;
            Crit_Percent = (float)_basePlayerStats.Crit_Percent;
            CritDMG_Percent = (float)_basePlayerStats.CritDMG_Percent;
            Speed = _basePlayerStats.Speed;
            Evasion = (float)_basePlayerStats.Evasion;
        }

        // Update Player
        public override void Update(GameTime gameTime, KeyboardState keyboardCur, KeyboardState keyboardPrev
            , MouseState mouseCur, MouseState mousePrev, float topDepth, float middleDepth, float bottomDepth)
        {
            var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Movement Control
            MovementControl(deltaSeconds, keyboardCur);

            // Update layer depth
            UpdateLayerDepth(topDepth, middleDepth, bottomDepth);

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
            foreach (var entity in EntityManager.Instance.Entities.Where(e => !e.IsDestroyed))
            {
                if (BoundingDetection.Intersects(entity.BoundingHitBox))
                {
                    if (!entity.IsDying)
                    {
                        entity.HP -= TotalDamage(ATK, PercentATK, entity.DEF_Percent);

                        var knockBackDirection = (entity.Position - new Vector2(0, 50)) - Position;
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
            var GameObject = ObjectManager.Instance.GameObjects;
            foreach (var gameObject in GameObject)
            {
                if (BoundingCollection.Intersects(gameObject.BoundingCollection))
                {
                    GameGlobals.Instance.IsDetectedItem = true;
                    break;
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
            foreach (var gameObject in ObjectManager.Instance.GameObjects)
            {
                if (BoundingCollection.Intersects(gameObject.BoundingCollection))
                {
                    if (!gameObject.IsCollected
                        && InventoryManager.Instance.Inventory.Count < InventoryManager.Instance.MaximunSlot)
                    {
                        gameObject.IsCollected = true;
                    }
                }
            }
        }

        // Crafting TBD
        private void CheckTableCraftDetection()
        {
            if (GameGlobals.Instance.TableCraft.Count != 0)
            {
                var TableCraft = GameGlobals.Instance.TableCraft;
                foreach (var obj in TableCraft)
                {
                    if (BoundingDetectCollisions.Intersects(obj))
                    {
                        if (PlayerManager.Instance.Inventory["0"] >= 1
                            && PlayerManager.Instance.Inventory["1"] >= 1)
                        {
                            HudSystem.AddFeed(2);

                            if (PlayerManager.Instance.Inventory.ContainsKey("2"))
                            {
                                PlayerManager.Instance.Inventory["2"] += 1;
                                PlayerManager.Instance.Inventory["0"] -= 1;
                                PlayerManager.Instance.Inventory["1"] -= 1;
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

        private void UpdateLayerDepth(float topDepth, float middleDepth, float bottomDepth)
        {
            // Detect for LayerDepth
            Sprite.Depth = topDepth; // Default depth

            var TopLayerObject = GameGlobals.Instance.TopLayerObject;
            foreach (var obj in TopLayerObject)
            {
                if (obj.Intersects(BoundingDetectCollisions))
                {
                    Sprite.Depth = middleDepth;
                    break; // Exit the loop as soon as an intersection is found
                }
            }

            var MiddleLayerObject = GameGlobals.Instance.MiddleLayerObject;
            foreach (var obj in MiddleLayerObject)
            {
                if (obj.Intersects(BoundingDetectCollisions))
                {
                    Sprite.Depth = bottomDepth;
                    break;
                }
            }

            var BottomLayerObject = GameGlobals.Instance.BottomLayerObject;
            foreach (var obj in BottomLayerObject)
            {
                if (obj.Intersects(BoundingDetectCollisions))
                {
                    Sprite.Depth = bottomDepth + 0.2f;
                    break;
                }
            }
        }

        public PlayerStats GetStats()
        {
            return _basePlayerStats;
        }

        public float GetDepth()
        {
            return Sprite.Depth;
        }
    }
}
