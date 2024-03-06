using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
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
        public int EXP { get; set; }    // meant for playable
        public int EXPMaxCap { get; set; }     // meant for playable

        // Character Stats
        public int ATK { get; set; }
        public int HP { get; set; }
        public int MaxHP { get; set; }
        public float MaxMana { get; set; }
        public float Mana { get; set; }
        public float ManaRegenRate { get; set; }
        public float DEF { get; set; }
        public float Crit { get; set; }
        public float CritDMG { get; set; }
        public int Speed { get; set; }
        public float Evasion { get; set; }

        // Keeping the default stats values for Buffs and Debuffs 
        public int BaseATK { get; protected set; }
        public int BaseMaxHP { get; protected set; }
        public int BaseSpeed { get; protected set; }
        public float BaseMaxMana { get; protected set; }
        public float BaseManaRegenRate { get; protected set; }
        public float BaseDEF { get; protected set; }
        public float BaseCrit { get; protected set; }
        public float BaseCritDMG { get; protected set; }
        public float BaseEvasion { get; protected set; }

        public AnimatedSprite Sprite { get; protected set; }
        public string SpriteCycle { get; protected set; }
        public string CurrentAnimation { get; protected set; }
        
        public AnimatedSprite StatesSprite { get; set; }

        // Used Effect Name
        public string NormalHitEffectActivated { get; protected set; }
        public string NormalHitEffectAttacked { get; protected set; }
        public string NormalSkillEffectActivated { get; protected set; }
        public string NormalSkillEffectAttacked { get; protected set; }
        public string BurstSkillEffectActivated { get; protected set; }
        public string BurstSkillEffectAttacked { get; protected set; }
        public string PassiveSkillEffectActivated { get; protected set; }

        public Transform2 Transform { get; protected set; }
        public Vector2 Velocity, CombatNumVelocity;
        public float BoundingCollisionX, BoundingCollisionY;
        public RectangleF BoundingDetectCollisions;      // For dectect collisions
        public CircleF BoundingHitBox;
        public CircleF BoundingDetectEntity;
        public CircleF BoundingCollection;
        public CircleF BoundingAggro;

        protected int _currentNodeIndex = 0;     // Index of the current node in the path
        protected int _stoppingNodeIndex = 1;    // Distance index from the last node in the path for entity to stop moving
        protected AStar _pathFinding;
        protected Vector2 _initPos;

        protected Vector2 _targetNode;
        protected int _routeNodeIndex = 0;
        protected float _nextNodeTime;
        protected float _nextNodeTimer;

        protected float NodeCycleTime
        {
            get => _nextNodeTime;
            set
            {
                _nextNodeTime = value;
                _nextNodeTimer = value;
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
                    BoundingCollection.Center = value + new Vector2(0f, 60f);
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
        public float AggroDrawEffectTimer { get; set; }
        public float ActionTimer { get; set; }
        public float AttackedTimer { get; set; }
        public float CombatNumbersTimer { get; set; }
        public float StunningTimer { get; set; }
        public float KnockbackedTimer { get; set; }
        public float DyingTime { get; protected set; }
        public float DyingTimer { get; set; }
        public float BuffTimer { get; set; }
        public float DebuffTimer { get; set; }

        protected float _attackSpeed;
        protected float _cooldownAttack;
        protected float _cooldownAttackTimer;
        protected bool _isAttackCooldown;

        // Combat Nunber
        public int CombatNumCase { get; set; }

        // Boolean
        public bool IsRespawnable { get; protected set; }
        public bool IsAggroResettable { get; protected set; }
        public bool IsAggro { get; set; }
        public bool IsAggroEffectDraw { get; set; }
        public bool IsStunable { get; protected set; }
        public bool IsStunning { get; set; }
        public bool IsStunningEffectDraw { get; set; }
        public bool IsKnockbackable { get; protected set; }
        public bool IsKnockback { get; set; }
        public bool IsDying { get; set; }
        public bool IsDestroyed { get; set; }
        public bool IsMoving { get; set; }
        public bool IsAttacking { get; set; }
        public bool IsAttacked { get; set; }
        public bool IsBuffOn { get; set; }
        public bool IsDebuffOn { get; set; }
        public bool IsShowCombatNumbers { get; set; }
        public bool IsDetectCollistionObject { get; set; }
        public List<CombatNumberData> CombatLogs { get; protected set; }

        protected Entity()
        {
            Id = 0;
            Name = string.Empty;
            Level = 0;
            EXP = 0;

            BaseATK = 0;
            BaseMaxHP = 0;
            BaseMaxMana = 0;
            BaseManaRegenRate = 0;
            BaseDEF = 0;
            BaseCrit = 0;
            BaseCritDMG = 0;
            BaseEvasion = 0;
            BaseSpeed = 0;

            Transform = new Transform2
            {
                Scale = Vector2.One,
                Rotation = 0f,
                Position = Vector2.Zero
            };

            Velocity = Vector2.Zero;
            CombatNumVelocity = Vector2.Zero;
            SpriteCycle = "default";

            AggroDrawEffectTimer = 0f;
            AggroTimer = 0f;
            ActionTimer = 0f;
            AttackedTimer = 0f;
            StunningTimer = 0f;
            KnockbackedTimer = 0f;
            DyingTimer = 0f;
            CombatNumbersTimer = 0f;
            BuffTimer = 0f;
            DebuffTimer = 0f;

            _attackSpeed = 1f;
            _cooldownAttack = 0.4f;
            _cooldownAttackTimer = _cooldownAttack;
            _isAttackCooldown = false;

            IsRespawnable = true;
            IsStunable = true;
            IsStunning = false;
            IsStunningEffectDraw = false;
            IsAggroResettable = true;
            IsAggro = false;
            IsAggroEffectDraw = false;
            IsKnockbackable = true;
            IsKnockback = false;
            IsDying = false;
            IsDestroyed = false;
            IsMoving = false;
            IsAttacking = false;
            IsAttacked = false;
            IsBuffOn = false;
            IsDebuffOn = false;
            IsShowCombatNumbers = false;
            IsDetectCollistionObject = false;

            CombatLogs = [];
        }

        private Entity(Entity entity)
        {
            Id = entity.Id;
            Name = entity.Name;
            Level = entity.Level;
            EXP = entity.EXP;

            BaseATK = entity.BaseATK;
            BaseMaxHP = entity.BaseMaxHP;
            BaseMaxMana = entity.BaseMaxMana;
            BaseManaRegenRate = entity.BaseManaRegenRate;
            BaseDEF = entity.BaseDEF;
            BaseCrit = entity.BaseCrit;
            BaseCritDMG = entity.BaseCritDMG;
            BaseEvasion = entity.BaseEvasion;
            BaseSpeed = entity.BaseSpeed;

            Transform = new Transform2
            {
                Scale = Vector2.One,
                Rotation = 0f,
                Position = Vector2.Zero
            };

            Velocity = Vector2.Zero;
            CombatNumVelocity = Vector2.Zero;
            SpriteCycle = entity.SpriteCycle;

            AggroDrawEffectTimer = 0f;
            AggroTimer = 0f;
            ActionTimer = 0f;
            AttackedTimer = 0f;
            StunningTimer = 0f;
            KnockbackedTimer = 0f;
            DyingTimer = 0f;
            CombatNumbersTimer = 0f;
            BuffTimer = 0f;
            DebuffTimer = 0f;

            _attackSpeed = entity._attackSpeed;
            _cooldownAttack = entity._cooldownAttack;
            _cooldownAttackTimer = _cooldownAttack;
            _isAttackCooldown = entity._isAttackCooldown;

            IsRespawnable = entity.IsRespawnable;
            IsStunable = entity.IsStunable;
            IsStunning = false;
            IsStunningEffectDraw = false;
            IsAggroResettable = entity.IsAggroResettable;
            IsAggro = false;
            IsAggroEffectDraw = false;
            IsKnockbackable = entity.IsKnockbackable;
            IsKnockback = false;
            IsDying = false;
            IsDestroyed = false;
            IsMoving = false;
            IsAttacking = false;
            IsAttacked = false;
            IsBuffOn = false;
            IsDebuffOn = false;
            IsShowCombatNumbers = false;
            IsDetectCollistionObject = false;

            CombatLogs = [];
        }

        public virtual void Update(GameTime gameTime, KeyboardState keyboardCurrentState, KeyboardState keyboardPrevioseState
            , MouseState mouseCurrentState, MouseState mousePrevioseState) { }
        public virtual void Update(GameTime gameTime, float playerDepth, float topDepth, float middleDepth, float bottomDepth) { }
        public virtual void Draw(SpriteBatch spriteBatch) { }
        public virtual void DrawShadow(SpriteBatch spriteBatch, Texture2D shadowTexture) { }
        public virtual void Destroy()
        {
            IsDestroyed = true;
        }

        public virtual Vector2 SetCombatNumDirection()
        {
            return CombatNumVelocity;
        }

        protected virtual void SetupCharacterStats() { }

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
            MaxHP = (int)(charData.HP + ((level - 1) * (charData.HP * 0.1)));
            HP = MaxHP;
            DEF = (float)charData.DEF_Percent;
            Crit = (float)charData.Crit_Percent;
            CritDMG = (float)charData.CritDMG_Percent;
            Speed = charData.Speed;
            Evasion = (float)charData.Evasion;

            BaseATK = ATK;
            BaseMaxHP = MaxHP;
            BaseMaxMana = MaxMana;
            BaseManaRegenRate = ManaRegenRate;
            BaseDEF = DEF;
            BaseCrit = Crit;
            BaseCritDMG = CritDMG;
            BaseEvasion = Evasion;
            BaseSpeed = Speed;
        }

        // Knowing this MovementControl, CombatControl and some methods below this is for Mobs only
        protected void MovementControl(float deltaSeconds)
        {
            var walkSpeed = deltaSeconds * Speed;
            _initPos = Position;

            // Check Object Collsion
            CheckCollision();

            // Play Sprite: Idle 
            if (!IsAttacking && !IsMoving)
            {
                CurrentAnimation = SpriteCycle + "_walking";  // Idle
                Sprite.Play(CurrentAnimation);
            }

            // Check movement according to PathFinding
            if (!IsStunning && ((!IsKnockback && !IsAttacking) || (!IsAttacked && !IsAttacking)))
            {
                if (_pathFinding.GetPath().Count != 0)
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
                        if (Vector2.Distance(Position, new Vector2((_pathFinding.GetPath()[_currentNodeIndex + 1].col * 64) + 32
                            , (_pathFinding.GetPath()[_currentNodeIndex + 1].row * 64) + 32)) < 1f)
                        {
                            _currentNodeIndex++;
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
                    Position = _initPos;
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

        protected void UpdateTargetNode(float deltaSeconds, EntityData entityData)
        {
            _nextNodeTimer += deltaSeconds;

            if (_nextNodeTimer >= _nextNodeTime)
            {
                switch (PathFindingType)
                {
                    case PathFindingTypes.RoutePoint:
                        // Define the current route
                        var rountNodePoint = entityData.RoutePoint.ElementAt(_routeNodeIndex);
                        var position = new Vector2((float)rountNodePoint[0], (float)rountNodePoint[1]);

                        // Next route
                        _routeNodeIndex++;
                        if (_routeNodeIndex >= entityData.RoutePoint.Length)
                        {
                            _routeNodeIndex = 0;
                        }

                        _targetNode = position;
                        break;

                    case PathFindingTypes.RandomPoint:
                        var random = new Random();

                        // Define the patrol area from rectangle
                        var recArea = GameGlobals.Instance.MobPartrolArea.Where(b => b.Name.Equals(entityData.PartrolArea));
                        var rectangleX = (int)recArea.ElementAt(0).Bounds.X;
                        var rectangleY = (int)recArea.ElementAt(0).Bounds.Y;
                        var rectangleWidth = (int)recArea.ElementAt(0).Bounds.Width;
                        var rectangleHeight = (int)recArea.ElementAt(0).Bounds.Height;

                        // Generate random X and Y within the rectangle
                        var randomX = random.Next(rectangleX, rectangleX + rectangleWidth);
                        var randomY = random.Next(rectangleY, rectangleY + rectangleHeight);

                        _targetNode = new Vector2(randomX, randomY);
                        break;

                    case PathFindingTypes.StationaryPoint:
                        // Its Stationary so we do nothing here
                        break;
                }

                _nextNodeTimer = 0f;
            }
        }

        protected virtual void SetPathFindingNode(int returnPosX, int returnPosY)
        {
            if (IsAggro)
            {
                _pathFinding = new AStar(
                    (int)BoundingDetectCollisions.Center.X,
                    (int)BoundingDetectCollisions.Center.Y,
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
                        _pathFinding = new AStar(
                            (int)BoundingDetectCollisions.Center.X,
                            (int)BoundingDetectCollisions.Center.Y,
                            (int)_targetNode.X,
                            (int)_targetNode.Y
                        );
                        break;

                    case PathFindingTypes.StationaryPoint:
                        _pathFinding = new AStar(
                            (int)BoundingDetectCollisions.Center.X,
                            (int)BoundingDetectCollisions.Center.Y,
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
            if (IsAggro)
            {
                if (AggroTimer > 0f)
                {
                    AggroTimer -= deltaSeconds;
                }
                else if (IsAggroResettable)
                {
                    IsAggro = false;
                    IsAggroEffectDraw = false;
                }
                else IsAggroEffectDraw = false;

                if (AggroDrawEffectTimer > 0f) AggroDrawEffectTimer -= deltaSeconds;
            }

            // Decreasing KnockbackTimer
            if (IsKnockback && IsKnockbackable)
            {
                if (IsKnockbackable) Position += Velocity * KnockbackedTimer;

                if (KnockbackedTimer > 0f)
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
            if (IsStunning && IsStunable)
            {
                if (StunningTimer > 0f)
                {
                    StunningTimer -= deltaSeconds;
                }
                else
                {
                    IsStunning = false;
                    IsStunningEffectDraw = false;
                }
            }

            // Decreasing AttackedTimer
            if (IsAttacked)
            {
                if (AttackedTimer <= 1f)
                {
                    AttackedTimer += deltaSeconds;
                }
                else
                {
                    IsAttacked = false;
                    AttackedTimer = 0f;
                }
            }

            // Show Buff Timer
            if (IsBuffOn)
            {
                if (BuffTimer > 0f)
                {
                    BuffTimer -= deltaSeconds;
                }
                else
                {
                    IsBuffOn = false;
                }
            }

            // Show Debuff Timer
            if (IsDebuffOn)
            {
                if (DebuffTimer > 0f)
                {
                    DebuffTimer -= deltaSeconds;
                }
                else
                {
                    IsDebuffOn = false;
                }
            }

            // Show combat numbers timer
            if (IsShowCombatNumbers)
            {
                if (CombatNumbersTimer < 1f)
                {
                    foreach (var log in CombatLogs.Where(e => e.ElapsedTime < 1f))
                    {
                        log.ElapsedTime += deltaSeconds;
                        log.AlphaColor -= deltaSeconds * 0.06f;

                        if (log.ElapsedTime < 0.2f)
                        {
                            log.Scaling += deltaSeconds * 5f;
                        }
                        else if (log.ElapsedTime > 0.95f)
                        {
                            log.AlphaColor -= deltaSeconds * 5f;
                            log.Scaling -= deltaSeconds * 5f;

                            log.AlphaColor = log.AlphaColor > 0f ? log.AlphaColor : 0f;
                            log.Scaling = log.Scaling > 0f ? log.Scaling : 0f;
                        }
                    }
                }
                else
                {
                    IsShowCombatNumbers = false;
                    CombatNumVelocity = Vector2.Zero;
                }
            }
        }

        protected virtual void MinimumCapacity()
        {
            HP = Math.Max(0, Math.Min(HP, MaxHP));
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
                ActionTimer = _attackSpeed;
                _cooldownAttackTimer = _cooldownAttack;
            }

            if (IsAttacking)
            {
                // Delay attacking
                if (ActionTimer > 0)
                {
                    ActionTimer -= deltaSeconds;
                }
                else
                {
                    if (!_isAttackCooldown)
                    {
                        CheckAttackDetection();
                        _isAttackCooldown = true;
                    }
                    else
                    {
                        if (_cooldownAttackTimer > 0)
                        {
                            _cooldownAttackTimer -= deltaSeconds;
                        }
                        else
                        {
                            IsAttacking = false;
                            _isAttackCooldown = false;
                        }
                    }
                }
            }
        }

        protected virtual void CheckAttackDetection()
        {
            // Chedk Attacking Detec tion for Player
            if (BoundingDetectEntity.Intersects(PlayerManager.Instance.Player.BoundingHitBox))
            {
                if (PlayerManager.Instance.Player.HP > 0)
                {
                    int totalDamage = TotalDamage(ATK
                        , PlayerManager.Instance.Player.DEF
                        , PlayerManager.Instance.Player.Evasion);

                    var combatNumVelocity = PlayerManager.Instance.Player.SetCombatNumDirection();
                    PlayerManager.Instance.Player.AddCombatLogNumbers(Name
                        , totalDamage.ToString()
                        , CombatNumCase
                        , combatNumVelocity
                        , NormalHitEffectAttacked);

                    // In case the Attack doesn't Missed
                    if (CombatNumCase != 3)
                    {
                        // Player being hit by Mob
                        PlayerManager.Instance.Player.IsAttacked = true;

                        PlayerManager.Instance.Player.HP -= totalDamage;
                    }        
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
                if (Transform.Position.Y >= PlayerManager.Instance.Player.BoundingDetectCollisions.Center.Y)
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

        public virtual void AddCombatLogNumbers(string ActorName, string combatNumbers, int combatCase
            , Vector2 combatNumVelocity, string effectName)
        {
            IsShowCombatNumbers = true;
            CombatNumbersTimer = 0f;

            switch (combatCase)
            {
                case 0: // Damage Numbers Default
                    CombatLogs.Add(new CombatNumberData
                    {
                        Actor = ActorName,
                        Action = CombatNumberData.ActionType.Attack,
                        Value = combatNumbers,
                        EffectName = effectName,
                        IsEffectPlayed = false,
                        ElapsedTime = 0,
                        Velocity = combatNumVelocity,
                        OffSet = Vector2.Zero,
                        Color = Color.White,
                        StrokeColor = Color.Black,
                        Size = 2f,
                        StrokeSize = 1,
                        AlphaColor = 1f,
                        Scaling = 0f
                    });                  
                    break;

                case 1: // Damage Numbers Critical | Player taken damage too
                    CombatLogs.Add(new CombatNumberData
                    {
                        Actor = ActorName,
                        Action = CombatNumberData.ActionType.CriticalAttack,
                        Value = combatNumbers,
                        EffectName = effectName,
                        IsEffectPlayed = false,
                        ElapsedTime = 0,
                        Velocity = combatNumVelocity,
                        OffSet = Vector2.Zero,
                        Color = Color.Red,
                        StrokeColor = Color.Black,
                        Size = 2.2f,
                        StrokeSize = 1,
                        AlphaColor = 1f,                   
                        Scaling = 0f
                    });                
                    break;

                case 2: // Healing Numbers
                    CombatLogs.Add(new CombatNumberData
                    {
                        Actor = ActorName,
                        Action = CombatNumberData.ActionType.Recovery,
                        Value = combatNumbers,
                        EffectName = effectName,
                        IsEffectPlayed = false,
                        ElapsedTime = 0,
                        Velocity = combatNumVelocity + new Vector2(40, 0),
                        OffSet = Vector2.Zero,
                        Color = Color.LimeGreen,
                        StrokeColor = Color.Black,
                        Size = 2.1f,
                        StrokeSize = 1,
                        AlphaColor = 1f,                       
                        Scaling = 0f
                    });               
                    break;

                case 3: // Missed
                    CombatLogs.Add(new CombatNumberData
                    {
                        Actor = ActorName,
                        Action = CombatNumberData.ActionType.Missed,
                        Value = "Miss",
                        EffectName = "hit_effect_22",
                        IsEffectPlayed = false,
                        ElapsedTime = 0,
                        Velocity = combatNumVelocity,
                        OffSet = Vector2.Zero,
                        Color = Color.Yellow,
                        StrokeColor = Color.Black,
                        Size = 1.5f,
                        StrokeSize = 1,
                        AlphaColor = 1f,
                        Scaling = 0f
                    });                  
                    break;

                case 4: // Buffing
                    CombatLogs.Add(new CombatNumberData
                    {
                        Actor = ActorName,
                        Action = CombatNumberData.ActionType.Buff,
                        Value = combatNumbers,
                        EffectName = effectName,
                        IsEffectPlayed = false,
                        ElapsedTime = 0,
                        Velocity = combatNumVelocity,
                        OffSet = Vector2.Zero,
                        Color = Color.DodgerBlue,
                        StrokeColor = Color.Black,
                        Size = 1.7f,
                        StrokeSize = 1,
                        AlphaColor = 1f,
                        Scaling = 0f
                    });                 
                    break;

                case 5: // Mana Restores Numbers
                    CombatLogs.Add(new CombatNumberData
                    {
                        Actor = ActorName,
                        Action = CombatNumberData.ActionType.Recovery,
                        Value = combatNumbers,
                        EffectName = effectName,
                        IsEffectPlayed = false,
                        ElapsedTime = 0,
                        Velocity = combatNumVelocity + new Vector2(-60, 0),
                        OffSet = Vector2.Zero,
                        Color = Color.Aqua,
                        StrokeColor = Color.Black,
                        Size = 2.1f,
                        StrokeSize = 1,
                        AlphaColor = 1f,
                        Scaling = 0f
                    });
                    break;
            }
        }

        public virtual void DrawCombatNumbers(SpriteBatch spriteBatch)
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
                   
                    var text = $"{log.Value}";
                    var textSize = font.MeasureString(text);               

                    switch (log.Action)
                    {
                        case CombatNumberData.ActionType.Attack:
                            log.DrawStringPosition = new Vector2(Position.X - ((textSize.Width * 2f) / 2.2f)
                                , Position.Y - (log.Velocity.Y)) - offSet;
                            break;

                        case CombatNumberData.ActionType.CriticalAttack:
                            log.DrawStringPosition = new Vector2(Position.X - ((textSize.Width * 2.2f) / 2.2f)
                                , Position.Y - (log.Velocity.Y)) - offSet;
                            break;

                        case CombatNumberData.ActionType.Recovery:
                            log.DrawStringPosition = new Vector2(Position.X - ((textSize.Width * 2.1f) / 2.2f)
                                , Position.Y - (log.Velocity.Y)) - offSet;
                            break;

                        case CombatNumberData.ActionType.Missed:
                            log.DrawStringPosition = new Vector2(Position.X - ((textSize.Width * 1.5f) / 2.2f)
                                , Position.Y - (log.Velocity.Y)) - offSet;
                            break;

                        case CombatNumberData.ActionType.Buff:
                            log.DrawStringPosition = new Vector2(Position.X - ((textSize.Width * 1.7f) / 2.2f)
                                , Position.Y - (log.Velocity.Y)) - offSet;
                            break;
                    }

                    HUDSystem.DrawTextWithStroke(spriteBatch, font, text
                        , log.DrawStringPosition
                        , log.Color * log.AlphaColor
                        , log.Size * log.Scaling
                        , log.StrokeColor *= log.AlphaColor
                        , log.StrokeSize);
                }              
            }
        }

        public virtual object Clone()
        {
            return new Entity(this);
        }
    }
}
