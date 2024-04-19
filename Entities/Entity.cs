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
using static Medicraft.Systems.GameGlobals;

namespace Medicraft.Entities
{
    public class Entity : ICloneable
    {
        public int Id { get; protected set; }
        public int CharId { get; protected set; }
        public string Name { get; protected set; }
        public int Level { get; set; }
        public int EXP { get; set; }            // meant for playable
        public int EXPMaxCap { get; set; }     // meant for playable

        // Character Stats
        public const float InitHPRegenRate = 0.25f;
        public const float InitManaRegenRate = 0.50f;

        public float ATK { get; set; }
        public float HP { get; set; }
        public float MaxHP { get; set; }
        public float MaxMana { get; set; }
        public float Mana { get; set; }
        public float ManaRegenRate { get; set; }
        public float DEF { get; set; }
        public float Crit { get; set; }
        public float CritDMG { get; set; }
        public int Speed { get; set; }
        public float Evasion { get; set; }

        // Keeping the default stats values for Buffs and Debuffs 
        public float BaseATK { get; protected set; }
        public float BaseMaxHP { get; protected set; }
        public int BaseSpeed { get; protected set; }
        public float BaseMaxMana { get; protected set; }
        public float BaseManaRegenRate { get; protected set; }
        public float BaseDEF { get; protected set; }
        public float BaseCrit { get; protected set; }
        public float BaseCritDMG { get; protected set; }
        public float BaseEvasion { get; protected set; }

        public const float InitDepth = 0.1f;

        public AnimatedSprite Sprite { get; protected set; }
        public string SpriteCycle { get; protected set; } = "default";
        public string CurrentAnimation { get; protected set; }
        
        public AnimatedSprite StatesSprite { get; set; }
        public AnimatedSprite BuffSprite { get; set; }
        public AnimatedSprite DebuffSprite { get; set; }

        // Used Effect Name
        public string NormalHitEffectActivated { get; protected set; }
        public string NormalHitEffectAttacked { get; protected set; }
        public string NormalSkillEffectActivated { get; protected set; }
        public string NormalSkillEffectAttacked { get; protected set; }
        public string BurstSkillEffectActivated { get; protected set; }
        public string BurstSkillEffectAttacked { get; protected set; }
        public string PassiveSkillEffectActivated { get; protected set; }
        public string RecoveryEffect { get; protected set; } = "hit_skill_effect_1";

        public Transform2 Transform { get; protected set; }
        public Vector2 Velocity, CombatNumVelocity;
        public float BoundingCollisionX, BoundingCollisionY;
        public RectangleF BoundingDetectCollisions;      // For dectect collisions
        public CircleF BoundingHitBox;
        public CircleF BoundingDetectEntity;
        public float BaseBoundingDetectEntityRadius;
        public CircleF BoundingInteraction;
        public CircleF BoundingAggro;
        public Vector2 OffSetCircle = Vector2.Zero;

