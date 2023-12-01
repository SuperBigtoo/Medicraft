using Medicraft.Data.Models;
using Medicraft.Systems;
using Medicraft.Systems.PathFinding;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Sprites;
using System;

namespace Medicraft.Entities
{
    public class Slime : Entity
    {
        private readonly EntityStats _entityStats;
        private enum SlimeColor
        {
            yellow,
            red,
            green,
            blue
        }

        private string _slimeColor;

        private AStar _pathFinding;

        private Vector2 _initPos;

        private string _currentAnimation;

        private float _aggroTime;

        public Slime(AnimatedSprite sprite, EntityStats entityStats, Vector2 scale)
        {
            _entityStats = entityStats;
            Id = entityStats.Id;
            Name = entityStats.Name;
            ATK = entityStats.ATK;
            HP = entityStats.HP;
            DEF_Percent = (float)entityStats.DEF_Percent;
            Speed = entityStats.Speed;
            Evasion = (float)entityStats.Evasion;
            Sprite = sprite;
            _aggroTime = 0f;

            var position = new Vector2((float)entityStats.Position[0], (float)entityStats.Position[1]);
            Transform = new Transform2
            {
                Scale = scale,
                Rotation = 0f,
                Position = position
            };

            BoundingCollisionX = 5.5;
            BoundingCollisionY = 5;
            BoundingDetectCollisions = new Rectangle((int)((int)Position.X - Sprite.TextureRegion.Width / BoundingCollisionX)
                , (int)((int)Position.Y + Sprite.TextureRegion.Height / BoundingCollisionY)
                , (int)(Sprite.TextureRegion.Width / 3), Sprite.TextureRegion.Height / 5);

            BoundingHitBox = new CircleF(Position, 20);

            // For Aggro
            BoundingDetection = new CircleF(Position, 150);

            RandomSlimeColor();

            Sprite.Depth = 0.3f;
            Sprite.Play(_slimeColor + "walking");
        }

        private Slime(Slime slime)
        {
            _entityStats = slime._entityStats;

            Id = _entityStats.Id;
            Name = _entityStats.Name;
            ATK = _entityStats.ATK;
            HP = _entityStats.HP;
            DEF_Percent = (float)_entityStats.DEF_Percent;
            Speed = _entityStats.Speed;
            Evasion = (float)_entityStats.Evasion;

            Sprite = slime.Sprite;

            _aggroTime = slime._aggroTime;

            Transform = new Transform2
            {
                Scale = slime.Transform.Scale,
                Rotation = slime.Transform.Rotation,
                Position = slime.Transform.Position,
            };

            BoundingCollisionX = 5.5;
            BoundingCollisionY = 5;
            BoundingDetectCollisions = slime.BoundingDetectCollisions;

            BoundingHitBox = slime.BoundingHitBox;

            BoundingDetection = slime.BoundingDetection;

            RandomSlimeColor();

            Sprite.Depth = 0.3f;
            Sprite.Play(_slimeColor + "walking");
        }

        public override object Clone()
        {
            return new Slime(this);
        }

        private void RandomSlimeColor()
        {
            Random random = new Random();
            Array slimeColors = Enum.GetValues(typeof(SlimeColor));
            int randomIndex = random.Next(slimeColors.Length);
            SlimeColor randomColor = (SlimeColor)slimeColors.GetValue(randomIndex);

            switch (randomColor)
            {
                case SlimeColor.yellow:
                    _slimeColor = "yellow_";
                    break;

                case SlimeColor.red:
                    _slimeColor = "red_";
                    break;

                case SlimeColor.green:
                    _slimeColor = "green_";
                    break;

                case SlimeColor.blue:
                    _slimeColor = "blue_";
                    break;
            }
        }

        // Update Slime
        public override void Update(GameTime gameTime, float playerDepth, float depthFrontTile, float depthBehideTile)
        {
            var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (!IsDying)
            {
                _pathFinding = new AStar((int)Position.X, (int)((int)Position.Y + Sprite.TextureRegion.Height / BoundingCollisionY));

                // Combat Control
                CombatControl(deltaSeconds);

                // MovementControl
                MovementControl(deltaSeconds);

                // Update layer depth
                UpdateLayerDepth(playerDepth, depthFrontTile, depthBehideTile);
            }
            else
            {
                // Dying time before destroy
                _currentAnimation = _slimeColor + "dying";

                if (DyingTime > 0)
                {
                    DyingTime -= deltaSeconds;
                }
                else
                {
                    HudSystem.AddFeed("herb_2");

                    if (PlayerManager.Instance.Inventory.ContainsKey("herb_2"))
                    {
                        PlayerManager.Instance.Inventory["herb_2"] += 1;
                    }

                    PlayerManager.Instance.Coin += 10;

                    Destroy();
                }
            }

            Sprite.Play(_currentAnimation);
            Sprite.Update(deltaSeconds);
        }

