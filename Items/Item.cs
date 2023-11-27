using Medicraft.Data.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Sprites;
using System;

namespace Medicraft.Items
{
    public class Item : ICloneable
    {
        public int Id;
        public string Name;
        public string Description;
        public ItemStats ItemStats;
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

        protected Item()
        {
            Id = 0;
            Name = string.Empty;
            Description = string.Empty;
            Transform = new Transform2
            {
                Scale = Vector2.One,
                Rotation = 0f,
                Position = Vector2.Zero
            };

            BoundingCollection = new CircleF(Position, 16);

            IsCollected = false;
            IsDestroyed = false;
        }

        private Item(Item item)
        {
            Id = item.Id;
            Name = item.Name;
            Description = item.Description;
            Transform = item.Transform;
            BoundingCollection = item.BoundingCollection;
            IsCollected = false;
            IsDestroyed = false;
        }

        public virtual void Update(GameTime gameTime, float layerDepth) { }
        public virtual void Draw(SpriteBatch spriteBatch) { }
        public virtual void Destroy()
        {
            IsDestroyed = true;
        }
        public virtual object Clone()
        {
            return new Item(this);
        }
    }
}
