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
    public class WarpPoint : GameObject
    {
        public AnimatedSprite MagicCircleEffect { get; protected set; }
        public string SpiteCycle { private set; get; }

        private const float _shiningTime = 3f;
        private float _shiningTimer = 0;
        private bool _isWarped = false;

        public WarpPoint(AnimatedSprite sprite, ObjectData objectData, Vector2 scale)
        {
            MagicCircleEffect = new AnimatedSprite(Instance.MagicCircleEffect);

            Sprite = sprite;
            ObjectData = objectData;

            InitializeObjectData();

            SpiteCycle = objectData.SpiteCycle;
            IsVisible = objectData.IsVisible;
            IsDetectable = true;
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

            BoundingInteraction = new CircleF(
                Position + new Vector2(0, 70), 40);

            ParticleEffect = DrawEffectSystem.SetItemParticleEffect(ObjectType, Position);

            Sprite.Depth = InitDepth;
            Sprite.Play(SpiteCycle + "_floating");
        }

        private WarpPoint(WarpPoint warpPoint)
        {
            MagicCircleEffect = warpPoint.MagicCircleEffect;

            Sprite = warpPoint.Sprite;
            ObjectData = warpPoint.ObjectData;

            SpiteCycle = warpPoint.SpiteCycle;
            ObjectType = warpPoint.ObjectType;
            Id = warpPoint.Id;
            ReferId = warpPoint.ReferId;
            Name = warpPoint.Name;
            Description = warpPoint.Description;

            IsDetectable = warpPoint.IsDetectable;
            IsVisible = warpPoint.IsVisible;
            IsRespawnable = warpPoint.IsRespawnable;
            IsDestroyable = warpPoint.IsDestroyable;
            IsDetected = false;

            Transform = new Transform2
            {
                Scale = warpPoint.Transform.Scale,
                Rotation = warpPoint.Transform.Rotation,
                Position = warpPoint.Transform.Position
            };

            BoundingInteraction = new CircleF(
                Position + new Vector2(0, 70), 40);

            ParticleEffect = warpPoint.ParticleEffect;

            Sprite.Depth = InitDepth;
            Sprite.Play(SpiteCycle + "_floating");
        }

        public override void Update(GameTime gameTime, float layerDepth)
        {
            var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Sprite.Depth = layerDepth;

            var playerDepth = PlayerManager.Instance.Player.GetDepth();

            UpdateLayerDepth(layerDepth, playerDepth);
            UpdateParticleLayerDepth(deltaSeconds, Sprite.Depth);
            MagicCircleEffect.Depth = Sprite.Depth + 0.000002f;

            // Update Sign
            if (IsDetected)
            {
                SignSprite.Depth = InitDepth;
                SignSprite.Play("interact_1");
                SignSprite.Update(deltaSeconds);
            }

            if (!_isWarped)
            {
                Sprite.Play(SpiteCycle + "_floating");
            }
            else
            {
                Sprite.Play(SpiteCycle + "_shining");

                _shiningTimer += deltaSeconds;

                if (_shiningTimer > _shiningTime)
                {
                    _shiningTimer = 0;
                    _isWarped = false;
                }
            }

            MagicCircleEffect.Play(SpiteCycle + "_circle");
            MagicCircleEffect.Update(deltaSeconds);
            Sprite.Update(deltaSeconds);
        }

        public void OpenWarpPointPanel()
        {           
            // Toggle Pause PlayScreen
            Instance.IsGamePause = !Instance.IsGamePause;
            Instance.IsOpenGUI = !Instance.IsOpenGUI;

            // Toggle IsOpenWarpPointPanel & refresh crafting item display       
            Instance.IsOpenWarpPointPanel = false;
            if (UIManager.Instance.CurrentUI.Equals(UIManager.WarpPointPanel))
            {
                UIManager.Instance.CurrentUI = UIManager.PlayScreen;
                Instance.IsRefreshPlayScreenUI = false;
                PlaySoundEffect(Sound.Cancel1);
            }
            else
            {
                _isWarped = true;
                UIManager.Instance.CurrentUI = UIManager.WarpPointPanel;
                PlaySoundEffect(Sound.Click1);
            }

            PlayerManager.Instance.OnInteractWithObject(new InteractingObjectEventArgs(this));
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            DrawDetectedSign(spriteBatch);

            var transform = new Transform2
            {
                Scale = Transform.Scale,
                Rotation = 0f,
                Position = Position + new Vector2(0, Sprite.TextureRegion.Height / 2.4f)
            };

            spriteBatch.Draw(MagicCircleEffect, transform);
        }

        public override object Clone()
        {
            return new WarpPoint(this);
        }
    }
}
