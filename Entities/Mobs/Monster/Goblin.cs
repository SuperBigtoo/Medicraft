using Medicraft.Data.Models;
using Medicraft.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Sprites;

namespace Medicraft.Entities.Mobs.Monster
{
    public class Goblin : HostileMob
    {
        public Goblin(AnimatedSprite sprite, EntityData entityData, Vector2 scale)
        {
            Sprite = sprite;
            EntityData = entityData;

            // Initialize Character Data
            Id = entityData.Id;             // Mot to be confuse with CharId
            Level = entityData.Level;
            InitializeCharacterData(entityData.CharId, Level);

            attackSpeed = 0.25f;
            cooldownAttack = 0.5f;
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

            // Rec for check Collision
            BoundingDetectCollisions = new RectangleF(
                (int)((int)Position.X - Sprite.TextureRegion.Width / BoundingCollisionX),
                (int)((int)Position.Y + Sprite.TextureRegion.Height / BoundingCollisionY),
                (int)(Sprite.TextureRegion.Width / 4f),
                Sprite.TextureRegion.Height / 6);

            BoundingHitBox = new CircleF(Position, 25);         // Circle for Entity to hit
            BoundingDetectEntity = new CircleF(Position, 32);   // Circle for check attacking
            BoundingAggro = new CircleF(Position, 150);         // Circle for check aggro player        

            // Drops
            itemDropId = GameGlobals.Instance.RandomItemDrop();
            quantityDrop = GameGlobals.Instance.RandomItemQuantityDrop(itemDropId);
            goidCoinDrop = 10;
            expDrop = 12;

            // Set Effect
            NormalHitEffectAttacked = "hit_effect_1";

            Sprite.Depth = 0.1f;
            Sprite.Play(SpriteCycle + "_idle");
        }

        private Goblin(Goblin goblin)
        {
            Sprite = goblin.Sprite;
            EntityData = goblin.EntityData;

            EntityType = goblin.EntityType;
            Id = goblin.Id;
            Name = goblin.Name;
            ATK = goblin.ATK;
            MaxHP = goblin.MaxHP;
            HP = goblin.MaxHP;
            DEF = goblin.DEF;
            Speed = goblin.Speed;
            Evasion = goblin.Evasion;

            attackSpeed = goblin.attackSpeed;
            cooldownAttack = goblin.cooldownAttack;
            cooldownAttackTimer = cooldownAttack;
            DyingTime = goblin.DyingTime;

            IsRespawnable = goblin.IsRespawnable;

            AggroTime = goblin.AggroTime;
            IsAggroResettable = goblin.IsAggroResettable;
            IsKnockbackable = goblin.IsKnockbackable;

            PathFindingType = goblin.PathFindingType;
            NodeCycleTime = goblin.NodeCycleTime;

            var position = new Vector2(
                (float)goblin.EntityData.Position[0],
                (float)goblin.EntityData.Position[1]);

            Transform = new Transform2
            {
                Scale = goblin.Transform.Scale,
                Rotation = goblin.Transform.Rotation,
                Position = position,
            };

            BoundingCollisionX = goblin.BoundingCollisionX;
            BoundingCollisionY = goblin.BoundingCollisionY;

            BoundingDetectCollisions = goblin.BoundingDetectCollisions;
            BoundingHitBox = goblin.BoundingHitBox;
            BoundingAggro = goblin.BoundingAggro;
            BoundingDetectEntity = goblin.BoundingDetectEntity;

            itemDropId = GameGlobals.Instance.RandomItemDrop();
            quantityDrop = GameGlobals.Instance.RandomItemQuantityDrop(itemDropId);
            goidCoinDrop = goblin.goidCoinDrop;
            expDrop = goblin.expDrop;

            NormalHitEffectAttacked = goblin.NormalHitEffectAttacked;

            Sprite.Color = Color.White;
            Sprite.Depth = 0.1f;
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
            return new Goblin(this);
        }
    }
}
