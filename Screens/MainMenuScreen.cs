using Medicraft.Systems;
using Medicraft.Systems.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace Medicraft.Screens
{
    public class MainMenuScreen : Screen
    {
        private Texture2D _mainMenuBG;

        private float _volumeScale;
        private bool _startingBG;

        public MainMenuScreen()
        {
            _volumeScale = 0f;
            _startingBG = true;
            GameGlobals.Instance.IsMainBGEnding = false;

            // Toggle Pause PlayScreen
            GameGlobals.Instance.IsGamePause = false;
            GameGlobals.Instance.IsOpenGUI = false;

            // Toggle the IsOpenPauseMenu flag
            GameGlobals.Instance.IsOpenPauseMenu = false;
            GameGlobals.Instance.IsOpenMainMenu = false;
            UIManager.Instance.CurrentUI = UIManager.MainMenu;

            Camera.ResetCameraPosition(false);
            ScreenManager.Camera.SetPosition(GameGlobals.Instance.DefaultAdapterViewport / 2);
        }

        public override void LoadContent()
        {
            base.LoadContent();

            UIManager.Instance.CurrentUI = UIManager.MainMenu;

            _mainMenuBG = content.Load<Texture2D>("gui/main_menu_screen");

            Song bgMusic = GameGlobals.AddCurrentMapMusic(GameGlobals.Music.kokoro_hiraite, content);

            GameGlobals.PlayBackgroundMusic(bgMusic, true, _volumeScale);
        }

        public override void UnloadContent()
        {
            GameGlobals.Instance.CurrentMapMusics.Clear();

            base.UnloadContent();
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public override void Update(GameTime gameTime)
        {
            GameGlobals.UpdateMediaPlayerVolumeScale(_volumeScale);

            var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_startingBG)
            {
                _volumeScale += deltaSeconds * 0.1f;

                if (_volumeScale >= 1f)
                {
                    _volumeScale = 1f;
                    _startingBG = false;
                }  
            }          

            if (GameGlobals.Instance.IsMainBGEnding)
            {
                _volumeScale -= deltaSeconds * 0.8f;

                if (_volumeScale <= 0f)
                {
                    _volumeScale = 0f;              
                    MediaPlayer.Stop();
                    GameGlobals.Instance.IsMainBGEnding = false;
                }
            }
        }      

        public override void Draw(SpriteBatch spriteBatch)
        {
            var screenOffSet = new Vector2(
                (GameGlobals.Instance.DefaultAdapterViewport.X - GameGlobals.Instance.GameScreen.X) / 6, 0);

            spriteBatch.Draw(_mainMenuBG, ScreenManager.Camera.GetViewportPosition() - screenOffSet, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

            var alphaColor = 0.2f;
            if (UIManager.Instance.IsClickedLoadButton)
                alphaColor = 0.8f;

            spriteBatch.End();
            ScreenManager.DrawBackgound(spriteBatch, Color.Black, alphaColor, false);
        }
    }
}
