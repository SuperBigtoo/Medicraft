using Microsoft.Xna.Framework;

namespace Medicraft.Data.Models
{
    public class CombatNumberData
    {
        public enum ActionType
        {
            Attack,
            Heal,
            Buff,
            Debuff
        }

        public string Actor { get; set; }
        public ActionType Action { get; set;}
        public string Value { get; set; }
        public float ElapsedTime { get; set; }
        public Vector2 Velocity { get; set; }
        public Vector2 OffSet { get; set; }
        public Color Color { get; set; }
        public float AlphaColor { get; set; }
        public float ScaleFont { get; set; }


    }
}
