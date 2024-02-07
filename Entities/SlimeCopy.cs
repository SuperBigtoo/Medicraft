﻿using Medicraft.Data.Models;
using Medicraft.Systems;
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
        private readonly EntityData _entityStats;
        private enum SlimeColor
        {
            yellow,
            red,
            green,
            blue
        }

        public SlimeCopy(AnimatedSprite sprite, EntityData entityStats, Vector2 scale)
        {
            _entityStats = entityStats;

            Type = EntityType.Hostile;         
            Id = entityStats.Id;
            Name = entityStats.Name;
            ATK = entityStats.ATK;
            HP = entityStats.HP;
            DEF_Percent = (float)entityStats.DEF_Percent;
            Speed = entityStats.Speed;
            Evasion = (float)entityStats.Evasion;
            Sprite = sprite;

            AggroTime = 0f;
            AttackSpeed = 0.4f;
            CooldownAttack = 0.7f;
            AttackCooldownTime = CooldownAttack;

            IsKnockbackable = true;

            var position = new Vector2((float)entityStats.Position[0], (float)entityStats.Position[1]);
            Transform = new Transform2
            {
                Scale = scale,
                Rotation = 0f,
                Position = position
            };

            BoundingCollisionX = 5.5;
            BoundingCollisionY = 5;

            BoundingDetectCollisions = new Rectangle((int)((int)Position.X - Sprite.TextureRegion.Width / BoundingCollisionX)
                , (int)((int)Position.Y + Sprite.TextureRegion.Height / BoundingCollisionY)
                , Sprite.TextureRegion.Width / 2, Sprite.TextureRegion.Height / 2);

            BoundingHitBox = new CircleF(Position, 20);         // Circle for Entity to hit

            BoundingDetectEntity = new CircleF(Position, 30);   // Circle for check attacking

            BoundingAggro = new CircleF(Position, 150);         // Circle for check aggro player 

            RandomSlimeColor();

            Sprite.Depth = 0.1f;
            Sprite.Play(SpriteName + "_walking");
        }

        private SlimeCopy(SlimeCopy slime)
        {
            _entityStats = slime._entityStats;

            Type = slime.Type;
            Id = _entityStats.Id;
            Name = _entityStats.Name;
            ATK = _entityStats.ATK;
            HP = _entityStats.HP;
            DEF_Percent = (float)_entityStats.DEF_Percent;
            Speed = _entityStats.Speed;
            Evasion = (float)_entityStats.Evasion;
            Sprite = slime.Sprite;

            AggroTime = slime.AggroTime;
            AttackSpeed = slime.AttackSpeed;
            CooldownAttack = slime.CooldownAttack;
            AttackCooldownTime = CooldownAttack;

            IsKnockbackable = slime.IsKnockbackable;

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
            Sprite.Play(SpriteName + "_walking");
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

        // Update Slime
        public override void Update(GameTime gameTime, float playerDepth, float topDepth, float middleDepth, float bottomDepth)
        {
            var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (!IsDying)
            {
                // Setup Path Finding
                if (AggroTime > 0)
                {
                    PathFinding = new AStar((int)Position.X, (int)((int)Position.Y + Sprite.TextureRegion.Height / BoundingCollisionY)
                        , (int)PlayerManager.Instance.Player.Position.X, (int)PlayerManager.Instance.Player.Position.Y + 75);
                }
                else
                {
                    PathFinding = new AStar((int)Position.X, (int)((int)Position.Y + Sprite.TextureRegion.Height / BoundingCollisionY)
                        , (int)_entityStats.Position[0], (int)_entityStats.Position[1]);
                }

                // Combat Control
                CombatControl(deltaSeconds);

                // MovementControl
                MovementControl(deltaSeconds);

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

                if (DyingTime > 0)
                {
                    DyingTime -= deltaSeconds;
                }
                else
                {
                    InventoryManager.Instance.AddItem(0, 1);

                    InventoryManager.Instance.GoldCoin += 5;

                    Destroy();
                }
            }

            // Update time conditions
            UpdateTimeConditions(deltaSeconds);

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

        public override void SetCombatNumDirection()
        {
            float randomFloat = (float)(new Random().NextDouble() * 1.0f) - 0.75f;
            var NumDirection = Position
                - new Vector2(Position.X + Sprite.TextureRegion.Width * randomFloat
                , Position.Y - (Sprite.TextureRegion.Height * 1.50f));
            NumDirection.Normalize();

            DamageNumVelocity = NumDirection * (Sprite.TextureRegion.Height * 2);
        }
    }
}