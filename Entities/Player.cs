using Medicraft.Data.Models;
using Medicraft.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Sprites;

namespace Medicraft.Entities
{
    public class Player : Entity
    {
        GraphicsDevice graphicsDevice;

        private PlayerStats _playerStats;

        private string _movementAnimation;
        private string _combatAnimation;

        private Vector2 _initPos, _initHudPos, _initCamPos;

        private bool _isAttack;
        private float _attackTime;
        private float _attackSpeed;
        private float _knockbackForce;

        public Player(AnimatedSprite sprite, PlayerStats _playerStats)
        {
            this.sprite = sprite;
            this._playerStats = _playerStats;
            _isAttack = false;
            _attackTime = 0f;
            _attackSpeed = 0.25f;  // Stat
            _knockbackForce = 50f;  // Stat

            IsDetectCollistionObject = false;

            Vector2 _position = new Vector2((float)_playerStats.Position[0], (float)_playerStats.Position[1]);
            Transform = new Transform2
            {
                Scale = Vector2.One,
                Rotation = 0f,
                Position = _position
            };

            BoundingRec = new Rectangle((int)Position.X - sprite.TextureRegion.Width / 5, (int)Position.Y + sprite.TextureRegion.Height / 3
                , (int)(sprite.TextureRegion.Width / 2.5), sprite.TextureRegion.Height / 6);
            BoundingCircle = new CircleF(Position, 47.5f);
            BoundingDetectEntity = new CircleF(Position, 80f);

            this.sprite.Depth = 0.3f;
            this.sprite.Play("idle");
        }

        // Update Player
        public override void Update(GameTime gameTime, float depthFrontTile, float depthBehideTile)
        {
            var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Update layer depth
            UpdateLayerDepth(depthFrontTile, depthBehideTile);

            // Movement Control
            MovementControl(deltaSeconds);

            // Combat Control
            CombatControl(deltaSeconds);

            sprite.Update(deltaSeconds);
        }

        // Draw Player
        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(sprite, Transform);

            // Test Draw BoundingRec for Collision
            Texture2D pixelTexture = new Texture2D(ScreenManager.Instance.GraphicsDevice, 1, 1);
            pixelTexture.SetData(new Color[] { Color.White });
            spriteBatch.Draw(pixelTexture, BoundingRec, Color.Red);
        }

        // Movement
        private void MovementControl(float deltaSeconds)
        {
            var walkSpeed = deltaSeconds * _playerStats.Speed;
            Velocity = Vector2.Zero;
            _initPos = Position;
            _initHudPos = Singleton.Instance.addingHudPos;
            _initCamPos = Singleton.Instance.addingCameraPos;                        
            _movementAnimation = "idle";

            Singleton.Instance.keyboardPreviose = Singleton.Instance.keyboardCurrent;
            Singleton.Instance.keyboardCurrent = Keyboard.GetState();
            var keyboardState = Singleton.Instance.keyboardCurrent;

            if (!_isAttack)
            {
                if (keyboardState.IsKeyDown(Keys.W) || keyboardState.IsKeyDown(Keys.Up))
                {
                    _movementAnimation = "walking";
                    Velocity -= Vector2.UnitY;
                }

                if (keyboardState.IsKeyDown(Keys.S) || keyboardState.IsKeyDown(Keys.Down))
                {
                    _movementAnimation = "walking";
                    Velocity += Vector2.UnitY;;
                }

                if (keyboardState.IsKeyDown(Keys.A) || keyboardState.IsKeyDown(Keys.Left))
                {
                    _movementAnimation = "walking";
                    Velocity -= Vector2.UnitX;
                }

                if (keyboardState.IsKeyDown(Keys.D) || keyboardState.IsKeyDown(Keys.Right))
                {
                    _movementAnimation = "walking";
                    Velocity += Vector2.UnitX;
                    //Position += new Vector2(walkSpeed, 0);
                    //Singleton.Instance.addingHudPos += new Vector2(walkSpeed, 0);
                    //Singleton.Instance.addingCameraPos += new Vector2(walkSpeed, 0);
                }

                var isPlayerMoving = Velocity != Vector2.Zero;
                if (isPlayerMoving)
                {
                    Velocity.Normalize();
                    Position += Velocity * walkSpeed;
                    Singleton.Instance.addingHudPos += Velocity * walkSpeed;
                    Singleton.Instance.addingCameraPos += Velocity * walkSpeed;
                }

                // Detect Object Collsion
                var ObjectOnTile = Singleton.Instance.CollistionObject;
                foreach (var rect in ObjectOnTile)
                {
                    if (rect.Intersects(BoundingRec))
                    {   
                        IsDetectCollistionObject = true;
                        Position = _initPos;
                        Singleton.Instance.addingHudPos = _initHudPos;
                        Singleton.Instance.addingCameraPos = _initCamPos;
                    }
                    else
                    {
                        IsDetectCollistionObject = false;
                    }
                }
            }

            sprite.Play(_movementAnimation);
        }

        // Combat
        private void CombatControl(float deltaSeconds)
        {
            Singleton.Instance.mousePreviose = Singleton.Instance.mouseCurrent;
            Singleton.Instance.mouseCurrent = Mouse.GetState();

            if (Singleton.Instance.mouseCurrent.LeftButton == ButtonState.Pressed
                    && Singleton.Instance.mousePreviose.LeftButton == ButtonState.Released && !_isAttack)
            {
                _isAttack = true;
                _attackTime = _attackSpeed;

                CheckAttackDetection();
            }

            if (_attackTime > 0)
            {
                _attackTime -= deltaSeconds;
            }
            else _isAttack = false;
        }

        private void CheckAttackDetection()
        {
            if (EntityManager.Instance.entities.Count != 0) 
            {
                foreach (Entity en in EntityManager.Instance.entities)
                {
                    if (BoundingDetectEntity.Intersects(en.BoundingCircle))
                    {
                        if (!en.IsDestroyed)
                        {
                            Vector2 knockBackDirection = en.Position - this.Position;
                            knockBackDirection.Normalize();

                            en.Velocity = knockBackDirection * _knockbackForce;
                            en.IsKnockback = true;
                            en.StunTime = 0.2f;                           
                        }
                    }
                }
            }
        }

        private void UpdateLayerDepth(float depthFrontTile, float depthBehideTile)
        {
            // Detect for LayerDepth
            var OnGroundObject = Singleton.Instance.OnGroundObject;
            sprite.Depth = depthFrontTile; // Default depth
            foreach (var obj in OnGroundObject)
            {
                if (obj.Intersects(BoundingRec))
                {
                    sprite.Depth = depthBehideTile;
                    break; // Exit the loop as soon as an intersection is found
                }
            }
        }

        public PlayerStats GetPlayerStats()
        {
            return _playerStats;
        }

        public float GetPlayerDepth()
        {
            return sprite.Depth;
        }
    }
}
