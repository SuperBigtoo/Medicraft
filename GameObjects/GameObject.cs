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
        public enum GameObjectType
        {
            Item,
            QuestItem,
            CraftingTable,
            SavingTable,
            WarpPoint
        }
        public GameObjectType Type { get; protected set; }

        public ObjectData ObjectData { get; protected set; }

        public int Id { get; protected set; }
        public int ReferId { get; protected set; }
        public string Name { get; protected set; }
        public string Description { get; protected set; }

        public AnimatedSprite Sprite;
        public Transform2 Transform;
        public CircleF BoundingCollection;

        public ParticleEffect ParticleEffect;

        public Vector2 Position
        {
            get => Transform.Position;
            set
            {
                Transform.Position = value;
                BoundingCollection.Center = value;
            }
        }
        
        public bool IsRespawnable { get; protected set; }
        public bool IsCollected { get; set; }
        public bool IsDestroyed { get; set; }
        public bool IsVisible { get; set; }

        protected GameObject()
        {
            Id = 0;
            ReferId = 0;
            Name = string.Empty;
            Description = string.Empty;

            IsRespawnable = true;
            IsCollected = false;
            IsDestroyed = false;
            IsVisible = true;

            Transform = new Transform2
            {
                Scale = Vector2.One,
                Rotation = 0f,
                Position = Vector2.Zero
            };

            BoundingCollection = new CircleF(Position, 16);
        }

        private GameObject(GameObject gameObject)
        {
            Id = gameObject.Id;
            ReferId = gameObject.ReferId;
            Name = gameObject.Name;
            Description = gameObject.Description;

            IsRespawnable = gameObject.IsRespawnable;
            IsCollected = false;
            IsDestroyed = false;
            IsVisible = true;

            Transform = new Transform2
            {
                Scale = gameObject.Transform.Scale,
                Rotation = gameObject.Transform.Rotation,
                Position = gameObject.Transform.Position
            };

            BoundingCollection = new CircleF(Position, 16);
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

        protected virtual void UpdateLayerDepth(float defaultDepth, float playerDepth)
        {
            // Detect for LayerDepth
            Sprite.Depth = defaultDepth; // Default depth
            if (BoundingCollection.Intersects(PlayerManager.Instance.Player.BoundingDetectEntity))
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

        protected virtual void InitializeObjectData()
        {
            SetGameObjectType(ObjectData.Category);

            Id = ObjectData.Id;
            ReferId = ObjectData.ReferId;

            switch (Type)
            {
                case GameObjectType.Item:

                case GameObjectType.QuestItem:
                    var itemData = GameGlobals.Instance.ItemsDatas.Where(i => i.ItemId.Equals(ReferId));

                    Name = itemData.ElementAt(0).Name;
                    Description = itemData.ElementAt(0).Description;
                    break;

                case GameObjectType.CraftingTable:                  

                case GameObjectType.SavingTable:                  

                case GameObjectType.WarpPoint:
                    break;
            }          
        }

        protected virtual void SetGameObjectType(int category)
        {
            switch (category)
            {
                case 0:
                    Type = GameObjectType.Item;
                    break;

                case 1:
                    Type = GameObjectType.QuestItem;
                    break;

                case 2:
                    Type = GameObjectType.CraftingTable;
                    break;

                case 3:
                    Type = GameObjectType.SavingTable;
                    break;

                case 4:
                    Type = GameObjectType.WarpPoint;
                    break;
            }
        }
    }
}