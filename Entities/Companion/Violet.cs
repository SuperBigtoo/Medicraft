using Medicraft.Data.Models;
using Medicraft.Systems.PathFinding;
using MonoGame.Extended;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Sprites;
using Microsoft.Xna.Framework.Graphics;

namespace Medicraft.Entities.Companion
{
    public class Violet : Companion
    {
        public Violet(AnimatedSprite sprite, EntityData entityData, Vector2 scale)
        {
            Sprite = sprite;

            // Initialize Character Data
            Id = entityData.Id;             // Mot to be confuse with CharId
            Level = entityData.Level;
            InitializeCharacterData(entityData.CharId, Level);

            knockbackForce = 50f;
            percentNormalHit = 0.5f;
            hitRateNormal = 0.5f;
            hitRateNormalSkill = 0.9f;
            hitRateBurstSkill = 0.9f;

            attackSpeed = 0.25f;
            cooldownAttack = 0.5f;
            cooldownAttackTimer = cooldownAttack;

            DyingTime = 1.3f;
            IsAggroResettable = true;
            IsKnockbackable = true;

            var position = new Vector2((float)entityData.Position[0], (float)entityData.Position[1]);
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
                Sprite.TextureRegion.Height / 6
            );

            BoundingHitBox = new CircleF(Position, 42f);         // Circle for Entity to hit

            BoundingDetectEntity = new CircleF(Position, 80f);   // Circle for check attacking

            BoundingAggro = new CircleF(Position, 1);         // Circle for check aggro enemy mobs        

            _pathFinding = new AStar(
                (int)BoundingDetectCollisions.Center.X,
                (int)BoundingDetectCollisions.Center.Y,
                (int)entityData.Position[0],
                (int)entityData.Position[1]
            );

            // Set Effect
            NormalHitEffectAttacked = "hit_effect_9";
            NormalSkillEffectAttacked = "hit_effect_8";
            BurstSkillEffectAttacked = "hit_effect_6";

            Sprite.Depth = 0.1f;
            Sprite.Play(SpriteCycle + "_idle");
        }

        public override void Update(GameTime gameTime, float playerDepth, float topDepth, float middleDepth, float bottomDepth)
        {
            base.Update(gameTime, playerDepth, topDepth, middleDepth, bottomDepth);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }

        public override void DrawShadow(SpriteBatch spriteBatch, Texture2D shadowTexture)
        {
            var position = new Vector2(Position.X - shadowTexture.Width * 1.2f / 2f
                    , BoundingDetectCollisions.Bottom - Sprite.TextureRegion.Height * 1.2f / 10);

            spriteBatch.Draw(shadowTexture, position, null, Color.White
                , 0f, Vector2.Zero, 1.2f, SpriteEffects.None, Sprite.Depth + 0.0000025f);
        }

        public override Vector2 SetCombatNumDirection()
        {
            Vector2 offset = new(Position.X, Position.Y - Sprite.TextureRegion.Height * 1.5f);

            Vector2 numDirection = Position - offset;
            numDirection.Normalize();

            CombatNumVelocity = numDirection * Sprite.TextureRegion.Height;

            return CombatNumVelocity;
        }
    }
}
