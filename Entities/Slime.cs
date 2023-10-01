using Medicraft.Data.Models;
using Medicraft.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Sprites;

namespace Medicraft.Entities
{
    public class Slime : Entity
    {
        private EntityStats _entityStats;

        private string _movementAnimation;
        private string _combatAnimation;

        private float _aggroTime;

        private Vector2 _initPos;

        public Slime(AnimatedSprite _sprite, EntityStats _entityStats, Vector2 _scale)
        {
            this.sprite = _sprite;
            this._entityStats = _entityStats;
            _aggroTime = 0f;

            IsDetectCollistionObject = false;

            Vector2 _position = new Vector2((float)_entityStats.Position[0], (float)_entityStats.Position[1]);
            Transform = new Transform2
            {
                Scale = _scale,
                Rotation = 0f,
                Position = _position
            };

            BoundingRec = new Rectangle((int)Position.X - sprite.TextureRegion.Width / 5, (int)Position.Y + sprite.TextureRegion.Height / 3
                , (int)(sprite.TextureRegion.Width / 1.5), sprite.TextureRegion.Height / 6);
            BoundingCircle = new CircleF(Position, 25);
            BoundingDetectEntity = new CircleF(Position, 150);

            this.sprite.Depth = 0.3f;
            this.sprite.Play("idle");
        }

        // Update Slime
        public override void Update(GameTime gameTime, float playerDepth, float depthFrontTile, float depthBehideTile)
        {
            var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // MovementControl
            MovementControl(deltaSeconds);

            if (BoundingCircle.Intersects(PlayerManager.Instance.player.BoundingCircle))
            {
                var animation = "dead";

                sprite.Play(animation);
            }

            // Update layer depth
            UpdateLayerDepth(playerDepth, depthFrontTile, depthBehideTile);

            sprite.Update(deltaSeconds);
        }

        // Draw Slime
        public override void Draw(SpriteBatch spriteBatch)
        {            
            spriteBatch.Draw(sprite, Transform);

            // Test Draw BoundingRec for Collision
            Texture2D pixelTexture = new Texture2D(ScreenManager.Instance.GraphicsDevice, 1, 1);
            pixelTexture.SetData(new Color[] { Color.White });
            spriteBatch.Draw(pixelTexture, BoundingRec, Color.Red);
        }

        private void MovementControl(float deltaSeconds)
        {
            var walkSpeed = deltaSeconds * _entityStats.Speed;
            _movementAnimation = "idle";
            _initPos = Position;

            // Aggro
            if (BoundingDetectEntity.Intersects(PlayerManager.Instance.player.BoundingCircle))
            {
                _aggroTime = 5f;
            }

            if (_aggroTime > 0)
            {
                if (!IsKnockback && !IsDetectCollistionObject)
                {                   
                    if (Position.Y >= (PlayerManager.Instance.player.Position.Y + 50f))
                    {
                        _movementAnimation = "idle";
                        Position -= new Vector2(0, walkSpeed);
                    }

                    if (Position.Y < (PlayerManager.Instance.player.Position.Y - 30f))
                    {
                        _movementAnimation = "idle";
                        Position += new Vector2(0, walkSpeed);

                    }

                    if (Position.X > (PlayerManager.Instance.player.Position.X + 50f))
                    {
                        _movementAnimation = "idle";
                        Position -= new Vector2(walkSpeed, 0);

                    }

                    if (Position.X < (PlayerManager.Instance.player.Position.X - 50f))
                    {
                        _movementAnimation = "idle";
                        Position += new Vector2(walkSpeed, 0);
                    }

                }

                _aggroTime -= deltaSeconds;
            }

            // Knockback
            if (IsKnockback)
            {
                Position += Velocity * StunTime;

                if (StunTime > 0)
                {
                    StunTime -= deltaSeconds;
                }
                else
                {
                    IsKnockback = false;
                    Velocity = Vector2.Zero;
                }
            }

            // Detect Object Collsion
            var ObjectOnTile = Singleton.Instance.CollistionObject;
            foreach (var rect in ObjectOnTile)
            {
                if (rect.Intersects(BoundingRec))
                {
                    IsDetectCollistionObject = true;
                    Position = _initPos;
                }
                else
                {
                    IsDetectCollistionObject = false;
                }
            }

            sprite.Play(_movementAnimation);
        }

        private void CombatControl(float deltaSeconds)
        {

        }

        private void UpdateLayerDepth(float playerDepth, float depthFrontTile, float depthBehideTile)
        {
            // Detect for LayerDepth
            sprite.Depth = depthFrontTile; // Default depth
            if (BoundingCircle.Intersects(PlayerManager.Instance.player.BoundingDetectEntity))
            {
                if (Transform.Position.Y >= (PlayerManager.Instance.player.Position.Y + 40f))
                {
                    sprite.Depth = playerDepth - 0.1f; // In front Player
                }
                else
                {
                    sprite.Depth = playerDepth + 0.1f; // Behide Player
                }
            }
            else
            {
                var OnGroundObject = Singleton.Instance.OnGroundObject;
                foreach (var obj in OnGroundObject)
                {
                    if (obj.Intersects(BoundingRec))
                    {
                        sprite.Depth = depthBehideTile;
                        break; // Exit the loop as soon as an intersection is found
                    }
                }
            }
        }
    }
}
