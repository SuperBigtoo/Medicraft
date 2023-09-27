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
        private AnimatedSprite _sprite;
        private EntityStats _entityStats;

        private string _movementAnimation;
        private string _combatAnimation;

        private float _aggroTime;

        public Slime(AnimatedSprite _sprite, EntityStats _entityStats, Vector2 _scale)
        {
            this._sprite = _sprite;
            this._entityStats = _entityStats;
            _aggroTime = 0f;

            Vector2 _position = new Vector2((float)_entityStats.Position[0], (float)_entityStats.Position[1]);
            Transform = new Transform2
            {
                Scale = _scale,
                Rotation = 0f,
                Position = _position
            };

            BoundingCircle = new CircleF(Transform.Position, 25);
            BoundingDetectEntity = new CircleF(Transform.Position, 200);

            this._sprite.Play("idle");
        }

        // Update Slime
        public override void Update(GameTime gameTime)
        {
            var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // MovementControl
            MovementControl(deltaSeconds);

            if (BoundingCircle.Intersects(PlayerManager.Instance.player.BoundingCircle))
            {
                var animation = "dead";

                _sprite.Play(animation);
            }

            _sprite.Update(deltaSeconds);
        }

        // Draw Slime
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Transform.Position.Y >= (PlayerManager.Instance.player.Position.Y + 40f))
            {
                _sprite.Depth = 0.1f; // In front Player
            }
            else _sprite.Depth = 0.3f; // Behide Player

            spriteBatch.Draw(_sprite, Transform);
        }

        private void MovementControl(float deltaSeconds)
        {
            var walkSpeed = deltaSeconds * _entityStats.Speed;
            _movementAnimation = "idle";

            // Aggro
            if (BoundingDetectEntity.Intersects(PlayerManager.Instance.player.BoundingCircle))
            {
                _aggroTime = 5f;
            }

            if (_aggroTime > 0)
            {
                if (!IsKnockback)
                {
                    if (Position.Y >= (PlayerManager.Instance.player.Position.Y + 50f))
                    {
                        _movementAnimation = "idle";
                        Position -= new Vector2(0, walkSpeed);
                    }

                    if (Position.Y < (PlayerManager.Instance.player.Position.Y - 10f))
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

            _sprite.Play(_movementAnimation);
        }

        private void CombatControl(float deltaSeconds)
        {

        }
    }
}
