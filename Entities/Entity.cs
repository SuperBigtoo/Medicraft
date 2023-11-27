using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Sprites;
using Microsoft.Xna.Framework.Input;
using System;

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
        public AnimatedSprite Sprite;
        public Transform2 Transform;
        public Vector2 Velocity;
        public Rectangle BoundingDetectCollisions; // For dectect collisions
        public double BoundingCollisionX, BoundingCollisionY;
        public CircleF BoundingHitBox;
        public CircleF BoundingDetection;
        public CircleF BoundingCollection;

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
        public virtual object Clone()
        {
            return new Entity(this);
        }
    }
}
