﻿using Medicraft.Data.Models;
using Medicraft.Systems.Spawners;
using Medicraft.Systems.TilemapRenderer;
using Medicraft.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiledSharp;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Medicraft.Systems.Managers;

namespace Medicraft.Screens
{
    public class PlayScreen : Screen
    {
        protected TilemapOrthogonalRender _tileMapRender;
        protected TmxMap _tileMap;

        protected HUDSystem _hudSystem;
        protected DrawEffectSystem _drawEffectSystem;

        protected List<EntityData> _entityDatas;
        protected MobSpawner _mobSpawner;

        protected List<ObjectData> _objectDatas;
        protected ObjectSpawner _objectSpawner;

        public PlayScreen()
        {
            // Toggle the PlayScreen GUI flag
            GUIManager.Instance.CurrentGUI = GUIManager.PlayScreen;
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
            EntityManager.Instance.ClosestEnemy = null;

            // Clear List Entity, GameObject and MusicBG
            EntityManager.Instance.ClearEntity();
            ObjectManager.Instance.ClearGameObject();
            GameGlobals.Instance.CurrentMapMusics.Clear();

            base.UnloadContent();
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public override void Update(GameTime gameTime)
        {
            EntityManager.Instance.Update(gameTime);

            ObjectManager.Instance.Update(gameTime);

            if (!GameGlobals.Instance.IsShowPath)
                _tileMapRender?.Update(gameTime);

            _drawEffectSystem?.Update(gameTime);

            _hudSystem?.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            EntityManager.Instance.Draw(spriteBatch);

            ObjectManager.Instance.Draw(spriteBatch);

            if (!GameGlobals.Instance.IsShowPath)
                _tileMapRender?.Draw(spriteBatch);

            _drawEffectSystem?.Draw(spriteBatch);

            _hudSystem?.Draw(spriteBatch);
        }
    }
}
