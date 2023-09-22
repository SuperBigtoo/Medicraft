﻿using Medicraft.Data.Models;
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

        public Player(AnimatedSprite _sprite, Vector2 _position, PlayerStats _playerStats)
        {
            this._sprite = _sprite;
            this._playerStats = _playerStats;

            _transform = new Transform2
            {
                Scale = Vector2.One,
                Rotation = 0f,
                Position = _position
            };
            BoundingCircle = new CircleF(_transform.Position, 40);

            this._sprite.Play("idle");
        }

        // Update Player
        public override void Update(GameTime gameTime)
        {
            var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // MovementControl
            MovementControl(deltaSeconds);

            _sprite.Update(deltaSeconds);
        }

        // Draw Player
        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_sprite, _transform);
        }

        public void MovementControl(float deltaSeconds)
        {
            var walkSpeed = deltaSeconds * 200;
            var keyboardState = Keyboard.GetState();
            var animation = "idle";

            if (Singleton.Instance.IsGameActive)
            {
                if (keyboardState.IsKeyDown(Keys.W) || keyboardState.IsKeyDown(Keys.Up))
                {
                    animation = "walking";
                    Position -= new Vector2(0, walkSpeed);
                }

                if (keyboardState.IsKeyDown(Keys.S) || keyboardState.IsKeyDown(Keys.Down))
                {
                    animation = "walking";
                    Position += new Vector2(0, walkSpeed);
                }

                if (keyboardState.IsKeyDown(Keys.A) || keyboardState.IsKeyDown(Keys.Left))
                {
                    animation = "walking";
                    Position -= new Vector2(walkSpeed, 0);
                }

                if (keyboardState.IsKeyDown(Keys.D) || keyboardState.IsKeyDown(Keys.Right))
                {
                    animation = "walking";
                    Position += new Vector2(walkSpeed, 0);
                }
            }

            _sprite.Play(animation);
        }
    }
}
