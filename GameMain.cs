using Medicraft.Data.Models;
using Medicraft.Entites;
using Medicraft.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Serialization;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.Content;

namespace Medicraft
{
    public class GameMain : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private readonly EntityManager _entityManager;
        private readonly Singleton _singleton;
        private EntityStats _slime;
        private BitmapFont _font;

        public GameMain()
        {
            _singleton = Singleton.Instance;
            _entityManager = new EntityManager();
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            _graphics.PreferredBackBufferWidth = (int)_singleton.gameScreen.X;
            _graphics.PreferredBackBufferHeight = (int)_singleton.gameScreen.Y;
            _graphics.SynchronizeWithVerticalRetrace = false;
            Window.Position = new Point((GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width / 2) - (_graphics.PreferredBackBufferWidth / 2)
                , (GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height / 2) - (_graphics.PreferredBackBufferHeight / 2));
            _graphics.ApplyChanges();

            Singleton.Instance._playerPosition = new Vector2((_singleton.gameScreen.X - 70) / 2, (_singleton.gameScreen.Y - 110) / 2);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            // Test Player
            var _spriteSheet = Content.Load<SpriteSheet>("animation/MC1_animation.sf", new JsonContentLoader());
            var _playerSprite = new AnimatedSprite(_spriteSheet);
            Singleton.Instance._player = _entityManager.AddEntity(new Player(_playerSprite, Singleton.Instance._playerPosition));

            // Test Load Data
            // Load Data Game from JSON file
            _slime = Content.Load<EntityStats>("data/models/slime");
            // Load bitmap font
            _font = Content.Load<BitmapFont>("fonts/Mincraft_Ten/Mincraft_Ten");
        }

        protected override void UnloadContent() { }

        protected override void Update(GameTime gameTime)
        {
            Singleton.Instance.IsGameActive = IsActive;

            if (IsActive)
            {
                if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                    Exit();
            }

            // TODO: Add your update logic here
            _entityManager.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            // Test Sceen
            _spriteBatch.Begin();
            _spriteBatch.DrawString(_font, _slime.Name, new Vector2(50, 50), Color.White);
            _spriteBatch.DrawString(_font, "" + _slime.HP, new Vector2(50, 80), Color.White);
            _spriteBatch.DrawString(_font, "" + _slime.Atk, new Vector2(50, 110), Color.White);
            _spriteBatch.DrawString(_font, "" + _slime.Def, new Vector2(50, 140), Color.White);

            _entityManager.Draw(_spriteBatch);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}