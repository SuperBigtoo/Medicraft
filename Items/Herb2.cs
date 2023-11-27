using Medicraft.Data.Models;
using Medicraft.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Sprites;

namespace Medicraft.Items
{
    public class Herb2 : Item
    {
        public Herb2(AnimatedSprite sprite, ItemStats itemStats)
        {
            ItemStats = itemStats;
            Sprite = sprite;

            Id = itemStats.Id;
            Name = itemStats.Name;
            Description = itemStats.Description;

            Sprite.Play(Name);
        }

        public Herb2(AnimatedSprite sprite, ItemStats itemStats, Vector2 scale)
        {
            ItemStats = itemStats;
            Sprite = sprite;

            Id = itemStats.Id;
            Name = itemStats.Name;
            Description = itemStats.Description;

            var position = new Vector2((float)itemStats.Position[0], (float)itemStats.Position[1]);
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

        private Herb2(Herb2 herb2)
        {
            ItemStats = herb2.ItemStats;
            Sprite = herb2.Sprite;

            Id = herb2.Id;
            Name = herb2.Name;
            Description = herb2.Description;

            Transform = new Transform2
            {
                Scale = herb2.Transform.Scale,
                Rotation = herb2.Transform.Rotation,
                Position = herb2.Transform.Position
            };

            BoundingCollection = new CircleF(Position, 10);

            Sprite.Depth = 0.8f;
            Sprite.Play(Name);
        }

        public override void Update(GameTime gameTime, float layerDepth)
        {
            if (IsCollected)
            {
                HudSystem.AddFeed(Name);

                if (PlayerManager.Instance.Inventory.ContainsKey(Name))
                {
                    PlayerManager.Instance.Inventory[Name] += 1;
                }

                Destroy();
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Sprite, Transform);
        }

        public override object Clone()
        {
            return new Herb2(this);
        }
    }
}
