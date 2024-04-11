using Medicraft.Data.Models;
using Medicraft.Systems;
using Medicraft.Systems.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Sprites;
using System;
using System.Linq;
using static Medicraft.Systems.GameGlobals;

namespace Medicraft.Entities
{
    public class Player : Entity
    {
        public PlayerData PlayerData { get; private set; }

        public const float NormalSkillCost = 15f, BurstSkillCost = 20f;
        public const float BaseCooldownNormal = 16f, BaseCooldownBurst = 20f, BaseCooldownPassive = 60f;

        // Is Skill Cooldown
        public bool IsNormalSkillCooldown { get; private set; }
        public bool IsBurstSkillCooldown { get; private set; }
        public bool IsPassiveSkillCooldown { get; private set; }

        // Time Condition Skill
        public float NormalCooldownTime { get; private set; }
        public float BurstCooldownTime { get; private set; }
        public float PassiveCooldownTime { get; private set; }
        public float NormalCooldownTimer { get; private set; }
        public float BurstCooldownTimer { get; private set; }
        public float PassiveCooldownTimer { get; private set; }

        // Activate Skill
        public bool IsNormalSkillActivate { get; private set; }
        public bool IsPassiveSkillActivate { get; private set; }
        public float NormalActivatedTime { get; private set; }
        public float NormalActivatedTimer {  get; private set; }
        public float PassiveActivatedTime { get; private set; }
        public float PassiveActivatedTimer { get; private set; }      

        public int tempSpeed;
        public float tempATK, tempHP, tempMana, tempDEF, tempCrit, tempCritDMG, tempEvasion;

        private float _percentNormalHit;
        private readonly float _hitRateNormal, _hitRateNormalSkill, _hitRateBurstSkill;
        private bool _isCriticalAttack, _isAttackMissed;

        private Vector2 _initHudPos, _initCamPos;
        private readonly float _stepSoundTime = 0.3f;
        private float _stepSoundTimer = 0;

        public Player(AnimatedSprite sprite, PlayerData playerData)
        {
            Sprite = sprite;
            PlayerData = playerData;

            // Initialize Character Data
            Id = playerData.CharId;
            Level = PlayerData.Level;
            EXP = PlayerData.EXP;         
            InitializeCharacterData(playerData.CharId, Level);
                 
            IsKnockbackable = true;

            baseknockbackForce = 50f;
            knockbackForce = baseknockbackForce;
            _percentNormalHit = 0.5f;

            _hitRateNormal = 0.5f;
            _hitRateNormalSkill = 0.9f;
            _hitRateBurstSkill = 0.9f;

            IsNormalSkillCooldown = false;
            IsBurstSkillCooldown = false;
            IsPassiveSkillCooldown = false;

            NormalCooldownTime = BaseCooldownNormal;
            BurstCooldownTime = BaseCooldownBurst;
            PassiveCooldownTime = BaseCooldownPassive;

            NormalCooldownTimer = NormalCooldownTime;
            BurstCooldownTimer = BurstCooldownTime;
            PassiveCooldownTimer = PassiveCooldownTime;

            IsNormalSkillActivate = false;
            IsPassiveSkillActivate = false;

            NormalActivatedTimer = 0;
            PassiveActivatedTimer = 0;

            var position = new Vector2(
                (float)playerData.Position[0],
                (float)playerData.Position[1]);
            
            Transform = new Transform2
            {
                Scale = Vector2.One,
                Rotation = 0f,
                Position = position
            };

            BoundingCollisionX = 20f;
            BoundingCollisionY = 2.6f;
            BaseBoundingDetectEntityRadius = 80f;

            // Rec for check Collision
            BoundingDetectCollisions = new RectangleF(
                (int)((int)Position.X - sprite.TextureRegion.Width / BoundingCollisionX),
                (int)((int)Position.Y + sprite.TextureRegion.Height / BoundingCollisionY),
                sprite.TextureRegion.Width / 8,
                sprite.TextureRegion.Height / 8);

            BoundingHitBox = new CircleF(Position + new Vector2(0f, 32f), 42f);         // Circle for Entity to hit
            BoundingDetectEntity = new CircleF(
                Position + new Vector2(0f, 32f), BaseBoundingDetectEntityRadius);       // Circle for check attacking
            BoundingInteraction = new CircleF(Position + new Vector2(0f, 60f), 32f);     // Circle for check interaction with GameObjects
            BoundingAggro = new CircleF(Position + new Vector2(0f, 32f), 150);          // Circle for check aggro enemy mobs

            NormalHitEffectAttacked = "hit_effect_1";
            NormalSkillEffectActivated = "hit_skill_effect_3";
            BurstSkillEffectAttacked = "hit_effect_3";
            BurstSkillEffectActivated = "hit_skill_effect_2";
            PassiveSkillEffectActivated = "hit_skill_effect_1";

            Sprite.Depth = InitDepth;
            Sprite.Play(SpriteCycle + "_idle");
        }

