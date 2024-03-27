using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Medicraft.Systems;
using MonoGame.Extended.BitmapFonts;
using Medicraft.Systems.Managers;

namespace Medicraft.Screens
{
    public class SplashScreen : Screen
    {
        private Color _color;

        private Texture2D _logo;
        private BitmapFont _font;

        private bool _show;
        private float _alpha;
        private float _alphaTime;
        private int _index;

        private bool _isClicked;

        public SplashScreen()
        {
            _isClicked = false;
            _show = true;
            _index = 0;
            _alpha = 0f;
            _alphaTime = 0;
        }

        public override void LoadContent()
        {
            base.LoadContent();

            _font = _content.Load<BitmapFont>("fonts/Mincraft_Ten/Mincraft_Ten");
            _logo = _content.Load<Texture2D>("gui/logo_wakeup");

            GameGlobals.Instance.TestIcon = _logo;

            ScreenManager.Camera.ResetCameraPosition(false);
            ScreenManager.Camera.SetPosition(GameGlobals.Instance.GameScreenCenter);
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
            // Mouse Control
            var mouseCur = GameGlobals.Instance.CurMouse;
            var mousePrev = GameGlobals.Instance.PrevMouse;

            var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_show)
            {
                _alphaTime += deltaSeconds;

                if (_alpha < 1f) _alpha += deltaSeconds * 0.5f;

                if (_alphaTime >= 3f)
                {
                    _show = false;

                    if (_index == 3)
                    {
                        ScreenManager.Instance.TranstisionToScreen(ScreenManager.GameScreen.MainMenuScreen);
                    }
                }
            }
            else
            {                                     
                _alphaTime -= deltaSeconds;

                if (_alpha > 0f && _alphaTime <= 2f) _alpha -= deltaSeconds;

                if (_alphaTime <= 0f)
                {
                    _show = true;
                    _index++;
                }
            }

            if (mouseCur.LeftButton == ButtonState.Pressed && mousePrev.LeftButton == ButtonState.Released
                && !_index.Equals(3) && !_isClicked)
            {
                _isClicked = true;
                ScreenManager.Instance.TranstisionToScreen(ScreenManager.GameScreen.MainMenuScreen);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            string textString1;
            string textString2;
            string textString3;
            string textString4;
            Vector2 textSize1;
            Vector2 textSize2;
            Vector2 textSize3;
            Vector2 textSize4;
            float totalHeight;
            Vector2 position;

            switch (_index)
            {
                case 0:
                    Vector2 logoSize = new Vector2(_logo.Width, _logo.Height) * 0.5f;

                    position = new Vector2((GameGlobals.Instance.GameScreen.X - logoSize.X) / 2
                        , (GameGlobals.Instance.GameScreen.Y - logoSize.Y) / 2);

                    spriteBatch.Draw(_logo, position, null, Color.White * _alpha, 0f, Vector2.Zero, 0.5f
                        , SpriteEffects.None, 0f);                 
                    break;

                case 1:
                    textString1 = "This game is part of the Special Problems";
                    textString2 = "Study program for the Bachelor of Science degree.";
                    textString3 = "Department of Computer Science Faculty of Science";
                    textString4 = "King Mongkut's Institute of Technology Ladkrabang Academic year 2023";

                    textSize1 = _font.MeasureString(textString1);
                    textSize2 = _font.MeasureString(textString2);
                    textSize3 = _font.MeasureString(textString3);
                    textSize4 = _font.MeasureString(textString4);

                    totalHeight = textSize1.Y + textSize2.Y + textSize3.Y + textSize4.Y;
                    position = new Vector2((GameGlobals.Instance.GameScreen.X - textSize1.X) / 2
                        , (GameGlobals.Instance.GameScreen.Y - totalHeight) / 2);                                    

                    spriteBatch.DrawString(_font, textString1, position, Color.DodgerBlue * _alpha, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                    position.X += (textSize1.X - _font.MeasureString(textString2).Width) / 2;
                    position.Y += textSize1.Y;

                    spriteBatch.DrawString(_font, textString2, position, Color.SlateBlue * _alpha, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                    position.X += (textSize2.X - _font.MeasureString(textString3).Width) / 2;
                    position.Y += textSize2.Y;

                    spriteBatch.DrawString(_font, textString3, position, Color.DodgerBlue * _alpha, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                    position.X += (textSize3.X - _font.MeasureString(textString4).Width) / 2;
                    position.Y += textSize3.Y;

                    spriteBatch.DrawString(_font, textString4, position, Color.SlateBlue * _alpha, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                    break;

                case 2:
                    textString1 = "Presented by";
                    textString2 = "Phakorn Pasawast & Wanatchaporn Sanguanchua";
                    textSize1 = _font.MeasureString(textString1);
                    textSize2 = _font.MeasureString(textString2);

                    totalHeight = textSize1.Y + textSize2.Y;
                    position = new Vector2((GameGlobals.Instance.GameScreen.X - textSize1.X) / 2
                        , (GameGlobals.Instance.GameScreen.Y - totalHeight) / 2);
          
                    spriteBatch.DrawString(_font, textString1, position, Color.DodgerBlue * _alpha, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                    position.X += (textSize1.X - _font.MeasureString(textString2).Width) / 2;
                    position.Y += textSize1.Y;

                    spriteBatch.DrawString(_font, textString2, position, Color.SlateBlue * _alpha, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                    break;
            }
        }
    }
}
