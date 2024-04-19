using Medicraft.Data.Models;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Sprites;
using System.Collections.Generic;
using Medicraft.Systems.Managers;
using static Medicraft.Systems.GameGlobals;

namespace Medicraft.Entities.Mobs.Friendly
{
    public class Civilian : FriendlyMob
    {
        public Civilian(AnimatedSprite sprite, EntityData entityData, Vector2 scale)
        {
            Sprite = sprite;
            idleSpriteName = ["_idle"];
            EntityData = entityData;

            // Initialize Character Data
            Id = entityData.Id;             // Mot to be confuse with CharId
            Level = entityData.Level;
            InitializeCharacterData(entityData.CharId, Level);

            DialogData = entityData.DialogData;
            SetMobType(entityData.MobType);
            Name = entityData.Name;

            IsInteractable = entityData.IsInteractable;
            IsRespawnable = true;
            IsDestroyable = false;

            SetPathFindingType(entityData.PathFindingType);
            NodeCycleTime = entityData.NodeCycleTime;

            var position = new Vector2(
                (float)entityData.Position[0],
                (float)entityData.Position[1]);

            Transform = new Transform2
            {
                Scale = scale,
                Rotation = 0f,
                Position = position
            };

            BoundingCollisionX = 6;
            BoundingCollisionY = 3f;

            // Rec for check Collision
            BoundingDetectCollisions = new Rectangle(
                (int)((int)Position.X - Sprite.TextureRegion.Width / BoundingCollisionX),
                (int)((int)Position.Y + Sprite.TextureRegion.Height / BoundingCollisionY),
                (int)(Sprite.TextureRegion.Width / 2.5f),
                Sprite.TextureRegion.Height / 6);

            BoundingHitBox = new CircleF(Position, 32);
            BoundingDetectEntity = new CircleF(Position, 32);
            BoundingInteraction = new CircleF(Position, 50);

            Sprite.Depth = InitDepth;
            Sprite.Play(SpriteCycle + "_idle");
        }

        private Civilian(Civilian civilian)
        {
            Sprite = civilian.Sprite;
            idleSpriteName = civilian.idleSpriteName;
            EntityData = civilian.EntityData;

            EntityType = civilian.EntityType;
            Id = civilian.Id;
            Name = civilian.Name;
            ATK = civilian.ATK;
            MaxHP = civilian.MaxHP;
            HP = civilian.MaxHP;
            DEF = civilian.DEF;
            Speed = civilian.Speed;
            Evasion = civilian.Evasion;

            DialogData = civilian.DialogData;
            MobType = civilian.MobType;

            IsInteractable = civilian.IsInteractable;
            IsRespawnable = civilian.IsRespawnable;
            IsInteracting = false;
            IsDetected = false;

            PathFindingType = civilian.PathFindingType;
            NodeCycleTime = civilian.NodeCycleTime;

            var position = new Vector2(
                (float)civilian.EntityData.Position[0],
                (float)civilian.EntityData.Position[1]);

            Transform = new Transform2
            {
                Scale = civilian.Transform.Scale,
                Rotation = civilian.Transform.Rotation,
                Position = position
            };

            BoundingCollisionX = civilian.BoundingCollisionX;
            BoundingCollisionY = civilian.BoundingCollisionY;
            BoundingDetectCollisions = civilian.BoundingDetectCollisions;

            BoundingHitBox = civilian.BoundingHitBox;
            BoundingHitBox.Position = position;
            BoundingDetectEntity = civilian.BoundingDetectEntity;
            BoundingDetectEntity.Position = position;
            BoundingInteraction = civilian.BoundingInteraction;
            BoundingInteraction.Position = position;

            Sprite.Color = Color.White;
            Sprite.Depth = InitDepth;
            Sprite.Play(SpriteCycle + "_idle");
        }

        public override void Update(GameTime gameTime, float playerDepth, float topDepth, float middleDepth, float bottomDepth)
        {
            base.Update(gameTime, playerDepth, topDepth, middleDepth, bottomDepth);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            DrawDetectedSign(spriteBatch);
        }

        public override void DrawShadow(SpriteBatch spriteBatch, Texture2D shadowTexture)
        {
            var position = new Vector2(Position.X - shadowTexture.Width / 2f
                    , BoundingDetectCollisions.Bottom - Sprite.TextureRegion.Height / 9);

            spriteBatch.Draw(shadowTexture, position, null, Color.White
                , 0f, Vector2.Zero, 1f, SpriteEffects.None, Sprite.Depth + 0.0000025f);
        }

        public override object Clone()
        {
            return new Civilian(this);
        }
    }
}
