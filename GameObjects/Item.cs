using Medicraft.Data.Models;
using Medicraft.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Sprites;

namespace Medicraft.GameObjects
{
    public class Item : GameObject
    {
        public Item(AnimatedSprite sprite, ObjectStats objectStats, Vector2 scale)
        {
            Sprite = sprite;

            Id = objectStats.Id;
            Name = objectStats.Name;
            Description = objectStats.Description;

            IsCollected = false;
            IsDestroyed = false;
            IsVisible = true;

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

        private Item(Item item)
        {
            Sprite = item.Sprite;

            Id = item.Id;
            Name = item.Name;
            Description = item.Description;

            IsCollected = false;
            IsDestroyed = false;
            IsVisible = true;

            Transform = new Transform2
            {
                Scale = item.Transform.Scale,
                Rotation = item.Transform.Rotation,
                Position = item.Transform.Position
            };

            BoundingCollection = new CircleF(Position, 10);

            Sprite.Depth = 0.8f;
            Sprite.Play(Name);
        }

        public override void Update(GameTime gameTime, float layerDepth)
        {
            var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Sprite.Depth = layerDepth;

            if (IsCollected)
            {
                HudSystem.AddFeed(Name);

                if (PlayerManager.Instance.Inventory.ContainsKey(Name))
                {
                    PlayerManager.Instance.Inventory[Name] += 1;    // Adding item to Inventory... BUT the logic on how to check da item stack we'll see there...
                }
                Destroy();
            }

            Sprite.Play(Name);
            Sprite.Update(deltaSeconds);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }

        public override object Clone()
        {
            return new Item(this);
        }
    }
}
