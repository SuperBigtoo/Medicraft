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
    public class Vendor : FriendlyMob
    {
        public List<InventoryItemData> TradingItems { get; private set; }

        public Vendor(AnimatedSprite sprite, EntityData entityData, Vector2 scale)
        {
            Sprite = sprite;
            idleSpriteName = ["_idle"];
            EntityData = entityData;

            // Initialize Character Data
            Id = entityData.Id;             // Mot to be confuse with CharId
            Level = entityData.Level;
            InitializeCharacterData(entityData.CharId, Level);

            // Vendor
            DialogData = entityData.DialogData;
            SetMobType(entityData.MobType);
            InitializeTradingItems(entityData.TradingItemsData);
            Name = entityData.Name;

            IsAllwaysShowSignSprite = true;
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
            BoundingCollisionY = 3;

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

        private Vendor(Vendor vendor)
        {
            Sprite = vendor.Sprite;
            idleSpriteName = vendor.idleSpriteName;
            EntityData = vendor.EntityData;

            EntityType = vendor.EntityType;
            Id = vendor.Id;
            Name = vendor.Name;
            ATK = vendor.ATK;
            MaxHP = vendor.MaxHP;
            HP = vendor.MaxHP;
            DEF = vendor.DEF;
            Speed = vendor.Speed;
            Evasion = vendor.Evasion;

            DialogData = vendor.DialogData;
            InitializeTradingItems(EntityData.TradingItemsData);
            MobType = vendor.MobType;

            IsAllwaysShowSignSprite = vendor.IsAllwaysShowSignSprite;
            IsInteractable = vendor.IsInteractable;
            IsRespawnable = vendor.IsRespawnable;
            IsInteracting = false;
            IsDetected = false;

            PathFindingType = vendor.PathFindingType;
            NodeCycleTime = vendor.NodeCycleTime;

            var position = new Vector2(
                (float)vendor.EntityData.Position[0],
                (float)vendor.EntityData.Position[1]);

            Transform = new Transform2
            {
                Scale = vendor.Transform.Scale,
                Rotation = vendor.Transform.Rotation,
                Position = position
            };

            BoundingCollisionX = vendor.BoundingCollisionX;
            BoundingCollisionY = vendor.BoundingCollisionY;
            BoundingDetectCollisions = vendor.BoundingDetectCollisions;

            BoundingHitBox = vendor.BoundingHitBox;
            BoundingHitBox.Position = position;
            BoundingDetectEntity = vendor.BoundingDetectEntity;
            BoundingDetectEntity.Position = position;
            BoundingInteraction = vendor.BoundingInteraction;
            BoundingInteraction.Position = position;

            Sprite.Color = Color.White;
            Sprite.Depth = InitDepth;
            Sprite.Play(SpriteCycle + "_idle");
        }

        public void InitializeTradingItems(List<InventoryItemData> tradingItems)
        {
            TradingItems = [];

            // Setup Item in Trading Item List
            foreach (var item in tradingItems)
                TradingItems.Add(item);
        }

        public override void Update(GameTime gameTime, float playerDepth, float topDepth, float middleDepth, float bottomDepth)
        {
            base.Update(gameTime, playerDepth, topDepth, middleDepth, bottomDepth);

            var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Update Sign
            if (IsDetected)
            {           
                SignSprite.Play("interact_1");              
            }
            else SignSprite.Play("trading_1");

            SignSprite.Depth = InitDepth;
            SignSprite.Update(deltaSeconds);
        }

        public void OpenTradingPanel()
        {
            // Toggle Pause PlayScreen
            Instance.IsGamePause = !Instance.IsGamePause;
            Instance.IsOpenGUI = !Instance.IsOpenGUI;

            // Toggle IsOpenTradingPanel      
            Instance.IsOpenTradingPanel = false;
            if (UIManager.Instance.CurrentUI.Equals(UIManager.TradingPanel))
            {
                UIManager.Instance.CurrentUI = UIManager.PlayScreen;
                Instance.IsRefreshPlayScreenUI = false;
                PlaySoundEffect(Sound.ItemPurchase1);
            }
            else
            {
                UIManager.Instance.SetInteractingVendor(this);
                UIManager.Instance.CurrentUI = UIManager.TradingPanel;
                PlaySoundEffect(Sound.ItemPurchase1);
            }
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
            return new Vendor(this);
        }
    }
}