        // Draw Slime
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (GameGlobals.Instance.IsShowPath)
            {
                _pathFinding.Draw(spriteBatch);
            }

            spriteBatch.Draw(Sprite, Transform);

            // Test Draw BoundingRec for Collision
            if (GameGlobals.Instance.IsShowDetectBox)
            {
                Texture2D pixelTexture = new Texture2D(ScreenManager.Instance.GraphicsDevice, 1, 1);
                pixelTexture.SetData(new Color[] { Color.White });
                spriteBatch.Draw(pixelTexture, BoundingDetectCollisions, Color.Red);
            }
        }

        private int currentNodeIndex = 0; // Index of the current node in the path
        private void MovementControl(float deltaSeconds)
        {
            var walkSpeed = deltaSeconds * Speed;
            _initPos = Position;

            if (!IsAttacking) _currentAnimation = _slimeColor + "walking";

            // Aggro
            if (BoundingDetection.Intersects(PlayerManager.Instance.Player.BoundingHitBox))
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
                        else if (_pathFinding.GetPath().Count <= 4)
                        {
                            if (Position.Y >= (PlayerManager.Instance.Player.Position.Y + 50f))
                            {
                                _currentAnimation = _slimeColor + "walking";
                                Position -= new Vector2(0, walkSpeed);
                            }

                            if (Position.Y < (PlayerManager.Instance.Player.Position.Y - 30f))
                            {
                                _currentAnimation = _slimeColor + "walking";
                                Position += new Vector2(0, walkSpeed);

                            }

                            if (Position.X > (PlayerManager.Instance.Player.Position.X + 50f))
                            {
                                _currentAnimation = _slimeColor + "walking";
                                Position -= new Vector2(walkSpeed, 0);

                            }

                            if (Position.X < (PlayerManager.Instance.Player.Position.X - 50f))
                            {
                                _currentAnimation = _slimeColor + "walking";
                                Position += new Vector2(walkSpeed, 0);
                            }
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
                if (rect.Intersects(BoundingDetectCollisions))
                {
                    IsDetectCollistionObject = true;
                    Position = _initPos;
                }
                else
                {
                    IsDetectCollistionObject = false;
                }
            }
        }

        private void CombatControl(float deltaSeconds)
        {
            // Do Attack
            if (BoundingHitBox.Intersects(PlayerManager.Instance.Player.BoundingHitBox) && !IsAttacking)
            {
                _currentAnimation = _slimeColor + "attacking";

                IsAttacking = true;
                AttackingTime = 1f;
            }

            if (IsAttacking)
            {
                // Check attack timing
                if (AttackingTime > 0)
                {
                    AttackingTime -= deltaSeconds;
                }
                else
                {
                    // CheckAttackDetection
                    CheckAttackDetection();

                    IsAttacking = false;
                }
            }
        }

        private void CheckAttackDetection()
        {
            if (BoundingHitBox.Intersects(PlayerManager.Instance.Player.BoundingHitBox))
            {
                if (PlayerManager.Instance.Player.HP > 0)
                {
                    int totalDamage = ATK;
                    totalDamage -= (int)(totalDamage * PlayerManager.Instance.Player.DEF_Percent);

                    PlayerManager.Instance.Player.HP -= totalDamage;

                    if (PlayerManager.Instance.Player.HP <= 0)
                    {
                        PlayerManager.Instance.Player.HP = 100; // For testing
                    }
                }
            }
        }

        private void UpdateLayerDepth(float playerDepth, float depthFrontTile, float depthBehideTile)
        {
            // Detect for LayerDepth
            Sprite.Depth = depthFrontTile; // Default depth
            if (BoundingHitBox.Intersects(PlayerManager.Instance.Player.BoundingDetection))
            {
                if (Transform.Position.Y >= (PlayerManager.Instance.Player.Position.Y + 40f))
                {
                    Sprite.Depth = playerDepth - 0.1f; // In front Player
                }
                else
                {
                    Sprite.Depth = playerDepth + 0.1f; // Behide Player
                }
            }
            else
            {
                var ObjectOnLayer1 = GameGlobals.Instance.ObjectOnLayer1;
                foreach (var obj in ObjectOnLayer1)
                {
                    if (obj.Intersects(BoundingDetectCollisions))
                    {
                        Sprite.Depth = depthBehideTile;
                        break; // Exit the loop as soon as an intersection is found
                    }
                }

                var ObjectOnLayer2 = GameGlobals.Instance.ObjectOnLayer2;
                foreach (var obj in ObjectOnLayer2)
                {
                    if (obj.Intersects(BoundingDetectCollisions))
                    {
                        Sprite.Depth = depthBehideTile + 0.2f;
                        break; // Exit the loop as soon as an intersection is found
                    }
                }
            }
        }
    }
}
