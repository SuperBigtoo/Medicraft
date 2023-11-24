using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Sprites;
using Microsoft.Xna.Framework.Input;

namespace Medicraft.Entities
{
    public class Entity
    {
        public int Id;
        public string Name;
        public AnimatedSprite sprite;
        public Transform2 Transform;
        public Vector2 Velocity;
        public Rectangle BoundingRec; // For dectect collisions
        public CircleF BoundingCircle;
        public CircleF BoundingDetectEntity;

        public Vector2 Position
        {
            get => Transform.Position;
            set
            {
                Transform.Position = value;
                BoundingRec.X = (int)value.X - sprite.TextureRegion.Width / 5;
                BoundingRec.Y = (int)value.Y + sprite.TextureRegion.Height / 3;
                BoundingCircle.Center = value;
                BoundingDetectEntity.Center = value;
            }
        }

        public float StunTime { get; set; }
        public bool IsKnockback { get; set; }
        public bool IsDestroyed { get; set; }
        public bool IsMoving { get; set; }
        public bool IsAttacking { get; set; }
        public bool IsDetectCollistionObject { get; set; }

        protected Entity()
        {
            Velocity = Vector2.Zero;
            StunTime = 0;
            IsKnockback = false;
            IsDestroyed = false;
        }

        public virtual void Update(GameTime gameTime, KeyboardState keyboardCurrentState, KeyboardState keyboardPrevioseState
            , MouseState mouseCurrentState, MouseState mousePrevioseState, float depthFrontTile, float depthBehideTile) { }
        public virtual void Update(GameTime gameTime, float playerDepth, float depthFrontTile, float depthBehideTile) { }
        public virtual void Draw(SpriteBatch spriteBatch) { }

        public virtual void Destroy()
        {
            IsDestroyed = true;
        }
    }
}
