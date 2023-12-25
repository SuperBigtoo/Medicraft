using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Sprites;
using Microsoft.Xna.Framework.Input;
using System;
using Medicraft.Systems;
using Medicraft.Systems.PathFinding;

namespace Medicraft.Entities
{
    public class Entity : ICloneable
    {
        public int Id;
        public string Name;
        public int Level;
        public int EXP;
        public int ATK;
        public int HP;
        public float DEF_Percent;
        public float Crit_Percent;
        public float CritDMG_Percent;
        public int Speed;
        public float Evasion;

        public string SpriteName;
        public AnimatedSprite Sprite;
        public Transform2 Transform;
        public Vector2 Velocity;
        public Rectangle BoundingDetectCollisions; // For dectect collisions
        public double BoundingCollisionX, BoundingCollisionY;
        public CircleF BoundingHitBox;
        public CircleF BoundingDetection;
        public CircleF BoundingCollection;

        protected int currentNodeIndex = 0; // Index of the current node in the path
        protected AStar PathFinding;
        protected Vector2 InitPos;
        protected string CurrentAnimation;
        public float AggroTime;

        public Vector2 Position
        {
            get => Transform.Position;
            set
            {
                Transform.Position = value;
                BoundingDetectCollisions.X = (int)((int)value.X - Sprite.TextureRegion.Width / BoundingCollisionX);
                BoundingDetectCollisions.Y = (int)((int)value.Y + Sprite.TextureRegion.Height / BoundingCollisionY);
                
                if (Name.Equals("PlayerOne")) 
                {
                    BoundingHitBox.Center = value + new Vector2(0f, 32f);
                    BoundingDetection.Center = value + new Vector2(0f, 32f);
                    BoundingCollection.Center = value + new Vector2(0f, 64f);
                }
                else
                {
                    BoundingHitBox.Center = value;
                    BoundingDetection.Center = value;
                }
            }
        }

        public float AttackingTime { get; set; }
        public float StunTime { get; set; }
        public float DyingTime { get; set; }
        public bool IsKnockback { get; set; }
        public bool IsDying { get; set; }
        public bool IsDestroyed { get; set; }
        public bool IsMoving { get; set; }
        public bool IsAttacking { get; set; }
        public bool IsDetectCollistionObject { get; set; }

        protected Entity()
        {
            Id = 0;
            Name = string.Empty;
            Level = 0;
            EXP = 0;
            ATK = 0;
            HP = 0;
            DEF_Percent = 0;
            Crit_Percent = 0;
            CritDMG_Percent = 0;
            Speed = 0;
            Evasion = 0;
            Transform = new Transform2
            {
                Scale = Vector2.One,
                Rotation = 0f,
                Position = Vector2.Zero
            };
            Velocity = Vector2.Zero;
            AttackingTime = 0f;
            StunTime = 0f;
            DyingTime = 0f;
            IsKnockback = false;
            IsDying = false;
            IsDestroyed = false;
            IsMoving = false;
            IsAttacking = false;
            IsDetectCollistionObject = false;
        }

        private Entity(Entity entity)
        {
            Id = entity.Id;
            Name = entity.Name;
            Level = entity.Level;
            EXP = entity.EXP;
            ATK = entity.ATK;
            HP = entity.HP;
            DEF_Percent = entity.DEF_Percent;
            Crit_Percent = entity.Crit_Percent;
            CritDMG_Percent = entity.CritDMG_Percent;
            Speed = entity.Speed;
            Evasion = entity.Evasion;
            Transform = entity.Transform;
            Velocity = entity.Velocity;
            AttackingTime = entity.AttackingTime;
            StunTime = entity.StunTime;
            DyingTime = entity.DyingTime;
            IsKnockback = entity.IsKnockback;
            IsDying = entity.IsDying;
            IsDestroyed = entity.IsDestroyed;
            IsMoving = entity.IsMoving;
            IsAttacking = entity.IsAttacking;
            IsDetectCollistionObject = entity.IsDetectCollistionObject;
        }

        public virtual void Update(GameTime gameTime, KeyboardState keyboardCurrentState, KeyboardState keyboardPrevioseState
            , MouseState mouseCurrentState, MouseState mousePrevioseState, float depthFrontTile, float depthBehideTile) { }
        public virtual void Update(GameTime gameTime, float playerDepth, float depthFrontTile, float depthBehideTile) { }
        public virtual void Draw(SpriteBatch spriteBatch) { }
        public virtual void Destroy()
        {
            IsDestroyed = true;
        }

        // Knowing this that MovementControl, CombatControl and orther methods below this is for Mobs only

