using Medicraft.Data.Models;
using Medicraft.Entities;
using Medicraft.Systems.Managers;
using System.Linq;

namespace Medicraft.Systems
{
    public class ItemEffect
    {
        private readonly ItemEffectData _itemEffectData;

        private readonly string _name, _category, _description;    

        public ItemEffect(int itemEffectId)
        {
            _itemEffectData = GameGlobals.Instance.ItemEffectDatas.FirstOrDefault
                (i => i.ItemEffectId.Equals(itemEffectId));
            _name = _itemEffectData.Name;
            _category = _itemEffectData.Category;
            _description = _itemEffectData.Description;
        }

        public bool Activate(Entity entityTarget)
        {
            bool isItemUesd = false;

            switch (_category)
            {
                case "Recovery":

                    foreach (var effect in _itemEffectData.Effects)
                    {                      
                        if (effect.ActivationType.Equals("Instant"))
                        {
                            if (effect.Target.Equals("HP"))
                            {
                                var valueHP = (int)(entityTarget.BaseMaxHP * effect.Value);

                                isItemUesd = entityTarget.RestoresHP(_name, valueHP, true);
                            }
                            else if (effect.Target.Equals("Mana"))
                            {
                                var valueMana = entityTarget.BaseMaxMana * (float)effect.Value;

                                isItemUesd = entityTarget.RestoresMana(_name, valueMana, true);
                            }
                        }
                        else if (effect.ActivationType.Equals("Periodic"))
                        {
                            StatusEffectManager.Instance.AddStatusEffect(
                                entityTarget,
                                entityTarget.Name,
                                effect.EffectType + effect.Target + _name,
                                effect);

                            isItemUesd = true;
                        }
                    }
                    break;

                case "Boosting":

                    foreach (var effect in _itemEffectData.Effects)
                    {
                        StatusEffectManager.Instance.AddStatusEffect(
                            entityTarget,
                            entityTarget.Name,
                            effect.EffectType + effect.Target + _name,
                            effect);

                        isItemUesd = true;
                    }                    
                    break;
            }

            return isItemUesd;
        }

        public string GetName()
        {
            return _name;
        }

        public string GetCategory()
        {
            return _category;
        }

        public string GetDescription()
        {
            return _description;
        }
    }
}
