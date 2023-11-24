using Medicraft.Data.Models;
using Medicraft.Systems;
using Medicraft.Systems.PathFinding;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.Tiled;
using System;
using System.IO;

namespace Medicraft.Entities
{
    public class Slime : Entity
    {
        private EntityStats _entityStats;

        private AStar _pathFinding;
        private int[] prevStart = new int[]
        {
            0, 0
        };

        private string _currentAnimation;

        private float _aggroTime;

        public Slime(AnimatedSprite _sprite, EntityStats _entityStats, Vector2 _scale)
        {
            this.sprite = _sprite;
            this._entityStats = _entityStats;
            _aggroTime = 0f;

            IsDetectCollistionObject = false;

            Vector2 _position = new Vector2((float)_entityStats.Position[0], (float)_entityStats.Position[1]);
            Transform = new Transform2
            {
                Scale = _scale,
                Rotation = 0f,
                Position = _position
            };

            BoundingRec = new Rectangle((int)Position.X - sprite.TextureRegion.Width / 5, (int)Position.Y + sprite.TextureRegion.Height / 3
                , (int)(sprite.TextureRegion.Width / 3), sprite.TextureRegion.Height / 6);
            BoundingCircle = new CircleF(Position, 30);
            BoundingDetectEntity = new CircleF(Position, 150);

            this.sprite.Depth = 0.3f;
            this.sprite.Play("idle");
        }

        // Update Slime
        public override void Update(GameTime gameTime, float playerDepth, float depthFrontTile, float depthBehideTile)
        {
            var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            _pathFinding = new AStar((int)Position.X, (int)Position.Y + sprite.TextureRegion.Height / 3);
            //prevStart[0] = _pathFinding.GetPath()[0].row;
            //prevStart[1] = _pathFinding.GetPath()[0].col;

            // MovementControl
            MovementControl(deltaSeconds);

            if (BoundingCircle.Intersects(PlayerManager.Instance.Player.BoundingCircle))
            {
                var animation = "dead";

                sprite.Play(animation);
            }

            // Update layer depth
            UpdateLayerDepth(playerDepth, depthFrontTile, depthBehideTile);

            // Combat Control
            CombatControl(deltaSeconds);

            sprite.Update(deltaSeconds);
        }

        // Draw Slime
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (GameGlobals.Instance.IsShowPath)
            {
                _pathFinding.Draw(spriteBatch);
            }

            spriteBatch.Draw(sprite, Transform);

            // Test Draw BoundingRec for Collision
            if (GameGlobals.Instance.IsShowDetectBox)
            {
                Texture2D pixelTexture = new Texture2D(ScreenManager.Instance.GraphicsDevice, 1, 1);
                pixelTexture.SetData(new Color[] { Color.White });
                spriteBatch.Draw(pixelTexture, BoundingRec, Color.Red);
            }
        }

