using Microsoft.Xna.Framework;
using MonoGame.Extended.Sprites;

namespace Medicraft.Data.Models
{
    public class CombatNumberData
    {
        public enum ActionType
        {
            Attack,
            CriticalAttack,
            Heal,
            Missed,
            Buff,
            Debuff
        }

        public string Actor { get; set; }
        public ActionType Action { get; set;}
        public string Value { get; set; }
        public string EffectName { get; set; }
        public bool IsEffectPlayed { get; set; }
        public AnimatedSprite AnimatedSprite { get; set; }
        public float ElapsedTime { get; set; }
        public Vector2 Velocity { get; set; }
        public Vector2 OffSet { get; set; }
        public Vector2 DrawStringPosition { get; set; }
        public Color Color { get; set; }
        public Color StrokeColor { get; set; }
        public float Size { get; set; }
        public int StrokeSize { get; set; }
        public float AlphaColor { get; set; }
        public float Scaling { get; set; }


    }
}
