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
    public class Minotaur : HostileMob
    {
        public Minotaur(AnimatedSprite sprite, EntityData entityData, Vector2 scale)
        {
            Sprite = sprite;
            EntityData = entityData;

            // Initialize Character Data
            Id = entityData.Id;             // Mot to be confuse with CharId
            Level = entityData.Level;
            InitializeCharacterData(entityData.CharId, Level);

            attackSpeed = 0.5f;
            cooldownAttack = 1.5f;
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
            BoundingCollisionY = 3;
            BaseBoundingDetectEntityRadius = 60f;
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
            goidCoinDrop = 500;
            expDrop = 500;
            InitEXPandGoldByLevel(); // Only call one in here

            // Set Effect
            NormalHitEffectAttacked = "hit_effect_1";

            Sprite.Depth = InitDepth;
            Sprite.Play(SpriteCycle + "_idle");
        }

        private Minotaur(Minotaur minotaur)
        {
            Sprite = minotaur.Sprite;
            EntityData = minotaur.EntityData;

            Id = minotaur.Id;
            Level = minotaur.Level;
            InitializeCharacterData(minotaur.EntityData.CharId, Level);

            attackSpeed = minotaur.attackSpeed;
            cooldownAttack = minotaur.cooldownAttack;
            cooldownAttackTimer = cooldownAttack;
            knockbackForce = minotaur.knockbackForce;
            DyingTime = minotaur.DyingTime;

            IsRespawnable = minotaur.IsRespawnable;

            AggroTime = minotaur.AggroTime;
            IsAggroResettable = minotaur.IsAggroResettable;
            IsKnockbackable = minotaur.IsKnockbackable;

            PathFindingType = minotaur.PathFindingType;
            NodeCycleTime = minotaur.NodeCycleTime;

            var position = new Vector2(
                (float)minotaur.EntityData.Position[0],
                (float)minotaur.EntityData.Position[1]);

            Transform = new Transform2
            {
                Scale = minotaur.Transform.Scale,
                Rotation = minotaur.Transform.Rotation,
                Position = position,
            };

            BoundingCollisionX = minotaur.BoundingCollisionX;
            BoundingCollisionY = minotaur.BoundingCollisionY;
            BoundingDetectCollisions = minotaur.BoundingDetectCollisions;
            OffSetCircle = minotaur.OffSetCircle;

            BoundingHitBox = minotaur.BoundingHitBox;
            BoundingHitBox.Position = Position;
            BoundingAggro = minotaur.BoundingAggro;
            BoundingAggro.Position = Position;
            BoundingDetectEntity = minotaur.BoundingDetectEntity;
            BoundingDetectEntity.Position = Position;

            itemDropTimes = minotaur.itemDropTimes;
            goidCoinDrop = minotaur.goidCoinDrop;
            expDrop = minotaur.expDrop;

            NormalHitEffectAttacked = minotaur.NormalHitEffectAttacked;

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

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }

        public override void DrawShadow(SpriteBatch spriteBatch, Texture2D shadowTexture)
        {
            var position = new Vector2(
                (Position.X + 10f) - shadowTexture.Width * 2f / 2f,
                BoundingDetectCollisions.Bottom);

            spriteBatch.Draw(shadowTexture, position, null, Color.White
                , 0f, Vector2.Zero, 2f, SpriteEffects.None, Sprite.Depth + 0.0000025f);
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
            return new Minotaur(this);
        }
    }
}
