using Medicraft.Data.Models;
using Medicraft.Systems;
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
        private readonly EntityStats _entityStats;
        private enum SlimeColor
        {
            yellow,
            red,
            green,
            blue
        }

        public Slime(AnimatedSprite sprite, EntityStats entityStats, Vector2 scale)
        {
            _entityStats = entityStats;
            Id = entityStats.Id;
            Name = entityStats.Name;
            ATK = entityStats.ATK;
            HP = entityStats.HP;
            DEF_Percent = (float)entityStats.DEF_Percent;
            Speed = entityStats.Speed;
            Evasion = (float)entityStats.Evasion;
            Sprite = sprite;
            AggroTime = 0f;

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
                , (int)(Sprite.TextureRegion.Width / 3), Sprite.TextureRegion.Height / 5);

            BoundingHitBox = new CircleF(Position, 20);

            // For Aggro
            BoundingDetection = new CircleF(Position, 150);

            RandomSlimeColor();

            Sprite.Depth = 0.3f;
            Sprite.Play(SpriteName + "_walking");
        }

        private Slime(Slime slime)
        {
            _entityStats = slime._entityStats;

            Id = _entityStats.Id;
            Name = _entityStats.Name;
            ATK = _entityStats.ATK;
            HP = _entityStats.HP;
            DEF_Percent = (float)_entityStats.DEF_Percent;
            Speed = _entityStats.Speed;
            Evasion = (float)_entityStats.Evasion;

            Sprite = slime.Sprite;

            AggroTime = slime.AggroTime;

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

            BoundingDetection = slime.BoundingDetection;

            RandomSlimeColor();

            Sprite.Depth = 0.3f;
            Sprite.Play(SpriteName + "_walking");
        }

        public override object Clone()
        {
            return new Slime(this);
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
        public override void Update(GameTime gameTime, float playerDepth, float depthFrontTile, float depthBehideTile)
        {
            var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (!IsDying)
            {
                PathFinding = new AStar((int)Position.X, (int)((int)Position.Y + Sprite.TextureRegion.Height / BoundingCollisionY));

                // Combat Control
                CombatControl(deltaSeconds);

                // MovementControl
                MovementControl(deltaSeconds);

                // Update layer depth
                UpdateLayerDepth(playerDepth, depthFrontTile, depthBehideTile);
            }
            else
            {
                // Dying time before destroy
                CurrentAnimation = SpriteName + "_dying";

                if (DyingTime > 0)
                {
                    DyingTime -= deltaSeconds;
                }
                else
                {
                    HudSystem.AddFeed("herb_2");

                    if (PlayerManager.Instance.Inventory.ContainsKey("herb_2"))
                    {
                        PlayerManager.Instance.Inventory["herb_2"] += 1;
                    }

                    PlayerManager.Instance.Coin += 10;

                    Destroy();
                }
            }

            Sprite.Play(CurrentAnimation);
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
    }
}
