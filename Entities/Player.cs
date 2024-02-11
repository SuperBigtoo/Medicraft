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
        private readonly PlayerData _playerData;

        private float _knockbackForce;
        private readonly float _normalHitSpeed, _normalSkillSpeed, _burstSkillSpeed, _dyingSpeed;

        private float _percentNormalHit, _percentNormalSkill, _percentBurstSkill, _percentPassiveSkill;
        private float _normalSkillCooldownTime, _burstSkillCooldownTime, _passiveSkillCooldownTime;
        private const float _baseCooldownNormal = 16f, _baseCooldownBurst = 20f, _caseCooldownPassive = 60f;

        private bool _isCriticalAttack, _isAttackMissed;
        private bool _isNormalSkillCooldown, _isBurstSkillCooldown, _isPassiveSkillCooldown;

        private Vector2 _initHudPos, _initCamPos;

        public Player(AnimatedSprite sprite, PlayerData playerData)
        {
            Sprite = sprite;
            _playerData = playerData;

            // Initialize Character Data
            Id = 999;
            Level = _playerData.Level;
            EXP = _playerData.EXP;
            InitializeCharacterData(playerData.CharId, Level);
                 
            IsKnockbackable = true;            

            _knockbackForce = 50f;

            _normalHitSpeed = 0.4f;
            _normalSkillSpeed = 0.9f;
            _burstSkillSpeed = 0.7f;
            _dyingSpeed = 10f;
            
            _percentNormalHit = 0.5f;
            _percentNormalSkill = 0.5f;
            _percentBurstSkill = 1.75f;
            _percentPassiveSkill = 0.3f;

            _normalSkillCooldownTime = _baseCooldownNormal;
            _burstSkillCooldownTime = _baseCooldownBurst;
            _passiveSkillCooldownTime = _caseCooldownPassive;

            _isNormalSkillCooldown = false;
            _isBurstSkillCooldown = false;
            _isPassiveSkillCooldown = false;

            var position = new Vector2((float)playerData.Position[0], (float)playerData.Position[1]);
            
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
                , sprite.TextureRegion.Width / 8, sprite.TextureRegion.Height / 8);     // Rec for check Collision

            BoundingHitBox = new CircleF(Position + new Vector2(0f, 32f), 40f);         // Circle for Entity to hit

            BoundingDetectEntity = new CircleF(Position + new Vector2(0f, 32f), 80f);   // Circle for check attacking

            BoundingCollection = new CircleF(Position + new Vector2(0f, 64f), 30f);     // Circle for check interaction with GameObjects

            Sprite.Depth = 0.1f;
            Sprite.Play("idle");
        }

        protected override void InitializeCharacterData(int charId, int level)
        {           
            base.InitializeCharacterData(charId, level);

            // gonna calculate character stats with the equipment's stats too
        }

        // Update Player
        public override void Update(GameTime gameTime, KeyboardState keyboardCur, KeyboardState keyboardPrev
            , MouseState mouseCur, MouseState mousePrev)
        {
            var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (!PlayerManager.Instance.IsPlayerDead)
            {
                // Movement Control
                MovementControl(deltaSeconds, keyboardCur);

                // Update layer depth
                var topDepth = GameGlobals.Instance.TopEntityDepth;
                var middleDepth = GameGlobals.Instance.MiddleEntityDepth;
                var bottomDepth = GameGlobals.Instance.BottomEntityDepth;
                UpdateLayerDepth(topDepth, middleDepth, bottomDepth);

                // Combat Control
                CombatControl(deltaSeconds, keyboardCur, keyboardPrev, mouseCur, mousePrev);

                // Check interaction with GameObject
                CheckInteraction(keyboardCur, keyboardPrev);
            }
            else
            {
                if (!IsDying)
                {
                    IsDying = true;
                    CurrentAnimation = "dying";
                    Sprite.Play(CurrentAnimation);
                }             
            }

            // Update time conditions
            UpdateTimeConditions(deltaSeconds);
  
            Sprite.Update(deltaSeconds);
        }

        // Draw Player
        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Sprite, Transform);

            // Test Draw BoundingRec for Collision Check
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
                    Sprite.Play(CurrentAnimation);
                }
                else IsMoving = false;

                if (!IsMoving)
                {
                    CurrentAnimation = "idle";
                    Sprite.Play(CurrentAnimation);
                }

                // Check Object Collsion
                CheckCollision();

                // Update Camera Position
                ScreenManager.Instance.Camera.Update(deltaSeconds);
            }
        }

        protected override void CheckCollision()
        {
            var ObjectOnTile = GameGlobals.Instance.CollistionObject;
            foreach (var rect in ObjectOnTile)
            {
                if (BoundingDetectCollisions.Intersects(rect))
                {
                    IsDetectCollistionObject = true;
                    Position = InitPos;
                    GameGlobals.Instance.HUDPosition = _initHudPos;
                    GameGlobals.Instance.AddingCameraPos = _initCamPos;
                    break;
                }
                else IsDetectCollistionObject = false;
            }
        }

        // Combat
        private void CombatControl(float deltaSeconds, KeyboardState keyboardCur, KeyboardState keyboardPrev
            , MouseState mouseCur, MouseState mousePrev)
        {
            // Normal Hit
            if (!IsAttacking && mouseCur.LeftButton == ButtonState.Pressed && mousePrev.LeftButton == ButtonState.Released
                || !IsAttacking && keyboardCur.IsKeyDown(Keys.Space))
            {
                CurrentAnimation = "attacking_normal_hit";
                Sprite.Play(CurrentAnimation);

                IsAttacking = true;
                ActionTime = _normalHitSpeed;

                CheckAttackDetection(ATK, _percentNormalHit, false);
            }

            // Normal Skill
            if (keyboardCur.IsKeyDown(Keys.E) && !IsAttacking && !_isNormalSkillCooldown)
            {
                CurrentAnimation = "attacking_normal_skill";
                Sprite.Play(CurrentAnimation);

                IsAttacking = true;
                _isNormalSkillCooldown = true;
                ActionTime = _normalSkillSpeed;

                // Do normal skill & effect of Sets Item
            }

            // Burst Skill
            if (keyboardCur.IsKeyDown(Keys.Q) && !IsAttacking && !_isBurstSkillCooldown)
            {
                CurrentAnimation = "attacking_burst_skill";
                Sprite.Play(CurrentAnimation);

                IsAttacking = true;
                _isBurstSkillCooldown = true;
                ActionTime = _burstSkillSpeed;

                // Do burst skill & effect of Sets Item
                BoundingDetectEntity.Radius = 130f;
                CheckAttackDetection(ATK, _percentBurstSkill, true);
                BoundingDetectEntity.Radius = 80f;
            }

            // Passive Skill
            if (keyboardCur.IsKeyDown(Keys.G) && !IsAttacking && !_isPassiveSkillCooldown)
            {
                CurrentAnimation = "dying";
                Sprite.Play(CurrentAnimation);

                IsAttacking = true;
                _isPassiveSkillCooldown = true;
                ActionTime = _dyingSpeed;

                // Do passive skill
            }
        }

        private void CheckAttackDetection(int Atk, float HitPercent, bool IsUndodgeable)
        {
            foreach (var entity in EntityManager.Instance.Entities.Where(e => !e.IsDestroyed))
            {
                if (BoundingDetectEntity.Intersects(entity.BoundingHitBox))
                {
                    if (entity.Type == EntityType.Hostile)
                    {
                        if (!entity.IsDying)
                        {
                            var totalDamage = TotalDamage(Atk, HitPercent, entity.DEF_Percent, entity.Evasion, IsUndodgeable);
                            
                            if (CombatNumCase != 3) entity.HP -= totalDamage;

                            var knockBackDirection = (entity.Position - new Vector2(0, 50)) - Position;
                            knockBackDirection.Normalize();
                            entity.Velocity = knockBackDirection * _knockbackForce;

                            var combatNumVelocity = entity.SetCombatNumDirection();
                            entity.AddCombatLogNumbers(Name, totalDamage.ToString(), CombatNumCase, combatNumVelocity);

                            if (CombatNumCase != 3)
                            {
                                entity.IsKnockback = true;
                                entity.KnockbackedTime = 0.2f;
                            }
                         
                            entity.IsAttacked = true;                          
                            entity.AttackedTime = 0f;

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

        private int TotalDamage(int ATK, float HitPercent, float DefPercent, float EvasionPercent, bool IsUndodgeable)
        {
            var random = new Random();

            // Default total damage
            int totalDamage = (int)(ATK * HitPercent);

            // Check evasion
            int evaChance = random.Next(1, 101);
            if (evaChance <= EvasionPercent * 100 && !IsUndodgeable)
            {
                // if Attack Missed
                totalDamage = 0;
                CombatNumCase = 3;
                _isAttackMissed = true;
            }
            else
            {
                // if Attacked
                _isAttackMissed = false;

                // Check crit chance           
                int critChance = random.Next(1, 101);
                if (critChance <= Crit_Percent * 100)
                {
                    totalDamage += (int)(totalDamage * CritDMG_Percent);
                    CombatNumCase = 1;
                    _isCriticalAttack = true;
                }
                else
                {
                    CombatNumCase = 0;
                    _isCriticalAttack = false;
                }

                // Calculate DEF
                totalDamage -= (int)(totalDamage * DefPercent);
            }                     

            return totalDamage;
        }

        private void CheckInteraction(KeyboardState keyboardCur, KeyboardState keyboardPrev)
        {
            GameGlobals.Instance.IsDetectedGameObject = false;

            // Check Item Dectection
            var GameObject = ObjectManager.Instance.GameObjects;
            foreach (var gameObject in GameObject)
            {
                if (BoundingCollection.Intersects(gameObject.BoundingCollection))
                {
                    GameGlobals.Instance.IsDetectedGameObject = true;
                    break;
                }
            }

            // Check Table Craft
            //var TableCraft = GameGlobals.Instance.TableCraft;
            //foreach (var obj in TableCraft)
            //{
            //    if (BoundingDetectCollisions.Intersects(obj))
            //    {
            //        GameGlobals.Instance.IsDetectedItem = true;
            //        break;
            //    }
            //}

            // Check Interaction
            if (keyboardCur.IsKeyUp(Keys.F) && keyboardPrev.IsKeyDown(Keys.F))
            {
                if (GameGlobals.Instance.IsDetectedGameObject)
                {
                    CheckGameObject();
                    //CheckTableCraftDetection();
                }
            }
        }

        private void CheckGameObject()
        {
            foreach (var gameObject in ObjectManager.Instance.GameObjects)
            {
                if (BoundingCollection.Intersects(gameObject.BoundingCollection))
                {                   
                    switch (gameObject.Type)
                    {
                        case GameObjects.GameObject.GameObjectType.Item:
                            if (!gameObject.IsCollected
                                && InventoryManager.Instance.Inventory.Count < InventoryManager.Instance.MaximunSlot)
                            {
                                gameObject.IsCollected = true;
                            }
                            break;

                        case GameObjects.GameObject.GameObjectType.QuestItem:
                            if (!gameObject.IsCollected) gameObject.IsCollected = true;
                            break;

                        case GameObjects.GameObject.GameObjectType.CraftingTable:
                            break;

                        case GameObjects.GameObject.GameObjectType.SavingTable:
                            break;

                        case GameObjects.GameObject.GameObjectType.WarpPoint:
                            break;
                    }

                    break;
                }
            }
        }

        // Crafting TBD
        private void CheckTableCraftDetection()
        {
            //if (GameGlobals.Instance.TableCraft.Count != 0)
            //{
            //    var TableCraft = GameGlobals.Instance.TableCraft;
            //    foreach (var obj in TableCraft)
            //    {
            //        if (BoundingDetectCollisions.Intersects(obj))
            //        {
            //            if (PlayerManager.Instance.Inventory["0"] >= 1
            //                && PlayerManager.Instance.Inventory["1"] >= 1)
            //            {
            //                HudSystem.AddFeed(2, 1);

            //                if (PlayerManager.Instance.Inventory.ContainsKey("2"))
            //                {
            //                    PlayerManager.Instance.Inventory["2"] += 1;
            //                    PlayerManager.Instance.Inventory["0"] -= 1;
            //                    PlayerManager.Instance.Inventory["1"] -= 1;
            //                }
            //            }
            //            else
            //            {
            //                HudSystem.ShowInsufficientSign();
            //            }
            //            break;
            //        }
            //    }
            //}
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

        protected override void UpdateTimeConditions(float deltaSeconds)
        {
            // Check attack timing
            if (ActionTime > 0)
            {
                ActionTime -= deltaSeconds;
            }
            else IsAttacking = false;

            if (_isNormalSkillCooldown)
            {
                if (_normalSkillCooldownTime > 0)
                {
                    _normalSkillCooldownTime -= deltaSeconds;
                }
                else
                {
                    _normalSkillCooldownTime = _baseCooldownNormal;
                    _isNormalSkillCooldown = false;
                }
            }

            if (_isBurstSkillCooldown)
            {
                if (_burstSkillCooldownTime > 0)
                {
                    _burstSkillCooldownTime -= deltaSeconds;
                }
                else
                {
                    _burstSkillCooldownTime = _baseCooldownBurst;
                    _isBurstSkillCooldown = false;
                }
            }

            if (_isPassiveSkillCooldown)
            {
                if (_passiveSkillCooldownTime > 0)
                {
                    _passiveSkillCooldownTime -= deltaSeconds;
                }
                else
                {
                    _passiveSkillCooldownTime = _caseCooldownPassive;
                    _isPassiveSkillCooldown = false;
                }
            }

            base.UpdateTimeConditions(deltaSeconds);
        }

        public override Vector2 SetCombatNumDirection()
        {
            float randomFloat = (float)(new Random().NextDouble() * 0.5f) - 0.25f;
            var NumDirection = Position
                - new Vector2(Position.X + (Sprite.TextureRegion.Width / 2) * randomFloat
                , Position.Y - (Sprite.TextureRegion.Height));
            NumDirection.Normalize();

            CombatNumVelocity = NumDirection * (Sprite.TextureRegion.Height / 2);

            return CombatNumVelocity;
        }

        public PlayerData GetPlayerData()
        {
            return _playerData;
        }

        public float GetDepth()
        {
            return Sprite.Depth;
        }
    }
}
