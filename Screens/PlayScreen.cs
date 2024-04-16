using Medicraft.Data.Models;
using Medicraft.Systems.Spawners;
using Medicraft.Systems.TilemapRenderer;
using Medicraft.Systems;
using System.Collections.Generic;
using TiledSharp;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Medicraft.Systems.Managers;
using Microsoft.Xna.Framework.Media;

namespace Medicraft.Screens
{
    public class PlayScreen : Screen
    {
        protected TilemapOrthogonalRender tileMapRender;
        protected TmxMap tileMap;

        protected HUDSystem hudSystem;
        protected DrawEffectSystem drawEffectSystem;

        protected List<EntityData> entityDatas = [];
        protected List<ObjectData> objectDatas = [];

        protected float volumeScale;
        public bool startingBG;

        public PlayScreen()
        {
            volumeScale = 0f;
            startingBG = true;
            GameGlobals.Instance.IsMainBGEnding = false;

            // Toggle the PlayScreen GUI flag
            UIManager.Instance.CurrentUI = UIManager.PlayScreen;
            GameGlobals.Instance.IsOpenMainMenu = false;
            GameGlobals.Instance.IsRefreshPlayScreenUI = false;
        }

        public override void LoadContent()
        {
            base.LoadContent();

            // Set player position
            PlayerManager.Instance.SetupPlayerPosition();
        }

        public override void UnloadContent()
        {
            EntityManager.Instance.ClosestEnemyToCompanion = null;

            // Clear List Entity, GameObject and MusicBG
            EntityManager.Instance.ClearEntity();
            ObjectManager.Instance.ClearGameObject();
            GameGlobals.Instance.CurrentMapMusics.Clear();

            hudSystem = null;
            drawEffectSystem = null;

            base.UnloadContent();
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public override void Update(GameTime gameTime)
        {
            GameGlobals.UpdateMediaPlayerVolumeScale(volumeScale);

            var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (startingBG)
            {
                volumeScale += deltaSeconds * 0.1f;

                if (volumeScale >= 1f)
                {
                    volumeScale = 1f;
                    startingBG = false;
                }
            }

            if (GameGlobals.Instance.IsMainBGEnding)
            {
                volumeScale -= deltaSeconds * 0.8f;

                if (volumeScale <= 0f)
                {
                    volumeScale = 0f;
                    MediaPlayer.Stop();
                    GameGlobals.Instance.IsMainBGEnding = false;
                }
            }

            EntityManager.Instance.Update(gameTime);

            ObjectManager.Instance.Update(gameTime);

            if (!GameGlobals.Instance.IsShowPath)
                tileMapRender?.Update(gameTime);

            drawEffectSystem?.Update(gameTime);

            hudSystem?.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            EntityManager.Instance.Draw(spriteBatch);

            ObjectManager.Instance.Draw(spriteBatch);

            if (!GameGlobals.Instance.IsShowPath)
                tileMapRender?.Draw(spriteBatch);

            drawEffectSystem?.Draw(spriteBatch);

            if (!UIManager.Instance.IsShowDialogUI || !PlayerManager.Instance.IsPlayerDead)
                hudSystem?.Draw(spriteBatch);
        }
    }
}
