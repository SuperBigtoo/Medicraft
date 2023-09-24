﻿using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Medicraft.Entities
{
    public abstract class Entity
    {
        public string _name;
        public Transform2 _transform;
        public Vector2 Velocity;
        public CircleF BoundingCircle;
        public CircleF BoundingDetectEntity;

        public Vector2 Position
        {
            get => _transform.Position;
            set
            {
                _transform.Position = value;
                BoundingCircle.Center = value;
                BoundingDetectEntity.Center = value;
            }
        }

        public float StunTime { get; set; }
        public bool IsKnockback { get; set; }
        public bool IsDestroyed { get; set; }

        protected Entity()
        {
            Velocity = Vector2.Zero;
            StunTime = 0;
            IsKnockback = false;
            IsDestroyed = false;
        }

        public abstract void Update(GameTime gameTime);
        public abstract void Draw(SpriteBatch spriteBatch);

        public virtual void Destroy()
        {
            IsDestroyed = true;
        }
    }
}
