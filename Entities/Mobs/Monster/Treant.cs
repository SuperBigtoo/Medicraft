using Medicraft.Data.Models;
using Medicraft.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Sprites;

namespace Medicraft.Entities.Mobs.Monster
{
    public class Treant : HostileMob
    {
        public Treant(AnimatedSprite sprite, EntityData entityData, Vector2 scale)
        {
            Sprite = sprite;
            EntityData = entityData;

            // Initialize Character Data
            Id = entityData.Id;             // Mot to be confuse with CharId
            Level = entityData.Level;
            InitializeCharacterData(entityData.CharId, Level);

            attackSpeed = 0.4f;
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
            goidCoinDrop = 15;
            expDrop = 15;
            InitEXPandGoldByLevel(); // Only call one in here

            // Set Effect
            NormalHitEffectAttacked = "hit_effect_1";

            Sprite.Depth = InitDepth;
            Sprite.Play(SpriteCycle + "_idle");
        }

        private Treant(Treant treant)
        {
            Sprite = treant.Sprite;
            EntityData = treant.EntityData;

            Id = treant.Id;
            Level = treant.Level;
            InitializeCharacterData(treant.EntityData.CharId, Level);

            attackSpeed = treant.attackSpeed;
            cooldownAttack = treant.cooldownAttack;
            cooldownAttackTimer = cooldownAttack;
            DyingTime = treant.DyingTime;

            IsRespawnable = treant.IsRespawnable;

            AggroTime = treant.AggroTime;
            IsAggroResettable = treant.IsAggroResettable;
            IsKnockbackable = treant.IsKnockbackable;

            PathFindingType = treant.PathFindingType;
            NodeCycleTime = treant.NodeCycleTime;

            var position = new Vector2(
                (float)treant.EntityData.Position[0],
                (float)treant.EntityData.Position[1]);

            Transform = new Transform2
            {
                Scale = treant.Transform.Scale,
                Rotation = treant.Transform.Rotation,
                Position = position,
            };

            BoundingCollisionX = treant.BoundingCollisionX;
            BoundingCollisionY = treant.BoundingCollisionY;
            BoundingDetectCollisions = treant.BoundingDetectCollisions;

            BoundingHitBox = treant.BoundingHitBox;
            BoundingHitBox.Position = Position;
            BoundingAggro = treant.BoundingAggro;
            BoundingAggro.Position = Position;
            BoundingDetectEntity = treant.BoundingDetectEntity;
            BoundingDetectEntity.Position = Position;

            itemDropId = GameGlobals.RandomItemDrop();
            quantityDrop = GameGlobals.RandomItemQuantityDrop(itemDropId);
            goidCoinDrop = treant.goidCoinDrop;
            expDrop = treant.expDrop;

            NormalHitEffectAttacked = treant.NormalHitEffectAttacked;

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
            return new Treant(this);
        }
    }
}
