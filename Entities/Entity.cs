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
using Medicraft.Data.Models;
using System.Linq;
using Medicraft.Systems.Managers;

namespace Medicraft.Entities
{
    public class Entity : ICloneable
    {
        public int Id { get; protected set; }
        public int CharId { get; protected set; }
        public string Name { get; protected set; }
        public int Level { get; set; }
        public int EXP { get; set; }

        // Character Stats
        public int ATK { get; set; }
        public int HP { get; set; }
        public int MaximumHP { get; set; }
        public float DEF_Percent { get; set; }
        public float Crit_Percent { get; set; }
        public float CritDMG_Percent { get; set; }
        public int Speed { get; set; }
        public float Evasion { get; set; }

        public AnimatedSprite Sprite { get; protected set; }
        protected string SpriteCycle = "default";
        protected string CurrentAnimation;

        public Transform2 Transform;
        public Vector2 Velocity, CombatNumVelocity;    
        public double BoundingCollisionX, BoundingCollisionY;
        public Rectangle BoundingDetectCollisions;      // For dectect collisions
        public CircleF BoundingHitBox;
        public CircleF BoundingDetectEntity;
        public CircleF BoundingCollection;
        public CircleF BoundingAggro;
        
        protected int currentNodeIndex = 0;     // Index of the current node in the path
        protected int stoppingNodeIndex = 1;    // Distance index from the last node in the path for entity to stop moving
        protected AStar PathFinding;
        protected Vector2 InitPos;      

        protected Vector2 targetNode;
        protected float nextNodeTime;
        protected float nextNodeTimer;

        protected float NodeCycleTime
        {
            get => nextNodeTime;
            set
            {
                nextNodeTime = value;
                nextNodeTimer = value;
            }
        }

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

        public enum EntityTypes
        {
            Playable,
            Friendly,
            Hostile,
            Boss
        }
        
        public EntityTypes EntityType { get; protected set; }

        public enum PathFindingTypes
        {
            RoutePoint,
            RandomPoint,
            StationaryPoint
        }

        public PathFindingTypes PathFindingType { get; protected set; }

        // Timer Conditions
        public float AggroTime { get; protected set; }
        public float AggroTimer { get; set; }
        public float ActionTimer { get; set; }
        public float AttackedTimer { get; set; }     // Also used in positioning DrawCombatNumbers
        public float StunningTimer { get; set; }
        public float KnockbackedTimer { get; set; }
        public float DyingTime { get; protected set; }
        public float DyingTimer { get; set; }

        protected float AttackSpeed { get; set; }
        protected float CooldownAttack { get; set; }
        protected float CooldownAttackTimer { get; set; }
        protected bool IsAttackCooldown { get; set; }

        // Damage && Buff Numbers
        public int CombatNumCase { get; set; }
        public float CombatNumScale { get; set; }

        // Boolean
        public bool IsAggroResettable { get; protected set; }
        public bool IsAggro { get; set; }
        public bool IsStunable { get; protected set; }
        public bool IsStunning { get; set; }
        public bool IsKnockbackable { get; protected set; }
        public bool IsKnockback { get; set; }
        public bool IsDying { get; set; }
        public bool IsDestroyed { get; set; }
        public bool IsMoving { get; set; }
        public bool IsAttacking { get; set; }
        public bool IsAttacked { get; set; }
        public bool IsDetectCollistionObject { get; set; }
        public List<CombatNumberData> CombatLogs { get; protected set; }

        protected Entity()
        {
            Id = 0;
            Name = string.Empty;
            Level = 0;
            EXP = 0;
  
            Transform = new Transform2
            {
                Scale = Vector2.One,
                Rotation = 0f,
                Position = Vector2.Zero
            };

            Velocity = Vector2.Zero;
            CombatNumVelocity = Vector2.Zero;

            AggroTimer = 0f;
            ActionTimer = 0f;
            AttackedTimer = 0f;
            StunningTimer = 0f;
            KnockbackedTimer = 0f;
            DyingTimer = 0f;

            AttackSpeed = 1f;
            CooldownAttack = 0.4f;
            CooldownAttackTimer = CooldownAttack;
            IsAttackCooldown = false;

            CombatNumScale = 2f;

            IsStunable = true;
            IsStunning = false;
            IsAggroResettable = true;
            IsAggro = false;
            IsKnockbackable = true;
            IsKnockback = false;
            IsDying = false;
            IsDestroyed = false;
            IsMoving = false;
            IsAttacking = false;
            IsAttacked = false;
            IsDetectCollistionObject = false;

            CombatLogs = [];
        }

