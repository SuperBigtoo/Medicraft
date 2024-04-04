using Medicraft.Data.Models;
using Medicraft.Systems.Managers;
using Medicraft.Systems.PathFinding;
using Medicraft.Systems;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using System;
using MonoGame.Extended.Sprites;

namespace Medicraft.Entities.Mobs.Friendly
{
    public class Cat : FriendlyMob
    {
        public enum CatType
        {
            white,
            siamnese,
            cow,
            som,
            chok,
            brown,
            tiger,
            black
        }

        public Cat(AnimatedSprite sprite, EntityData entityData, Vector2 scale)
        {
            Sprite = sprite;
            EntityData = entityData;

            // Initialize Character Data
            Id = entityData.Id;             // Mot to be confuse with CharId
            Level = entityData.Level;
            InitializeCharacterData(entityData.CharId, Level);

            IsRespawnable = true;

            SetPathFindingType(entityData.PathFindingType);
            NodeCycleTime = entityData.NodeCycleTime;

            var position = new Vector2((float)entityData.Position[0], (float)entityData.Position[1]);
            Transform = new Transform2
            {
                Scale = scale,
                Rotation = 0f,
                Position = position
            };

            BoundingCollisionX = 4;
            BoundingCollisionY = 4;

            // Rec for check Collision
            BoundingDetectCollisions = new Rectangle(
                (int)((int)Position.X - Sprite.TextureRegion.Width / BoundingCollisionX),
                (int)((int)Position.Y + Sprite.TextureRegion.Height / BoundingCollisionY),
                (int)(Sprite.TextureRegion.Width / 2.5f),
                Sprite.TextureRegion.Height / 6);

            BoundingHitBox = new CircleF(Position, 32);         // Circle for Entity to hit
            BoundingDetectEntity = new CircleF(Position, 32);   // Circle for check attacking      

            RandomCatType();

            Sprite.Depth = 0.1f;
            Sprite.Play(SpriteCycle + "_idle_1");
        }

        private Cat(Cat cat)
        {
            Sprite = cat.Sprite;
            EntityData = cat.EntityData;

            EntityType = cat.EntityType;
            Id = cat.Id;
            Name = cat.Name;
            ATK = cat.ATK;
            MaxHP = cat.MaxHP;
            HP = cat.MaxHP;
            DEF = cat.DEF;
            Speed = cat.Speed;
            Evasion = cat.Evasion;

            IsRespawnable = cat.IsRespawnable;

            PathFindingType = cat.PathFindingType;
            NodeCycleTime = cat.NodeCycleTime;

            Transform = new Transform2
            {
                Scale = cat.Transform.Scale,
                Rotation = cat.Transform.Rotation,
                Position = cat.Transform.Position,
            };

            BoundingCollisionX = cat.BoundingCollisionX;
            BoundingCollisionY = cat.BoundingCollisionY;

            BoundingDetectCollisions = cat.BoundingDetectCollisions;
            BoundingHitBox = cat.BoundingHitBox;
            BoundingDetectEntity = cat.BoundingDetectEntity;

            RandomCatType();

            Sprite.Color = Color.White;
            Sprite.Depth = 0.1f;
            Sprite.Play(SpriteCycle + "_idle_1");
        }

        public override void Update(GameTime gameTime, float playerDepth, float topDepth, float middleDepth, float bottomDepth)
        {
            base.Update(gameTime, playerDepth, topDepth, middleDepth, bottomDepth);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }

        private void RandomCatType()
        {
            var random = new Random();
            Array slimeColors = Enum.GetValues(typeof(CatType));
            int randomIndex = random.Next(slimeColors.Length);
            CatType randomColor = (CatType)slimeColors.GetValue(randomIndex);

            switch (randomColor)
            {
                case CatType.white:
                    SpriteCycle = "white";
                    break;

                case CatType.siamnese:
                    SpriteCycle = "siamnese";
                    break;

                case CatType.cow:
                    SpriteCycle = "cow";
                    break;

                case CatType.som:
                    SpriteCycle = "som";
                    break;

                case CatType.chok:
                    SpriteCycle = "chok";
                    break;

                case CatType.brown:
                    SpriteCycle = "brown";
                    break;

                case CatType.tiger:
                    SpriteCycle = "tiger";
                    break;

                case CatType.black:
                    SpriteCycle = "black";
                    break;
            }
        }

        public override object Clone()
        {
            return new Cat(this);
        }
    }
}
