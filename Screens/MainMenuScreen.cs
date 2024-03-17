using Medicraft.Systems;
using Medicraft.Systems.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Medicraft.Screens
{
    public class MainMenuScreen : Screen
    {
        private float _volumeScale;
        private bool _startingBG, _endingBG;

        public MainMenuScreen()
        {
            _volumeScale = 0f;
            _startingBG = true;
            _endingBG = false;
        }

        public override void LoadContent()
        {
            base.LoadContent();

            var bgMusic = GameGlobals.Instance.GetBackgroundMusic(GameGlobals.Music.kokoro_hiraite);
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = GameGlobals.Instance.BackgroundMusicVolume * _volumeScale;
            MediaPlayer.Play(bgMusic);
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
            GameGlobals.Instance.PrevMouse = GameGlobals.Instance.CurMouse;
            GameGlobals.Instance.CurMouse = Mouse.GetState();
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

            if (GameGlobals.Instance.CurMouse.LeftButton == ButtonState.Pressed
                    && GameGlobals.Instance.PrevMouse.LeftButton == ButtonState.Released)
            {
                _endingBG = true;
                ScreenManager.Instance.TranstisionToScreen(ScreenManager.GameScreen.TestScreen);
            }

            if (_endingBG)
            {
                _volumeScale -= deltaSeconds * 0.75f;

                if (_volumeScale <= 0f)
                {
                    _volumeScale = 0f;
                    _endingBG = false;
                    MediaPlayer.Stop();
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            ScreenManager.Instance.GraphicsDevice.Clear(Color.CornflowerBlue);
        }
    }
}