        private Entity(Entity entity)
        {
            Id = entity.Id;
            Name = entity.Name;
            Level = entity.Level;
            EXP = entity.EXP;

            Transform = new Transform2
            {
                Scale = Vector2.One,
                Rotation = 0f,
                Position = Vector2.Zero
            };

            Velocity = Vector2.Zero;
            CombatNumVelocity = Vector2.Zero;

            AggroTimer = entity.AggroTimer;
            ActionTimer = entity.ActionTimer;
            AttackedTimer = entity.AttackedTimer;
            StunningTimer = entity.StunningTimer;
            KnockbackedTimer = entity.KnockbackedTimer;
            DyingTimer = entity.DyingTimer;

            AttackSpeed = entity.AttackSpeed;
            CooldownAttack = entity.CooldownAttack;
            CooldownAttackTimer = CooldownAttack;
            IsAttackCooldown = entity.IsAttackCooldown;

            CombatNumScale = entity.CombatNumScale;

            IsStunable = entity.IsStunable;
            IsStunning = entity.IsStunning;
            IsAggroResettable = entity.IsAggroResettable;
            IsAggro = entity.IsAggro;
            IsKnockbackable = entity.IsKnockbackable;
            IsKnockback = entity.IsKnockback;
            IsDying = entity.IsDying;
            IsDestroyed = entity.IsDestroyed;
            IsMoving = entity.IsMoving;
            IsAttacking = entity.IsAttacking;
            IsAttacked = entity.IsAttacked;
            IsDetectCollistionObject = entity.IsDetectCollistionObject;

            CombatLogs = [];
        }

        public virtual void Update(GameTime gameTime, KeyboardState keyboardCurrentState, KeyboardState keyboardPrevioseState
            , MouseState mouseCurrentState, MouseState mousePrevioseState) { }
        public virtual void Update(GameTime gameTime, float playerDepth, float topDepth, float middleDepth, float bottomDepth) { }
        public virtual void Draw(SpriteBatch spriteBatch) { }
        public virtual void Destroy()
        {
            IsDestroyed = true;
        }

        public virtual Vector2 SetCombatNumDirection()
        {
            return CombatNumVelocity;
        }

        protected virtual void InitializeCharacterData(int charId, int level)
        {
            var charData = GameGlobals.Instance.CharacterDatas.Where(c => c.CharId.Equals(charId));

            CharId = charData.ElementAt(0).CharId;
            Name = charData.ElementAt(0).Name;

            SetEntityType(charData.ElementAt(0).Category);

            SetCharacterStats(charData.ElementAt(0), level);
        }

        protected virtual void SetEntityType(int category)
        {
            switch (category)
            {
                case 0:
                    EntityType = EntityTypes.Playable;
                    break;

                case 1:
                    EntityType = EntityTypes.Friendly;
                    break;

                case 2:
                    EntityType = EntityTypes.Hostile;
                    break;

                case 3:
                    EntityType = EntityTypes.Boss;
                    break;
            }
        }

        protected virtual void SetPathFindingType(int category)
        {
            switch (category)
            {
                case 0:
                    PathFindingType = PathFindingTypes.RoutePoint;
                    break;

                case 1:
                    PathFindingType = PathFindingTypes.RandomPoint;
                    break;

                case 2:
                    PathFindingType = PathFindingTypes.StationaryPoint;
                    break;
            }
        }

