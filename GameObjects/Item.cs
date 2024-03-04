using Medicraft.Data.Models;
using Medicraft.Systems;
using Medicraft.Systems.Managers;
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
            ObjectData = objectData;
          
            InitializeObjectData();

            IsRespawnable = true;
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

            BoundingCollection = new CircleF(Position, 16);

            ParticleEffect = DrawEffectSystem.SetItemParticleEffect(Position);

            Sprite.Depth = 0.1f;
            Sprite.Play(ReferId.ToString());
        }

        private Item(Item item)
        {
            Sprite = item.Sprite;
            ObjectData = item.ObjectData;
            ParticleEffect = item.ParticleEffect;

            Type = item.Type;
            Id = item.Id;
            ReferId= item.ReferId;

            Name = item.Name;
            Description = item.Description;

            IsRespawnable = item.IsRespawnable;
            IsCollected = false;
            IsDestroyed = false;
            IsVisible = true;

            Transform = new Transform2
            {
                Scale = item.Transform.Scale,
                Rotation = item.Transform.Rotation,
                Position = item.Transform.Position
            };

            BoundingCollection = item.BoundingCollection;

            Sprite.Depth = 0.1f;
            Sprite.Play(ReferId.ToString());
        }

        public override void Update(GameTime gameTime, float layerDepth)
        {
            var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Sprite.Depth = layerDepth;

            var playerDepth = PlayerManager.Instance.Player.GetDepth();

            UpdateLayerDepth(layerDepth, playerDepth);

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