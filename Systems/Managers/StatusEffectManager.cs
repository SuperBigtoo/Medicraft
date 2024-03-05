using Medicraft.Data.Models;
using Medicraft.Entities;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Medicraft.Systems.Managers
{
    public class StatusEffectManager
    {
        private List<StatusEffect> statusEffects;

        private List<StatusEffect> effectsToRemove;

        public int TempATK { get; set; }
        public int TempHP { get; set; }
        public int TempSpeed { get; set; }
        public float TempMana { get; set; }
        public float TempDEF { get; set; }
        public float TempCrit { get; set; }
        public float TempCritDMG { get; set; }
        public float TempEvasion { get; set; }

        private const float _activateTime = 3f;

        private float _deltaSeconds, _activateTimer = 0;

        private static StatusEffectManager instance;

        private StatusEffectManager()
        {
            statusEffects = [];
            effectsToRemove = [];
        }

        public void Update(GameTime gameTime)
        {
            _deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // If statusEffects has any Effect ActivationType = Periodic then calculate timer else reset timer
            if (statusEffects.Any(statusEffect => statusEffect.Effect.ActivationType.Equals("Periodic")))
            {
                _activateTimer -= _deltaSeconds;
            }
            else _activateTimer = 0;

            foreach (var statusEffect in statusEffects)
            {             
                statusEffect.Effect.Timer -= _deltaSeconds;

                // Decreasing effect time
                if (statusEffect.Effect.Timer <= 0)
                {
                    switch (statusEffect.Effect.EffectType)
                    {
                        case "Buff":
                            ApplyEffect(statusEffect, false);                           
                            break;

                        case "Debuff":
                            ApplyEffect(statusEffect, true);
                            break;

                        default:
                            break;
                    }

                    statusEffect.Effect.IsActive = false;
                    statusEffect.Effect.Timer = statusEffect.Effect.Time;

                    effectsToRemove.Add(statusEffect);
                }
                else
                {
                    if (statusEffect.Effect.ActivationType.Equals("Instant"))
                    {
                        // Activate one time
                        if (!statusEffect.Effect.IsActive)
                        {
                            switch (statusEffect.Effect.EffectType)
                            {
                                case "Buff":
                                    ApplyEffect(statusEffect, true);
                                    break;

                                case "Debuff":
                                    ApplyEffect(statusEffect, false);
                                    break;
                            }

                            statusEffect.Effect.IsActive = true;
                        }
                    }
                    else if (statusEffect.Effect.ActivationType.Equals("Periodic"))
                    {
                        // Activate every 3s
                        if (_activateTimer <= 0)
                        {
                            // Activate effect
                            switch (statusEffect.Effect.EffectType)
                            {
                                case "Regenerate":
                                    RegenerateEffect(statusEffect);
                                    break;
                            }

                            statusEffect.Effect.IsActive = true;

                            var periodicEffects = statusEffects.Where(
                                statusEffect => statusEffect.Effect.ActivationType.Equals("Periodic"));

                            if (periodicEffects.All(statusEffect => statusEffect.Effect.IsActive))
                            {
                                _activateTimer = _activateTime;

                                foreach (var periodicEffect in periodicEffects)
                                {
                                    periodicEffect.Effect.IsActive = false;
                                }
                            }
                        }
                    }               
                }
            }

            // remove all effect that has time = 0
            foreach (var effectToRemove in effectsToRemove)
            {
                statusEffects.Remove(effectToRemove);
            }
            effectsToRemove.Clear();
        }

        private void RegenerateEffect(StatusEffect statusEffect)
        {
            switch (statusEffect.Effect.Target)
            {
                case "HP":
                    var valueHP = (int)(statusEffect.TargetEntity.BaseMaxHP * statusEffect.Effect.Value);

                    System.Diagnostics.Debug.WriteLine($"HP regen effect : {valueHP}");

                    if (statusEffect.TargetEntity.Name.Equals("Noah"))
                    {
                        PlayerManager.Instance.Player.RestoresHP(statusEffect.Effect.EffectType, valueHP, false);
                    }
                    else statusEffect.TargetEntity.HP += valueHP;

                    break;

                case "Mana":
                    var valueMana = statusEffect.TargetEntity.BaseMaxMana * (float)statusEffect.Effect.Value;

                    System.Diagnostics.Debug.WriteLine($"Mana regen effect : {valueMana}");

                    if (statusEffect.TargetEntity.Name.Equals("Noah"))
                    {
                        PlayerManager.Instance.Player.RestoresMana(statusEffect.Effect.EffectType, valueMana, false);
                    }
                    else statusEffect.TargetEntity.Mana += valueMana;

                    break;
            }
        }

        private void ApplyEffect(StatusEffect statusEffect, bool isBuff)
        {
            if (isBuff)   // Buff Activate | Debuff Deactivate
            {
                switch (statusEffect.Effect.Target)
                {
                    case "MaxHP":
                        var valueMaxHP = (int)(statusEffect.TargetEntity.BaseMaxHP * statusEffect.Effect.Value);
                        var valueHP = (int)(statusEffect.TargetEntity.HP * statusEffect.Effect.Value);

                        statusEffect.TargetEntity.MaxHP += valueMaxHP;
                        statusEffect.TargetEntity.HP += valueHP;
                        break;

                    case "MaxMana":
                        var valueMaxMana = (statusEffect.TargetEntity.BaseMaxMana * statusEffect.Effect.Value);
                        var valueMana = (statusEffect.TargetEntity.Mana * statusEffect.Effect.Value);

                        statusEffect.TargetEntity.MaxMana += (float)valueMaxMana;
                        statusEffect.TargetEntity.Mana += (float)valueMana;
                        break;

                    case "ATK":
                        var valueATK = (int)(statusEffect.TargetEntity.BaseATK * statusEffect.Effect.Value);

                        statusEffect.TargetEntity.ATK += valueATK;
                        break;

                    case "DEF":
                        var valueDEF = statusEffect.Effect.Value;

                        statusEffect.TargetEntity.DEF += (float)valueDEF;
                        break;

                    case "Crit":
                        var valueCrit = statusEffect.Effect.Value;

                        statusEffect.TargetEntity.Crit += (float)valueCrit;
                        break;

                    case "CritDMG":
                        var valueCritDMG = statusEffect.Effect.Value;

                        statusEffect.TargetEntity.CritDMG += (float)valueCritDMG;
                        break;

                    case "Evasion":
                        var valueEvasion = statusEffect.Effect.Value;

                        statusEffect.TargetEntity.Evasion += (float)valueEvasion;
                        break;

                    case "Speed":
                        var valueSpeed = (int)(statusEffect.TargetEntity.BaseSpeed * statusEffect.Effect.Value);

                        statusEffect.TargetEntity.Speed += valueSpeed;
                        break;
                }
            }
            else // Buff Deactivate | Debuff Activate
            {
                switch (statusEffect.Effect.Target)
                {
                    case "MaxHP":
                        var valueMaxHP = (int)(statusEffect.TargetEntity.BaseMaxHP * statusEffect.Effect.Value);
                        var valueHP = (int)(statusEffect.TargetEntity.HP * statusEffect.Effect.Value);

                        statusEffect.TargetEntity.MaxHP -= valueMaxHP;
                        statusEffect.TargetEntity.HP -= valueHP;
                        break;

                    case "MaxMana":
                        var valueMaxMana = (statusEffect.TargetEntity.BaseMaxMana * statusEffect.Effect.Value);
                        var valueMana = (statusEffect.TargetEntity.Mana * statusEffect.Effect.Value);

                        statusEffect.TargetEntity.MaxMana -= (float)valueMaxMana;
                        statusEffect.TargetEntity.Mana -= (float)valueMana;
                        break;

                    case "ATK":
                        var valueATK = (int)(statusEffect.TargetEntity.BaseATK * statusEffect.Effect.Value);

                        statusEffect.TargetEntity.ATK -= valueATK;
                        break;

                    case "DEF":
                        var valueDEF = statusEffect.Effect.Value;

                        statusEffect.TargetEntity.DEF -= (float)valueDEF;
                        break;

                    case "Crit":
                        var valueCrit = statusEffect.Effect.Value;

                        statusEffect.TargetEntity.Crit -= (float)valueCrit;
                        break;

                    case "CritDMG":
                        var valueCritDMG = statusEffect.Effect.Value;

                        statusEffect.TargetEntity.CritDMG -= (float)valueCritDMG;
                        break;

                    case "Evasion":
                        var valueEvasion = statusEffect.Effect.Value;

                        statusEffect.TargetEntity.Evasion -= (float)valueEvasion;
                        break;

                    case "Speed":
                        var valueSpeed = (int)(statusEffect.TargetEntity.BaseSpeed * statusEffect.Effect.Value);

                        statusEffect.TargetEntity.Speed -= valueSpeed;
                        break;
                }
            }
        }

        public void AddStatusEffect(Entity entity, string effectName, Effect effect)
        {
            if (statusEffects.Count != 0)
            {
                if (statusEffects.Any(statusEffect => statusEffect.EffectName.Equals(effectName)))
                {
                    var statusEffect = statusEffects.FirstOrDefault(se => se.EffectName.Equals(effectName));

                    statusEffect.Effect.Timer = statusEffect.Effect.Time;
                }
                else
                {
                    statusEffects.Add(new StatusEffect(entity, effectName, effect));
                }
            }
            else
            {
                statusEffects.Add(new StatusEffect(entity, effectName, effect));
            }
        }

        public static StatusEffectManager Instance
        {
            get
            {
                instance ??= new StatusEffectManager();
                return instance;
            }
        }
    }

    public class StatusEffect(Entity entity, string effectName, Effect effect)
    {
        public Entity TargetEntity { get; set; } = entity;
        public string EffectName { get; set; } = effectName;
        public Effect Effect { get; set; } = effect;
    }
}
