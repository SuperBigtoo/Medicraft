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
    public class Slime : Entity
    {
        public EntityData EntityData { get; private set; }
        private enum SlimeColor
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

            AttackSpeed = 0.4f;
            CooldownAttack = 0.7f;
            CooldownAttackTimer = CooldownAttack;
            DyingTime = 1.3f;

            IsAggroResettable = true;
            IsKnockbackable = true;

            SetPathFindingType(2);
            NextNodeTime = 10f;

            var position = new Vector2((float)entityData.Position[0], (float)entityData.Position[1]);
            Transform = new Transform2
            {
                Scale = scale,
                Rotation = 0f,
                Position = position
            };

            BoundingCollisionX = 4;
            BoundingCollisionY = 12;

            BoundingDetectCollisions = new Rectangle((int)((int)Position.X - Sprite.TextureRegion.Width / BoundingCollisionX),
                (int)((int)Position.Y + Sprite.TextureRegion.Height / BoundingCollisionY),
                (int)(Sprite.TextureRegion.Width / 2.5f),
                Sprite.TextureRegion.Height / 6
            );     // Rec for check Collision

            BoundingHitBox = new CircleF(Position, 20);         // Circle for Entity to hit

            BoundingDetectEntity = new CircleF(Position, 30);   // Circle for check attacking

            BoundingAggro = new CircleF(Position, 150);         // Circle for check aggro player        

            PathFinding = new AStar(
                BoundingDetectCollisions.Center.X,
                BoundingDetectCollisions.Center.Y,
                (int)EntityData.Position[0],
                (int)EntityData.Position[1]
            );

            RandomSlimeColor();

            Sprite.Depth = 0.1f;
            Sprite.Play(SpriteName + "_walking");
        }

        private Slime(Slime slime)
        {
            Sprite = slime.Sprite;
            EntityData = slime.EntityData;

            EntityType = slime.EntityType;        
            Id = slime.Id;
            Name = slime.Name;
            ATK = slime.ATK;
            MaximumHP = slime.MaximumHP;
            HP = slime.MaximumHP;
            DEF_Percent = slime.DEF_Percent;
            Speed = slime.Speed;
            Evasion = slime.Evasion;

            AttackSpeed = slime.AttackSpeed;
            CooldownAttack = slime.CooldownAttack;
            CooldownAttackTimer = CooldownAttack;
            DyingTime = slime.DyingTime;

            IsAggroResettable = slime.IsAggroResettable;
            IsKnockbackable = slime.IsKnockbackable;

            PathFindingType = slime.PathFindingType;
            NextNodeTime = slime.NextNodeTime;

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

            PathFinding = new AStar(
                BoundingDetectCollisions.Center.X,
                BoundingDetectCollisions.Center.Y,
                (int)EntityData.Position[0],
                (int)EntityData.Position[1]
            );

            RandomSlimeColor();

            Sprite.Depth = 0.1f;
            Sprite.Play(SpriteName + "_walking");
        }    

        // Update Slime
        public override void Update(GameTime gameTime, float playerDepth, float topDepth, float middleDepth, float bottomDepth)
        {
            var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (!IsDying)
            {                
                NextPathFindingNode(deltaSeconds);

                // Setup Path Finding
                SetPathFindingNode((int)EntityData.Position[0], (int)EntityData.Position[1]);

                if (!PlayerManager.Instance.IsPlayerDead)
                {
                    // Combat Control
                    CombatControl(deltaSeconds);

                    // MovementControl
                    MovementControl(deltaSeconds);
                }
                
                // Update layer depth
                UpdateLayerDepth(playerDepth, topDepth, middleDepth, bottomDepth);
            }
            else
            {
                // Dying time before destroy
                CurrentAnimation = SpriteName + "_dying";
                Sprite.Play(CurrentAnimation);

                // Check Object Collsion
                CheckCollision();

                if (DyingTimer < DyingTime)
                {
                    DyingTimer += deltaSeconds;
                }
                else
                {
                    InventoryManager.Instance.AddItem(1, 1);

                    InventoryManager.Instance.GoldCoin += 10;

                    Destroy();
                }
            }

            // Update time conditions
            UpdateTimerConditions(deltaSeconds);

            if (PlayerManager.Instance.IsPlayerDead)
            {
                CurrentAnimation = SpriteName + "_walking";  // Idle
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
            if (GameGlobals.Instance.IsShowDetectBox)
            {
                var pixelTexture = new Texture2D(ScreenManager.Instance.GraphicsDevice, 1, 1);
                pixelTexture.SetData(new Color[] { Color.White });
                spriteBatch.Draw(pixelTexture, BoundingDetectCollisions, Color.Red);
            }
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
                    SpriteName = "yellow";
                    break;

                case SlimeColor.red:
                    SpriteName = "red";
                    break;

                case SlimeColor.green:
                    SpriteName = "green";
                    break;

                case SlimeColor.blue:
                    SpriteName = "blue";
                    break;
            }
        }

        public override object Clone()
        {
            return new Slime(this);
        }

        public override Vector2 SetCombatNumDirection()
        {
            Vector2 offset = new Vector2(Position.X, Position.Y - Sprite.TextureRegion.Height * 1.5f);

            Vector2 numDirection = Position - offset;
            numDirection.Normalize();

            CombatNumVelocity = numDirection * Sprite.TextureRegion.Height;

            return CombatNumVelocity;
        }
    }
}