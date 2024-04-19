using Medicraft.Data.Models;
using Medicraft.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Sprites;

namespace Medicraft.Entities.Mobs.Monster
{
    public class IceGolem : HostileMob
    {
        public IceGolem(AnimatedSprite sprite, EntityData entityData, Vector2 scale)
        {
            Sprite = sprite;
            EntityData = entityData;

            // Initialize Character Data
            Id = entityData.Id;             // Mot to be confuse with CharId
            Level = entityData.Level;
            InitializeCharacterData(entityData.CharId, Level);

            attackSpeed = 0.6f;
            cooldownAttack = 0.6f;
            cooldownAttackTimer = cooldownAttack;
            DyingTime = 1.3f;

            IsRespawnable = true;

            AggroTime = entityData.AggroTime;
            IsAggroResettable = true;
            IsKnockbackable = true;

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

            BoundingCollisionX = 9;
            BoundingCollisionY = 4;
            BaseBoundingDetectEntityRadius = 40f;

            // Rec for check Collision
            BoundingDetectCollisions = new RectangleF(
                (int)((int)Position.X - Sprite.TextureRegion.Width / BoundingCollisionX),
                (int)((int)Position.Y + Sprite.TextureRegion.Height / BoundingCollisionY),
                (int)(Sprite.TextureRegion.Width / 4f),
                Sprite.TextureRegion.Height / 6);

            BoundingHitBox = new CircleF(Position, 25);         // Circle for Entity to hit
            BoundingDetectEntity = new CircleF(
                Position, BaseBoundingDetectEntityRadius);      // Circle for check attacking
            BoundingAggro = new CircleF(Position, 170);         // Circle for check aggro player        

            // Drops
            itemDropId = GameGlobals.RandomItemDrop();
            quantityDrop = GameGlobals.RandomItemQuantityDrop(itemDropId);
            goidCoinDrop = 20;
            expDrop = 20;
            InitEXPandGoldByLevel(); // Only call one in here

            // Set Effect
            NormalHitEffectAttacked = "hit_effect_1";

            Sprite.Depth = InitDepth;
            Sprite.Play(SpriteCycle + "_idle");
        }

        private IceGolem(IceGolem iceGolem)
        {
            Sprite = iceGolem.Sprite;
            EntityData = iceGolem.EntityData;

            Id = iceGolem.Id;
            Level = iceGolem.Level;
            InitializeCharacterData(iceGolem.EntityData.CharId, Level);

            attackSpeed = iceGolem.attackSpeed;
            cooldownAttack = iceGolem.cooldownAttack;
            cooldownAttackTimer = cooldownAttack;
            DyingTime = iceGolem.DyingTime;

            IsRespawnable = iceGolem.IsRespawnable;

            AggroTime = iceGolem.AggroTime;
            IsAggroResettable = iceGolem.IsAggroResettable;
            IsKnockbackable = iceGolem.IsKnockbackable;

            PathFindingType = iceGolem.PathFindingType;
            NodeCycleTime = iceGolem.NodeCycleTime;

            var position = new Vector2(
                (float)iceGolem.EntityData.Position[0],
                (float)iceGolem.EntityData.Position[1]);

            Transform = new Transform2
            {
                Scale = iceGolem.Transform.Scale,
                Rotation = iceGolem.Transform.Rotation,
                Position = position,
            };

            BoundingCollisionX = iceGolem.BoundingCollisionX;
            BoundingCollisionY = iceGolem.BoundingCollisionY;
            BoundingDetectCollisions = iceGolem.BoundingDetectCollisions;

            BoundingHitBox = iceGolem.BoundingHitBox;
            BoundingHitBox.Position = Position;
            BoundingAggro = iceGolem.BoundingAggro;
            BoundingAggro.Position = Position;
            BoundingDetectEntity = iceGolem.BoundingDetectEntity;
            BoundingDetectEntity.Position = Position;

            itemDropId = GameGlobals.RandomItemDrop();
            quantityDrop = GameGlobals.RandomItemQuantityDrop(itemDropId);
            goidCoinDrop = iceGolem.goidCoinDrop;
            expDrop = iceGolem.expDrop;

            NormalHitEffectAttacked = iceGolem.NormalHitEffectAttacked;

            Sprite.Color = Color.White;
            Sprite.Depth = InitDepth;
            Sprite.Play(SpriteCycle + "_walking");
        }

        public override void Update(GameTime gameTime, float playerDepth, float topDepth, float middleDepth, float bottomDepth)
        {
            base.Update(gameTime, playerDepth, topDepth, middleDepth, bottomDepth);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }

        public override object Clone()
        {
            return new IceGolem(this);
        }
    }
}
