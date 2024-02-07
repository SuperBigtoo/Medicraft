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
        public Item(AnimatedSprite sprite, ObjectData objectData, Vector2 scale)
        {
            Sprite = sprite;

            Type = GameObjectType.Item;
            Id = objectData.Id;
            ReferId = objectData.ReferId;

            Name = GameGlobals.Instance.ItemsData[objectData.ReferId].Name;
            Description = GameGlobals.Instance.ItemsData[objectData.ReferId].Description;

            IsCollected = false;
            IsDestroyed = false;
            IsVisible = true;

            var position = new Vector2((float)objectData.Position[0], (float)objectData.Position[1]);
            Transform = new Transform2
            {
                Scale = scale,
                Rotation = 0f,
                Position = position
            };

            BoundingCollection = new CircleF(Position, 10);

            Sprite.Depth = 0.1f;
            Sprite.Play(ReferId.ToString());
        }

        private Item(Item item)
        {
            Sprite = item.Sprite;

            Type = item.Type;
            Id = item.Id;
            ReferId= item.ReferId;

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

            Sprite.Depth = 0.1f;
            Sprite.Play(ReferId.ToString());
        }

        public override void Update(GameTime gameTime, float layerDepth)
        {
            var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Sprite.Depth = layerDepth;

            if (IsCollected)
            {
                // Adding item to Inventory... BUT the logic on how to check da item stack we'll see there...
                InventoryManager.Instance.AddItem(ReferId, 1);

                Destroy();
            }

            Sprite.Play(ReferId.ToString());
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