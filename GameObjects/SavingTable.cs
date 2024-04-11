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
    public class SavingTable : GameObject
    {
        public SavingTable(AnimatedSprite sprite, ObjectData objectData, Vector2 scale)
        {
            Sprite = sprite;
            ObjectData = objectData;

            InitializeObjectData();

            IsVisible = objectData.IsVisible;
            IsRespawnable = objectData.IsRespawnable;
            IsDestroyable = true;

            var position = new Vector2(
                (float)objectData.Position[0],
                (float)objectData.Position[1]);

            Transform = new Transform2
            {
                Scale = scale,
                Rotation = 0f,
                Position = position
            };

            BoundingInteraction = new CircleF(Position, 32);

            ParticleEffect = DrawEffectSystem.SetItemParticleEffect(ObjectType, Position);

            Sprite.Depth = InitDepth;
            Sprite.Play("saving_1");
        }

        private SavingTable(SavingTable savingTable)
        {
            Sprite = savingTable.Sprite;
            ObjectData = savingTable.ObjectData;

            ObjectType = savingTable.ObjectType;
            Id = savingTable.Id;
            ReferId = savingTable.ReferId;
            Name = savingTable.Name;
            Description = savingTable.Description;

            IsVisible = savingTable.IsVisible;
            IsRespawnable = savingTable.IsRespawnable;
            IsDestroyable = savingTable.IsDestroyable;
            IsDetected = false;

            Transform = new Transform2
            {
                Scale = savingTable.Transform.Scale,
                Rotation = savingTable.Transform.Rotation,
                Position = savingTable.Transform.Position
            };

            BoundingInteraction = savingTable.BoundingInteraction;

            ParticleEffect = savingTable.ParticleEffect;

            Sprite.Depth = InitDepth;
            Sprite.Play("saving_1");
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
                Sprite.Play("saving_2");

                SignSprite.Depth = InitDepth;
                SignSprite.Play("collecting_0");
                SignSprite.Update(deltaSeconds);
            }
            else Sprite.Play("saving_1");

            Sprite.Update(deltaSeconds);
        }

        public static void OpenSavingPanel()
        {
            // Toggle Pause PlayScreen
            Instance.IsGamePause = !Instance.IsGamePause;
            Instance.IsOpenGUI = !Instance.IsOpenGUI;

            // Toggle IsOpenSaveMenuPanel              
            Instance.IsOpenSaveMenuPanel = false;
            if (UIManager.Instance.CurrentUI.Equals(UIManager.SaveMenu))
            {
                UIManager.Instance.CurrentUI = UIManager.PlayScreen;
                Instance.IsRefreshPlayScreenUI = false;
                PlaySoundEffect(Sound.Cancel1);
            }
            else
            {
                UIManager.Instance.CurrentUI = UIManager.SaveMenu;
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
            return new SavingTable(this);
        }
    }
}
