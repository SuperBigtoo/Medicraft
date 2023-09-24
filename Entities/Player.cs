using Medicraft.Data.Models;
using Medicraft.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Sprites;

namespace Medicraft.Entities
{
    public class Player : Entity
    {
        private AnimatedSprite _sprite;
        private PlayerStats _playerStats;

        private string _movementAnimation;
        private string _combatAnimation;

        private bool _isAttack;
        private float _attackTime;
        private float _attackSpeed;
        private float _knockbackForce;

        public Player(AnimatedSprite _sprite, PlayerStats _playerStats, Vector2 _position)
        {
            this._sprite = _sprite;
            this._playerStats = _playerStats;
            _isAttack = false;
            _attackTime = 0f;
            _attackSpeed = 0.25f;  // Stat
            _knockbackForce = 50f;  // Stat

            _transform = new Transform2
            {
                Scale = Vector2.One,
                Rotation = 0f,
                Position = _position
            };

            BoundingCircle = new CircleF(_transform.Position, 47.5f);
            BoundingDetectEntity = new CircleF(_transform.Position, 80f);

            this._sprite.Depth = 0.2f;
            this._sprite.Play("idle");
        }

        // Update Player
        public override void Update(GameTime gameTime)
        {
            var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // MovementControl
            MovementControl(deltaSeconds);

            // CombatControl
            CombatControl(deltaSeconds);

            _sprite.Update(deltaSeconds);
        }

        // Draw Player
        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_sprite, _transform);
        }

        // Movement
        private void MovementControl(float deltaSeconds)
        {
            var walkSpeed = deltaSeconds * _playerStats.Speed;
            var keyboardState = Keyboard.GetState();
            _movementAnimation = "idle";

            if (!_isAttack)
            {
                if (keyboardState.IsKeyDown(Keys.W) || keyboardState.IsKeyDown(Keys.Up))
                {
                    _movementAnimation = "walking";
                    Position -= new Vector2(0, walkSpeed);
                }

                if (keyboardState.IsKeyDown(Keys.S) || keyboardState.IsKeyDown(Keys.Down))
                {
                    _movementAnimation = "walking";
                    Position += new Vector2(0, walkSpeed);
                }

                if (keyboardState.IsKeyDown(Keys.A) || keyboardState.IsKeyDown(Keys.Left))
                {
                    _movementAnimation = "walking";
                    Position -= new Vector2(walkSpeed, 0);
                }

                if (keyboardState.IsKeyDown(Keys.D) || keyboardState.IsKeyDown(Keys.Right))
                {
                    _movementAnimation = "walking";
                    Position += new Vector2(walkSpeed, 0);
                }
            }

            _sprite.Play(_movementAnimation);
        }

        // Combat
        private void CombatControl(float deltaSeconds)
        {
            Singleton.Instance.mousePreviose = Singleton.Instance.mouseCurrent;
            Singleton.Instance.mouseCurrent = Mouse.GetState();

            if (Singleton.Instance.mouseCurrent.LeftButton == ButtonState.Pressed
                    && Singleton.Instance.mousePreviose.LeftButton == ButtonState.Released && !_isAttack)
            {
                _isAttack = true;
                _attackTime = _attackSpeed;

                CheckAttackDetection();
            }

            if (_attackTime > 0)
            {
                _attackTime -= deltaSeconds;
            }
            else _isAttack = false;
        }

        private void CheckAttackDetection()
        {
            if (EntityManager.Instance.entities.Count != 0) 
            {
                foreach (Entity en in EntityManager.Instance.entities)
                {
                    if (BoundingDetectEntity.Intersects(en.BoundingCircle))
                    {
                        if (!en.IsDestroyed)
                        {
                            Vector2 knockBackDirection = en.Position - this.Position;
                            knockBackDirection.Normalize();

                            en.Velocity = knockBackDirection * _knockbackForce;
                            en.IsKnockback = true;
                            en.StunTime = 0.2f;
                            
                            //en.Position += velocity;
                        }
                    }
                }
            }
        }
    }
}
