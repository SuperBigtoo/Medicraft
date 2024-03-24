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
        }

        public override void LoadContent()
        {
            base.LoadContent();

            GUIManager.Instance.CurrentGUI = GUIManager.MainMenu;

            _mainMenuBG = _content.Load<Texture2D>("gui/main_menu_screen");

            Song _bgMusic = GameGlobals.Instance.AddCurrentMapMusic(GameGlobals.Music.kokoro_hiraite, _content);

            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = GameGlobals.Instance.BackgroundMusicVolume * _volumeScale;
            MediaPlayer.Play(_bgMusic);
        }

        public override void UnloadContent()
        {
            base.UnloadContent();
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public override void Update(GameTime gameTime)
        {
            MediaPlayer.Volume = GameGlobals.Instance.BackgroundMusicVolume * _volumeScale;


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
                _volumeScale -= deltaSeconds * 0.75f;

                if (_volumeScale <= 0f)
                {
                    _volumeScale = 0f;              
                    MediaPlayer.Stop();
                    GameGlobals.Instance.IsMainBGEnding = false;
                }
            }
        }

        public static void PlayTime()
        {
            GameGlobals.Instance.IsMainBGEnding = true;
            PlayerManager.Instance.Initialize(true);
            GameGlobals.Instance.InitialCameraPos = GameGlobals.Instance.GameScreenCenter;
            ScreenManager.Instance.TranstisionToScreen(ScreenManager.GameScreen.TestScreen);

            // Toggle the IsOpenMainMenu flag
            GUIManager.Instance.CurrentGUI = GUIManager.PlayScreen;
            GameGlobals.Instance.IsOpenMainMenu = false;
            GameGlobals.Instance.IsRefreshPlayScreenUI = false;          
        }

        public override void Draw(SpriteBatch spriteBatch)
        {      
            spriteBatch.Draw(_mainMenuBG, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            ScreenManager.DrawBackgound(spriteBatch, Color.Black, 0.2f);
        }
    }
}