        private int currentNodeIndex = 0; // Index of the current node in the path
        private void MovementControl(float deltaSeconds)
        {
            var walkSpeed = deltaSeconds * _entityStats.Speed;
            _currentAnimation = "idle";

            // Aggro
            if (BoundingDetectEntity.Intersects(PlayerManager.Instance.Player.BoundingCircle))
            {
                _aggroTime = 5f;
            }

            if (_aggroTime > 0)
            {
                if (!IsKnockback && !IsAttacking)
                {
                    if (_pathFinding.GetPath().Count != 0)
                    {
                        if (currentNodeIndex < _pathFinding.GetPath().Count - 1)
                        {
                            // Calculate direction to the next node
                            Vector2 direction = new Vector2(_pathFinding.GetPath()[currentNodeIndex + 1].col
                                - _pathFinding.GetPath()[currentNodeIndex].col
                                , _pathFinding.GetPath()[currentNodeIndex + 1].row
                                - _pathFinding.GetPath()[currentNodeIndex].row);
                            direction.Normalize();

                            // Move the character towards the next node
                            Position += direction * walkSpeed;

                            // Check if the character has reached the next node
                            if (Vector2.Distance(Position, new Vector2((_pathFinding.GetPath()[currentNodeIndex + 1].col * 64) + 32
                                , (_pathFinding.GetPath()[currentNodeIndex + 1].row * 64) + 32)) < 1f)
                            {
                                currentNodeIndex++;
                            }

                            //System.Diagnostics.Debug.WriteLine($"Pos Mob: {Position.X} {Position.Y}");
                            //System.Diagnostics.Debug.WriteLine($"Pos Node: {(_pathFinding.GetPath()[currentNodeIndex + 1].col * 64) + 32} {(_pathFinding.GetPath()[currentNodeIndex + 1].row * 64) + 32}");
                        }
                    }
                    else if (_pathFinding.GetPath().Count <= 4)
                    {
                        if (Position.Y >= (PlayerManager.Instance.Player.Position.Y + 50f))
                        {
                            _currentAnimation = "idle";
                            Position -= new Vector2(0, walkSpeed);
                        }

                        if (Position.Y < (PlayerManager.Instance.Player.Position.Y - 30f))
                        {
                            _currentAnimation = "idle";
                            Position += new Vector2(0, walkSpeed);

                        }

                        if (Position.X > (PlayerManager.Instance.Player.Position.X + 50f))
                        {
                            _currentAnimation = "idle";
                            Position -= new Vector2(walkSpeed, 0);

                        }

                        if (Position.X < (PlayerManager.Instance.Player.Position.X - 50f))
                        {
                            _currentAnimation = "idle";
                            Position += new Vector2(walkSpeed, 0);
                        }
                    }
                }
                _aggroTime -= deltaSeconds;
            }

            // Knockback
            if (IsKnockback)
            {
                Position += Velocity * StunTime;

                if (StunTime > 0)
                {
                    StunTime -= deltaSeconds;
                }
                else
                {
                    IsKnockback = false;
                    Velocity = Vector2.Zero;
                }
            }

            // Detect Object Collsion
            var ObjectOnTile = GameGlobals.Instance.CollistionObject;
            foreach (var rect in ObjectOnTile)
            {
                if (rect.Intersects(BoundingRec))
                {
                    IsDetectCollistionObject = true;

                    // Calculate the intersection depth
                    Rectangle intersection = Rectangle.Intersect(rect, BoundingRec);
                    Vector2 depth = new Vector2(intersection.Width, intersection.Height);

                    // Determine the direction of the collision
                    float absDepthX = Math.Abs(depth.X);
                    float absDepthY = Math.Abs(depth.Y);

                    if (absDepthX < absDepthY)
                    {
                        // Adjust the position horizontally
                        if (depth.X < 0)
                            Position += new Vector2(absDepthX, 0);
                        else
                            Position -= new Vector2(absDepthX, 0);
                    }
                    else
                    {
                        // Adjust the position vertically
                        if (depth.Y < 0)
                            Position += new Vector2(0, absDepthY);
                        else
                            Position -= new Vector2(0, absDepthY);
                    }
                }
                else
                {
                    IsDetectCollistionObject = false;
                }
            }

            sprite.Play(_currentAnimation);
        }

        private void CombatControl(float deltaSeconds)
        {

        }

        private void UpdateLayerDepth(float playerDepth, float depthFrontTile, float depthBehideTile)
        {
            // Detect for LayerDepth
            sprite.Depth = depthFrontTile; // Default depth
            if (BoundingCircle.Intersects(PlayerManager.Instance.Player.BoundingDetectEntity))
            {
                if (Transform.Position.Y >= (PlayerManager.Instance.Player.Position.Y + 40f))
                {
                    sprite.Depth = playerDepth - 0.1f; // In front Player
                }
                else
                {
                    sprite.Depth = playerDepth + 0.1f; // Behide Player
                }
            }
            else
            {
                var OnGroundObject = GameGlobals.Instance.OnGroundObject;
                foreach (var obj in OnGroundObject)
                {
                    if (obj.Intersects(BoundingRec))
                    {
                        sprite.Depth = depthBehideTile;
                        break; // Exit the loop as soon as an intersection is found
                    }
                }
            }
        }
    }
}
