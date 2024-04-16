using Medicraft.Data.Models;
using Medicraft.Systems;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Sprites;
using Medicraft.Systems.Managers;
using static Medicraft.Systems.GameGlobals;
using MonoGame.Extended.Particles;
using System.Xml.Linq;

namespace Medicraft.GameObjects
{
    public class RestPoint : GameObject
    {
        public RestPoint(AnimatedSprite sprite, ObjectData objectData, Vector2 scale)
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

        private RestPoint(RestPoint restPoint)
        {
            Sprite = restPoint.Sprite;
            ObjectData = restPoint.ObjectData;

            ObjectType = restPoint.ObjectType;
            Id = restPoint.Id;
            ReferId = restPoint.ReferId;
            Name = restPoint.Name;
            Description = restPoint.Description;

            IsVisible = restPoint.IsVisible;
            IsRespawnable = restPoint.IsRespawnable;
            IsDestroyable = restPoint.IsDestroyable;
            IsDetectable = restPoint.IsDetectable;
            IsDetected = false;

            Transform = new Transform2
            {
                Scale = restPoint.Transform.Scale,
                Rotation = restPoint.Transform.Rotation,
                Position = restPoint.Transform.Position
            };

            BoundingInteraction = restPoint.BoundingInteraction;

            ParticleEffect = restPoint.ParticleEffect;

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
                Sprite.Play("heart_2");

                SignSprite.Depth = InitDepth;
                SignSprite.Play("interact_1");
                SignSprite.Update(deltaSeconds);
            }
            else Sprite.Play("heart_1");

            Sprite.Update(deltaSeconds);
        }

        public void Rest()
        {
            ScreenManager.Instance.TranstisionToScreen(ScreenManager.GameScreen.None);

            PlayerManager.Instance.Player.RestoresHP(Name, PlayerManager.Instance.Player.BaseMaxHP, true);
            PlayerManager.Instance.Player.RestoresMana(Name, PlayerManager.Instance.Player.BaseMaxMana, true);

            foreach (var companion in PlayerManager.Instance.Companions)
            {
                if (!companion.IsDead)
                {
                    companion.RestoresHP(Name, companion.BaseMaxHP, true);
                    companion.RestoresMana(Name, companion.BaseMaxMana, true);
                }
                else companion.Revived();
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            DrawDetectedSign(spriteBatch);
        }

        public override object Clone()
        {
            return new RestPoint(this);
        }
    }
}
