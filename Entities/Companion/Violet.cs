using Medicraft.Data.Models;
using MonoGame.Extended;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Sprites;
using Microsoft.Xna.Framework.Graphics;
using Medicraft.Systems.Managers;
using System;
using System.Linq;
using static Medicraft.Systems.GameGlobals;

namespace Medicraft.Entities.Companion
{
    public class Violet : Companion
    {
        // Passive
        private CircleF _boundingPassive;
        private readonly float _brillianceAuraArea = 200f;

        public Violet(AnimatedSprite sprite, CompanionData companionData, Vector2 scale, int indexCompa) : base(scale)
        {
            Sprite = sprite;
            CompanionData = companionData;
            CompanionId = indexCompa;

            // Initialize Character Data
            Id = companionData.CharId;  // Mot to be confuse with CharId
            Level = companionData.Level;
            InitializeCharacterData(companionData.CharId, Level);

            percentDamageNormalHit = 0.5f;
            hitRateNormal = 0.5f;
            hitRateNormalSkill = 0.9f;
            hitRateBurstSkill = 0.9f;

            attackSpeed = 0.25f;
            cooldownAttack = 0.75f;
            cooldownAttackTimer = cooldownAttack;

            // Skill
            BaseCooldownNormal = 20f;
            BaseCooldownBurst = 45f;
            InitialCooldownSkill();

            // Passive
            _boundingPassive = new CircleF(Position, _brillianceAuraArea);

            BoundingCollisionX = 16;
            BoundingCollisionY = 2.6f;
            BaseBoundingDetectEntityRadius = 80f;

            // Rec for check Collision
            BoundingDetectCollisions = new RectangleF(
                (int)((int)Position.X - Sprite.TextureRegion.Width / BoundingCollisionX),
                (int)((int)Position.Y + Sprite.TextureRegion.Height / BoundingCollisionY),
                (int)(Sprite.TextureRegion.Width / 6.5f),
                Sprite.TextureRegion.Height / 8);

            BoundingHitBox = new CircleF(Position, 42f);        // Circle for Entity to hit
            BoundingDetectEntity = new CircleF(
                Position, BaseBoundingDetectEntityRadius);      // Circle for check attacking
            BoundingAggro = new CircleF(Position, 60);          // Circle for check aggro enemy mobs

            // Set Effect
            NormalHitEffectAttacked = "hit_effect_9";
            NormalSkillEffectAttacked = "hit_effect_8";
            BurstSkillEffectAttacked = "hit_effect_6";
            BurstSkillEffectActivated = "hit_skill_effect_6";

            Sprite.Depth = InitDepth;
            Sprite.Play(SpriteCycle + "_idle");
        }

        public override void Update(GameTime gameTime, float playerDepth, float topDepth, float middleDepth, float bottomDepth)
        {
            _boundingPassive.Center = Position;

            base.Update(gameTime, playerDepth, topDepth, middleDepth, bottomDepth);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }

