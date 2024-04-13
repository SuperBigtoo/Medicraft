using Medicraft.Data.Models;
using Medicraft.Systems;
using Medicraft.Systems.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Particles;
using MonoGame.Extended.Sprites;
using System;
using System.Linq;

namespace Medicraft.GameObjects
{
    public class GameObject : ICloneable
    {       
        public ObjectData ObjectData { get; protected set; }

        public int Id { get; protected set; }
        public int ReferId { get; protected set; }
        public string Name { get; protected set; }
        public string Description { get; protected set; }

        public const float InitDepth = 0.1f;

        public AnimatedSprite Sprite { get; protected set; }
        public AnimatedSprite SignSprite { get; protected set; }
        public Transform2 Transform { get; protected set; }
        public CircleF BoundingInteraction;

        public ParticleEffect ParticleEffect { get; protected set; }

        public Vector2 Position
        {
            get => Transform.Position;
            set
            {
                Transform.Position = value;
                BoundingInteraction.Center = value;
            }
        }

        public bool IsDetectable { get; protected set; }
        public bool IsDetected { get; set; }
        public bool IsRespawnable { get; protected set; }
        public bool IsCollected { get; set; }
        public bool IsDestroyable { get; protected set; }
        public bool IsDestroyed { get; set; }
        public bool IsVisible { get; set; }

        public enum GameObjectType
        {
            Item,
            QuestObject,
            CraftingTable,
            SavingTable,
            WarpPoint
        }
        public GameObjectType ObjectType { get; protected set; }  

        protected GameObject()
        {
            SignSprite = new AnimatedSprite(GameGlobals.Instance.UIBooksIconHUD)
            {
                Depth = InitDepth
            };

            Id = 0;
            ReferId = 0;
            Name = string.Empty;
            Description = string.Empty;

            IsDetectable = false;
            IsDetected = false;
            IsRespawnable = true;
            IsCollected = false;
            IsDestroyable = false;
            IsDestroyed = false;
            IsVisible = true;

            Transform = new Transform2
            {
                Scale = Vector2.One,
                Rotation = 0f,
                Position = Vector2.Zero
            };
        }

        private GameObject(GameObject gameObject)
        {
            SignSprite = gameObject.SignSprite;
            SignSprite.Depth = InitDepth;

            Id = gameObject.Id;
            ReferId = gameObject.ReferId;
            Name = gameObject.Name;
            Description = gameObject.Description;

            IsDetectable = gameObject.IsDetectable;
            IsDetected = false;
            IsRespawnable = gameObject.IsRespawnable;
            IsCollected = false;
            IsDestroyable = false;
            IsDestroyed = false;
            IsVisible = true;

            Transform = new Transform2
            {
                Scale = gameObject.Transform.Scale,
                Rotation = gameObject.Transform.Rotation,
                Position = gameObject.Transform.Position
            };
        }

        public virtual void Update(GameTime gameTime, float layerDepth) { }

        public virtual void Draw(SpriteBatch spriteBatch)
        {       
            spriteBatch.Draw(Sprite, Transform);

            if (ParticleEffect != null)
                spriteBatch.Draw(ParticleEffect);
        }

        public virtual void DrawShadow(SpriteBatch spriteBatch, Texture2D shadowTexture)
        {
            var position = new Vector2(Position.X - (shadowTexture.Width * 0.5f) / 2.2f
                , Position.Y + (shadowTexture.Height * 0.5f) / 1.8f);

            spriteBatch.Draw(shadowTexture, position, null, Color.White
                , 0f, Vector2.Zero, 0.5f, SpriteEffects.None, Sprite.Depth + 0.0000025f);
        }

        public virtual void DrawDetectedSign(SpriteBatch spriteBatch)
        {
            if (IsDetected)
            {
                var position = new Vector2(
                    Position.X,
                    Position.Y - ((Sprite.TextureRegion.Height / 2) + SignSprite.TextureRegion.Height));

                var transform = new Transform2
                {
                    Scale = Vector2.One,
                    Rotation = 0f,
                    Position = position
                };

                spriteBatch.Draw(SignSprite, transform);
            }
        }

        public virtual void Destroy()
        {
            if (IsDestroyable) return;

            IsDestroyed = true;
        }

        public virtual object Clone()
        {
            return new GameObject(this);
        }

        protected virtual void UpdateLayerDepth(float defaultDepth, float playerDepth)
        {
            // Detect for LayerDepth
            Sprite.Depth = defaultDepth; // Default depth
            if (BoundingInteraction.Intersects(PlayerManager.Instance.Player.BoundingDetectEntity))
            {
                if (Transform.Position.Y >= PlayerManager.Instance.Player.BoundingDetectCollisions.Center.Y)
                {
                    Sprite.Depth = playerDepth - 0.00002f; // In front Player
                }
                else
                {
                    Sprite.Depth = playerDepth + 0.00002f; // Behide Player
                }
            }
        }

        protected virtual void UpdateParticleLayerDepth(float deltaSeconds, float layerDepth)
        {
            if (ParticleEffect == null) return;

            foreach (var particleEmitter in ParticleEffect.Emitters)
            {
                particleEmitter.LayerDepth = layerDepth - 0.000001f;
                particleEmitter.Parameters.Opacity = new Range<float>(0.15f, 0.6f);
            }

            ParticleEffect.Update(deltaSeconds);
        }

        protected virtual void InitializeObjectData()
        {
            SetGameObjectType(ObjectData.Category);

            Id = ObjectData.Id;
            ReferId = ObjectData.ReferId;
            IsDestroyable = ObjectData.IsDestroyable;
            IsRespawnable = ObjectData.IsRespawnable;
            IsVisible = ObjectData.IsVisible;

            switch (ObjectType)
            {
                case GameObjectType.Item:
                case GameObjectType.QuestObject:
                    var itemData = GameGlobals.Instance.ItemsDatas.FirstOrDefault
                        (i => i.ItemId.Equals(ReferId));
                    Name = itemData.Name;
                    Description = itemData.Description;                  
                break;

                case GameObjectType.CraftingTable:                  
                case GameObjectType.SavingTable:                  
                case GameObjectType.WarpPoint:
                    Name = ObjectData.Name;
                    Description = ObjectData.Description;
                break;
            }          
        }

        protected virtual void SetGameObjectType(int category)
        {
            switch (category)
            {
                case 0:
                    ObjectType = GameObjectType.Item;
                break;

                case 1:
                    ObjectType = GameObjectType.QuestObject;
                break;

                case 2:
                    ObjectType = GameObjectType.CraftingTable;
                break;

                case 3:
                    ObjectType = GameObjectType.SavingTable;
                break;

                case 4:
                    ObjectType = GameObjectType.WarpPoint;
                break;
            }
        }
    }
}