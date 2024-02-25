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
        public PlayerData PlayerData { get; private set; }

        private float _knockbackForce, _percentNormalHit, _percentPassiveSkill;
        private readonly float _hitRateNormal, _hitRateNormalSkill, _hitRateBurstSkill;

        private const float _baseCooldownNormal = 16f, _baseCooldownBurst = 20f, _baseCooldownPassive = 60f;
        private bool _isNormalSkillCooldown, _isBurstSkillCooldown, _isPassiveSkillCooldown;
        public float NormalCooldownTimer { get; private set; }
        public float BurstCooldownTimer { get; private set; }
        public float PassiveCooldownTimer { get; private set; }

        private bool _isNormalSkillActivated, _isPassiveSkillActivated;
        public float NormalActivatedTimer {  get; private set; }
        public float PassiveActivatedTimer { get; private set; }

        private bool _isCriticalAttack, _isAttackMissed;

        // For keeping the default stat value when skill activated  
        private int _tmpATK, _tmpMaxHP, _tmpSpeed;
        private float _tmpDEF, _tmpCrit, _tmpCritDMG, _tmpEvasion;

        private Vector2 _initHudPos, _initCamPos;

        public Player(AnimatedSprite sprite, PlayerData playerData)
        {
            Sprite = sprite;
            PlayerData = playerData;

            // Initialize Character Data
            Id = 999;
            Level = PlayerData.Level;
            EXP = PlayerData.EXP;
            InitializeCharacterData(playerData.CharId, Level);
                 
            IsKnockbackable = true;            

            _knockbackForce = 50f;
            _percentNormalHit = 0.5f;
            _percentPassiveSkill = 0.3f;

            _hitRateNormal = 0.5f;
            _hitRateNormalSkill = 0.9f;
            _hitRateBurstSkill = 0.9f;                     

            _isNormalSkillCooldown = false;
            _isBurstSkillCooldown = false;
            _isPassiveSkillCooldown = false;

            NormalCooldownTimer = _baseCooldownNormal;
            BurstCooldownTimer = _baseCooldownBurst;
            PassiveCooldownTimer = _baseCooldownPassive;

            _isNormalSkillActivated = false;
            _isPassiveSkillActivated = false;

            NormalActivatedTimer = 0;
            PassiveActivatedTimer = 0;

            var position = new Vector2((float)playerData.Position[0], (float)playerData.Position[1]);
            
            Transform = new Transform2
            {
                Scale = Vector2.One,
                Rotation = 0f,
                Position = position
            };

            BoundingCollisionX = 20;
            BoundingCollisionY = 2.60;

            BoundingDetectCollisions = new Rectangle((int)((int)Position.X - sprite.TextureRegion.Width / BoundingCollisionX),
                (int)((int)Position.Y + sprite.TextureRegion.Height / BoundingCollisionY),
                sprite.TextureRegion.Width / 8,
                sprite.TextureRegion.Height / 8);     // Rec for check Collision

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
            UpdateTimerConditions(deltaSeconds);
  
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
                ActionTimer = _hitRateNormal;

                CheckAttackDetection(ATK, _percentNormalHit, false, 0f);
            }

            // Normal Skill
            if (keyboardCur.IsKeyDown(Keys.E) && !IsAttacking && !_isNormalSkillCooldown)
            {
                CurrentAnimation = "attacking_normal_skill";
                Sprite.Play(CurrentAnimation);

                IsAttacking = true;
                _isNormalSkillCooldown = true;
                ActionTimer = _hitRateNormalSkill;

                // Do normal skill & effect of Sets Item
                NormalSkillControl(PlayerData.Abilities.NormalSkillLevel);
            }

            // Burst Skill
            if (keyboardCur.IsKeyDown(Keys.Q) && !IsAttacking && !_isBurstSkillCooldown)
            {
                CurrentAnimation = "attacking_burst_skill";
                Sprite.Play(CurrentAnimation);

                IsAttacking = true;
                _isBurstSkillCooldown = true;
                ActionTimer = _hitRateBurstSkill;

                // Do burst skill & effect of Sets Item
                BurstSkillControl(PlayerData.Abilities.BurstSkillLevel);
            }

            // Passive Skill
            if (HP < (MaximumHP * 0.1) && !_isPassiveSkillCooldown)
            {
                _isPassiveSkillCooldown = true;

                // Do passive skill
                PassiveSkillControl(PlayerData.Abilities.PassiveSkillLevel);
            }
        }

        // Skills Control
        /// <summary>
        /// Increase Player character's stats such as ATK, Crit_Percent and CritDMG_Percent for an amount of time
        /// </summary>
        /// <param name="skillLevel">Normal Skill level base on PlayerData.Abilities.NormalSkillLevel</param>
        private void NormalSkillControl(int skillLevel)
        {
            _tmpATK = ATK;
            _tmpCrit = Crit_Percent;
            _tmpCritDMG = CritDMG_Percent;

            switch (skillLevel)
            {
                case 1:
                    ATK += (int)(ATK * 0.5);
                    Crit_Percent += 0.1f;
                    CritDMG_Percent += 0.5f;
                    NormalActivatedTimer = 8f;
                    break;

                case 2:
                    break;

                case 3:
                    break;

                case 4:
                    break;

                case 5:
                    break;

                case 6:
                    break;

                case 7:
                    break;

                case 8:
                    break;

                case 9:
                    break;

                case 10:
                    break;
            }

            _isNormalSkillActivated = true;
        }

        /// <summary>
        /// Deals large damage in a wide area surrounding the Player Character knockback and stun mobs for an amount of time
        /// </summary>
        /// <param name="skillLevel">Burst Skill level base on PlayerData.Abilities.BurstSkillLevel</param>
        private void BurstSkillControl(int skillLevel)
        {
            switch (skillLevel)
            {
                case 1:
                    BoundingDetectEntity.Radius = 140f;
                    _knockbackForce = 60f;
                    CheckAttackDetection(ATK, 1.75f, true, 1.5f);
                    break;

                case 2:
                    break;

                case 3:
                    break;

                case 4:
                    break;

                case 5:
                    break;

                case 6:
                    break;

                case 7:
                    break;

                case 8:
                    break;

                case 9:
                    break;

                case 10:
                    break;
            }

            BoundingDetectEntity.Radius = 80f;
            _knockbackForce = 50f;
        }

        /// <summary>
        /// Instantly restore Player Character's HP when the HP is below 10% and increase DEF_Percent for an amount of time
        /// </summary>
        /// <param name="skillLevel">Passive Skill level base on PlayerData.Abilities.PassiveSkillLevel</param>
        private void PassiveSkillControl(int skillLevel)
        {
            _tmpDEF = DEF_Percent;

            switch (skillLevel)
            {
                case 1:
                    HP += (int)(MaximumHP * 0.25);
                    DEF_Percent += 0.25f;
                    PassiveActivatedTimer = 8f;
                    break;

                case 2:
                    break;

                case 3:
                    break;

                case 4:
                    break;

                case 5:
                    break;

                case 6:
                    break;

                case 7:
                    break;

                case 8:
                    break;

                case 9:
                    break;

                case 10:
                    break;
            }

            _isPassiveSkillActivated = true;
        }

        // Check Attack
        private void CheckAttackDetection(int Atk, float HitPercent, bool IsUndodgeable, float StunTime)
        {
            foreach (var entity in EntityManager.Instance.Entities.Where(e => !e.IsDestroyed))
            {
                if (BoundingDetectEntity.Intersects(entity.BoundingHitBox))
                {
                    if (entity.EntityType == EntityTypes.Hostile)
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

                            entity.IsAttacked = true;
                            entity.AttackedTimer = 0f;

                            if (entity.HP <= 0)
                            {
                                entity.IsDying = true;
                            }

                            if (CombatNumCase != 3)
                            {
                                entity.IsKnockback = true;
                                entity.KnockbackedTimer = 0.2f;
                            }

                            if (StunTime > 0f && entity.IsStunable)
                            {
                                entity.IsStunning = true;
                                entity.StunningTimer = StunTime;
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
                                && InventoryManager.Instance.InventoryBag.Count < InventoryManager.Instance.MaximunSlot)
                            {
                                gameObject.IsCollected = true;
                            }
                            else
                            {
                                HudSystem.ShowInsufficientSign();
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

        protected override void UpdateTimerConditions(float deltaSeconds)
        {
            // Check attack timing
            if (ActionTimer > 0)
            {
                ActionTimer -= deltaSeconds;
            }
            else IsAttacking = false;

            // Cooldown time Normal Skill
            if (_isNormalSkillCooldown)
            {
                if (NormalCooldownTimer > 0)
                {
                    NormalCooldownTimer -= deltaSeconds;
                }
                else
                {
                    NormalCooldownTimer = _baseCooldownNormal;
                    _isNormalSkillCooldown = false;
                }
            }

            // Activated time Normal Skill
            if (_isNormalSkillActivated)
            {
                if (NormalActivatedTimer > 0)
                {
                    NormalActivatedTimer -= deltaSeconds;
                }
                else
                {
                    ATK = _tmpATK;
                    Crit_Percent = _tmpCrit;
                    CritDMG_Percent = _tmpCritDMG;
                    NormalActivatedTimer = 0;
                    _isNormalSkillActivated = false;                 
                }
            }

            // Cooldown time Burst Skill
            if (_isBurstSkillCooldown)
            {
                if (BurstCooldownTimer > 0)
                {
                    BurstCooldownTimer -= deltaSeconds;
                }
                else
                {
                    BurstCooldownTimer = _baseCooldownBurst;
                    _isBurstSkillCooldown = false;
                }
            }

            // Cooldown time Passive Skill
            if (_isPassiveSkillCooldown)
            {
                if (PassiveCooldownTimer > 0)
                {
                    PassiveCooldownTimer -= deltaSeconds;
                }
                else
                {
                    PassiveCooldownTimer = _baseCooldownPassive;
                    _isPassiveSkillCooldown = false;
                }
            }

            // Activated time Passive Skill
            if (_isPassiveSkillActivated)
            {
                if (PassiveActivatedTimer > 0)
                {
                    PassiveActivatedTimer -= deltaSeconds;
                }
                else
                {
                    DEF_Percent = _tmpDEF;
                    PassiveActivatedTimer = 0;
                    _isPassiveSkillActivated = false;
                }
            }

            base.UpdateTimerConditions(deltaSeconds);
        }

        public override Vector2 SetCombatNumDirection()
        {
            Vector2 offset = new Vector2(Position.X, Position.Y - Sprite.TextureRegion.Height * 1.5f);

            Vector2 numDirection = Position - offset;
            numDirection.Normalize();

            CombatNumVelocity = numDirection * Sprite.TextureRegion.Height / 2;

            return CombatNumVelocity;
        }

        public float GetDepth()
        {
            return Sprite.Depth;
        }
    }
}
