using Medicraft.Data.Models;
using Medicraft.Systems;
using Medicraft.Systems.Managers;
using Medicraft.Systems.PathFinding;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Sprites;
using System;

namespace Medicraft.Entities.Mobs.Monster
{
    public class Slime : Entity
    {
        public EntityData EntityData { get; private set; }
        public enum SlimeColor
        {
            yellow,
            red,
            green,
            blue
        }

        private const int _goidCoinDrop = 10, _expDrop = 10;

        private readonly int _itemDropId, _quantityDrop;

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

            var position = new Vector2((float)entityData.Position[0], (float)entityData.Position[1]);
            Transform = new Transform2
            {
                Scale = scale,
                Rotation = 0f,
                Position = position
            };

            BoundingCollisionX = 4;
            BoundingCollisionY = 12;

            // Rec for check Collision
            BoundingDetectCollisions = new RectangleF(
                (int)((int)Position.X - Sprite.TextureRegion.Width / BoundingCollisionX),
                (int)((int)Position.Y + Sprite.TextureRegion.Height / BoundingCollisionY),
                (int)(Sprite.TextureRegion.Width / 2.5f),
                Sprite.TextureRegion.Height / 6);

            BoundingHitBox = new CircleF(Position, 20);         // Circle for Entity to hit

            BoundingDetectEntity = new CircleF(Position, 30);   // Circle for check attacking

            BoundingAggro = new CircleF(Position, 150);         // Circle for check aggro player        

            RandomSlimeColor();

            _itemDropId = GameGlobals.Instance.RandomItemDrop();
            _quantityDrop = GameGlobals.Instance.RandomItemQuantityDrop(_itemDropId);

            // Set Effect
            NormalHitEffectAttacked = "hit_effect_1";

            Sprite.Depth = 0.1f;
            Sprite.Play(SpriteCycle + "_walking");
        }

        private Slime(Slime slime)
        {
            Sprite = slime.Sprite;
            EntityData = slime.EntityData;

            EntityType = slime.EntityType;
            Id = slime.Id;
            Name = slime.Name;
            ATK = slime.ATK;
            MaxHP = slime.MaxHP;
            HP = slime.MaxHP;
            DEF = slime.DEF;
            Speed = slime.Speed;
            Evasion = slime.Evasion;

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

            Transform = new Transform2
            {
                Scale = slime.Transform.Scale,
                Rotation = slime.Transform.Rotation,
                Position = slime.Transform.Position,
            };

            BoundingCollisionX = slime.BoundingCollisionX;
            BoundingCollisionY = slime.BoundingCollisionY;

            BoundingDetectCollisions = slime.BoundingDetectCollisions;

            BoundingHitBox = slime.BoundingHitBox;

            BoundingAggro = slime.BoundingAggro;

            BoundingDetectEntity = slime.BoundingDetectEntity;

            RandomSlimeColor();

            _itemDropId = GameGlobals.Instance.RandomItemDrop();
            _quantityDrop = GameGlobals.Instance.RandomItemQuantityDrop(_itemDropId);

            NormalHitEffectAttacked = slime.NormalHitEffectAttacked;

            Sprite.Color = Color.White;
            Sprite.Depth = 0.1f;
            Sprite.Play(SpriteCycle + "_walking");
        }

        // Update Slime
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
                    InventoryManager.Instance.AddItem(_itemDropId, _quantityDrop);

                    InventoryManager.Instance.AddGoldCoin(_goidCoinDrop);

                    PlayerManager.Instance.AddPlayerEXP(_expDrop);

                    Destroy();
                }
            }

            if (PlayerManager.Instance.IsPlayerDead)
            {
                CurrentAnimation = SpriteCycle + "_walking";  // Idle
                Sprite.Play(CurrentAnimation);
            }

            // Update time conditions
            UpdateTimerConditions(deltaSeconds);

            // Ensure hp or mana doesn't exceed the maximum & minimum value
            MinimumCapacity();

            Sprite.Update(deltaSeconds);
        }

        // Draw Slime
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (GameGlobals.Instance.IsShowPath)
            {
                //pathFinding.Draw(spriteBatch);
            }

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