using Medicraft.Data.Models;
using Medicraft.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
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
        public bool IsVisible { get; set; }

        protected GameObject()
        {
            Id = 0;
            ReferId = 0;
            Name = string.Empty;
            Description = string.Empty;

            IsCollected = false;
            IsDestroyed = false;
            IsVisible = true;

            Transform = new Transform2
            {
                Scale = Vector2.One,
                Rotation = 0f,
                Position = Vector2.Zero
            };

            BoundingCollection = new CircleF(Position, 10);
        }

        private GameObject(GameObject gameObject)
        {
            Id = gameObject.Id;
            ReferId = gameObject.ReferId;
            Name = gameObject.Name;
            Description = gameObject.Description;

            IsCollected = false;
            IsDestroyed = false;
            IsVisible = true;

            Transform = new Transform2
            {
                Scale = gameObject.Transform.Scale,
                Rotation = gameObject.Transform.Rotation,
                Position = gameObject.Transform.Position
            };

            BoundingCollection = new CircleF(Position, 10);
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