        protected override void CombatControl(float deltaSeconds)
        {
            if (IsAggro && EntityManager.Instance.ClosestEnemyToCompanion != null
                && !PlayerManager.Instance.IsRecallCompanion)
            {
                foreach (var entity in EntityManager.Instance.Entities.Where(e => !e.IsDestroyed
                && e.EntityType == EntityTypes.Hostile || e.EntityType == EntityTypes.Boss))
                {
                    if (BoundingDetectEntity.Intersects(entity.BoundingHitBox)
                        && ScreenManager.Instance.IsScreenLoaded && !IsAttacking && !IsStunning)
                    {
                        // Burst Skill
                        if (!IsAttacking && !IsBurstSkillCooldown)
                        {
                            CurrentAnimation = SpriteCycle + "_attacking_burst_skill";
                            Sprite.Play(CurrentAnimation);

                            IsAttacking = true;
                            IsBurstSkillCooldown = true;
                            ActionTimer = hitRateBurstSkill;

                            // Do burst skill & effect of Sets Item
                            BurstSkillControl(CompanionData.Abilities.BurstSkillLevel);

                            CombatNumCase = Buff;
                            var combatNumVelocity = SetCombatNumDirection();
                            AddCombatLogNumbers(Name,
                                "Freeze!",
                                CombatNumCase,
                                combatNumVelocity,
                                BurstSkillEffectActivated);

                            PlaySoundEffect(Sound.FrostNova);
                            break;
                        }

                        // Normal Skill
                        if (!IsAttacking && !IsNormalSkillCooldown)
                        {
                            CurrentAnimation = SpriteCycle + "_attacking_normal_skill";
                            Sprite.Play(CurrentAnimation);

                            IsAttacking = true;
                            IsNormalSkillCooldown = true;
                            ActionTimer = hitRateNormalSkill;

                            // Do normal skill & effect of Sets Item
                            NormalSkillControl(CompanionData.Abilities.NormalSkillLevel);
                            PlaySoundEffect(Sound.frostbolt_1);
                            break;
                        }
                    }
                }
            }

            // Passive Skill
            {
                // For companion
                ManaRegenRate = BaseManaRegenRate + PassiveSkillControl(CompanionData.Abilities.PassiveSkillLevel);

                // For player
                if (_boundingPassive.Intersects(PlayerManager.Instance.Player.BoundingHitBox))
                {
                    if (!IsPassiveSkillActivate)
                    {
                        PlayerManager.Instance.Player.ManaRegenRate += PassiveSkillControl
                            (CompanionData.Abilities.PassiveSkillLevel);

                        IsPassiveSkillActivate = true;
                    }
                }
                else
                {
                    if (IsPassiveSkillActivate)
                    {
                        PlayerManager.Instance.Player.ManaRegenRate -= PassiveSkillControl
                            (CompanionData.Abilities.PassiveSkillLevel);

                        IsPassiveSkillActivate = false;
                    }
                }
            }

            base.CombatControl(deltaSeconds);
        }

        protected override void UpdateDying(GameTime gameTime)
        {
            _boundingPassive.Radius = 0f;

            base.UpdateDying(gameTime);
        }

        public override void Revived()
        {
            _boundingPassive.Radius = _brillianceAuraArea;

            base.Revived();
        }

        protected override void UpdateTimerConditions(float deltaSeconds)
        {
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

            base.UpdateTimerConditions(deltaSeconds);
        }

        /// <summary>
        /// Blasts a target enemy with a wave of damaging frost that slows movement
        /// </summary>
        /// <param name="skillLevel">Normal Skill level base on CompanionData.Abilities.NormalSkillLevel</param>
        private void NormalSkillControl(int skillLevel)
        {
            float slowValue = 0;
            float timeValue = 0;

            switch (skillLevel)
            {
                case 1:
                    slowValue = 0.25f;
                    timeValue = 3f;
                    CheckAttackDetection(ATK, 1f, true, 0f, NormalSkillEffectAttacked);
                    break;

                case 2:
                    slowValue = 0.25f;
                    timeValue = 3f;
                    CheckAttackDetection(ATK, 1.1f, true, 0f, NormalSkillEffectAttacked);
                    break;

                case 3:
                    slowValue = 0.25f;
                    timeValue = 3f;
                    CheckAttackDetection(ATK, 1.2f, true, 0f, NormalSkillEffectAttacked);
                    break;

                case 4:
                    slowValue = 0.35f;
                    timeValue = 4f;
                    CheckAttackDetection(ATK, 1.3f, true, 0f, NormalSkillEffectAttacked);
                    break;

                case 5:
                    slowValue = 0.35f;
                    timeValue = 4f;
                    CheckAttackDetection(ATK, 1.4f, true, 0f, NormalSkillEffectAttacked);
                    break;

                case 6:
                    slowValue = 0.35f;
                    timeValue = 4f;
                    CheckAttackDetection(ATK, 1.5f, true, 0f, NormalSkillEffectAttacked);
                    break;

                case 7:
                    slowValue = 0.45f;
                    timeValue = 5f;
                    CheckAttackDetection(ATK, 1.6f, true, 0f, NormalSkillEffectAttacked);
                    break;

                case 8:
                    slowValue = 0.45f;
                    timeValue = 5f;
                    CheckAttackDetection(ATK, 1.7f, true, 0f, NormalSkillEffectAttacked);
                    break;

                case 9:
                    slowValue = 0.45f;
                    timeValue = 5f;
                    CheckAttackDetection(ATK, 1.8f, true, 0f, NormalSkillEffectAttacked);
                    break;

                case 10:
                    slowValue = 0.6f;
                    timeValue = 6f;
                    CheckAttackDetection(ATK, 2f, true, 0f, NormalSkillEffectAttacked);
                    break;
            }

            // Do effect to enemy target
            StatusEffectManager.Instance.AddStatusEffect(
                EntityManager.Instance.ClosestEnemyToCompanion,
                Name,
                "Frost Bolt",
                new Data.Models.Effect()
                {
                    Target = "Speed",
                    ActivationType = "Instant",
                    EffectType = "Debuff",
                    Value = slowValue,
                    Time = timeValue,
                    Timer = timeValue,
                    IsActive = false,
                });
        }

