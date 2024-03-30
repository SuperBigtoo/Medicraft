using Medicraft.Data.Models;
using Medicraft.Systems.Managers;
using Medicraft.Systems.PathFinding;
using Medicraft.Systems;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using System;
using MonoGame.Extended.Sprites;

namespace Medicraft.Entities.Mobs.Monster
{
    public class Goblin : Entity
    {
        public EntityData EntityData { get; private set; }

        private const int _goidCoinDrop = 10, _expDrop = 12;

        private readonly int _itemDropId, _quantityDrop;

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

            BoundingHitBox = new CircleF(Position, 25);         // Circle for Entity to hit

            BoundingDetectEntity = new CircleF(Position, 32);   // Circle for check attacking

            BoundingAggro = new CircleF(Position, 150);         // Circle for check aggro player        

            pathFinding = new AStar(
                (int)BoundingDetectCollisions.Center.X,
                (int)BoundingDetectCollisions.Center.Y,
                (int)EntityData.Position[0],
                (int)EntityData.Position[1]);

            _itemDropId = GameGlobals.Instance.RandomItemDrop();
            _quantityDrop = GameGlobals.Instance.RandomItemQuantityDrop(_itemDropId);

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

            Transform = new Transform2
            {
                Scale = goblin.Transform.Scale,
                Rotation = goblin.Transform.Rotation,
                Position = goblin.Transform.Position,
            };

            BoundingCollisionX = goblin.BoundingCollisionX;
            BoundingCollisionY = goblin.BoundingCollisionY;

            BoundingDetectCollisions = goblin.BoundingDetectCollisions;
            BoundingHitBox = goblin.BoundingHitBox;
            BoundingAggro = goblin.BoundingAggro;
            BoundingDetectEntity = goblin.BoundingDetectEntity;

            pathFinding = new AStar(
                (int)BoundingDetectCollisions.Center.X,
                (int)BoundingDetectCollisions.Center.Y,
                (int)EntityData.Position[0],
                (int)EntityData.Position[1]);

            _itemDropId = GameGlobals.Instance.RandomItemDrop();
            _quantityDrop = GameGlobals.Instance.RandomItemQuantityDrop(_itemDropId);

            NormalHitEffectAttacked = goblin.NormalHitEffectAttacked;

            Sprite.Color = Color.White;
            Sprite.Depth = 0.1f;
            Sprite.Play(SpriteCycle + "_walking");
        }

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
                CurrentAnimation = SpriteCycle + "_idle";  // Idle
                Sprite.Play(CurrentAnimation);
            }

            // Update time conditions
            UpdateTimerConditions(deltaSeconds);

            // Blinking if attacked
            HitBlinking(deltaSeconds);

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

        public override object Clone()
        {
            return new Goblin(this);
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
