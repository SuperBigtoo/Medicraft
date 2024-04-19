using Medicraft.Data.Models;
using Medicraft.Systems;
using Medicraft.Systems.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Sprites;
using System;
using System.Linq;

namespace Medicraft.Entities.Mobs.Boss
{
    public class TheKeeper : HostileMob
    {
        public bool IsSkillCooldown { get; protected set; } = false;
        public bool IsSkillActivated { get; protected set; } = false;
        public float SkillCooldownTime { get; protected set; } = 20f;
        public float SkillCooldownTimer { get; protected set; }

        private float _skillTimer = 0f, _baseSkillHit = 0.9f;
        private int _hitFrameIndex = 0;
        private float[] _hitFrame = [ 0.52f, 0.39f ];

        public TheKeeper(AnimatedSprite sprite, EntityData entityData, Vector2 scale)
        {
            Sprite = sprite;
            EntityData = entityData;

            // Initialize Character Data
            Id = entityData.Id;             // Mot to be confuse with CharId
            Level = entityData.Level;
            InitializeCharacterData(entityData.CharId, Level);

            attackSpeed = 0.80f;
            cooldownAttack = 1f;
            cooldownAttackTimer = cooldownAttack;
            knockbackForce = 60f;
            DyingTime = 1.5f;

            IsRespawnable = true;
            IsKnockbackable = false;

            AggroTime = entityData.AggroTime;
            IsAggroResettable = true;

            SetPathFindingType(entityData.PathFindingType);
            NodeCycleTime = entityData.NodeCycleTime;

            var position = new Vector2(
                (float)entityData.Position[0],
                (float)entityData.Position[1]);

            Transform = new Transform2
            {
                Scale = scale,
                Rotation = 0f,
                Position = position
            };

            BoundingCollisionX = 10;
            BoundingCollisionY = 2.8f;
            BaseBoundingDetectEntityRadius = 80f;
            OffSetCircle = new Vector2(0, 100);

            // Rec for check Collision
            BoundingDetectCollisions = new RectangleF(
                (int)((int)Position.X - Sprite.TextureRegion.Width / BoundingCollisionX),
                (int)((int)Position.Y + Sprite.TextureRegion.Height / BoundingCollisionY),
                (int)(Sprite.TextureRegion.Width / 5f),
                Sprite.TextureRegion.Height / 10);

            BoundingHitBox = new CircleF(Position + OffSetCircle, 70);         // Circle for Entity to hit
            BoundingDetectEntity = new CircleF(
                Position + OffSetCircle, BaseBoundingDetectEntityRadius);      // Circle for check attacking
            BoundingAggro = new CircleF(Position + OffSetCircle, 170);         // Circle for check aggro player        

            // Drops
            itemDropTimes = new Random().Next(4, 11);
            goidCoinDrop = 750;
            expDrop = 750;
            InitEXPandGoldByLevel(); // Only call one in here

            // Set Effect
            NormalHitEffectAttacked = "hit_effect_1";

            Sprite.Depth = InitDepth;
            Sprite.Play(SpriteCycle + "_idle");
        }

        private TheKeeper(TheKeeper theKeeper)
        {
            Sprite = theKeeper.Sprite;
            EntityData = theKeeper.EntityData;

            Id = theKeeper.Id;
            Level = theKeeper.Level;
            InitializeCharacterData(theKeeper.EntityData.CharId, Level);

            attackSpeed = theKeeper.attackSpeed;
            cooldownAttack = theKeeper.cooldownAttack;
            cooldownAttackTimer = cooldownAttack;
            knockbackForce = theKeeper.knockbackForce;
            DyingTime = theKeeper.DyingTime;

            IsRespawnable = theKeeper.IsRespawnable;

            AggroTime = theKeeper.AggroTime;
            IsAggroResettable = theKeeper.IsAggroResettable;
            IsKnockbackable = theKeeper.IsKnockbackable;

            PathFindingType = theKeeper.PathFindingType;
            NodeCycleTime = theKeeper.NodeCycleTime;

            var position = new Vector2(
                (float)theKeeper.EntityData.Position[0],
                (float)theKeeper.EntityData.Position[1]);

            Transform = new Transform2
            {
                Scale = theKeeper.Transform.Scale,
                Rotation = theKeeper.Transform.Rotation,
                Position = position,
            };

            BoundingCollisionX = theKeeper.BoundingCollisionX;
            BoundingCollisionY = theKeeper.BoundingCollisionY;
            BoundingDetectCollisions = theKeeper.BoundingDetectCollisions;
            OffSetCircle = theKeeper.OffSetCircle;

            BoundingHitBox = theKeeper.BoundingHitBox;
            BoundingHitBox.Position = Position;
            BoundingAggro = theKeeper.BoundingAggro;
            BoundingAggro.Position = Position;
            BoundingDetectEntity = theKeeper.BoundingDetectEntity;
            BoundingDetectEntity.Position = Position;

            itemDropTimes = theKeeper.itemDropTimes;
            goidCoinDrop = theKeeper.goidCoinDrop;
            expDrop = theKeeper.expDrop;

            NormalHitEffectAttacked = theKeeper.NormalHitEffectAttacked;

            Sprite.Color = Color.White;
            Sprite.Depth = InitDepth;
            Sprite.Play(SpriteCycle + "_walking");
        }