        /// <summary>
        /// Deals a moderate damage in a wide area surrounding the Character and stun mobs for an amount of time
        /// </summary>
        /// <param name="skillLevel">Burst Skill level base on CompanionData.Abilities.BurstSkillLevel</param>
        private void BurstSkillControl(int skillLevel)
        {
            switch (skillLevel)
            {
                case 1:
                    BoundingDetectEntity.Radius = 140f;
                    knockbackForce = 1f;
                    CheckAttackDetection(ATK, 0.6f, true, 3.25f, BurstSkillEffectAttacked);
                    break;

                case 2:
                    BoundingDetectEntity.Radius = 140f;
                    knockbackForce = 1f;
                    CheckAttackDetection(ATK, 0.7f, true, 3.5f, BurstSkillEffectAttacked);
                    break;

                case 3:
                    BoundingDetectEntity.Radius = 140f;
                    knockbackForce = 1f;
                    CheckAttackDetection(ATK, 0.8f, true, 3.75f, BurstSkillEffectAttacked);
                    break;

                case 4:
                    BoundingDetectEntity.Radius = 140f;
                    knockbackForce = 1f;
                    CheckAttackDetection(ATK, 0.9f, true, 4f, BurstSkillEffectAttacked);
                    break;

                case 5:
                    BoundingDetectEntity.Radius = 140f;
                    knockbackForce = 1f;
                    CheckAttackDetection(ATK, 1f, true, 4.25f, BurstSkillEffectAttacked);
                    break;

                case 6:
                    BoundingDetectEntity.Radius = 140f;
                    knockbackForce = 1f;
                    CheckAttackDetection(ATK, 1.1f, true, 4.5f, BurstSkillEffectAttacked);
                    break;

                case 7:
                    BoundingDetectEntity.Radius = 140f;
                    knockbackForce = 1f;
                    CheckAttackDetection(ATK, 1.2f, true, 4.75f, BurstSkillEffectAttacked);
                    break;

                case 8:
                    BoundingDetectEntity.Radius = 140f;
                    knockbackForce = 1f;
                    CheckAttackDetection(ATK, 1.3f, true, 5f, BurstSkillEffectAttacked);
                    break;

                case 9:
                    BoundingDetectEntity.Radius = 140f;
                    knockbackForce = 1f;
                    CheckAttackDetection(ATK, 1.4f, true, 5.25f, BurstSkillEffectAttacked);
                    break;

                case 10:
                    BoundingDetectEntity.Radius = 140f;
                    knockbackForce = 1f;
                    CheckAttackDetection(ATK, 1.5f, true, 5.5f, BurstSkillEffectAttacked);
                    break;
            }

            BoundingDetectEntity.Radius = BaseBoundingDetectEntityRadius;
            knockbackForce = baseknockbackForce;
        }

        /// <summary>
        /// Gives additional mana regeneration to nearby friendly units.
        /// </summary>
        /// <param name="skillLevel">Passive Skill level base on CompanionData.Abilities.PassiveSkillLevel</param>
        /// <returns></returns>
        private float PassiveSkillControl(int skillLevel)
        {
            float manaRegenValue = 0;

            switch (skillLevel)
            {
                case 1:
                    manaRegenValue = 0.3f;
                    break;

                case 2:
                    manaRegenValue = 0.35f;
                    break;

                case 3:
                    manaRegenValue = 0.40f;
                    break;

                case 4:
                    manaRegenValue = 0.45f;
                    break;

                case 5:
                    manaRegenValue = 0.50f;
                    break;

                case 6:
                    manaRegenValue = 0.55f;
                    break;

                case 7:
                    manaRegenValue = 0.60f;
                    break;

                case 8:
                    manaRegenValue = 0.65f;
                    break;

                case 9:
                    manaRegenValue = 0.70f;
                    break;

                case 10:
                    manaRegenValue = 0.75f;
                    break;
            }

            return (float)Math.Round(manaRegenValue, 2);
        }
    }
}
