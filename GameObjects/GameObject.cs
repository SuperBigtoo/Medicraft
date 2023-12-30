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
        public ObjectStats ObjectStats;
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

        public GameObject() { }

        public GameObject(AnimatedSprite sprite, ObjectStats objectStats, Vector2 scale)
        {
            ObjectStats = objectStats;
            Sprite = sprite;

            Id = objectStats.Id;
            Name = objectStats.Name;
            Description = objectStats.Description;

            var position = new Vector2((float)objectStats.Position[0], (float)objectStats.Position[1]);
            Transform = new Transform2
            {
                Scale = scale,
                Rotation = 0f,
                Position = position
            };

            BoundingCollection = new CircleF(Position, 10);

            Sprite.Depth = 0.8f;
            Sprite.Play(Name);
        }

        private GameObject(GameObject objectStats)
        {
            ObjectStats = objectStats.ObjectStats;
            Sprite = objectStats.Sprite;

            Id = objectStats.Id;
            Name = objectStats.Name;
            Description = objectStats.Description;

            Transform = new Transform2
            {
                Scale = objectStats.Transform.Scale,
                Rotation = objectStats.Transform.Rotation,
                Position = objectStats.Transform.Position
            };

            BoundingCollection = new CircleF(Position, 10);

            Sprite.Depth = 0.8f;
            Sprite.Play(Name);
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