        protected int currentNodeIndex = 0;     // Index of the current node in the path
        protected int stoppingNodeIndex = 1;    // Distance index from the last node in the path for entity to stop moving
        public AStar pathFinding;
        protected Vector2 prevPos;
        protected Vector2 targetNode;
        protected int routeNodeIndex = 0;
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
                    BoundingInteraction.Center = value + new Vector2(0f, 60f);
                    BoundingAggro.Center = value + new Vector2(0f, 32f);
                }
                else
                {
                    BoundingHitBox.Center = value + OffSetCircle;
                    BoundingDetectEntity.Center = value + OffSetCircle;
                    BoundingInteraction.Center = value + OffSetCircle;
                    BoundingAggro.Center = value + OffSetCircle;
                }
            }
        }

        public enum EntityTypes
        {
            Playable,
            Companion,
            FriendlyMob,
            HostileMob,
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
        public float AggroTime { get; protected set; } = 5f;
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

        // Attack Control
        protected float baseknockbackForce;
        protected float knockbackForce;
        protected float attackSpeed;
        protected float cooldownAttack;
        protected float cooldownAttackTimer;
        protected bool isAttackCooldown;

        // Blinking Damage Taken
        protected bool isBlinkingPlayed = false;
        protected readonly float blinkingTime = 1f;
        protected float blinkingTimer = 0f;

        // Combat Nunber
        public const int DamageDefault = 0;
        public const int DamageCrit = 1;
        public const int Healing = 2;
        public const int Missed = 3;
        public const int Buff = 4;
        public const int ManaRestores = 5;
        public const int Debuff = 6;
        public const int GoinCoinAdded = 7;
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
        public bool IsDestroyable {  get; protected set; }
        public bool IsDestroyed { get; set; }
        public bool IsMoving { get; set; }
        public bool IsAttacking { get; set; }
        public bool IsAttacked { get; set; }
        public bool IsBuffOn { get; set; }
        public bool IsDebuffOn { get; set; }
        public bool IsShowCombatNumbers { get; set; }
        public bool IsDetectCollistionObject { get; set; }
        public bool IsPathFindingUpdate {  get; set; }
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

            Velocity = Vector2.Zero;
            CombatNumVelocity = Vector2.Zero;

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

            baseknockbackForce = 25f;
            knockbackForce = baseknockbackForce;
            attackSpeed = 1f;
            cooldownAttack = 0.4f;
            cooldownAttackTimer = cooldownAttack;
            isAttackCooldown = false;

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
            IsDestroyable = false;
            IsDestroyed = false;
            IsMoving = false;
            IsAttacking = false;
            IsAttacked = false;
            IsBuffOn = false;
            IsDebuffOn = false;
            IsShowCombatNumbers = false;
            IsDetectCollistionObject = false;
            IsPathFindingUpdate = false;

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

            baseknockbackForce = entity.baseknockbackForce;
            knockbackForce = baseknockbackForce;
            attackSpeed = entity.attackSpeed;
            cooldownAttack = entity.cooldownAttack;
            cooldownAttackTimer = cooldownAttack;
            isAttackCooldown = entity.isAttackCooldown;

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
            IsDestroyable = entity.IsDestroyable;
            IsDestroyed = false;
            IsMoving = false;
            IsAttacking = false;
            IsAttacked = false;
            IsBuffOn = false;
            IsDebuffOn = false;
            IsShowCombatNumbers = false;
            IsDetectCollistionObject = false;
            IsPathFindingUpdate = false;

            CombatLogs = [];
        }

        public virtual void Update(GameTime gameTime, KeyboardState keyboardCurrentState, KeyboardState keyboardPrevioseState
            , MouseState mouseCurrentState, MouseState mousePrevioseState) { }
        public virtual void Update(GameTime gameTime, float playerDepth, float topDepth, float middleDepth, float bottomDepth) { }
        public virtual void Draw(SpriteBatch spriteBatch) { }
        public virtual void DrawShadow(SpriteBatch spriteBatch, Texture2D shadowTexture) { }
        public virtual void Destroy()
        {
            if (IsDestroyable) return;

            IsDestroyed = true;
        }

        public virtual Vector2 SetCombatNumDirection()
        {
            Vector2 offset = new(Position.X, Position.Y - Sprite.TextureRegion.Height * 1.5f);

            Vector2 numDirection = Position - offset;
            numDirection.Normalize();

            CombatNumVelocity = numDirection * Sprite.TextureRegion.Height / 2;

            return CombatNumVelocity;
        }

        protected virtual void SetupCharacterStats() { }

        protected virtual void InitializeCharacterData(int charId, int level)
        {
            var charData = Instance.CharacterDatas.FirstOrDefault
                (c => c.CharId.Equals(charId));

            CharId = charData.CharId;
            Name = charData.Name;

            SetEntityType(charData.Category);

            SetCharacterStats(charData, level);
        }

        protected virtual void SetEntityType(int category)
        {
            switch (category)
            {
                case 0:
                    EntityType = EntityTypes.Playable;
                    break;

                case 1:
                    EntityType = EntityTypes.Companion;
                    break;

                case 2:
                    EntityType = EntityTypes.FriendlyMob;
                    break;

                case 3:
                    EntityType = EntityTypes.HostileMob;
                    break;

                case 4:
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

        public virtual void SetCharacterStats(CharacterData charData, int level)
        {
            BaseATK = ATK = (float)(charData.ATK + ((level - 1) * 2));
            BaseMaxHP = MaxHP = HP = (float)(charData.HP + ((level - 1) * (charData.HP * 0.1)));
            BaseMaxMana = MaxMana = Mana = (float)(100f + ((level - 1) * (100f * 0.05)));
            BaseManaRegenRate = ManaRegenRate = InitManaRegenRate;
            BaseDEF = DEF = (float)charData.DEF_Percent;
            BaseCrit = Crit = (float)charData.Crit_Percent;
            BaseCritDMG = CritDMG = (float)charData.CritDMG_Percent;
            BaseSpeed = Speed = charData.Speed;
            BaseEvasion = Evasion = (float)charData.Evasion;
        }

        // Knowing this MovementControl, CombatControl and some methods below this is for Mobs only
        protected virtual void MovementControl(float deltaSeconds)
        {
            var walkSpeed = deltaSeconds * Speed;
            prevPos = Position;

            // Check Object Collsion
            CheckCollision();

            // Play Sprite: Idle 
            if (!IsAttacking && !IsMoving)
            {
                CurrentAnimation = SpriteCycle + "_idle";  // Idle
                Sprite.Play(CurrentAnimation);
            }

            // Check movement according to PathFinding
            if (ScreenManager.Instance.IsScreenLoaded && !UIManager.Instance.IsShowDialogUI
                && !IsStunning && ((!IsKnockback && !IsAttacking) || (!IsAttacked && !IsAttacking)))
            {
                if (pathFinding != null || pathFinding.GetPath().Count != 0)
                {
                    if (currentNodeIndex < pathFinding.GetPath().Count - stoppingNodeIndex)
                    {
                        // Calculate direction to the next node
                        var direction = new Vector2(
                            pathFinding.GetPath()[currentNodeIndex + 1].Col - pathFinding.GetPath()[currentNodeIndex].Col,
                            pathFinding.GetPath()[currentNodeIndex + 1].Row - pathFinding.GetPath()[currentNodeIndex].Row);
                        direction.Normalize();

                        // Move the character towards the next node
                        Position += direction * walkSpeed;
                        IsMoving = true;

                        // Check Animation
                        if (direction.Y < 0)
                        {
                            CurrentAnimation = SpriteCycle + "_walking";     // Up
                        }
                        if (direction.Y > 0)
                        {
                            CurrentAnimation = SpriteCycle + "_walking";     // Down
                        }
                        if (direction.X < 0)
                        {
                            CurrentAnimation = SpriteCycle + "_walking";     // Left
                        }
                        if (direction.X > 0)
                        {
                            CurrentAnimation = SpriteCycle + "_walking";     // Right
                        }

                        Sprite.Play(CurrentAnimation);

                        // Check if the character has passed the next node
                        var tileSize = Instance.TILE_SIZE;
                        var nextNodePosition = new Vector2(
                            pathFinding.GetPath()[currentNodeIndex + 1].Col * tileSize + tileSize / 2,
                            pathFinding.GetPath()[currentNodeIndex + 1].Row * tileSize + tileSize / 2);

                        var boundingCenter = new Vector2(
                            BoundingDetectCollisions.Center.X,
                            BoundingDetectCollisions.Center.Y);

                        if ((boundingCenter - nextNodePosition).Length() < tileSize * stoppingNodeIndex + tileSize / 4)
                        {
                            currentNodeIndex++; // Increase currentNodeIndex
                        }
                        //if ((boundingCenter - nextNodePosition).Length() < tileSize / 2)
                        //{
                        //    currentNodeIndex++;
                        //}
                    }
                    else IsMoving = false;
                }
            }
            else IsMoving = false;
        }

        protected virtual void CheckCollision()
        {
            var ObjectOnTile = Instance.CollistionObject;
            foreach (var rect in ObjectOnTile)
            {
                if (BoundingDetectCollisions.Intersects(rect))
                {
                    IsDetectCollistionObject = true;
                    Position = prevPos;
                    Velocity = Vector2.Zero;
                    break;
                }
                else IsDetectCollistionObject = false;
            }
        }

        protected virtual void CheckAggro()
        {
            switch (EntityType)
            {
                case EntityTypes.HostileMob:

                case EntityTypes.Boss:
                    // Setup Aggrotimer if detected player or companion
                    var isPlayerDetected = BoundingAggro.Intersects(PlayerManager.Instance.Player.BoundingHitBox);

                    var isCompanionDetected = false;
                    if (PlayerManager.Instance.Companions.Count != 0)
                    {
                        var companion = PlayerManager.Instance.Companions[PlayerManager.Instance.CurrCompaIndex];

                        if (companion != null && !companion.IsDying)
                            isCompanionDetected = BoundingDetectEntity.Intersects(companion.BoundingHitBox);
                    }

                    if (isPlayerDetected || isCompanionDetected)
                    {
                        AggroTimer = AggroTime;
                        IsAggro = true;
                    }
                    break;

                case EntityTypes.FriendlyMob:
                    break;

                case EntityTypes.Companion:
                    
                    if (IsAttacked)
                    {
                        AggroTimer = AggroTime;
                        IsAggro = true;
                    }

                    foreach (var entity in EntityManager.Instance.Entities.Where(e => !e.IsDying
                            && e.EntityType == EntityTypes.HostileMob || e.EntityType == EntityTypes.Boss))
                    {
                        if (BoundingAggro.Intersects(entity.BoundingHitBox))
                        {
                            AggroTimer = AggroTime;
                            IsAggro = true;
                            break;
                        }
                    }

                    if (IsAggro || PlayerManager.Instance.Player.IsAttacked || PlayerManager.Instance.Player.IsAttacking)
                    {
                        BoundingAggro.Radius = 180f;
                    }
                    else BoundingAggro.Radius = 60f;

                    if (EntityManager.Instance.ClosestEnemyToCompanion != null
                        && EntityManager.Instance.ClosestEnemyToCompanion.IsDying)
                    {                       
                        IsAggro = false;
                        AggroTimer = 0;
                    }
                    
                    break;
            }
        }

        public void UpdateNode(EntityData entityData)
        {
            switch (PathFindingType)
            {
                case PathFindingTypes.RoutePoint:
                    // Define the current route
                    var rountNodePoint = entityData.RoutePoint.ElementAt(routeNodeIndex);

                    if (rountNodePoint != null)
                    {
                        var position = new Vector2((float)rountNodePoint[0], (float)rountNodePoint[1]);

                        // Next route
                        routeNodeIndex++;
                        if (routeNodeIndex >= entityData.RoutePoint.Length)
                            routeNodeIndex = 0;

                        targetNode = position;
                    }
                    else targetNode = new Vector2(Position.X, Position.Y);

                    break;

                case PathFindingTypes.RandomPoint:
                    var random = new Random();

                    // Define the patrol area from rectangle
                    var recArea = Instance.MobPartrolArea.FirstOrDefault
                        (b => b.Name.Equals(entityData.PartrolArea));

                    if (recArea != null)
                    {
                        var rectangleX = (int)recArea.Bounds.X;
                        var rectangleY = (int)recArea.Bounds.Y;
                        var rectangleWidth = (int)recArea.Bounds.Width;
                        var rectangleHeight = (int)recArea.Bounds.Height;

                        // Generate random X and Y within the rectangle
                        var randomX = random.Next(rectangleX, rectangleX + rectangleWidth);
                        var randomY = random.Next(rectangleY, rectangleY + rectangleHeight);

                        targetNode = new Vector2(randomX, randomY);
                    }
                    else targetNode = new Vector2(Position.X, Position.Y);

                    break;

                case PathFindingTypes.StationaryPoint:
                    // Its Stationary so we do nothing here
                    targetNode = new Vector2((float)entityData.Position[0], (float)entityData.Position[1]);
                    break;
            }
        }

        protected virtual void UpdateTargetNode(float deltaSeconds, EntityData entityData)
        {
            nextNodeTimer += deltaSeconds;

            if (nextNodeTimer >= nextNodeTime)
            {
                switch (PathFindingType)
                {
                    case PathFindingTypes.RoutePoint:
                        // Define the current route
                        var rountNodePoint = entityData.RoutePoint.ElementAt(routeNodeIndex);

                        if (rountNodePoint != null)
                        {
                            var position = new Vector2((float)rountNodePoint[0], (float)rountNodePoint[1]);

                            // Next route
                            routeNodeIndex++;
                            if (routeNodeIndex >= entityData.RoutePoint.Length)
                                routeNodeIndex = 0;

                            targetNode = position;
                        }
                        else targetNode = new Vector2(Position.X, Position.Y);

                        break;

                    case PathFindingTypes.RandomPoint:
                        var random = new Random();

                        // Define the patrol area from rectangle
                        var recArea = Instance.MobPartrolArea.FirstOrDefault
                            (b => b.Name.Equals(entityData.PartrolArea));

                        if (recArea != null)
                        {
                            var rectangleX = (int)recArea.Bounds.X;
                            var rectangleY = (int)recArea.Bounds.Y;
                            var rectangleWidth = (int)recArea.Bounds.Width;
                            var rectangleHeight = (int)recArea.Bounds.Height;

                            // Generate random X and Y within the rectangle
                            var randomX = random.Next(rectangleX, rectangleX + rectangleWidth);
                            var randomY = random.Next(rectangleY, rectangleY + rectangleHeight);

                            targetNode = new Vector2(randomX, randomY);
                        }
                        else targetNode = new Vector2(Position.X, Position.Y);

                        break;

                    case PathFindingTypes.StationaryPoint:
                        // Its Stationary so we do nothing here
                        targetNode = new Vector2((float)entityData.Position[0], (float)entityData.Position[1]);
                        break;
                }

                nextNodeTimer = 0f;
                IsPathFindingUpdate = true;
            }
        }

        protected virtual void SetPathFindingNode(int returnPosX, int returnPosY)
        {
            switch (EntityType)
            {
                case EntityTypes.FriendlyMob:

                case EntityTypes.HostileMob:

                case EntityTypes.Boss:
                    if (IsAggro)
                    {
                        currentNodeIndex = 0;

                        // Find Closest Entity: Player or Companion
                        var entities = new List<Entity>
                        {
                            PlayerManager.Instance.Player
                        };

                        if (PlayerManager.Instance.Companions.Count != 0 && !PlayerManager.Instance.IsCompanionDead)
                        {
                            var compa = PlayerManager.Instance.Companions[PlayerManager.Instance.CurrCompaIndex];
                            if (compa != null)
                                entities.Add(compa);
                        }

                        var closestEntity = FindClosestEntity(entities, this);

                        pathFinding = new AStar(
                            (int)BoundingDetectCollisions.Center.X,
                            (int)BoundingDetectCollisions.Center.Y,
                            (int)closestEntity.BoundingDetectCollisions.Center.X,
                            (int)closestEntity.BoundingDetectCollisions.Center.Y);
                    }
                    else
                    {
                        if (IsPathFindingUpdate)
                        {
                            currentNodeIndex = 0;

                            switch (PathFindingType)
                            {
                                case PathFindingTypes.RoutePoint:

                                case PathFindingTypes.RandomPoint:
                                    pathFinding = new AStar(
                                        (int)BoundingDetectCollisions.Center.X,
                                        (int)BoundingDetectCollisions.Center.Y,
                                        (int)targetNode.X,
                                        (int)targetNode.Y);
                                    break;

                                case PathFindingTypes.StationaryPoint:
                                    pathFinding = new AStar(
                                        (int)BoundingDetectCollisions.Center.X,
                                        (int)BoundingDetectCollisions.Center.Y,
                                        returnPosX,
                                        returnPosY);
                                    break;
                            }

                            IsPathFindingUpdate = false;
                        }
                    }
                    break;

                case EntityTypes.Companion:
                    if (!PlayerManager.Instance.IsRecallCompanion && IsAggro 
                        && EntityManager.Instance.ClosestEnemyToCompanion != null
                        && !EntityManager.Instance.ClosestEnemyToCompanion.IsDying)
                    {
                        currentNodeIndex = 0;

                        pathFinding = new AStar(
                            (int)BoundingDetectCollisions.Center.X,
                            (int)BoundingDetectCollisions.Center.Y,
                            (int)EntityManager.Instance.ClosestEnemyToCompanion.BoundingDetectCollisions.Center.X,
                            (int)EntityManager.Instance.ClosestEnemyToCompanion.BoundingDetectCollisions.Center.Y);
                    }
                    else
                    {
                        currentNodeIndex = 0;

                        pathFinding = new AStar(
                        (int)BoundingDetectCollisions.Center.X,
                        (int)BoundingDetectCollisions.Center.Y,
                        returnPosX,
                        returnPosY);
                    }
                    break;
            } 
        }

        protected virtual Entity FindClosestEntity(List<Entity> entities, Entity targetEntity)
        {                     
            Entity closestEntity = null;
            float minDistance = float.MaxValue;

            if (entities != null || entities.Count != 0)
                foreach (Entity entity in entities)
                {
                    // Calculate the distance between the player and the current entity
                    float distance = Vector2.Distance(targetEntity.Position, entity.Position);

                    // Check if the current entity is closer than the previous closest entity
                    if (distance < minDistance)
                    {
                        closestEntity = entity;
                        minDistance = distance;
                    }
                }

            return closestEntity;
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

            // Decreasing BuffTimer
            if (BuffTimer > 0f)
            {
                IsBuffOn = true;
                BuffTimer -= deltaSeconds;
            }
            else IsBuffOn = false;

            // Decreasing DebuffTimer
            if (DebuffTimer > 0f)
            {
                IsDebuffOn = true;
                DebuffTimer -= deltaSeconds;
            }
            else IsDebuffOn = false;

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
        protected virtual void CombatControl(float deltaSeconds)
        {
            // Check Dectected
            var isPlayerDetected = BoundingDetectEntity.Intersects(PlayerManager.Instance.Player.BoundingHitBox);

            var isCompanionDetected = false;
            if (PlayerManager.Instance.Companions.Count != 0 
                && !PlayerManager.Instance.IsCompanionDead
                && PlayerManager.Instance.IsCompanionSummoned)
            {
                var companion = PlayerManager.Instance.Companions[PlayerManager.Instance.CurrCompaIndex];

                if (companion != null && !companion.IsDying)
                    isCompanionDetected = BoundingDetectEntity.Intersects(companion.BoundingHitBox);
            }

            // Do Attack
            if ((isPlayerDetected || isCompanionDetected)
                && ScreenManager.Instance.IsScreenLoaded && !UIManager.Instance.IsShowDialogUI
                && !IsAttacking && !IsStunning)
            {
                CurrentAnimation = SpriteCycle + "_attacking";
                Sprite.Play(CurrentAnimation);

                IsAttacking = true;
                ActionTimer = attackSpeed;
                cooldownAttackTimer = cooldownAttack;
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
                    if (!isAttackCooldown)
                    {
                        CheckAttackDetection();
                        isAttackCooldown = true;

                        PlayMobAttackSound();
                    }
                    else
                    {
                        if (cooldownAttackTimer > 0)
                        {
                            cooldownAttackTimer -= deltaSeconds;
                        }
                        else
                        {
                            IsAttacking = false;
                            isAttackCooldown = false;
                        }
                    }
                }
            }
        }

        protected virtual void CheckAttackDetection()
        {
            // Check Attacking Detection for Player and Companions
            var isPlayerDetected = BoundingDetectEntity.Intersects(PlayerManager.Instance.Player.BoundingHitBox);

            var isCompanionDetected = false;
            if (PlayerManager.Instance.Companions.Count != 0)
            {
                var companion = PlayerManager.Instance.Companions[PlayerManager.Instance.CurrCompaIndex];

                if (companion != null)
                    isCompanionDetected = BoundingDetectEntity.Intersects(companion.BoundingHitBox);
            }

            if (isPlayerDetected)
            {
                if (PlayerManager.Instance.Player.HP > 0)
                {
                    float totalDamage = TotalDamage(ATK
                        , PlayerManager.Instance.Player.DEF
                        , PlayerManager.Instance.Player.Evasion);

                    var combatNumVelocity = PlayerManager.Instance.Player.SetCombatNumDirection();
                    PlayerManager.Instance.Player.AddCombatLogNumbers(Name
                        , ((int)totalDamage).ToString()
                        , CombatNumCase
                        , combatNumVelocity
                        , NormalHitEffectAttacked);

                    // In case the Attack doesn't Missed
                    if (CombatNumCase != Missed)
                    {
                        // Player being hit by Mob
                        PlayerManager.Instance.Player.IsAttacked = true;
                        PlayerManager.Instance.Player.HP -= totalDamage;

                        if (PlayerManager.Instance.Player.IsKnockbackable)
                        {
                            var knockBackDirection = (PlayerManager.Instance.Player.Position + new Vector2(0f, 32f)) - Position;
                            knockBackDirection.Normalize();
                            PlayerManager.Instance.Player.Velocity = knockBackDirection * knockbackForce;

                            PlayerManager.Instance.Player.IsKnockback = true;
                            PlayerManager.Instance.Player.KnockbackedTimer = 0.3f;
                        }
                    }        
                }
            }


            if (isCompanionDetected)
            {
                var companion = PlayerManager.Instance.Companions[PlayerManager.Instance.CurrCompaIndex];

                if (companion.HP > 0)
                {
                    float totalDamage = TotalDamage(ATK
                        , companion.DEF
                        , companion.Evasion);

                    var combatNumVelocity = companion.SetCombatNumDirection();
                    companion.AddCombatLogNumbers(Name
                        , ((int)totalDamage).ToString()
                        , CombatNumCase
                        , combatNumVelocity
                        , NormalHitEffectAttacked);

                    // In case the Attack doesn't Missed
                    if (CombatNumCase != Missed)
                    {
                        // companion being hit by Mob
                        companion.IsAttacked = true;
                        companion.HP -= totalDamage;

                        if (companion.IsKnockbackable)
                        {
                            var knockBackDirection = companion.Position - Position;
                            knockBackDirection.Normalize();
                            companion.Velocity = knockBackDirection * knockbackForce;

                            companion.IsKnockback = true;
                            companion.KnockbackedTimer = 0.2f;
                        }
                    }
                }
            }
        }

        protected virtual void CheckAttackDetection(float percentHit)
        {
            // Check Attacking Detection for Player and Companions
            var isPlayerDetected = BoundingDetectEntity.Intersects(PlayerManager.Instance.Player.BoundingHitBox);

            var isCompanionDetected = false;
            if (PlayerManager.Instance.Companions.Count != 0)
            {
                var companion = PlayerManager.Instance.Companions[PlayerManager.Instance.CurrCompaIndex];

                if (companion != null)
                    isCompanionDetected = BoundingDetectEntity.Intersects(companion.BoundingHitBox);
            }

            if (isPlayerDetected)
            {
                if (PlayerManager.Instance.Player.HP > 0)
                {
                    float totalDamage = TotalDamage(ATK * percentHit
                        , PlayerManager.Instance.Player.DEF
                        , PlayerManager.Instance.Player.Evasion);

                    var combatNumVelocity = PlayerManager.Instance.Player.SetCombatNumDirection();
                    PlayerManager.Instance.Player.AddCombatLogNumbers(Name
                        , ((int)totalDamage).ToString()
                        , CombatNumCase
                        , combatNumVelocity
                        , NormalHitEffectAttacked);

                    // In case the Attack doesn't Missed
                    if (CombatNumCase != Missed)
                    {
                        // Player being hit by Mob
                        PlayerManager.Instance.Player.IsAttacked = true;
                        PlayerManager.Instance.Player.HP -= totalDamage;

                        if (PlayerManager.Instance.Player.IsKnockbackable)
                        {
                            var knockBackDirection = (PlayerManager.Instance.Player.Position + new Vector2(0f, 32f)) - Position;
                            knockBackDirection.Normalize();
                            PlayerManager.Instance.Player.Velocity = knockBackDirection * knockbackForce;

                            PlayerManager.Instance.Player.IsKnockback = true;
                            PlayerManager.Instance.Player.KnockbackedTimer = 0.3f;
                        }
                    }
                }
            }


            if (isCompanionDetected)
            {
                var companion = PlayerManager.Instance.Companions[PlayerManager.Instance.CurrCompaIndex];

                if (companion.HP > 0)
                {
                    float totalDamage = TotalDamage(ATK * percentHit
                        , companion.DEF
                        , companion.Evasion);

                    var combatNumVelocity = companion.SetCombatNumDirection();
                    companion.AddCombatLogNumbers(Name
                        , ((int)totalDamage).ToString()
                        , CombatNumCase
                        , combatNumVelocity
                        , NormalHitEffectAttacked);

                    // In case the Attack doesn't Missed
                    if (CombatNumCase != Missed)
                    {
                        // companion being hit by Mob
                        companion.IsAttacked = true;
                        companion.HP -= totalDamage;

                        if (companion.IsKnockbackable)
                        {
                            var knockBackDirection = companion.Position - Position;
                            knockBackDirection.Normalize();
                            companion.Velocity = knockBackDirection * knockbackForce;

                            companion.IsKnockback = true;
                            companion.KnockbackedTimer = 0.2f;
                        }
                    }
                }
            }
        }

        protected virtual float TotalDamage(float ATK, float DefPercent, float EvasionPercent)
        {
            var random = new Random();

            // Default total damage
            float totalDamage = ATK;

            // Check evasion
            int evaChance = random.Next(1, 101);
            if (evaChance <= EvasionPercent * 100)
            {
                // if Attack Missed
                totalDamage = 0;
                CombatNumCase = Missed;
            }
            else
            {
                // if Attacked
                // Check crit chance           
                int critChance = random.Next(1, 101);
                if (critChance <= Crit * 100)
                {
                    totalDamage += (int)(totalDamage * CritDMG);
                    CombatNumCase = DamageCrit;
                }
                else CombatNumCase = DamageDefault;

                // Calculate DEF
                totalDamage -= (int)(totalDamage * DefPercent);
            }

            return totalDamage;
        }

        protected virtual void PlayMobAttackSound()
        {
            switch (GetType().Name)
            {
                default:
                case "Slime":
                    PlaySoundEffect([Sound.Bite, Sound.Claw]);
                    break;

                case "Goblin":
                    PlaySoundEffect([Sound.dullSwoosh1, Sound.dullSwoosh2, Sound.dullSwoosh3, Sound.dullSwoosh4]);
                    break;

                case "Minotaur":
                    PlaySoundEffect([Sound.metalSwoosh1, Sound.metalSwoosh2, Sound.metalSwoosh3, Sound.metalSwoosh4]);
                    break;
            }
        }

        protected virtual void HitBlinking(float deltaSeconds)
        {
            if (IsAttacked && !isBlinkingPlayed) isBlinkingPlayed = true;

            if (isBlinkingPlayed && blinkingTimer < blinkingTime)
            {
                blinkingTimer += deltaSeconds;

                var blinkSpeed = 15f; // Adjust the speed of the blinking effect
                var alphaMultiplier = MathF.Sin(blinkingTimer * blinkSpeed);

                // Ensure alphaMultiplier is within the valid range [0, 1]
                alphaMultiplier = MathHelper.Clamp(alphaMultiplier, 0.25f, 2f);

                Sprite.Color = new Color(255, 105, 105) * Math.Min(alphaMultiplier, 1f);
            }
            else
            {
                isBlinkingPlayed = false;
                blinkingTimer = 0;
                Sprite.Color = Color.White;
            }
        }

        protected virtual void UpdateLayerDepth(float playerDepth, float topDepth, float middleDepth, float bottomDepth)
        {
            // Detect for LayerDepth
            Sprite.Depth = topDepth; // Default depth
            if (BoundingHitBox.Intersects(PlayerManager.Instance.Player.BoundingDetectEntity))
            {
                if (BoundingDetectCollisions.Bottom >= PlayerManager.Instance.Player.BoundingDetectCollisions.Bottom)
                {
                    Sprite.Depth = playerDepth - 0.00001f; // In front Player
                }
                else Sprite.Depth = playerDepth + 0.00001f; // Behide Player
            }

            var TopLayerObject = Instance.TopLayerObject;
            foreach (var obj in TopLayerObject)
            {
                if (obj.Intersects(BoundingDetectCollisions))
                {
                    Sprite.Depth = middleDepth;
                    break; // Exit the loop as soon as an intersection is found
                }
            }

            var MiddleLayerObject = Instance.MiddleLayerObject;
            foreach (var obj in MiddleLayerObject)
            {
                if (obj.Intersects(BoundingDetectCollisions))
                {
                    Sprite.Depth = bottomDepth;
                    break;
                }
            }

            var BottomLayerObject = Instance.BottomLayerObject;
            foreach (var obj in BottomLayerObject)
            {
                if (obj.Intersects(BoundingDetectCollisions))
                {
                    Sprite.Depth = bottomDepth + 0.2f;
                    break;
                }
            }
        }

        public virtual void AddCombatLogNumbers(string ActorName, string combatText, int combatCase
            , Vector2 combatNumVelocity, string effectName)
        {
            IsShowCombatNumbers = true;
            CombatNumbersTimer = 0f;

            switch (combatCase)
            {
                case DamageDefault: // Damage Numbers Default
                    CombatLogs.Add(new CombatNumberData
                    {
                        Actor = ActorName,
                        Action = CombatNumberData.ActionType.Attack,
                        Value = combatText,
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

                    PlaySoundEffect([Sound.damage01, Sound.damage02]);
                    break;

                case DamageCrit: // Damage Numbers Critical
                    CombatLogs.Add(new CombatNumberData
                    {
                        Actor = ActorName,
                        Action = CombatNumberData.ActionType.CriticalAttack,
                        Value = combatText,
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

                    PlaySoundEffect([Sound.damageCrit1, Sound.damageCrit2]);
                    break;

                case Healing: // Healing Numbers
                    CombatLogs.Add(new CombatNumberData
                    {
                        Actor = ActorName,
                        Action = CombatNumberData.ActionType.Recovery,
                        Value = combatText,
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

                    PlaySoundEffect(Sound.Heal_Low_Base);
                    break;

                case Missed: // Missed
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

                    PlaySoundEffect([Sound.Miss1, Sound.Miss2, Sound.Miss3]);
                    break;

                case Buff: // Buff
                    CombatLogs.Add(new CombatNumberData
                    {
                        Actor = ActorName,
                        Action = CombatNumberData.ActionType.Buff,
                        Value = combatText,
                        EffectName = effectName,
                        IsEffectPlayed = false,
                        ElapsedTime = 0,
                        Velocity = combatNumVelocity,
                        OffSet = Vector2.Zero,
                        Color = Color.DodgerBlue,
                        StrokeColor = Color.Black,
                        Size = 1.5f,
                        StrokeSize = 1,
                        AlphaColor = 1f,
                        Scaling = 0f
                    });

                    PlaySoundEffect(Sound.Powerup);
                    break;

                case ManaRestores: // Mana Restores Numbers
                    CombatLogs.Add(new CombatNumberData
                    {
                        Actor = ActorName,
                        Action = CombatNumberData.ActionType.Recovery,
                        Value = combatText,
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

                    PlaySoundEffect(Sound.Recovery2);
                    break;

                case Debuff: // Debuff
                    CombatLogs.Add(new CombatNumberData
                    {
                        Actor = ActorName,
                        Action = CombatNumberData.ActionType.Debuff,
                        Value = combatText,
                        EffectName = effectName,
                        IsEffectPlayed = false,
                        ElapsedTime = 0,
                        Velocity = combatNumVelocity,
                        OffSet = Vector2.Zero,
                        Color = Color.DarkViolet,
                        StrokeColor = Color.Black,
                        Size = 1.5f,
                        StrokeSize = 1,
                        AlphaColor = 1f,
                        Scaling = 0f
                    });

                    PlaySoundEffect(Sound.Debuff1);
                    break;

                case GoinCoinAdded:
                    CombatLogs.Add(new CombatNumberData
                    {
                        Actor = ActorName,
                        Action = CombatNumberData.ActionType.Collecting,
                        Value = combatText,
                        EffectName = effectName,
                        IsEffectPlayed = false,
                        ElapsedTime = 0,
                        Velocity = combatNumVelocity,
                        OffSet = Vector2.Zero,
                        Color = Color.Gold,
                        StrokeColor = Color.Black,
                        Size = 1.75f,
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
                var font = Instance.FontTA16Bit;

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
                            log.DrawStringPosition = new Vector2(Position.X - ((textSize.Width * 2.1f) / 2.05f)
                                , Position.Y - (log.Velocity.Y)) - offSet;
                            break;

                        case CombatNumberData.ActionType.Missed:
                        case CombatNumberData.ActionType.Buff:
                        case CombatNumberData.ActionType.Debuff:
                            log.DrawStringPosition = new Vector2(Position.X - ((textSize.Width * 1.5f) / 2.25f)
                                , Position.Y - (log.Velocity.Y)) - offSet;
                            break;
                        case CombatNumberData.ActionType.Collecting:
                            log.DrawStringPosition = new Vector2(Position.X - ((textSize.Width * 1.75f) / 2.375f)
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

        public bool RestoresHP(string actorName, float value, bool isCheckingCap)
        {
            if (isCheckingCap)
            {
                if (HP < MaxHP)
                {
                    value = (value + HP) > MaxHP ? MaxHP - HP : value;
                    HP += value;

                    CombatNumCase = Healing;
                    var combatNumVelocity = SetCombatNumDirection();
                    AddCombatLogNumbers(actorName
                        , ((int)value).ToString()
                        , CombatNumCase
                        , combatNumVelocity
                        , RecoveryEffect);

                    return true;
                }
            }
            else
            {
                if (HP < MaxHP)
                {
                    value = (value + HP) > MaxHP ? MaxHP - HP : value;
                    HP += value;
                }
                else value = 0;

                CombatNumCase = Healing;
                var combatNumVelocity = SetCombatNumDirection();
                AddCombatLogNumbers(actorName
                    , ((int)value).ToString()
                    , CombatNumCase
                    , combatNumVelocity
                    , RecoveryEffect);

                return true;
            }

            return false;
        }

        public bool RestoresMana(string actorName, float value, bool isCheckingCap)
        {
            if (isCheckingCap)
            {
                if (Mana < MaxMana)
                {
                    value = value + Mana > MaxMana ? MaxMana - Mana : value;
                    Mana += value;

                    CombatNumCase = ManaRestores;
                    var combatNumVelocity = SetCombatNumDirection();
                    AddCombatLogNumbers(actorName
                        , ((int)value).ToString()
                        , CombatNumCase
                        , combatNumVelocity
                        , RecoveryEffect);

                    return true;
                }
            }
            else
            {
                if (Mana < MaxMana)
                {
                    value = value + Mana > MaxMana ? MaxMana - Mana : value;
                    Mana += value;
                }
                else value = 0;

                CombatNumCase = ManaRestores;
                var combatNumVelocity = SetCombatNumDirection();
                AddCombatLogNumbers(actorName
                    , ((int)value).ToString()
                    , CombatNumCase
                    , combatNumVelocity
                    , RecoveryEffect);

                return true;
            }

            return false;
        }

        public float GetDepth()
        {
            return Sprite.Depth;
        }

        public float GetCurrentHealthPercentage()
        {
            return HP / MaxHP;
        }

        public float GetCurrentManaPercentage()
        {
            return Mana / MaxMana;
        }

        public virtual object Clone()
        {
            return new Entity(this);
        }
    }
}
