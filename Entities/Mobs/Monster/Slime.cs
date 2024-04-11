using Medicraft.Data.Models;
using Medicraft.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Sprites;
using System;

namespace Medicraft.Entities.Mobs.Monster
{
    public class Slime : HostileMob
    {
        public enum SlimeColor
        {
            yellow,
            red,
            green,
            blue
        }

        public Slime(AnimatedSprite sprite, EntityData entityData, Vector2 scale)
        {
            Sprite = sprite;
            EntityData = entityData;

            // Initialize Character Data
            Id = entityData.Id;             // Mot to be confuse with CharId
            Level = entityData.Level;
            InitializeCharacterData(entityData.CharId, Level);

            attackSpeed = 0.25f;
            cooldownAttack = 0.7f;
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

            BoundingCollisionX = 4;
            BoundingCollisionY = 12;
            BaseBoundingDetectEntityRadius = 40f;

            // Rec for check Collision
            BoundingDetectCollisions = new RectangleF(
                (int)((int)Position.X - Sprite.TextureRegion.Width / BoundingCollisionX),
                (int)((int)Position.Y + Sprite.TextureRegion.Height / BoundingCollisionY),
                (int)(Sprite.TextureRegion.Width / 2.5f),
                Sprite.TextureRegion.Height / 6);

            BoundingHitBox = new CircleF(Position, 20);         // Circle for Entity to hit
            BoundingDetectEntity = new CircleF(
                Position, BaseBoundingDetectEntityRadius);      // Circle for check attacking
            BoundingAggro = new CircleF(Position, 150);         // Circle for check aggro player        

            RandomSlimeColor();

            itemDropId = GameGlobals.RandomItemDrop();
            quantityDrop = GameGlobals.RandomItemQuantityDrop(itemDropId);
            goidCoinDrop = 5;
            expDrop = 10;
            InitEXPandGoldByLevel(); // Only call one in here

            // Set Effect
            NormalHitEffectAttacked = "hit_effect_1";

            Sprite.Depth = InitDepth;
            Sprite.Play(SpriteCycle + "_walking");
        }

        private Slime(Slime slime)
        {
            Sprite = slime.Sprite;
            EntityData = slime.EntityData;

            Id = slime.Id;
            Level = slime.Level;
            InitializeCharacterData(slime.EntityData.CharId, Level);

            attackSpeed = slime.attackSpeed;
            cooldownAttack = slime.cooldownAttack;
            cooldownAttackTimer = cooldownAttack;
            DyingTime = slime.DyingTime;

            IsRespawnable = slime.IsRespawnable;

            AggroTime = slime.AggroTime;
            IsAggroResettable = slime.IsAggroResettable;
            IsKnockbackable = slime.IsKnockbackable;

            PathFindingType = slime.PathFindingType;
            NodeCycleTime = slime.NodeCycleTime;

            var position = new Vector2(
                (float)slime.EntityData.Position[0],
                (float)slime.EntityData.Position[1]);

            Transform = new Transform2
            {
                Scale = slime.Transform.Scale,
                Rotation = slime.Transform.Rotation,
                Position = position,
            };

            BoundingCollisionX = slime.BoundingCollisionX;
            BoundingCollisionY = slime.BoundingCollisionY;

            BoundingDetectCollisions = slime.BoundingDetectCollisions;
            BoundingDetectCollisions.Position = Position;
            BoundingHitBox = slime.BoundingHitBox;
            BoundingHitBox.Position = Position;
            BoundingAggro = slime.BoundingAggro;
            BoundingAggro.Position = Position;
            BoundingDetectEntity = slime.BoundingDetectEntity;
            BoundingDetectCollisions.Position = Position;

            RandomSlimeColor();

            itemDropId = GameGlobals.RandomItemDrop();
            quantityDrop = GameGlobals.RandomItemQuantityDrop(itemDropId);
            goidCoinDrop = slime.goidCoinDrop;
            expDrop = slime.expDrop;

            NormalHitEffectAttacked = slime.NormalHitEffectAttacked;

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

        private void RandomSlimeColor()
        {
            var random = new Random();
            Array slimeColors = Enum.GetValues(typeof(SlimeColor));
            int randomIndex = random.Next(slimeColors.Length);
            SlimeColor randomColor = (SlimeColor)slimeColors.GetValue(randomIndex);

            switch (randomColor)
            {
                case SlimeColor.yellow:
                    SpriteCycle = "yellow";
                    break;

                case SlimeColor.red:
                    SpriteCycle = "red";
                    break;

                case SlimeColor.green:
                    SpriteCycle = "green";
                    break;

                case SlimeColor.blue:
                    SpriteCycle = "blue";
                    break;
            }
        }

        public override object Clone()
        {
            return new Slime(this);
        }
    }
}