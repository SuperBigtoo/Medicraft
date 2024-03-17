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
    public class Cat : Entity
    {
        public EntityData EntityData { get; private set; }
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

        private bool _isPlayIdle = false;

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
                Sprite.TextureRegion.Height / 6
            );

            BoundingHitBox = new CircleF(Position, 32);         // Circle for Entity to hit

            BoundingDetectEntity = new CircleF(Position, 32);   // Circle for check attacking      

            _pathFinding = new AStar(
                (int)BoundingDetectCollisions.Center.X,
                (int)BoundingDetectCollisions.Center.Y,
                (int)EntityData.Position[0],
                (int)EntityData.Position[1]
            );

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

            _pathFinding = new AStar(
                (int)BoundingDetectCollisions.Center.X,
                (int)BoundingDetectCollisions.Center.Y,
                (int)EntityData.Position[0],
                (int)EntityData.Position[1]
            );

            RandomCatType();

            Sprite.Color = Color.White;
            Sprite.Depth = 0.1f;
            Sprite.Play(SpriteCycle + "_idle_1");
        }

        // Update Slime
        public override void Update(GameTime gameTime, float playerDepth, float topDepth, float middleDepth, float bottomDepth)
        {
            var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            UpdateTargetNode(deltaSeconds, EntityData);

            // Setup PathFinding
            SetPathFindingNode((int)EntityData.Position[0], (int)EntityData.Position[1]);

            if (!PlayerManager.Instance.IsPlayerDead)
            {
                // MovementControl
                MovementControl(deltaSeconds);
            }

            // Update layer depth
            UpdateLayerDepth(playerDepth, topDepth, middleDepth, bottomDepth);

            // Update time conditions
            UpdateTimerConditions(deltaSeconds);

            Sprite.Update(deltaSeconds);
        }

        protected override void MovementControl(float deltaSeconds)
        {
            var walkSpeed = deltaSeconds * Speed;
            _initPos = Position;

            // Check Object Collsion
            CheckCollision();

            // Play Sprite: Idle 
            if (!IsMoving && !_isPlayIdle)
            {
                _isPlayIdle = true;
                double randomValue = new Random().NextDouble();
                string idle = (randomValue < 0.5) ? "_idle_1" : "_idle_2";
                CurrentAnimation = SpriteCycle + idle;
                Sprite.Play(CurrentAnimation);
            }

            // Check movement according to PathFinding
            if (ScreenManager.Instance.IsScreenLoaded)
            {
                if (_pathFinding.GetPath().Count != 0)
                {
                    if (_pathFinding.GetPath().Count > 2)
                    {
                        if (_currentNodeIndex < _pathFinding.GetPath().Count - _stoppingNodeIndex)
                        {
                            // Calculate direction to the next node
                            var direction = new Vector2(_pathFinding.GetPath()[_currentNodeIndex + 1].col
                                - _pathFinding.GetPath()[_currentNodeIndex].col, _pathFinding.GetPath()[_currentNodeIndex + 1].row
                                - _pathFinding.GetPath()[_currentNodeIndex].row);
                            direction.Normalize();

                            // Move the character towards the next node
                            Position += direction * walkSpeed;
                            IsMoving = true;
                            _isPlayIdle = false;

                            // Check Animation
                            if (direction.Y == -1)
                            {
                                CurrentAnimation = SpriteCycle + "_walking_up";     // Up
                            }
                            if (direction.Y == 1)
                            {
                                CurrentAnimation = SpriteCycle + "_walking_down";     // Down
                            }
                            if (direction.X == -1)
                            {
                                CurrentAnimation = SpriteCycle + "_walking_left";     // Left
                            }
                            if (direction.X == 1)
                            {
                                CurrentAnimation = SpriteCycle + "_walking_right";     // Right
                            }

                            Sprite.Play(CurrentAnimation);
                        }                  
                    }
                    else IsMoving = false;
                }
            }
            else IsMoving = false;
        }

        // Draw Slime
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (GameGlobals.Instance.IsShowPath)
            {
                _pathFinding.Draw(spriteBatch);
            }

            spriteBatch.Draw(Sprite, Transform);

            var shadowTexture = GameGlobals.Instance.GetShadowTexture(GameGlobals.ShadowTextureName.shadow_1);

            DrawShadow(spriteBatch, shadowTexture);

            // Test Draw BoundingRec for Collision
            if (GameGlobals.Instance.IsDebugMode)
            {
                var pixelTexture = new Texture2D(ScreenManager.Instance.GraphicsDevice, 1, 1);
                pixelTexture.SetData(new Color[] { Color.White });
                spriteBatch.Draw(pixelTexture, (Rectangle)BoundingDetectCollisions, Color.Red);
            }
        }

        public override void DrawShadow(SpriteBatch spriteBatch, Texture2D shadowTexture)
        {
            var position = new Vector2(Position.X - shadowTexture.Width / 2f
                    , BoundingDetectCollisions.Bottom - Sprite.TextureRegion.Height / 10);

            spriteBatch.Draw(shadowTexture, position, null, Color.White
                , 0f, Vector2.Zero, 1f, SpriteEffects.None, Sprite.Depth + 0.0000025f);
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
