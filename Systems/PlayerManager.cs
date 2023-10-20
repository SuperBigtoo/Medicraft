using Medicraft.Data.Models;
using Medicraft.Entities;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Sprites;

namespace Medicraft.Systems
{
    public class PlayerManager
    {
        private Player player;

        private static PlayerManager instance;
        private PlayerManager()
        {
        }

        public void Initialize(AnimatedSprite playerSprite, PlayerStats playerStats)
        {
            if (Singleton.Instance.gameSave.Count != 0)
            {
                var gameSave = Singleton.Instance.gameSave[Singleton.Instance.gameSaveIdex];
                playerStats = gameSave.PlayerStats;

                Singleton.Instance.cameraPosition = new Vector2((float)gameSave.Camera_Position[0]
                    , (float)gameSave.Camera_Position[1]);

                Singleton.Instance.addingHudPos = new Vector2((float)gameSave.HUD_Position[0]
                    , (float)gameSave.HUD_Position[1]);

                player = new Player(playerSprite, playerStats);
            }
            else
            {
                player = new Player(playerSprite, playerStats);
            }
        }

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

        public Player GetPlayer() 
        {
            return player;
        }
    }
}
