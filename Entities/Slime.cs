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

        public Slime(AnimatedSprite _sprite, Vector2 _position, EntityStats _entityStats)
        {
            this._sprite = _sprite;
            this._entityStats = _entityStats;

            _transform = new Transform2
            {
                Scale = Vector2.One,
                Rotation = 0f,
                Position = _position
            };
            BoundingCircle = new CircleF(_transform.Position, 24);

            this._sprite.Play("idle");
        }

        // Update Slime
        public override void Update(GameTime gameTime)
        {
            var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var animation = "idle";
                        
            if (Singleton.Instance.IsGameActive)
            {
                if (BoundingCircle.Intersects(Singleton.Instance._player.BoundingCircle))
                {
                    animation = "dead";
                }
            }

            _sprite.Play(animation);

            _sprite.Update(deltaSeconds);
        }

        // Draw Slime
        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_sprite, _transform);
        }
    }
}