        protected void MovementControl(float deltaSeconds)
        {
            var walkSpeed = deltaSeconds * Speed;
            InitPos = Position;

            if (!IsAttacking) CurrentAnimation = SpriteName + "_walking";  // Idle

            // Aggro
            if (BoundingDetection.Intersects(PlayerManager.Instance.Player.BoundingHitBox))
            {
                AggroTime = 5f;
            }

            if (AggroTime > 0)
            {
                if (!IsKnockback && !IsAttacking)
                {
                    if (PathFinding.GetPath().Count != 0)
                    {
                        if (currentNodeIndex < PathFinding.GetPath().Count - 1)
                        {
                            // Calculate direction to the next node
                            var direction = new Vector2(PathFinding.GetPath()[currentNodeIndex + 1].col
                                - PathFinding.GetPath()[currentNodeIndex].col, PathFinding.GetPath()[currentNodeIndex + 1].row
                                - PathFinding.GetPath()[currentNodeIndex].row);
                            direction.Normalize();

                            // Move the character towards the next node
                            Position += direction * walkSpeed;

                            // Check Animation
                            if (Position.Y >= (PlayerManager.Instance.Player.Position.Y + 50f))
                            {
                                CurrentAnimation = SpriteName + "_walking";     // Up
                            }
                            if (Position.Y < (PlayerManager.Instance.Player.Position.Y - 30f))
                            {
                                CurrentAnimation = SpriteName + "_walking";     // Down
                            }
                            if (Position.X > (PlayerManager.Instance.Player.Position.X + 50f))
                            {
                                CurrentAnimation = SpriteName + "_walking";     // Left
                            }
                            if (Position.X < (PlayerManager.Instance.Player.Position.X - 50f))
                            {
                                CurrentAnimation = SpriteName + "_walking";     // Right
                            }

                            // Check if the character has reached the next node
                            if (Vector2.Distance(Position, new Vector2((PathFinding.GetPath()[currentNodeIndex + 1].col * 64) + 32
                                , (PathFinding.GetPath()[currentNodeIndex + 1].row * 64) + 32)) < 1f)
                            {
                                currentNodeIndex++;
                            }

                            //System.Diagnostics.Debug.WriteLine($"Pos Mob: {Position.X} {Position.Y}");
                            //System.Diagnostics.Debug.WriteLine($"Pos Node: {(_pathFinding.GetPath()[currentNodeIndex + 1].col * 64) + 32} {(_pathFinding.GetPath()[currentNodeIndex + 1].row * 64) + 32}");
                        }
                        else if (PathFinding.GetPath().Count <= 4)
                        {
                            if (Position.Y >= (PlayerManager.Instance.Player.Position.Y + 50f))
                            {
                                CurrentAnimation = SpriteName + "_walking";     // Up
                                Position -= new Vector2(0, walkSpeed);
                            }

                            if (Position.Y < (PlayerManager.Instance.Player.Position.Y - 30f))
                            {
                                CurrentAnimation = SpriteName + "_walking";     // Down
                                Position += new Vector2(0, walkSpeed);
                            }

                            if (Position.X > (PlayerManager.Instance.Player.Position.X + 50f))
                            {
                                CurrentAnimation = SpriteName + "_walking";     // Left
                                Position -= new Vector2(walkSpeed, 0);
                            }

                            if (Position.X < (PlayerManager.Instance.Player.Position.X - 50f))
                            {
                                CurrentAnimation = SpriteName + "_walking";     // Right
                                Position += new Vector2(walkSpeed, 0);
                            }
                        }
                    }
                }
                AggroTime -= deltaSeconds;
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
                    Position = InitPos;
                }
                else
                {
                    IsDetectCollistionObject = false;
                }
            }
        }

        protected void CombatControl(float deltaSeconds)
        {
            // Do Attack
            if (BoundingHitBox.Intersects(PlayerManager.Instance.Player.BoundingHitBox) && !IsAttacking)
            {
                CurrentAnimation = SpriteName + "_attacking";

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

        protected virtual void CheckAttackDetection()
        {
            if (BoundingHitBox.Intersects(PlayerManager.Instance.Player.BoundingHitBox))
            {
                if (PlayerManager.Instance.Player.HP > 0)
                {
                    int totalDamage = ATK;
                    totalDamage -= (int)(totalDamage * PlayerManager.Instance.Player.DEF_Percent);

                    PlayerManager.Instance.Player.HP -= totalDamage;
                }
            }
        }

        protected virtual void UpdateLayerDepth(float playerDepth, float depthFrontTile, float depthBehideTile)
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

        public virtual object Clone()
        {
            return new Entity(this);
        }
    }
}