        protected virtual void SetCharacterStats(CharacterData charData, int level)
        {
            ATK = charData.ATK + ((level - 1) * 2);
            MaximumHP = charData.HP + ((level - 1) * 10);
            HP = MaximumHP;
            DEF_Percent = (float)charData.DEF_Percent;
            Crit_Percent = (float)charData.Crit_Percent;
            CritDMG_Percent = (float)charData.CritDMG_Percent;
            Speed = charData.Speed;
            Evasion = (float)charData.Evasion;
        }

        // Knowing this that MovementControl, CombatControl and some methods below this is for Mobs only
        protected void MovementControl(float deltaSeconds)
        {
            var walkSpeed = deltaSeconds * Speed;
            InitPos = Position;

            // Check Object Collsion
            CheckCollision();

            // Play Sprite: Idle 
            if (!IsAttacking || !IsMoving)
            {
                CurrentAnimation = SpriteCycle + "_walking";  // Idle
                Sprite.Play(CurrentAnimation);
            }

            // Check movement according to PathFinding
            if (!IsStunning && (!IsKnockback && !IsAttacking || !IsAttacked && !IsAttacking))
            {
                if (PathFinding.GetPath().Count != 0)
                {
                    if (currentNodeIndex < PathFinding.GetPath().Count - stoppingNodeIndex)
                    {
                        // Calculate direction to the next node
                        var direction = new Vector2(PathFinding.GetPath()[currentNodeIndex + 1].col
                            - PathFinding.GetPath()[currentNodeIndex].col, PathFinding.GetPath()[currentNodeIndex + 1].row
                            - PathFinding.GetPath()[currentNodeIndex].row);
                        direction.Normalize();

                        // Move the character towards the next node
                        Position += direction * walkSpeed;
                        IsMoving = true;

                        // Check Animation
                        if (Position.Y >= (PlayerManager.Instance.Player.Position.Y + 50f))
                        {
                            CurrentAnimation = SpriteCycle + "_walking";     // Up
                        }
                        if (Position.Y < (PlayerManager.Instance.Player.Position.Y - 30f))
                        {
                            CurrentAnimation = SpriteCycle + "_walking";     // Down
                        }
                        if (Position.X > (PlayerManager.Instance.Player.Position.X + 50f))
                        {
                            CurrentAnimation = SpriteCycle + "_walking";     // Left
                        }
                        if (Position.X < (PlayerManager.Instance.Player.Position.X - 50f))
                        {
                            CurrentAnimation = SpriteCycle + "_walking";     // Right
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
                    else IsMoving = false;
                }
            }
            else IsMoving = false;
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

        protected virtual void CheckAggro()
        {
            switch (EntityType)
            {
                case EntityTypes.Hostile:

                case EntityTypes.Boss:
                    // Setup Aggrotimer if detected player hit box
                    if (BoundingAggro.Intersects(PlayerManager.Instance.Player.BoundingHitBox))
                    {
                        AggroTimer = AggroTime;
                        IsAggro = true;
                    }
                    break;

                case EntityTypes.Friendly:

                case EntityTypes.Playable:
                    break;
            }
        }

        protected void SetTargetNode(float deltaSeconds)
        {
            nextNodeTimer += deltaSeconds;

            if (nextNodeTimer >= nextNodeTime)
            {
                switch (PathFindingType)
                {
                    case PathFindingTypes.RoutePoint:
                        break;

                    case PathFindingTypes.RandomPoint:
                        var random = new Random();

                        // Define the patrol area from rectangle
                        var rectangleX = 929;
                        var rectangleY = 351;
                        var rectangleWidth = 481;
                        var rectangleHeight = 289;

                        // Generate random X and Y within the rectangle
                        var randomX = random.Next(rectangleX, rectangleX + rectangleWidth);
                        var randomY = random.Next(rectangleY, rectangleY + rectangleHeight);

                        targetNode = new Vector2(randomX, randomY);
                        break;

                    case PathFindingTypes.StationaryPoint:
                        break;
                } 

                nextNodeTimer = 0f;
            }
        }

        protected virtual void SetPathFindingNode(int returnPosX, int returnPosY)
        {
            if (IsAggro)
            {
                PathFinding = new AStar(
                    BoundingDetectCollisions.Center.X,
                    BoundingDetectCollisions.Center.Y,
                    (int)PlayerManager.Instance.Player.Position.X,
                    (int)PlayerManager.Instance.Player.Position.Y + 75
                );
            }
            else
            {
                switch (PathFindingType)
                {
                    case PathFindingTypes.RoutePoint:

                    case PathFindingTypes.RandomPoint:
                        PathFinding = new AStar(
                            BoundingDetectCollisions.Center.X,
                            BoundingDetectCollisions.Center.Y,
                            (int)targetNode.X,
                            (int)targetNode.Y
                        );
                        break;

                    case PathFindingTypes.StationaryPoint:
                        PathFinding = new AStar(
                            BoundingDetectCollisions.Center.X,
                            BoundingDetectCollisions.Center.Y,
                            returnPosX,
                            returnPosY
                        );
                        break;
                }            
            }
        }

        protected virtual void UpdateTimerConditions(float deltaSeconds)
        {
            // Decreasing AggroTimer
            if (IsAggro && IsAggroResettable)
            {
                if (AggroTimer > 0)
                {
                    AggroTimer -= deltaSeconds;
                }
                else
                {
                    IsAggro = false;
                }
            }          

            // Decreasing KnockbackTimer
            if (IsKnockback)
            {
                if (IsKnockbackable) Position += Velocity * KnockbackedTimer;

                if (KnockbackedTimer > 0)
                {
                    KnockbackedTimer -= deltaSeconds;
                }
                else
                {
                    IsKnockback = false;                  
                    Velocity = Vector2.Zero;
                }
            }

            // Decreasing StunningTimer
            if (IsStunning)
            {
                if (StunningTimer > 0)
                {
                    StunningTimer -= deltaSeconds;
                }
                else
                {
                    IsStunning = false;
                }
            }

            // Decreasing AttackedTimer
            if (IsAttacked)
            {
                if (AttackedTimer < 1)
                {
                    foreach (var log in CombatLogs.Where(e => e.ElapsedTime < 1f))
                    {
                        log.ElapsedTime += deltaSeconds;
                        log.AlphaColor -= deltaSeconds * 0.4f;

                        if (log.ElapsedTime < 0.2f)
                        {                          
                            log.ScaleFont += deltaSeconds * 5f;
                        }
                        else if (log.ElapsedTime > 0.95f)
                        {
                            log.AlphaColor -= deltaSeconds * 5f;
                            log.ScaleFont -= deltaSeconds * 5f;

                            log.AlphaColor = log.AlphaColor > 0? log.AlphaColor : 0;
                            log.ScaleFont = log.ScaleFont > 0? log.ScaleFont: 0;
                        }
                    }

                    AttackedTimer += deltaSeconds;
                }
                else
                {
                    IsAttacked = false;
                    CombatNumVelocity = Vector2.Zero;
                }
            }
        }

        // Mostly this one for Hostile Mobs
        protected void CombatControl(float deltaSeconds)
        {
            // Do Attack
            if (BoundingDetectEntity.Intersects(PlayerManager.Instance.Player.BoundingHitBox) && !IsAttacking && !IsStunning)
            {
                CurrentAnimation = SpriteCycle + "_attacking";
                Sprite.Play(CurrentAnimation);

                IsAttacking = true;
                ActionTimer = AttackSpeed;
                CooldownAttackTimer = CooldownAttack;
            }

            if (IsAttacking)
            {
                // Check attack timing
                if (ActionTimer > 0)
                {
                    ActionTimer -= deltaSeconds;
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
                        if (CooldownAttackTimer > 0)
                        {
                            CooldownAttackTimer -= deltaSeconds;
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

        protected virtual void CheckAttackDetection()
        {
            if (BoundingDetectEntity.Intersects(PlayerManager.Instance.Player.BoundingHitBox))
            {
                if (PlayerManager.Instance.Player.HP > 0)
                {
                    int totalDamage = TotalDamage(ATK, PlayerManager.Instance.Player.DEF_Percent
                        , PlayerManager.Instance.Player.Evasion);

                    if (CombatNumCase != 3)
                    {
                        PlayerManager.Instance.Player.HP -= totalDamage;
                    }

                    var combatNumVelocity = PlayerManager.Instance.Player.SetCombatNumDirection();
                    PlayerManager.Instance.Player.AddCombatLogNumbers(Name, totalDamage.ToString(), CombatNumCase, combatNumVelocity);

                    PlayerManager.Instance.Player.IsAttacked = true;
                    PlayerManager.Instance.Player.AttackedTimer = 0f;
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
                CombatNumCase = 3;
            }
            else
            {
                // if Attacked
                CombatNumCase = 1;
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

        public virtual void AddCombatLogNumbers(string ActorName, string combatNumbers, int combatCase, Vector2 combatNumVelocity)
        {
            switch (combatCase)
            {
                case 0: // Damage Numbers Default
                    CombatLogs.Add(new CombatNumberData {
                        Actor = ActorName,
                        Action = CombatNumberData.ActionType.Attack,
                        Value = combatNumbers,
                        ElapsedTime = 0,
                        Velocity = combatNumVelocity,
                        OffSet = Vector2.Zero,
                        Color = Color.White,
                        AlphaColor = 1f,
                        ScaleFont = 0f
                    });
                    CombatNumScale = 2.1f;
                    break;

                case 1: // Damage Numbers Critical | Player Damage Taken
                    CombatLogs.Add(new CombatNumberData
                    {
                        Actor = ActorName,
                        Action = CombatNumberData.ActionType.Attack,
                        Value = combatNumbers,
                        ElapsedTime = 0,
                        Velocity = combatNumVelocity,
                        OffSet = Vector2.Zero,
                        Color = Color.Red,
                        AlphaColor = 1f,
                        ScaleFont = 0f
                    });
                    CombatNumScale = 2.5f;
                    break;

                case 2: // Buff & Healing Numbers
                    break;

                case 3: // Missed
                    CombatLogs.Add(new CombatNumberData
                    {
                        Actor = ActorName,
                        Action = CombatNumberData.ActionType.Attack,
                        Value = "Miss",
                        ElapsedTime = 0,
                        Velocity = combatNumVelocity,
                        OffSet = Vector2.Zero,
                        Color = Color.Yellow,
                        AlphaColor = 1f,
                        ScaleFont = 0f
                    });
                    CombatNumScale = 1.75f;
                    break;
            }
        }

        public virtual void DrawCombatNumbers(SpriteBatch spriteBatch, int combatCase)
        {
            if (CombatLogs.Count != 0)
            {
                var font = GameGlobals.Instance.FontTA16Bit;               

                foreach (var log in CombatLogs.Where(e => e.ElapsedTime < 1f))
                {
                    var offSet = log.OffSet;

                    if (log.ElapsedTime < 0.2f)
                    {
                        offSet = log.OffSet = log.Velocity * 1.5f * log.ElapsedTime;
                    }
                    else if (log.ElapsedTime >= 0.2f && log.ElapsedTime <= 0.95f)
                    {
                        offSet = log.OffSet = log.Velocity * 0.5f * log.ElapsedTime;
                    }
                    else if (log.ElapsedTime > 0.95f)
                    {
                        offSet = log.OffSet = log.Velocity * 0.5f * log.ElapsedTime;
                    }

                    Vector2 drawStringPosition = new Vector2(Position.X, Position.Y
                        - (log.Velocity.Y))
                        - offSet;

                    var textSize = font.MeasureString(log.Value);
                    drawStringPosition.X -= textSize.Width / 1.25f;

                    spriteBatch.DrawString(font, $"{log.Value}", drawStringPosition, log.Color * log.AlphaColor,
                        0f, Vector2.Zero, CombatNumScale * log.ScaleFont, SpriteEffects.None, 0f);
                }              
            }
        }

        public virtual object Clone()
        {
            return new Entity(this);
        }
    }
}
