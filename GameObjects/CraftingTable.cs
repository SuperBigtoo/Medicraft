using Medicraft.Data.Models;
using Medicraft.Systems;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Sprites;
using Medicraft.Systems.Managers;
using static Medicraft.Systems.GameGlobals;

namespace Medicraft.GameObjects
{
    public class CraftingTable : GameObject
    {
        public string CraftingType { get; private set; }

        private string _spriteDefault, _spriteDetected;

        public CraftingTable(AnimatedSprite sprite, ObjectData objectData, Vector2 scale)
        {
            Sprite = sprite;
            ObjectData = objectData;

            InitializeObjectData();
            SetCraftingType(objectData.CraftingType);

            IsVisible = objectData.IsVisible;
            IsRespawnable = objectData.IsRespawnable;
            IsDestroyable = true;
            IsDetectable = true;

            var position = new Vector2(
                (float)objectData.Position[0],
                (float)objectData.Position[1]);

            Transform = new Transform2
            {
                Scale = scale,
                Rotation = 0f,
                Position = position
            };

            BoundingInteraction = new CircleF(Position, 40);

            ParticleEffect = DrawEffectSystem.SetItemParticleEffect(ObjectType, Position);

            Sprite.Depth = InitDepth;
            Sprite.Play(_spriteDefault);
        }

        private CraftingTable(CraftingTable craftingTable)
        {
            Sprite = craftingTable.Sprite;
            ObjectData = craftingTable.ObjectData;

            ObjectType = craftingTable.ObjectType;
            Id = craftingTable.Id;
            ReferId = craftingTable.ReferId;
            Name = craftingTable.Name;
            Description = craftingTable.Description;

            CraftingType = craftingTable.CraftingType;
            _spriteDefault = craftingTable._spriteDefault;
            _spriteDetected = craftingTable._spriteDetected;

            IsDetectable = craftingTable.IsDetectable;
            IsVisible = craftingTable.IsVisible;
            IsRespawnable = craftingTable.IsRespawnable;
            IsDestroyable = craftingTable.IsDestroyable;
            IsDetected = false;

            Transform = new Transform2
            {
                Scale = craftingTable.Transform.Scale,
                Rotation = craftingTable.Transform.Rotation,
                Position = craftingTable.Transform.Position
            };

            BoundingInteraction = craftingTable.BoundingInteraction;

            ParticleEffect = craftingTable.ParticleEffect;

            Sprite.Depth = InitDepth;
            Sprite.Play(_spriteDefault);
        }

        public override void Update(GameTime gameTime, float layerDepth)
        {
            var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Update layer for Particle
            var layerParticla = PlayerManager.Instance.Player.GetDepth();
            if (BoundingInteraction.Intersects(PlayerManager.Instance.Player.BoundingDetectEntity))
            {
                if (Transform.Position.Y >= PlayerManager.Instance.Player.BoundingDetectCollisions.Center.Y)
                {
                    layerParticla -= 0.00002f; // In front Player
                }
                else layerParticla += 0.00002f; // Behide Player
            }
            UpdateParticleLayerDepth(deltaSeconds, layerParticla);

            // Update Sign
            if (IsDetected)
            {
                Sprite.Play(_spriteDetected);

                SignSprite.Depth = InitDepth;
                SignSprite.Play("interact_1");
                SignSprite.Update(deltaSeconds);
            }
            else Sprite.Play(_spriteDefault);

            Sprite.Update(deltaSeconds);
        }

        private void SetCraftingType(int craftingType)
        {
            switch (craftingType)
            {
                case 0:
                    CraftingType = UIManager.ConsumableItem;
                    _spriteDefault = "cooking_1";
                    _spriteDetected = "cooking_2";
                    break;

                case 1:
                    CraftingType = UIManager.Equipment;
                    _spriteDefault = "crafting_1";
                    _spriteDetected = "crafting_2";
                    break;

                case 2:
                    CraftingType = UIManager.ThaiTraditionalMedicine;
                    _spriteDefault = "crafting_potion_1";
                    _spriteDetected = "crafting_potion_2";
                    break;
            }
        }

        public void OpenCraftingPanel()
        {
            // Toggle Pause PlayScreen
            Instance.IsGamePause = !Instance.IsGamePause;
            Instance.IsOpenGUI = !Instance.IsOpenGUI;

            // Toggle IsOpenCraftingPanel & refresh crafting item display       
            Instance.IsOpenCraftingPanel = false;
            if (UIManager.Instance.CurrentUI.Equals(UIManager.CraftingPanel))
            {
                UIManager.Instance.CurrentUI = UIManager.PlayScreen;
                Instance.IsRefreshPlayScreenUI = false;
                PlaySoundEffect(Sound.Cancel1);
            }
            else
            {
                UIManager.Instance.CurrentCraftingList = CraftingType;
                UIManager.Instance.CurrentUI = UIManager.CraftingPanel;
                PlaySoundEffect(Sound.Click1);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            DrawDetectedSign(spriteBatch);
        }

        public override object Clone()
        {
            return new CraftingTable(this);
        }
    }
}
