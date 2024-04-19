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
        public int QuantityDrop { get; protected set; }

        public Item(AnimatedSprite sprite, ObjectData objectData, Vector2 scale)
        {
            Sprite = sprite;
            ObjectData = objectData;
          
            InitializeObjectData();

            IsVisible = objectData.IsVisible;
            IsRespawnable = objectData.IsRespawnable;
            IsDetectable = true;

            var position = new Vector2(
                (float)objectData.Position[0],
                (float)objectData.Position[1]);

            var currMap = ScreenManager.Instance.CurrentMap;
            if (currMap.Equals("map_2") || currMap.Equals("battlezone_2") || currMap.Equals("dungeon_2")
                || currMap.Equals("map_3") || currMap.Equals("battlezone_3") || currMap.Equals("dungeon_3"))
            {
                position.X += 480;
                position.Y += 480;
            }

            Transform = new Transform2
            {
                Scale = scale,
                Rotation = 0f,
                Position = position
            };

            BoundingInteraction = new CircleF(Position, 16);

            ParticleEffect = DrawEffectSystem.SetItemParticleEffect(ObjectType, Position);

            QuantityDrop = GameGlobals.RandomItemQuantityDrop(ReferId);

            Sprite.Depth = InitDepth;
            Sprite.Play(ReferId.ToString());
        }

        private Item(Item item)
        {
            Sprite = item.Sprite;
            ObjectData = item.ObjectData;                 

            ObjectType = item.ObjectType;
            Id = item.Id;
            ReferId= item.ReferId;
            Name = item.Name;
            Description = item.Description;

            IsDetectable= item.IsDetectable;
            IsVisible = item.IsVisible;
            IsRespawnable = item.IsRespawnable;
            IsCollected = false;
            IsDestroyed = false;
            IsDetected = false;

            Transform = new Transform2
            {
                Scale = item.Transform.Scale,
                Rotation = item.Transform.Rotation,
                Position = item.Transform.Position
            };

            BoundingInteraction = item.BoundingInteraction;

            ParticleEffect = item.ParticleEffect;

            QuantityDrop = GameGlobals.RandomItemQuantityDrop(ReferId);

            Sprite.Depth = InitDepth;
            Sprite.Play(ReferId.ToString());
        }

        public override void Update(GameTime gameTime, float layerDepth)
        {
            var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Sprite.Depth = layerDepth;

            var playerDepth = PlayerManager.Instance.Player.GetDepth();

            UpdateLayerDepth(layerDepth, playerDepth);
            UpdateParticleLayerDepth(deltaSeconds, Sprite.Depth);

            // When Detected
            if (IsDetected)
            {
                SignSprite.Depth = InitDepth;
                SignSprite.Play("collecting_1");
                SignSprite.Update(deltaSeconds);
            }

            // When Collected
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

            var shadowTexture = GameGlobals.GetShadowTexture(GameGlobals.ShadowTextureName.shadow_1);

            DrawShadow(spriteBatch, shadowTexture);

            DrawDetectedSign(spriteBatch);
        }

        public override object Clone()
        {
            return new Item(this);
        }
    }
}