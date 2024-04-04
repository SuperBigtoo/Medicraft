using Medicraft.Data.Models;
using Medicraft.Systems.Managers;
using Medicraft.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Sprites;
using System;

namespace Medicraft.Entities.Mobs
{
    public class HostileMob : Entity
    {
        public EntityData EntityData { get; protected set; }

        protected int goidCoinDrop, expDrop;

        protected int itemDropId, quantityDrop;

        protected HostileMob() { }

        public override void Update(GameTime gameTime, float playerDepth, float topDepth, float middleDepth, float bottomDepth)
        {
            var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (!IsDying)
            {
                UpdateTargetNode(deltaSeconds, EntityData);

                // Setup PathFinding
                SetPathFindingNode((int)EntityData.Position[0], (int)EntityData.Position[1]);

                if (!PlayerManager.Instance.IsPlayerDead)
                {
                    // Combat Control
                    CombatControl(deltaSeconds);

                    // MovementControl
                    MovementControl(deltaSeconds);

                    // Check Aggro Player
                    CheckAggro();
                }

                // Blinking if attacked
                HitBlinking(deltaSeconds);

                // Update layer depth
                UpdateLayerDepth(playerDepth, topDepth, middleDepth, bottomDepth);
            }
            else
            {
                // Dying time before destroy
                CurrentAnimation = SpriteCycle + "_dying";
                Sprite.Play(CurrentAnimation);

                // Check Object Collsion
                CheckCollision();

                isBlinkingPlayed = false;
                blinkingTimer = 0;
                Sprite.Color = Color.White;

                if (DyingTimer < DyingTime)
                {
                    DyingTimer += deltaSeconds;

                    var blinkSpeed = 15f; // Adjust the speed of the blinking effect
                    var alphaMultiplier = MathF.Sin(DyingTimer * blinkSpeed);

                    // Ensure alphaMultiplier is within the valid range [0, 1]
                    alphaMultiplier = MathHelper.Clamp(alphaMultiplier, 0.25f, 2f);

                    Sprite.Color = Color.White * Math.Min(alphaMultiplier, 1f);
                }
                else
                {
                    // Exp & Item Drop
                    InventoryManager.Instance.AddItem(itemDropId, quantityDrop);

                    InventoryManager.Instance.AddGoldCoin(goidCoinDrop);

                    PlayerManager.Instance.AddPlayerEXP(expDrop);

                    Destroy();
                }
            }

            if (PlayerManager.Instance.IsPlayerDead)
            {
                CurrentAnimation = SpriteCycle + "_idle";  // Idle
                Sprite.Play(CurrentAnimation);
            }

            // Update time conditions
            UpdateTimerConditions(deltaSeconds);

            // Ensure hp or mana doesn't exceed the maximum & minimum value
            MinimumCapacity();

            Sprite.Update(deltaSeconds);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Sprite, Transform);

            var shadowTexture = GameGlobals.Instance.GetShadowTexture(GameGlobals.ShadowTextureName.shadow_1);

            DrawShadow(spriteBatch, shadowTexture);

            // Test Draw BoundingRec for Collision
            if (GameGlobals.Instance.IsDebugMode)
            {
                var pixelTexture = new Texture2D(ScreenManager.Instance.GraphicsDevice, 1, 1);
                pixelTexture.SetData(new Color[] { Color.White });
                spriteBatch.Draw(pixelTexture, (Rectangle)BoundingDetectCollisions, Color.Red);
            }
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
