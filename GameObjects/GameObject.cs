using Medicraft.Data.Models;
using Medicraft.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Sprites;
using System;

namespace Medicraft.GameObjects
{
    public class GameObject : ICloneable
    {
        public int Id;
        public string Name;
        public string Description;

        public AnimatedSprite Sprite;
        public Transform2 Transform;
        public CircleF BoundingCollection;

        public Vector2 Position
        {
            get => Transform.Position;
            set
            {
                Transform.Position = value;
                BoundingCollection.Center = value;
            }
        }
        public bool IsCollected { get; set; }
        public bool IsDestroyed { get; set; }
        public bool IsVisible { get; set; }

        protected GameObject()
        {
            Id = 0;
            Name = string.Empty;
            Description = string.Empty;

            IsCollected = false;
            IsDestroyed = false;
            IsVisible = true;

            Transform = new Transform2
            {
                Scale = Vector2.One,
                Rotation = 0f,
                Position = Vector2.Zero
            };

            BoundingCollection = new CircleF(Position, 10);
        }

        private GameObject(GameObject gameObject)
        {
            Id = gameObject.Id;
            Name = gameObject.Name;
            Description = gameObject.Description;

            IsCollected = false;
            IsDestroyed = false;
            IsVisible = true;

            Transform = new Transform2
            {
                Scale = gameObject.Transform.Scale,
                Rotation = gameObject.Transform.Rotation,
                Position = gameObject.Transform.Position
            };

            BoundingCollection = new CircleF(Position, 10);
        }

        public virtual void Update(GameTime gameTime, float layerDepth) { }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Sprite, Transform);
        }

        public virtual void Destroy()
        {
            IsDestroyed = true;
        }

        public virtual object Clone()
        {
            return new GameObject(this);
        }
    }
}
