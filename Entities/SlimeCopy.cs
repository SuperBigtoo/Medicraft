using Medicraft.Data.Models;
using Medicraft.Systems;
using Medicraft.Systems.Managers;
using Medicraft.Systems.PathFinding;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Sprites;
using System;

namespace Medicraft.Entities
{
    public class SlimeCopy : Entity
    {
        public EntityData EntityData { get; private set; }
        private enum SlimeColor
        {
            yellow,
            red,
            green,
            blue
        }

        public SlimeCopy(AnimatedSprite sprite, EntityData entityData, Vector2 scale)
        {
            Sprite = sprite;
            EntityData = entityData;

            // Initialize Character Data
            Id = entityData.Id;
            Level = entityData.Level;
            InitializeCharacterData(entityData.CharId, Level);

            AttackSpeed = 0.4f;
            CooldownAttack = 0.7f;
            CooldownAttackTimer = CooldownAttack;
            DyingTime = 1.3f;

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

            BoundingCollisionX = 5.5;
            BoundingCollisionY = 4;

            BoundingDetectCollisions = new Rectangle((int)((int)Position.X - Sprite.TextureRegion.Width / BoundingCollisionX)
                , (int)((int)Position.Y + Sprite.TextureRegion.Height / BoundingCollisionY)
                , Sprite.TextureRegion.Width / 2, Sprite.TextureRegion.Height / 2);

            BoundingHitBox = new CircleF(Position, 20);         // Circle for Entity to hit

            BoundingDetectEntity = new CircleF(Position, 30);   // Circle for check attacking

            BoundingAggro = new CircleF(Position, 150);         // Circle for check aggro player 

            RandomSlimeColor();

            Sprite.Depth = 0.1f;
            Sprite.Play(SpriteCycle + "_walking");
        }

        private SlimeCopy(SlimeCopy slime)
        {
            Sprite = slime.Sprite;
            EntityData = slime.EntityData;

            EntityType = slime.EntityType;
            Id = slime.Id;
            Name = slime.Name;
            ATK = slime.ATK;
            MaxHP = slime.MaxHP;
            HP = slime.MaxHP;
            DEF_Percent = slime.DEF_Percent;
            Speed = slime.Speed;
            Evasion = slime.Evasion;

            AttackSpeed = slime.AttackSpeed;
            CooldownAttack = slime.CooldownAttack;
            CooldownAttackTimer = CooldownAttack;
            DyingTime = slime.DyingTime;

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

            BoundingCollisionX = 5.5;
            BoundingCollisionY = 5;
            BoundingDetectCollisions = slime.BoundingDetectCollisions;

            BoundingHitBox = slime.BoundingHitBox;

            BoundingAggro = slime.BoundingAggro;

            BoundingDetectEntity = slime.BoundingDetectEntity;

            RandomSlimeColor();

            Sprite.Depth = 0.1f;
            Sprite.Play(SpriteCycle + "_walking");
        }

        public override object Clone()
        {
            return new SlimeCopy(this);
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

        // Update Slime
        public override void Update(GameTime gameTime, float playerDepth, float topDepth, float middleDepth, float bottomDepth)
        {
            var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (!IsDying)
            {
                // Setup Path Finding
                SetPathFindingNode((int)EntityData.Position[0], (int)EntityData.Position[1]);

                if (!PlayerManager.Instance.IsPlayerDead)
                {
                    // Combat Control
                    CombatControl(deltaSeconds);

                    // MovementControl
                    MovementControl(deltaSeconds);

                    // Check Aggro
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
                }
                else
                {
                    InventoryManager.Instance.AddItem(0, 1);

                    InventoryManager.Instance.AddGoldCoin(5);

                    Destroy();
                }
            }

            // Update time conditions
            UpdateTimerConditions(deltaSeconds);

            if (PlayerManager.Instance.IsPlayerDead)
            {
                CurrentAnimation = SpriteCycle + "_walking";  // Idle
                Sprite.Play(CurrentAnimation);
            }

            Sprite.Update(deltaSeconds);
        }

        // Draw Slime
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (GameGlobals.Instance.IsShowPath)
            {
                PathFinding.Draw(spriteBatch);
            }

            spriteBatch.Draw(Sprite, Transform);

            // Test Draw BoundingRec for Collision
            if (GameGlobals.Instance.IsDebugMode)
            {
                var pixelTexture = new Texture2D(ScreenManager.Instance.GraphicsDevice, 1, 1);
                pixelTexture.SetData(new Color[] { Color.White });
                spriteBatch.Draw(pixelTexture, (Rectangle)BoundingDetectCollisions, Color.Red);
            }
        }

        public override Vector2 SetCombatNumDirection()
        {
            float randomFloat = (float)(new Random().NextDouble() * 1.0f) - 0.75f;
            var NumDirection = Position
                - new Vector2(Position.X + Sprite.TextureRegion.Width * randomFloat
                , Position.Y - (Sprite.TextureRegion.Height * 1.50f));
            NumDirection.Normalize();

            CombatNumVelocity = NumDirection * (Sprite.TextureRegion.Height * 2);

            return CombatNumVelocity;
        }
    }
}