        protected override void InitializeCharacterData(int charId, int level)
        {           
            base.InitializeCharacterData(charId, level);       

            // Set current HP & Mana
            HP = (float)(BaseMaxHP * PlayerData.CurrentHPPercentage);
            Mana = (float)(BaseMaxMana * PlayerData.CurrentManaPercentage);
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
                var topDepth = Instance.TopEntityDepth;
                var middleDepth = Instance.MiddleEntityDepth;
                var bottomDepth = Instance.BottomEntityDepth;
                UpdateLayerDepth(topDepth, middleDepth, bottomDepth);

                // Combat Control
                CombatControl(deltaSeconds, keyboardCur, keyboardPrev, mouseCur, mousePrev);

                // Blinking if attacked
                HitBlinking(deltaSeconds);

                // Check interaction with GameObject
                PlayerManager.Instance.CheckInteraction(keyboardCur, keyboardPrev);

                // Mana regeneration
                ManaRegeneration(deltaSeconds);
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
            }

            // Update time conditions
            UpdateTimerConditions(deltaSeconds);

            // Ensure hp or mana doesn't exceed the maximum & minimum value
            MinimumCapacity();

            Sprite.Update(deltaSeconds);
        }

        // Draw Player
        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Sprite, Transform);

            var shadowTexture = GetShadowTexture(ShadowTextureName.shadow_1);

            DrawShadow(spriteBatch, shadowTexture);

            // Test Draw BoundingRec for Collision Check
            if (Instance.IsDebugMode)
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

        // Movement
        private void MovementControl(float deltaSeconds, KeyboardState keyboardState)
        {
            var walkSpeed = deltaSeconds * Speed;
            Velocity = Vector2.Zero;
            prevPos = Position;
            _initHudPos = Instance.TopLeftCornerPos;
            _initCamPos = Instance.AddingCameraPos;                                             

            if (!IsAttacking && !IsStunning && ScreenManager.Instance.IsScreenLoaded)
            {
                if (keyboardState.IsKeyDown(Keys.W) || keyboardState.IsKeyDown(Keys.Up))
                {
                    CurrentAnimation = SpriteCycle + "_walking_up";
                    IsMoving = true;   
                    Velocity -= Vector2.UnitY;
                }

                if (keyboardState.IsKeyDown(Keys.S) || keyboardState.IsKeyDown(Keys.Down))
                {
                    CurrentAnimation = SpriteCycle + "_walking_down";
                    IsMoving = true;
                    Velocity += Vector2.UnitY;
                }

                if (keyboardState.IsKeyDown(Keys.A) || keyboardState.IsKeyDown(Keys.Left))
                {
                    CurrentAnimation = SpriteCycle + "_walking_left";
                    IsMoving = true;
                    Velocity -= Vector2.UnitX;
                }

                if (keyboardState.IsKeyDown(Keys.D) || keyboardState.IsKeyDown(Keys.Right))
                {
                    CurrentAnimation = SpriteCycle + "_walking_right";
                    IsMoving = true;
                    Velocity += Vector2.UnitX;

                    // Classic
                    //Position += new Vector2(walkSpeed, 0);
                    //Singleton.Instance.addingHudPos += new Vector2(walkSpeed, 0);
                    //Singleton.Instance.addingCameraPos += new Vector2(walkSpeed, 0);
                }

                var isPlayerMoving = Velocity != Vector2.Zero;
                if (isPlayerMoving)
                {
                    Velocity.Normalize();
                    Position += Velocity * walkSpeed;
                    Instance.TopLeftCornerPos += Velocity * walkSpeed;
                    Instance.AddingCameraPos += Velocity * walkSpeed;
                    Sprite.Play(CurrentAnimation);

                    _stepSoundTimer += deltaSeconds;

                    if (_stepSoundTimer > _stepSoundTime)
                    {
                        _stepSoundTimer = 0;
                        PlaySoundEffect(Sound.Step_grass);
                    }
                }
                else IsMoving = false;

                if (!IsMoving)
                {
                    CurrentAnimation = SpriteCycle + "_idle";
                    Sprite.Play(CurrentAnimation);
                }

                // Check Object Collsion
                CheckCollision();

                // Update Camera Position
                ScreenManager.Camera.Update(deltaSeconds);
            }
        }

        protected override void CheckCollision()
        {
            var ObjectOnTile = Instance.CollistionObject;
            foreach (var rect in ObjectOnTile)
            {
                if (BoundingDetectCollisions.Intersects(rect))
                {
                    IsDetectCollistionObject = true;
                    Position = prevPos;
                    Instance.TopLeftCornerPos = _initHudPos;
                    Instance.AddingCameraPos = _initCamPos;
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
            if (ScreenManager.Instance.IsScreenLoaded && (!IsAttacking && !IsStunning 
                && !UIManager.Instance.IsQuickMenuFocus && mouseCur.LeftButton == ButtonState.Pressed
                && mousePrev.LeftButton == ButtonState.Released || !IsAttacking && keyboardCur.IsKeyDown(Keys.Space)))
            {
                CurrentAnimation = SpriteCycle + "_attacking_normal_hit";
                Sprite.Play(CurrentAnimation);

                IsAttacking = true;
                ActionTimer = _hitRateNormal;

                CheckAttackDetection(ATK, _percentNormalHit, false, 0f, NormalHitEffectAttacked);

                PlaySoundEffect([Sound.dullSwoosh1, Sound.dullSwoosh2, Sound.dullSwoosh3, Sound.dullSwoosh4]);
            }

            // Normal Skill
            if (keyboardCur.IsKeyDown(Keys.E) && !IsAttacking && !IsNormalSkillCooldown && Mana >= NormalSkillCost)
            {
                CurrentAnimation = SpriteCycle + "_attacking_normal_skill";
                Sprite.Play(CurrentAnimation);

                IsAttacking = true;
                IsNormalSkillCooldown = true;
                ActionTimer = _hitRateNormalSkill;

                // Do normal skill & effect of Sets Item
                NormalSkillControl(PlayerData.Abilities.NormalSkillLevel);

                CombatNumCase = Buff;
                var combatNumVelocity = SetCombatNumDirection();
                AddCombatLogNumbers(Name,
                    "POWER UP",
                    CombatNumCase,
                    combatNumVelocity,
                    NormalSkillEffectActivated);
            }

            // Burst Skill
            if (keyboardCur.IsKeyDown(Keys.Q) && !IsAttacking && !IsBurstSkillCooldown && Mana >= BurstSkillCost)
            {
                CurrentAnimation = SpriteCycle + "_attacking_burst_skill";
                Sprite.Play(CurrentAnimation);

                IsAttacking = true;
                IsBurstSkillCooldown = true;
                ActionTimer = _hitRateBurstSkill;

                // Do burst skill & effect of Sets Item
                BurstSkillControl(PlayerData.Abilities.BurstSkillLevel);

                CombatNumCase = Buff;
                var combatNumVelocity = SetCombatNumDirection();
                AddCombatLogNumbers(Name,
                    "Strike!",
                    CombatNumCase,
                    combatNumVelocity,
                    BurstSkillEffectActivated);

                PlaySoundEffect(Sound.frostbolt_1);

                PlaySoundEffect([Sound.ColossusSmash_Impact_01, Sound.Shield_Bash_04]);
            }

            // Passive Skill
            if (HP < (MaxHP * 0.1) && !IsPassiveSkillCooldown)
            {
                IsPassiveSkillCooldown = true;

                // Do passive skill
                var combatText = (int)PassiveSkillControl(PlayerData.Abilities.PassiveSkillLevel);

                CombatNumCase = Healing;
                var combatNumVelocity = SetCombatNumDirection();
                AddCombatLogNumbers(
                    Name,
                    combatText.ToString(),
                    CombatNumCase,
                    combatNumVelocity,
                    PassiveSkillEffectActivated);
            }
        }

        // Skills Control
        /// <summary>
        /// Increase Player character's stats such as ATK, Crit_Percent and CritDMG_Percent for an amount of time
        /// </summary>
        /// <param name="skillLevel">Normal Skill level base on PlayerData.Abilities.NormalSkillLevel</param>
        private void NormalSkillControl(int skillLevel)
        {
            var manaCost = NormalSkillCost;

            switch (skillLevel)
            {
                case 1:
                    tempATK = (int)(BaseATK * 0.15);
                    tempCrit = 0.1f;
                    tempCritDMG = 0.2f;
                    NormalActivatedTime = 6f;

                    ATK += tempATK;
                    Crit += tempCrit;
                    CritDMG += tempCritDMG;
                    NormalActivatedTimer = NormalActivatedTime;
                    break;

                case 2:
                    tempATK = (int)(BaseATK * 0.2);
                    tempCrit = 0.125f;
                    tempCritDMG = 0.3f;
                    NormalActivatedTime = 6f;

                    ATK += tempATK;
                    Crit += tempCrit;
                    CritDMG += tempCritDMG;
                    NormalActivatedTimer = NormalActivatedTime;
                    break;

                case 3:
                    tempATK = (int)(BaseATK * 0.25);
                    tempCrit = 0.15f;
                    tempCritDMG = 0.4f;
                    NormalActivatedTime = 6f;

                    ATK += tempATK;
                    Crit += tempCrit;
                    CritDMG += tempCritDMG;
                    NormalActivatedTimer = NormalActivatedTime;
                    break;

                case 4:
                    tempATK = (int)(BaseATK * 0.3);
                    tempCrit = 0.175f;
                    tempCritDMG = 0.5f;
                    NormalActivatedTime = 6f;

                    ATK += tempATK;
                    Crit += tempCrit;
                    CritDMG += tempCritDMG;
                    NormalActivatedTimer = NormalActivatedTime;
                    break;

                case 5:
                    tempATK = (int)(BaseATK * 0.35);
                    tempCrit = 0.2f;
                    tempCritDMG = 0.6f;
                    NormalActivatedTime = 6f;

                    ATK += tempATK;
                    Crit += tempCrit;
                    CritDMG += tempCritDMG;
                    NormalActivatedTimer = NormalActivatedTime;
                    break;

                case 6:
                    tempATK = (int)(BaseATK * 0.4);
                    tempCrit = 0.225f;
                    tempCritDMG = 0.7f;
                    NormalActivatedTime = 6f;

                    ATK += tempATK;
                    Crit += tempCrit;
                    CritDMG += tempCritDMG;
                    NormalActivatedTimer = NormalActivatedTime;
                    break;

                case 7:
                    tempATK = (int)(BaseATK * 0.45);
                    tempCrit = 0.25f;
                    tempCritDMG = 0.8f;
                    NormalActivatedTime = 6f;

                    ATK += tempATK;
                    Crit += tempCrit;
                    CritDMG += tempCritDMG;
                    NormalActivatedTimer = NormalActivatedTime;
                    break;

                case 8:
                    tempATK = (int)(BaseATK * 0.5);
                    tempCrit = 0.275f;
                    tempCritDMG = 0.9f;
                    NormalActivatedTime = 6f;

                    ATK += tempATK;
                    Crit += tempCrit;
                    CritDMG += tempCritDMG;
                    NormalActivatedTimer = NormalActivatedTime;
                    break;

                case 9:
                    tempATK = (int)(BaseATK * 0.55);
                    tempCrit = 0.3f;
                    tempCritDMG = 1f;
                    NormalActivatedTime = 6f;

                    ATK += tempATK;
                    Crit += tempCrit;
                    CritDMG += tempCritDMG;
                    NormalActivatedTimer = NormalActivatedTime;
                    break;

                case 10:
                    tempATK = (int)(BaseATK * 0.6);
                    tempCrit = 0.325f;
                    tempCritDMG = 1.1f;
                    NormalActivatedTime = 6f;

                    ATK += tempATK;
                    Crit += tempCrit;
                    CritDMG += tempCritDMG;
                    NormalActivatedTimer = NormalActivatedTime;
                    break;
            }

            IsNormalSkillActivate = true;
            Mana -= manaCost;
        }

        /// <summary>
        /// Deals large damage in a wide area surrounding the Player Character knockback and stun mobs for an amount of time
        /// </summary>
        /// <param name="skillLevel">Burst Skill level base on PlayerData.Abilities.BurstSkillLevel</param>
        private void BurstSkillControl(int skillLevel)
        {
            var manaCost = BurstSkillCost;

            switch (skillLevel)
            {
                case 1:
                    BoundingDetectEntity.Radius = 140f;
                    knockbackForce = 60f;
                    CheckAttackDetection(ATK, 1.25f, true, 1f, BurstSkillEffectAttacked);
                    break;

                case 2:
                    BoundingDetectEntity.Radius = 140f;
                    knockbackForce = 60f;
                    CheckAttackDetection(ATK, 1.4f, true, 1.25f, BurstSkillEffectAttacked);
                    break;

                case 3:
                    BoundingDetectEntity.Radius = 140f;
                    knockbackForce = 70f;
                    CheckAttackDetection(ATK, 1.55f, true, 1.5f, BurstSkillEffectAttacked);
                    break;

                case 4:
                    BoundingDetectEntity.Radius = 140f;
                    knockbackForce = 70f;
                    CheckAttackDetection(ATK, 1.7f, true, 1.75f, BurstSkillEffectAttacked);
                    break;

                case 5:
                    BoundingDetectEntity.Radius = 140f;
                    knockbackForce = 80f;
                    CheckAttackDetection(ATK, 1.85f, true, 2f, BurstSkillEffectAttacked);
                    break;

                case 6:
                    BoundingDetectEntity.Radius = 140f;
                    knockbackForce = 80f;
                    CheckAttackDetection(ATK, 2f, true, 2.25f, BurstSkillEffectAttacked);
                    break;

                case 7:
                    BoundingDetectEntity.Radius = 140f;
                    knockbackForce = 90f;
                    CheckAttackDetection(ATK, 2.15f, true, 2.5f, BurstSkillEffectAttacked);
                    break;

                case 8:
                    BoundingDetectEntity.Radius = 140f;
                    knockbackForce = 90f;
                    CheckAttackDetection(ATK, 2.3f, true, 2.75f, BurstSkillEffectAttacked);
                    break;

                case 9:
                    BoundingDetectEntity.Radius = 140f;
                    knockbackForce = 100f;
                    CheckAttackDetection(ATK, 2.45f, true, 3f, BurstSkillEffectAttacked);
                    break;

                case 10:
                    BoundingDetectEntity.Radius = 140f;
                    knockbackForce = 100f;
                    CheckAttackDetection(ATK, 2.6f, true, 3.25f, BurstSkillEffectAttacked);
                    break;
            }

            BoundingDetectEntity.Radius = BaseBoundingDetectEntityRadius;
            knockbackForce = baseknockbackForce;
            Mana -= manaCost;
        }

        /// <summary>
        /// Instantly restore Player Character's HP when the HP is below 10% and increase DEF_Percent for an amount of time
        /// </summary>
        /// <param name="skillLevel">Passive Skill level base on PlayerData.Abilities.PassiveSkillLevel</param>
        private float PassiveSkillControl(int skillLevel)
        {
            float healingValue = 0;       

            switch (skillLevel)
            {
                case 1:
                    healingValue = BaseMaxHP * 0.25f;
                    tempDEF = 0.15f;
                    PassiveActivatedTime = 6f;

                    HP += healingValue;
                    DEF += tempDEF;
                    PassiveActivatedTimer = PassiveActivatedTime;
                    break;

                case 2:
                    healingValue = BaseMaxHP * 0.275f;
                    tempDEF = 0.175f;
                    PassiveActivatedTime = 6f;

                    HP += healingValue;
                    DEF += tempDEF;
                    PassiveActivatedTimer = PassiveActivatedTime;
                    break;

                case 3:
                    healingValue = BaseMaxHP * 0.3f;
                    tempDEF = 0.2f;
                    PassiveActivatedTime = 7f;

                    HP += healingValue;
                    DEF += tempDEF;
                    PassiveActivatedTimer = PassiveActivatedTime;
                    break;

                case 4:
                    healingValue = BaseMaxHP * 0.325f;
                    tempDEF = 0.225f;
                    PassiveActivatedTime = 7f;

                    HP += healingValue;
                    DEF += tempDEF;
                    PassiveActivatedTimer = PassiveActivatedTime;
                    break;

                case 5:
                    healingValue = BaseMaxHP * 0.35f;
                    tempDEF = 0.25f;
                    PassiveActivatedTime = 8f;

                    HP += healingValue;
                    DEF += tempDEF;
                    PassiveActivatedTimer = PassiveActivatedTime;
                    break;

                case 6:
                    healingValue = BaseMaxHP * 0.375f;
                    tempDEF = 0.275f;
                    PassiveActivatedTime = 8f;

                    HP += healingValue;
                    DEF += tempDEF;
                    PassiveActivatedTimer = PassiveActivatedTime;
                    break;

                case 7:
                    healingValue = BaseMaxHP * 0.4f;
                    tempDEF = 0.3f;
                    PassiveActivatedTime = 9f;

                    HP += healingValue;
                    DEF += tempDEF;
                    PassiveActivatedTimer = PassiveActivatedTime;
                    break;

                case 8:
                    healingValue = BaseMaxHP * 0.425f;
                    tempDEF = 0.325f;
                    PassiveActivatedTime = 9f;

                    HP += healingValue;
                    DEF += tempDEF;
                    PassiveActivatedTimer = PassiveActivatedTime;
                    break;

                case 9:
                    healingValue = BaseMaxHP * 0.45f;
                    tempDEF = 0.35f;
                    PassiveActivatedTime = 10f;

                    HP += healingValue;
                    DEF += tempDEF;
                    PassiveActivatedTimer = PassiveActivatedTime;
                    break;

                case 10:
                    healingValue = BaseMaxHP * 0.5f;
                    tempDEF = 0.4f;
                    PassiveActivatedTime = 10f;

                    HP += healingValue;
                    DEF += tempDEF;
                    PassiveActivatedTimer = PassiveActivatedTime;
                    break;
            }

            IsPassiveSkillActivate = true;

            return healingValue;
        }

        // Check Attack
        private void CheckAttackDetection(float atk, float percentHit, bool isUndodgeable, float stunTime, string effectAttacked)
        {
            foreach (var entity in EntityManager.Instance.Entities.Where(e => !e.IsDestroyed && !e.IsDying
                && (e.EntityType == EntityTypes.Hostile || e.EntityType == EntityTypes.Boss)))
            {
                if (entity.BoundingHitBox.Intersects(BoundingDetectEntity))
                {
                    var totalDamage = TotalDamage(atk, percentHit, entity.DEF, entity.Evasion, isUndodgeable);

                    var combatNumVelocity = entity.SetCombatNumDirection();
                    entity.AddCombatLogNumbers(Name, ((int)totalDamage).ToString()
                        , CombatNumCase, combatNumVelocity, effectAttacked);

                    // In case the Attack doesn't Missed
                    if (CombatNumCase != Missed)
                    {
                        // Mob being hit by Player
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

        private float TotalDamage(float ATK, float HitPercent, float DefPercent, float EvasionPercent, bool IsUndodgeable)
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
                CombatNumCase = Missed;
                _isAttackMissed = true;
            }
            else
            {
                // if Attacked
                _isAttackMissed = false;

                // Check crit chance           
                int critChance = random.Next(1, 101);
                if (critChance <= Crit * 100)
                {
                    totalDamage += (int)(totalDamage * CritDMG);
                    CombatNumCase = DamageCrit;
                    _isCriticalAttack = true;
                }
                else
                {
                    CombatNumCase = DamageDefault;
                    _isCriticalAttack = false;
                }

                // Calculate DEF
                totalDamage -= (int)(totalDamage * DefPercent);
            }                     

            return totalDamage;
        }

        private void ManaRegeneration(float deltaSeconds)
        {
            var manaRegenAmount = ManaRegenRate * deltaSeconds;

            Mana += manaRegenAmount;
        }

        private void UpdateLayerDepth(float topDepth, float middleDepth, float bottomDepth)
        {
            // Detect for LayerDepth
            Sprite.Depth = topDepth; // Default depth

            var TopLayerObject = Instance.TopLayerObject;
            foreach (var obj in TopLayerObject)
            {
                if (obj.Intersects(BoundingDetectCollisions))
                {
                    Sprite.Depth = middleDepth;
                    break; // Exit the loop as soon as an intersection is found
                }
            }

            var MiddleLayerObject = Instance.MiddleLayerObject;
            foreach (var obj in MiddleLayerObject)
            {
                if (obj.Intersects(BoundingDetectCollisions))
                {
                    Sprite.Depth = bottomDepth;
                    break;
                }
            }

            var BottomLayerObject = Instance.BottomLayerObject;
            foreach (var obj in BottomLayerObject)
            {
                if (obj.Intersects(BoundingDetectCollisions))
                {
                    Sprite.Depth = bottomDepth + 0.2f;
                    break;
                }
            }
        }

        protected override void MinimumCapacity()
        {
            // Ensure current mana doesn't exceed the maximum mana value and minimum 0
            Mana = Math.Max(0, Math.Min(Mana, MaxMana));

            base.MinimumCapacity();
        }

        protected override void UpdateTimerConditions(float deltaSeconds)
        {
            // Check attack timing for Player Character
            if (ActionTimer > 0)
            {
                ActionTimer -= deltaSeconds;
            }
            else IsAttacking = false;

            // Cooldown time Normal Skill
            if (IsNormalSkillCooldown)
            {
                if (NormalCooldownTimer > 0)
                {
                    NormalCooldownTimer -= deltaSeconds;
                }
                else
                {
                    NormalCooldownTimer = NormalCooldownTime;
                    IsNormalSkillCooldown = false;
                }
            }

            // Activated time Normal Skill
            if (IsNormalSkillActivate)
            {
                if (NormalActivatedTimer > 0)
                {
                    NormalActivatedTimer -= deltaSeconds;
                }
                else
                {
                    ATK -= tempATK;
                    Crit -= tempCrit;
                    CritDMG -= tempCritDMG;
                    NormalActivatedTimer = 0;
                    IsNormalSkillActivate = false;                 
                }
            }

            // Cooldown time Burst Skill
            if (IsBurstSkillCooldown)
            {
                if (BurstCooldownTimer > 0)
                {
                    BurstCooldownTimer -= deltaSeconds;
                }
                else
                {
                    BurstCooldownTimer = BurstCooldownTime;
                    IsBurstSkillCooldown = false;
                }
            }

            // Cooldown time Passive Skill
            if (IsPassiveSkillCooldown)
            {
                if (PassiveCooldownTimer > 0)
                {
                    PassiveCooldownTimer -= deltaSeconds;
                }
                else
                {
                    PassiveCooldownTimer = PassiveCooldownTime;
                    IsPassiveSkillCooldown = false;
                }
            }

            // Activated time Passive Skill
            if (IsPassiveSkillActivate)
            {
                if (PassiveActivatedTimer > 0)
                {
                    PassiveActivatedTimer -= deltaSeconds;
                }
                else
                {
                    DEF -= tempDEF;
                    PassiveActivatedTimer = 0;
                    IsPassiveSkillActivate = false;
                }
            }

            base.UpdateTimerConditions(deltaSeconds);
        }

        public override Vector2 SetCombatNumDirection()
        {
            Vector2 offset = new(Position.X, Position.Y - Sprite.TextureRegion.Height * 1.5f);

            Vector2 numDirection = Position - offset;
            numDirection.Normalize();

            CombatNumVelocity = numDirection * Sprite.TextureRegion.Height / 2;

            return CombatNumVelocity;
        }

        public float GetCurrentEXPPercentage()
        {
            return (float)EXP / EXPMaxCap;
        }
    }
}
