using Medicraft.Data.Models;
using MonoGame.Extended;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Sprites;
using Microsoft.Xna.Framework.Graphics;

namespace Medicraft.Entities.Companion
{
    public class Violet : Companion
    {
        public Violet(AnimatedSprite sprite, CompanionData companionData, Vector2 scale, int indexCompa) : base(scale)
        {
            Sprite = sprite;
            CompanionData = companionData;
            CompanionId = indexCompa;

            // Initialize Character Data
            Id = companionData.CharId;             // Mot to be confuse with CharId
            Level = companionData.Level;
            InitializeCharacterData(companionData.CharId, Level);

            percentNormalHit = 0.5f;
            hitRateNormal = 0.5f;
            hitRateNormalSkill = 0.9f;
            hitRateBurstSkill = 0.9f;

            attackSpeed = 0.25f;
            cooldownAttack = 0.75f;
            cooldownAttackTimer = cooldownAttack;

            AggroTime = 1f;
            DyingTime = 1.3f;
            IsAggroResettable = true;
            IsKnockbackable = true;

            BoundingCollisionX = 16;
            BoundingCollisionY = 2.6f;

            // Rec for check Collision
            BoundingDetectCollisions = new RectangleF(
                (int)((int)Position.X - Sprite.TextureRegion.Width / BoundingCollisionX),
                (int)((int)Position.Y + Sprite.TextureRegion.Height / BoundingCollisionY),
                (int)(Sprite.TextureRegion.Width / 6.5f),
                Sprite.TextureRegion.Height / 8);

            BoundingHitBox = new CircleF(Position, 42f);         // Circle for Entity to hit

            BoundingDetectEntity = new CircleF(Position, 80f);   // Circle for check attacking

            BoundingAggro = new CircleF(Position, 50);         // Circle for check aggro enemy mobs

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
    }
}
