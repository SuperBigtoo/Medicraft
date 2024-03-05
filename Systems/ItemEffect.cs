using Medicraft.Data.Models;
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
            _itemEffectData = GameGlobals.Instance.ItemEffectDatas.FirstOrDefault(i => i.ItemEffectId.Equals(itemEffectId));
            _name = _itemEffectData.Name;
            _category = _itemEffectData.Category;
            _description = _itemEffectData.Description;
        }

        public bool Activate()
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
                                var valueHP = (int)(PlayerManager.Instance.Player.BaseMaxHP * effect.Value);

                                isItemUesd = PlayerManager.Instance.Player.RestoresHP(_name, valueHP, true);
                            }
                            else if (effect.Target.Equals("Mana"))
                            {
                                var valueMana = PlayerManager.Instance.Player.BaseMaxMana * (float)effect.Value;

                                isItemUesd = PlayerManager.Instance.Player.RestoresMana(_name, valueMana, true);
                            }
                        }
                        else if (effect.ActivationType.Equals("Periodic"))
                        {
                            StatusEffectManager.Instance.AddStatusEffect(PlayerManager.Instance.Player
                                , effect.EffectType + effect.Target + _name, effect);

                            isItemUesd = true;
                        }
                    }
                    break;

                case "Boosting":

                    foreach (var effect in _itemEffectData.Effects)
                    {
                        StatusEffectManager.Instance.AddStatusEffect(PlayerManager.Instance.Player
                            , effect.EffectType + effect.Target + _name, effect);

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
