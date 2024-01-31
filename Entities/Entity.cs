using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Sprites;
using Microsoft.Xna.Framework.Input;
using System;
using Medicraft.Systems;
using Medicraft.Systems.PathFinding;
using System.Collections.Generic;

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
        public Vector2 Velocity, DamageNumVelocity;
        public Rectangle BoundingDetectCollisions; // For dectect collisions
        public double BoundingCollisionX, BoundingCollisionY;
        public CircleF BoundingHitBox;
        public CircleF BoundingDetectEntity;
        public CircleF BoundingCollection;
        public CircleF BoundingAggro;

        protected int currentNodeIndex = 0; // Index of the current node in the path
        protected AStar PathFinding;
        protected Vector2 InitPos;
        protected string CurrentAnimation;      

        public Vector2 Position
        {
            get => Transform.Position;
            set
            {
                Transform.Position = value;
                BoundingDetectCollisions.X = (int)((int)value.X - Sprite.TextureRegion.Width / BoundingCollisionX);
                BoundingDetectCollisions.Y = (int)((int)value.Y + Sprite.TextureRegion.Height / BoundingCollisionY);
                
                if (Name.Equals("Noah")) 
                {
                    BoundingHitBox.Center = value + new Vector2(0f, 32f);
                    BoundingDetectEntity.Center = value + new Vector2(0f, 32f);
                    BoundingCollection.Center = value + new Vector2(0f, 64f);
                }
                else
                {
                    BoundingHitBox.Center = value;
                    BoundingDetectEntity.Center = value;
                    BoundingAggro.Center = value;
                }
            }
        }

        public enum EntityType
        {
            Hostile,
            Friendly,
            Playable
        }
        
        public EntityType Type { get; protected set; }

        // Time Conditions
        public float AggroTime { get; set; }
        public float ActionTime { get; set; }
        public float AttackedTime { get; set; }
        public float KnockbackedTime { get; set; }
        public float DyingTime { get; set; }

        protected float AttackSpeed { get; set; }
        protected float CooldownAttack { get; set; }
        protected float AttackCooldownTime { get; set; }
        protected bool IsAttackCooldown { get; set; }

        public Color DamageNumColor { get; set; }
        public float DamageNumScale { get; set; }
        public float AlphaColor { get; set; }
        public float ScaleFont { get; set; }

        public bool IsKnockbackable { get; set; }
        public bool IsKnockback { get; set; }
        public bool IsDying { get; set; }
        public bool IsDestroyed { get; set; }
        public bool IsMoving { get; set; }
        public bool IsAttacking { get; set; }
        public bool IsAttacked { get; set; }
        public bool IsDetectCollistionObject { get; set; }
        public List<string> DamageNumbers { get; protected set; }

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
            DamageNumVelocity = Vector2.Zero;

            AggroTime = 0f;
            ActionTime = 0f;
            AttackedTime = 0f;
            KnockbackedTime = 0f;
            DyingTime = 0f;

            AttackSpeed = 1f;
            CooldownAttack = 0.4f;
            AttackCooldownTime = CooldownAttack;
            IsAttackCooldown = false;

            DamageNumColor = Color.White;
            DamageNumScale = 2f;
            AlphaColor = 1f;
            ScaleFont = 1f;

            IsKnockbackable = true;
            IsKnockback = false;
            IsDying = false;
            IsDestroyed = false;
            IsMoving = false;
            IsAttacking = false;
            IsAttacked = false;
            IsDetectCollistionObject = false;

            DamageNumbers = new List<string>();
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
            DamageNumVelocity = entity.DamageNumVelocity;
            AggroTime = entity.AggroTime;
            ActionTime = entity.ActionTime;
            AttackedTime = entity.AttackedTime;
            KnockbackedTime = entity.KnockbackedTime;
            DyingTime = entity.DyingTime;
            AttackSpeed = entity.AttackSpeed;
            CooldownAttack = entity.CooldownAttack;
            AttackCooldownTime = CooldownAttack;
            IsAttackCooldown = entity.IsAttackCooldown;
            DamageNumColor = entity.DamageNumColor;
            DamageNumScale = entity.DamageNumScale;
            AlphaColor = entity.AlphaColor;
            ScaleFont = entity.ScaleFont;
            IsKnockbackable = entity.IsKnockbackable;
            IsKnockback = entity.IsKnockback;
            IsDying = entity.IsDying;
            IsDestroyed = entity.IsDestroyed;
            IsMoving = entity.IsMoving;
            IsAttacking = entity.IsAttacking;
            IsAttacked = entity.IsAttacked;
            IsDetectCollistionObject = entity.IsDetectCollistionObject;
            DamageNumbers = entity.DamageNumbers;
        }

        public virtual void Update(GameTime gameTime, KeyboardState keyboardCurrentState, KeyboardState keyboardPrevioseState
            , MouseState mouseCurrentState, MouseState mousePrevioseState, float topDepth, float middleDepth, float bottomDepth) { }
        public virtual void Update(GameTime gameTime, float playerDepth, float topDepth, float middleDepth, float bottomDepth) { }
        public virtual void Draw(SpriteBatch spriteBatch) { }
        public virtual void Destroy()
        {
            IsDestroyed = true;
        }

        public virtual void SetDamageNumDirection() { }

        // Knowing this that MovementControl, CombatControl and some methods below this is for Mobs only

        protected void MovementControl(float deltaSeconds)
        {
            var walkSpeed = deltaSeconds * Speed;
            InitPos = Position;

            // Check Object Collsion
            CheckCollision();

            if (!IsAttacking)
            {
                CurrentAnimation = SpriteName + "_walking";  // Idle
                Sprite.Play(CurrentAnimation);
            }

            // Setup Aggro time if detected player hit box
            if (BoundingAggro.Intersects(PlayerManager.Instance.Player.BoundingHitBox))
            {
                AggroTime = 5f;
            }

            if (!PlayerManager.Instance.IsPlayerDead)
            {
                // Checking for movement
                if (!IsKnockback && !IsAttacking || !IsAttacked && !IsAttacking)
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

                            Sprite.Play(CurrentAnimation);

                            // Check if the character has reached the next node
                            if (Vector2.Distance(Position, new Vector2((PathFinding.GetPath()[currentNodeIndex + 1].col * 64) + 32
                                , (PathFinding.GetPath()[currentNodeIndex + 1].row * 64) + 32)) < 1f)
                            {
                                currentNodeIndex++;
                            }

                            //System.Diagnostics.Debug.WriteLine($"Pos Mob: {Position.X} {Position.Y}");          
                        }
                        //else if (PathFinding.GetPath().Count <= 1)
                        //{
                        //    if (Position.Y >= (PlayerManager.Instance.Player.Position.Y + 50f))
                        //    {
                        //        CurrentAnimation = SpriteName + "_walking";     // Up
                        //        Position -= new Vector2(0, walkSpeed);
                        //    }

                        //    if (Position.Y < (PlayerManager.Instance.Player.Position.Y - 30f))
                        //    {
                        //        CurrentAnimation = SpriteName + "_walking";     // Down
                        //        Position += new Vector2(0, walkSpeed);
                        //    }

                        //    if (Position.X > (PlayerManager.Instance.Player.Position.X + 50f))
                        //    {
                        //        CurrentAnimation = SpriteName + "_walking";     // Left
                        //        Position -= new Vector2(walkSpeed, 0);
                        //    }

                        //    if (Position.X < (PlayerManager.Instance.Player.Position.X - 50f))
                        //    {
                        //        CurrentAnimation = SpriteName + "_walking";     // Right
                        //        Position += new Vector2(walkSpeed, 0);
                        //    }
                        //}
                    }
                }
            } 
        }

        protected virtual void CheckCollision()
        {
            var ObjectOnTile = GameGlobals.Instance.CollistionObject;
            foreach (var rect in ObjectOnTile)
            {
                if (BoundingDetectCollisions.Intersects(rect))
                {
                    IsDetectCollistionObject = true;
                    Position = InitPos;
                    Velocity = Vector2.Zero;
                    break;
                }
                else
                {
                    IsDetectCollistionObject = false;
                }
            }
        }

        protected virtual void UpdateTimeConditions(float deltaSeconds)
        {
            // Decreasing Aggro time
            if (AggroTime > 0)
            {
                AggroTime -= deltaSeconds;
            }

            // Knockback
            if (IsKnockback)
            {
                if (IsKnockbackable) Position += Velocity * KnockbackedTime;

                if (KnockbackedTime > 0)
                {
                    KnockbackedTime -= deltaSeconds;
                }
                else
                {
                    IsKnockback = false;                  
                    Velocity = Vector2.Zero;
                }
            }

            // Attacked
            if (IsAttacked)
            {
                if (AttackedTime < 1)
                {
                    AttackedTime += deltaSeconds;
                    AlphaColor -= deltaSeconds * 0.5f;
                    ScaleFont -= deltaSeconds * 0.4f;
                }
                else
                {
                    IsAttacked = false;
                    AlphaColor = 1f;
                    ScaleFont = 1f;
                    DamageNumVelocity = Vector2.Zero;
                }
            }
        }

        protected void CombatControl(float deltaSeconds)
        {
            if (!PlayerManager.Instance.IsPlayerDead)
            {
                // Do Attack
                if (BoundingDetectEntity.Intersects(PlayerManager.Instance.Player.BoundingHitBox) && !IsAttacking)
                {
                    CurrentAnimation = SpriteName + "_attacking";
                    Sprite.Play(CurrentAnimation);

                    IsAttacking = true;
                    ActionTime = AttackSpeed;
                    AttackCooldownTime = CooldownAttack;
                }

                if (IsAttacking)
                {
                    // Check attack timing
                    if (ActionTime > 0)
                    {
                        ActionTime -= deltaSeconds;
                    }
                    else
                    {
                        if (!IsAttackCooldown)
                        {
                            CheckAttackDetection();
                            IsAttackCooldown = true;
                        }
                        else
                        {
                            if (AttackCooldownTime > 0)
                            {
                                AttackCooldownTime -= deltaSeconds;
                            }
                            else
                            {
                                IsAttacking = false;
                                IsAttackCooldown = false;
                            }
                        }
                    }
                }
            }
        }

        protected virtual void CheckAttackDetection()
        {
            if (BoundingDetectEntity.Intersects(PlayerManager.Instance.Player.BoundingHitBox))
            {
                if (PlayerManager.Instance.Player.HP > 0)
                {
                    int totalDamage = TotalDamage(ATK, PlayerManager.Instance.Player.DEF_Percent
                        , PlayerManager.Instance.Player.Evasion);

                    var isAttackMissed = false;

                    if (totalDamage == 0)
                    {
                        isAttackMissed = true;
                    }
                    else PlayerManager.Instance.Player.HP -= totalDamage;

                    PlayerManager.Instance.Player.SetDamageNumDirection();
                    PlayerManager.Instance.Player.AddDamageNumbers(totalDamage.ToString(), true, isAttackMissed);

                    PlayerManager.Instance.Player.IsAttacked = true;
                    PlayerManager.Instance.Player.AttackedTime = 0f;
                    PlayerManager.Instance.Player.AlphaColor = 1f;
                    PlayerManager.Instance.Player.ScaleFont = 1f;
                }
            }
        }

        protected virtual int TotalDamage(int ATK, float DefPercent, float EvasionPercent)
        {
            var random = new Random();

            // Default total damage
            int totalDamage = ATK;

            // Check evasion
            int evaChance = random.Next(1, 101);
            if (evaChance <= EvasionPercent * 100)
            {
                // if Attack Missed
                totalDamage = 0;
            }
            else
            {
                // if Attacked
                // Calculate DEF
                totalDamage -= (int)(totalDamage * DefPercent);
            }

            return totalDamage;
        }

        protected virtual void UpdateLayerDepth(float playerDepth, float topDepth, float middleDepth, float bottomDepth)
        {
            // Detect for LayerDepth
            Sprite.Depth = topDepth; // Default depth
            if (BoundingHitBox.Intersects(PlayerManager.Instance.Player.BoundingDetectEntity))
            {
                if (Transform.Position.Y >= (PlayerManager.Instance.Player.Position.Y + 60f))
                {
                    Sprite.Depth = playerDepth - 0.00001f; // In front Player
                }
                else
                {
                    Sprite.Depth = playerDepth + 0.00001f; // Behide Player
                }
            }

            var TopLayerObject = GameGlobals.Instance.TopLayerObject;
            foreach (var obj in TopLayerObject)
            {
                if (obj.Intersects(BoundingDetectCollisions))
                {
                    Sprite.Depth = middleDepth;
                    break; // Exit the loop as soon as an intersection is found
                }
            }

            var MiddleLayerObject = GameGlobals.Instance.MiddleLayerObject;
            foreach (var obj in MiddleLayerObject)
            {
                if (obj.Intersects(BoundingDetectCollisions))
                {
                    Sprite.Depth = bottomDepth;
                    break;
                }
            }

            var BottomLayerObject = GameGlobals.Instance.BottomLayerObject;
            foreach (var obj in BottomLayerObject)
            {
                if (obj.Intersects(BoundingDetectCollisions))
                {
                    Sprite.Depth = bottomDepth + 0.2f;
                    break;
                }
            }
        }

        public virtual void AddDamageNumbers(string damageNumbers, bool isCriticalAttack, bool isAttackMissed)
        {
            if (isAttackMissed)
            {
                DamageNumbers.Add("Miss");
                DamageNumColor = Color.Yellow;
                DamageNumScale = 2.15f;
            }
            else
            {
                DamageNumbers.Add(damageNumbers);

                if (isCriticalAttack)
                {
                    DamageNumColor = Color.Red;
                    DamageNumScale = 2.25f;
                }
                else
                {
                    DamageNumColor = Color.White;
                    DamageNumScale = 2f;
                }
            }
        }

        public virtual void DrawDamageNumbers(SpriteBatch spriteBatch)
        {
            if (DamageNumbers.Count != 0)
            {
                var n = DamageNumbers.Count - 1;

                var font = GameGlobals.Instance.FontTA16Bit;

                spriteBatch.DrawString(font, $"{DamageNumbers[n]}", (Position - DamageNumVelocity / 1.5f) - (DamageNumVelocity * AttackedTime)
                    , DamageNumColor * AlphaColor, 0f, Vector2.Zero, DamageNumScale * ScaleFont, SpriteEffects.None, 0f);
            }
        }

        public virtual object Clone()
        {
            return new Entity(this);
        }
    }
}
