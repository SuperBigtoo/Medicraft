﻿namespace Medicraft.Data.Models
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
    }
}
