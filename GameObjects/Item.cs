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

            IsVisible = objectData.IsVisible;
            IsRespawnable = objectData.IsRespawnable;
            IsCollected = false;
            IsDestroyed = false;        

            var position = new Vector2((float)objectData.Position[0], (float)objectData.Position[1]);
            Transform = new Transform2
            {
                Scale = scale,
                Rotation = 0f,
                Position = position
            };

            BoundingCollection = new CircleF(Position, 16);

            ParticleEffect = DrawEffectSystem.SetItemParticleEffect(Position);

            QuantityDrop = GameGlobals.Instance.RandomItemQuantityDrop(ReferId);

            Sprite.Depth = 0.1f;
            Sprite.Play(ReferId.ToString());
        }

        private Item(Item item)
        {
            Sprite = item.Sprite;
            ObjectData = item.ObjectData;                 

            Type = item.Type;
            Id = item.Id;
            ReferId= item.ReferId;
            Name = item.Name;
            Description = item.Description;

            IsVisible = item.IsVisible;
            IsRespawnable = item.IsRespawnable;
            IsCollected = false;
            IsDestroyed = false;

            Transform = new Transform2
            {
                Scale = item.Transform.Scale,
                Rotation = item.Transform.Rotation,
                Position = item.Transform.Position
            };

            BoundingCollection = item.BoundingCollection;

            ParticleEffect = item.ParticleEffect;

            QuantityDrop = GameGlobals.Instance.RandomItemQuantityDrop(ReferId);

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
                InventoryManager.Instance.AddItem(ReferId, QuantityDrop);

                Destroy();
            }

            Sprite.Play(ReferId.ToString());
            Sprite.Update(deltaSeconds);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            var shadowTexture = GameGlobals.Instance.GetShadowTexture(GameGlobals.ShadowTextureName.shadow_1);

            DrawShadow(spriteBatch, shadowTexture);
        }

        public override void DrawShadow(SpriteBatch spriteBatch, Texture2D shadowTexture)
        {
            var positionPlayer = new Vector2(Position.X - (shadowTexture.Width * 0.5f) / 2.2f
                , Position.Y + (shadowTexture.Height * 0.5f) / 2f);

            spriteBatch.Draw(shadowTexture, positionPlayer, null, Color.White
                , 0f, Vector2.Zero, 0.5f, SpriteEffects.None, Sprite.Depth + 0.0000025f);
        }

        public override object Clone()
        {
            return new Item(this);
        }
    }
}