        public override void Update(GameTime gameTime, float playerDepth, float topDepth, float middleDepth, float bottomDepth)
        {
            base.Update(gameTime, playerDepth, topDepth, middleDepth, bottomDepth);

            if (IsDying)
            {
                IsAggro = false;
                GameGlobals.Instance.IsEnteringBossFight = false;
                GameGlobals.Instance.SpawnerDatas.FirstOrDefault
                    (s => s.ChapterId.Equals(EntityData.BossChapterId)).IsBossDead = true;
            }

            if (IsAggro)
            {
                GameGlobals.Instance.IsEnteringBossFight = true;
                EntityManager.Instance.Boss = this;
            }
            else
            {
                GameGlobals.Instance.IsEnteringBossFight = false;
                EntityManager.Instance.Boss = null;
                HP = BaseMaxHP;
                Mana = BaseMaxMana;
            }
        }

        protected override void CombatControl(float deltaSeconds)
        {
            // Check Dectected
            var isPlayerDetected = BoundingDetectEntity.Intersects(PlayerManager.Instance.Player.BoundingHitBox);

            var isCompanionDetected = false;
            if (PlayerManager.Instance.Companions.Count != 0
                && !PlayerManager.Instance.IsCompanionDead
                && PlayerManager.Instance.IsCompanionSummoned)
            {
                var companion = PlayerManager.Instance.Companions[PlayerManager.Instance.CurrCompaIndex];

                if (companion != null && !companion.IsDying)
                    isCompanionDetected = BoundingDetectEntity.Intersects(companion.BoundingHitBox);
            }

            // Do Skill Attack
            if ((isPlayerDetected || isCompanionDetected)
                && ScreenManager.Instance.IsScreenLoaded && !UIManager.Instance.IsShowDialogUI
                && !IsAttacking && !IsStunning && !IsSkillCooldown)
            {
                CurrentAnimation = SpriteCycle + "_skill";
                Sprite.Play(CurrentAnimation);

                IsAttacking = true;
                IsSkillCooldown = true;
                IsSkillActivated = true;
                SkillCooldownTimer = SkillCooldownTime;

                BoundingDetectEntity.Radius = 120f;

                var spriteSheet = GameGlobals.Instance.EntitySpriteSheets.FirstOrDefault
                    (e => e.Key.Equals(CharId)).Value;
                var cycleEffect = spriteSheet.Cycles.FirstOrDefault(c => c.Key.Equals("default_skill"));
                ActionTimer = (cycleEffect.Value.FrameDuration * cycleEffect.Value.Frames.Capacity);

                _hitFrameIndex = 0;
                _skillTimer = _hitFrame[_hitFrameIndex];
            }

            if (IsAttacking && IsSkillActivated)
            {
                if (ActionTimer > 0)
                {
                    ActionTimer -= deltaSeconds;
                }
                else
                {
                    IsAttacking = false;
                    IsSkillActivated = false;
                    BoundingDetectEntity.Radius = BaseBoundingDetectEntityRadius;
                }

                // do attack
                {
                    if (_skillTimer > 0)
                    {
                        _skillTimer -= deltaSeconds;
                    }
                    else if (_hitFrameIndex < _hitFrame.Length)
                    {
                        CheckAttackDetection(_baseSkillHit);
                        PlayMobAttackSound();

                        _hitFrameIndex++;

                        if (_hitFrameIndex < _hitFrame.Length)
                        {
                            _skillTimer = _hitFrame[_hitFrameIndex];
                        }
                        else _skillTimer = 10f;
                    }
                }
            }

            if (IsSkillCooldown)
            {
                SkillCooldownTimer -= deltaSeconds;

                if (SkillCooldownTimer < 0)
                {
                    IsSkillCooldown = false;
                }
            }

            if (IsSkillActivated) return;

            base.CombatControl(deltaSeconds);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }

        public override void DrawShadow(SpriteBatch spriteBatch, Texture2D shadowTexture)
        {
            var position = new Vector2(
                Position.X - (shadowTexture.Width * 3.2f / 2f),
                BoundingDetectCollisions.Center.Y - 20f);

            spriteBatch.Draw(shadowTexture, position, null, Color.White
                , 0f, Vector2.Zero, 3.2f, SpriteEffects.None, Sprite.Depth + 0.0000025f);
        }

        public override Vector2 SetCombatNumDirection()
        {
            Vector2 offset = new(Position.X, Position.Y - Sprite.TextureRegion.Height / 4);

            Vector2 numDirection = Position - offset;
            numDirection.Normalize();

            CombatNumVelocity = numDirection * Sprite.TextureRegion.Height / 4;

            return CombatNumVelocity;
        }

        public override object Clone()
        {
            return new TheKeeper(this);
        }
    }
}
