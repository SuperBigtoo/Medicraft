﻿using Medicraft.Data.Models;
using Medicraft.Entities;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Medicraft.Systems.Managers
{
    public class StatusEffectManager
    {
        public List<StatusEffect> StatusEffects { get; private set; }

        public List<StatusEffect> EffectsToRemove { get; private set; }

        private const float _activateTime = 3f;

        private float _deltaSeconds, _activateTimer = 0;

        private static StatusEffectManager instance;

        private StatusEffectManager()
        {
            StatusEffects = [];
            EffectsToRemove = [];
        }

        public void Update(GameTime gameTime)
        {
            _deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // If statusEffects has any Effect ActivationType = Periodic then do timer else reset timer
            if (StatusEffects.Any(statusEffect => statusEffect.Effect.ActivationType.Equals("Periodic")))
            {
                _activateTimer -= _deltaSeconds;
            }
            else _activateTimer = 0;

            foreach (var statusEffect in StatusEffects)
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

                    EffectsToRemove.Add(statusEffect);
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

                            var periodicEffects = StatusEffects.Where(
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
            foreach (var effectToRemove in EffectsToRemove)
            {
                StatusEffects.Remove(effectToRemove);
            }
            EffectsToRemove.Clear();
        }

        private void RegenerateEffect(StatusEffect statusEffect)
        {
            switch (statusEffect.Effect.Target)
            {
                case "HP":
                    var valueHP = (int)(statusEffect.TargetEntity.BaseMaxHP * statusEffect.Effect.Value);

                    if (statusEffect.TargetEntity.Name.Equals("Noah"))
                    {
                        PlayerManager.Instance.Player.RestoresHP(statusEffect.Effect.EffectType, valueHP, false);
                    }
                    else statusEffect.TargetEntity.HP += valueHP;

                    break;

                case "Mana":
                    var valueMana = statusEffect.TargetEntity.BaseMaxMana * (float)statusEffect.Effect.Value;

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
            // Buff Activate | Debuff Deactivate
            switch (statusEffect.Effect.Target)
            {
                case "MaxHP":
                    var valueMaxHP = (int)(statusEffect.TargetEntity.BaseMaxHP * statusEffect.Effect.Value);
                    var valueHP = (int)(statusEffect.TargetEntity.HP * statusEffect.Effect.Value);

                    statusEffect.TargetEntity.MaxHP = isBuff ? statusEffect.TargetEntity.MaxHP + valueMaxHP : statusEffect.TargetEntity.MaxHP - valueMaxHP;
                    statusEffect.TargetEntity.HP = isBuff ? statusEffect.TargetEntity.HP + valueHP : statusEffect.TargetEntity.HP - valueHP;
                    break;

                case "MaxMana":
                    var valueMaxMana = (int)(statusEffect.TargetEntity.BaseMaxMana * statusEffect.Effect.Value);
                    var valueMana = (int)(statusEffect.TargetEntity.Mana * statusEffect.Effect.Value);

                    statusEffect.TargetEntity.MaxMana = isBuff ? statusEffect.TargetEntity.MaxMana + valueMaxMana : statusEffect.TargetEntity.MaxMana - valueMaxMana;
                    statusEffect.TargetEntity.Mana = isBuff ? statusEffect.TargetEntity.Mana + valueMana : statusEffect.TargetEntity.Mana - valueMana;
                    break;

                case "ATK":
                    var valueATK = (int)(statusEffect.TargetEntity.BaseATK * statusEffect.Effect.Value);

                    statusEffect.TargetEntity.ATK = isBuff ? statusEffect.TargetEntity.ATK + valueATK : statusEffect.TargetEntity.ATK - valueATK;
                    break;

                case "DEF":
                    var valueDEF = (float)statusEffect.Effect.Value;

                    statusEffect.TargetEntity.DEF = isBuff ? statusEffect.TargetEntity.DEF + valueDEF : statusEffect.TargetEntity.DEF - valueDEF;
                    break;

                case "Crit":
                    var valueCrit = (float)statusEffect.Effect.Value;

                    statusEffect.TargetEntity.Crit = isBuff ? statusEffect.TargetEntity.Crit + valueCrit : statusEffect.TargetEntity.Crit - valueCrit;
                    break;

                case "CritDMG":
                    var valueCritDMG = (float)statusEffect.Effect.Value;

                    statusEffect.TargetEntity.CritDMG = isBuff ? statusEffect.TargetEntity.CritDMG + valueCritDMG : statusEffect.TargetEntity.CritDMG - valueCritDMG;
                    break;

                case "Evasion":
                    var valueEvasion = (float)statusEffect.Effect.Value;

                    statusEffect.TargetEntity.Evasion = isBuff ? statusEffect.TargetEntity.Evasion + valueEvasion : statusEffect.TargetEntity.Evasion - valueEvasion;
                    break;

                case "Speed":
                    var valueSpeed = (int)(statusEffect.TargetEntity.BaseSpeed * statusEffect.Effect.Value);

                    statusEffect.TargetEntity.Speed = isBuff ? statusEffect.TargetEntity.Speed + valueSpeed : statusEffect.TargetEntity.Speed - valueSpeed;
                    break;
            }
        }

        public void AddStatusEffect(Entity entity, string effectName, Effect effect)
        {
            //if (StatusEffects.Count != 0)
            //{
            //    if (StatusEffects.Any(statusEffect => statusEffect.EffectName.Equals(effectName)))
            //    {
            //        var statusEffect = StatusEffects.FirstOrDefault(se => se.EffectName.Equals(effectName));

            //        statusEffect.Effect.Timer = statusEffect.Effect.Time;
            //    }
            //    else
            //    {
            //        StatusEffects.Add(new StatusEffect(entity, effectName, effect));
            //    }
            //}
            //else
            //{
            //    StatusEffects.Add(new StatusEffect(entity, effectName, effect));
            //}

            // Check if any status effect with the given name exists
            var existingEffect = StatusEffects.FirstOrDefault(se => se.EffectName.Equals(effectName));

            if (existingEffect != null)
            {
                // Update the timer of the existing status effect
                existingEffect.Effect.Timer = existingEffect.Effect.Time;
            }
            else
            {
                // Add a new status effect
                StatusEffects.Add(new StatusEffect(entity, effectName, effect));
            }

            // Check if it a Buff or Debuff
            if (effect.EffectType.Equals("Buff"))
            {
                entity.BuffTimer = effect.Time;
            }
            else if (effect.EffectType.Equals("Debuff"))
            {
                entity.DebuffTimer = effect.Time;
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
