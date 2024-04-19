using Medicraft.Data.Models;
using Medicraft.Systems;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Sprites;
using Medicraft.Systems.Managers;
using MonoGame.Extended.Particles;

namespace Medicraft.GameObjects
{
    public class QuestObject : GameObject
    {
        public QuestObject(AnimatedSprite sprite, ObjectData objectData, Vector2 scale)
        {
            Sprite = sprite;
            ObjectData = objectData;

            InitializeObjectData();

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

            BoundingInteraction = new CircleF(Position, 140);

            ParticleEffect = DrawEffectSystem.SetItemParticleEffect(ObjectType, Position);

            Sprite.Depth = InitDepth;
            Sprite.Play("saving_1");
        }

        private QuestObject(QuestObject questObject)
        {
            Sprite = questObject.Sprite;
            ObjectData = questObject.ObjectData;

            ObjectType = questObject.ObjectType;
            Id = questObject.Id;
            ReferId = questObject.ReferId;
            Name = questObject.Name;
            Description = questObject.Description;

            IsVisible = questObject.IsVisible;
            IsRespawnable = questObject.IsRespawnable;
            IsDestroyable = questObject.IsDestroyable;
            IsDetectable = questObject.IsDetectable;
            IsDetected = false;

            Transform = new Transform2
            {
                Scale = questObject.Transform.Scale,
                Rotation = questObject.Transform.Rotation,
                Position = questObject.Transform.Position
            };

            BoundingInteraction = questObject.BoundingInteraction;

            ParticleEffect = questObject.ParticleEffect;

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
                Sprite.Play("pin_2");

                if (ObjectData.IsAllowInteract)
                {
                    SignSprite.Depth = InitDepth;
                    SignSprite.Play("interact_1");
                    SignSprite.Update(deltaSeconds);
                } 
            }
            else Sprite.Play("pin_1");

            Sprite.Update(deltaSeconds);
        }

        public void Interact()
        {
            if (!ObjectData.IsAllowInteract) return;

            PlayerManager.Instance.OnInteractWithObject(new InteractingObjectEventArgs(this));
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!ObjectData.IsShowSignInteract)
            {
                if (ParticleEffect != null)
                    spriteBatch.Draw(ParticleEffect);
            }
            else
            {
                base.Draw(spriteBatch);

                DrawDetectedSign(spriteBatch);
            }
        }

        public override object Clone()
        {
            return new QuestObject(this);
        }
    }
}
