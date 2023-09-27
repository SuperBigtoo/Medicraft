using Medicraft.Entities;
using Microsoft.Xna.Framework;

namespace Medicraft.Systems
{
    public class PlayerManager
    {
        public Player player;

        private static PlayerManager instance;
        private PlayerManager() {}

        public static PlayerManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new PlayerManager();
                }
                return instance;
            }
        }
    }
}
