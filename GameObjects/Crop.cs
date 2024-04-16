using Medicraft.Data.Models;
using Medicraft.Systems.Managers;
using Medicraft.Systems;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Sprites;
using System.Collections.Generic;

namespace Medicraft.GameObjects
{
    public class Crop : GameObject
    {
        public enum CropStages
        {
            Empty,
            Growing,
            Harvestable
        }
        private CropStages cropStage;
        public CropStages CropStage
        {
            get => cropStage;
            protected set
            {
                switch (value)
                {
                    case CropStages.Empty:
                        SpiteCycle = "crop_empty";
                        cropStage = CropStages.Empty;
                        ObjectData.CropStage = 0;                   
                        ObjectData.GrowingTimer = 0;
                        ObjectData.ReferId = -1;
                        break;

                    case CropStages.Growing:
                        SpiteCycle = "crop_growing_0";
                        cropStage = CropStages.Growing;
                        GrowingTimer = GrowingTime;
                        ObjectData.CropStage = 1;
                        ObjectData.GrowingTimer = GrowingTime;
                        break;

                    case CropStages.Harvestable:
                        SpiteCycle = ReferId.ToString();
                        cropStage = CropStages.Harvestable;
                        ObjectData.CropStage = 2;
                        ObjectData.GrowingTimer = 0;
                        break;
                }
            }
        }
        public float GrowingTime { get; private set; } = 60f;
        public float GrowingTimer { get; private set; } = 0;
        public int QuantityDrop { get; protected set; }
        public string SpiteCycle { get; private set; }

        public Crop(AnimatedSprite sprite, ObjectData objectData, Vector2 scale)
        {
            Sprite = sprite;
            ObjectData = objectData;

            InitializeObjectData();
            InitCropStage(objectData.CropStage);
            GrowingTimer = objectData.GrowingTimer;

            IsVisible = objectData.IsVisible;
            IsRespawnable = objectData.IsRespawnable;
            IsDetectable = true;
            IsDestroyable = false;

            var position = new Vector2((float)objectData.Position[0], (float)objectData.Position[1]);
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
            Sprite.Play(SpiteCycle);
        }

        private Crop(Crop crop)
        {
            Sprite = crop.Sprite;
            ObjectData = crop.ObjectData;

            ObjectType = crop.ObjectType;
            Id = crop.Id;
            ReferId = crop.ReferId;
            Name = crop.Name;
            Description = crop.Description;

            CropStage = crop.CropStage;
            GrowingTime = crop.GrowingTime;
            GrowingTimer = crop.GrowingTimer;

            IsDetectable = crop.IsDetectable;
            IsDestroyable = crop.IsDestroyed;
            IsVisible = crop.IsVisible;
            IsRespawnable = crop.IsRespawnable;
            IsCollected = false;
            IsDetected = false;

            Transform = new Transform2
            {
                Scale = crop.Transform.Scale,
                Rotation = crop.Transform.Rotation,
                Position = crop.Transform.Position
            };

            BoundingInteraction = crop.BoundingInteraction;

            ParticleEffect = crop.ParticleEffect;

            QuantityDrop = crop.QuantityDrop;

            Sprite.Depth = InitDepth;
            Sprite.Play(SpiteCycle);
        }

        public override void Update(GameTime gameTime, float layerDepth)
        {
            var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            UpdateCropTimer(gameTime);

            Sprite.Depth = layerDepth;
            var playerDepth = PlayerManager.Instance.Player.GetDepth();

            UpdateLayerDepth(layerDepth, playerDepth);
            UpdateParticleLayerDepth(deltaSeconds, Sprite.Depth);

            // When Detected
            var hotbarItem = PlayerManager.Instance.SelectedHotbarItem;
            if (IsDetected)
            {
                if (CropStage == CropStages.Harvestable)
                {
                    SignSprite.Play("collecting_1");
                }
                else if (CropStage == CropStages.Empty && hotbarItem.Value != null
                    && hotbarItem.Value.GetTag().Equals("plantable"))
                {
                    SignSprite.Play("interact_1");
                }
                else SignSprite.Play("blank");

                SignSprite.Depth = InitDepth;
                SignSprite.Update(deltaSeconds);
            }

            // When Collected
            if (CropStage == CropStages.Harvestable && IsCollected)
            {
                InventoryManager.Instance.AddItem(ReferId, QuantityDrop);
                CropStage = CropStages.Empty;
                QuantityDrop = 0;
            }

            Sprite.Play(SpiteCycle);
            Sprite.Update(deltaSeconds);
        }

        public void Cropping(KeyValuePair<int, InventoryItemData> hotbarItem)
        {
            if (hotbarItem.Value != null && hotbarItem.Value.GetTag().Equals("plantable") && hotbarItem.Value.Count != 0)
            {
                ObjectData.ReferId = ReferId = hotbarItem.Value.ItemId;
                QuantityDrop = GameGlobals.RandomItemQuantityDrop(hotbarItem.Value.ItemId);
                CropStage = CropStages.Growing;
                GrowingTimer = GrowingTime;
                IsCollected = false;

                // then remove item 1
                InventoryManager.Instance.RemoveItem(hotbarItem.Key, hotbarItem.Value, 1);
                
                UIManager.Instance.RefreshHotbar();
            }
            else return;
        }

        public void UpdateCropTimer(GameTime gameTime)
        {
            var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (GrowingTimer < 0 && CropStage == CropStages.Growing)
            {
                CropStage = CropStages.Harvestable;
            }
            else if (CropStage == CropStages.Growing)
            {
                // Update Time
                GrowingTimer -= deltaSeconds;
                ObjectData.GrowingTimer = GrowingTimer;

                if (GrowingTimer <= GrowingTime / 2)
                    SpiteCycle = "crop_growing_1";
            }
        }

        private void InitCropStage(int cropStage)
        {
            switch (cropStage)
            {
                case 0:
                    CropStage = CropStages.Empty;
                    break;

                case 1:
                    CropStage = CropStages.Growing;
                    break;

                case 2:
                    CropStage = CropStages.Harvestable;
                    break;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            var shadowTexture = GameGlobals.GetShadowTexture(GameGlobals.ShadowTextureName.shadow_1);

            DrawShadow(spriteBatch, shadowTexture);

            if (CropStage != CropStages.Growing)
                DrawDetectedSign(spriteBatch);
        }

        public void DrawGrowingTimer(SpriteBatch spriteBatch)
        {
            var text = $"{(int)GrowingTimer}";
            var textSize = GameGlobals.Instance.FontSensation.MeasureString(text);
            var position = Position - new Vector2(textSize.Width / 2
                , (textSize.Height / 2) + (Sprite.TextureRegion.Height / 1.5f));

            HUDSystem.DrawTextWithStroke(
                spriteBatch,
                GameGlobals.Instance.FontSensation,
                text,
                position,
                Color.White, 1f, Color.Black, 1);
        }

        public override object Clone()
        {
            return new Crop(this);
        }
    }
}
