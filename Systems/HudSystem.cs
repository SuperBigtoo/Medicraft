using Medicraft.Data.Models;
using Medicraft.Systems.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Sprites;
using System.Linq;

namespace Medicraft.Systems
{
    public class HudSystem
    {
        private readonly BitmapFont[] _fonts;

        private Vector2 _hudTopLeftCorner;

        private Texture2D _heartTexture, _herb1Texture, _herb2Texture, _drugTexture
            , _coinTexture, _pressFTexture, _insufficient;

        private readonly AnimatedSprite _spriteItemPack;

        private float _deltaSeconds;

        private bool _nextFeed;

        private float _insufficientTime;
        private readonly float _maxDisplayTime = 3f;

        public HudSystem(BitmapFont[] fonts, Texture2D[] textures, AnimatedSprite sprite)
        {
            _fonts = fonts;

            _heartTexture = textures[0];
            _herb1Texture = textures[1];
            _herb2Texture = textures[2];
            _drugTexture = textures[3];
            _coinTexture = textures[4];
            _pressFTexture = textures[5];
            _insufficient = textures[6];

            _spriteItemPack = sprite;

            _nextFeed = false;
            _insufficientTime = _maxDisplayTime;

            GameGlobals.Instance.HUDPosition = PlayerManager.Instance.Player.Position - new Vector2(720, 450);
        }

        public void Update(GameTime gameTime)
        {
            _deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (GameGlobals.Instance.CollectedItemFeed.Count != 0)
            {
                GameGlobals.Instance.DisplayFeedTime -= _deltaSeconds;

                if (GameGlobals.Instance.DisplayFeedTime <= 0)
                {
                    _nextFeed = true;
                    GameGlobals.Instance.DisplayFeedTime = GameGlobals.Instance.MaximumDisplayFeedTime;
                }
                else _nextFeed = false;
            }

            if (GameGlobals.Instance.ShowInsufficientSign)
            {
                _insufficientTime -= _deltaSeconds;

                if (_insufficientTime <= 0)
                {
                    GameGlobals.Instance.ShowInsufficientSign = false;
                    _insufficientTime = _maxDisplayTime;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            var _graphicsDevice = ScreenManager.Instance.GraphicsDevice;
            _hudTopLeftCorner = GameGlobals.Instance.HUDPosition;

            spriteBatch.End();
            spriteBatch.Begin(
                SpriteSortMode.Deferred,
                samplerState: SamplerState.PointClamp,
                blendState: BlendState.AlphaBlend,
                transformMatrix: ScreenManager.Instance.Camera.GetTransform(_graphicsDevice.Viewport.Width
                , _graphicsDevice.Viewport.Height)
            );

            // Draw HP mobs
            DrawHPMobs(spriteBatch);

            // Draw combat numbers mobs & player
            DrawCombatNumbers(spriteBatch);

            // Draw Press F Sign
            DrawPressF(spriteBatch);

            // Draw Insufficient Sign
            DrawInsufficientSign(spriteBatch);

            // Draw Feed Items
            DrawCollectedItem(spriteBatch);

            // Draw Quest List
            DrawQuestList(spriteBatch);

            // Draw HUD Bar
            DrawHudBar(spriteBatch);

            // dis one here for testing
            var rect = new Rectangle((int)_hudTopLeftCorner.X + 50, (int)_hudTopLeftCorner.Y + 55, 200, 300);

            spriteBatch.DrawString(_fonts[3], $"Player ATK: {PlayerManager.Instance.Player.ATK}"
                , new Vector2(55f, 60f) + _hudTopLeftCorner, Color.White, clippingRectangle: rect);

            spriteBatch.DrawString(_fonts[3], $"Player Crit: {PlayerManager.Instance.Player.Crit_Percent}"
                , new Vector2(55f, 75f) + _hudTopLeftCorner, Color.White, clippingRectangle: rect);

            spriteBatch.DrawString(_fonts[3], $"Player CritDMG: {PlayerManager.Instance.Player.CritDMG_Percent}"
                , new Vector2(55f, 90f) + _hudTopLeftCorner, Color.White, clippingRectangle: rect);

            spriteBatch.DrawString(_fonts[3], $"Player DEF: {PlayerManager.Instance.Player.DEF_Percent}"
                , new Vector2(55f, 105f) + _hudTopLeftCorner, Color.White, clippingRectangle: rect);

            spriteBatch.DrawString(_fonts[3], $"Cooldown Normal Skill: {PlayerManager.Instance.Player.NormalCooldownTimer}"
                , new Vector2(55f, 135f) + _hudTopLeftCorner, Color.White);

            spriteBatch.DrawString(_fonts[3], $"Cooldown Normal Skill: {PlayerManager.Instance.Player.BurstCooldownTimer}"
                , new Vector2(55f, 150f) + _hudTopLeftCorner, Color.White);

            spriteBatch.DrawString(_fonts[3], $"Cooldown Normal Skill: {PlayerManager.Instance.Player.PassiveCooldownTimer}"
                , new Vector2(55f, 165f) + _hudTopLeftCorner, Color.White);

            spriteBatch.DrawString(_fonts[3], $"Normal Skill Time: {PlayerManager.Instance.Player.NormalActivatedTimer}"
                , new Vector2(55f, 200f) + _hudTopLeftCorner, Color.White);

            spriteBatch.DrawString(_fonts[3], $"Passive Skill Time: {PlayerManager.Instance.Player.PassiveActivatedTimer}"
                , new Vector2(55f, 215f) + _hudTopLeftCorner, Color.White);
        }

        private void DrawHudBar(SpriteBatch spriteBatch)
        {
            spriteBatch.FillRectangle(-720 + _hudTopLeftCorner.X, 0 + _hudTopLeftCorner.Y, 2640
                , 25, Color.Black * 0.4f);

            spriteBatch.DrawString(_fonts[0], $" Mobs: {EntityManager.Instance.entities.Count}"
                , Vector2.Zero + _hudTopLeftCorner, Color.White);

            spriteBatch.DrawString(_fonts[0], $"Time Spawn: {(int)EntityManager.Instance.spawnTime}"
                , new Vector2(100f, 0f) + _hudTopLeftCorner, Color.White);

            spriteBatch.DrawString(_fonts[0], $"X: {(int)PlayerManager.Instance.Player.Position.X}"
                , new Vector2(240f, 0f) + _hudTopLeftCorner, Color.White);

            spriteBatch.DrawString(_fonts[0], $"Y: {(int)PlayerManager.Instance.Player.Position.Y}"
                , new Vector2(320f, 0f) + _hudTopLeftCorner, Color.White);

            var textString1 = "Player HP: " + PlayerManager.Instance.Player.HP;
            Vector2 textSize1 = _fonts[0].MeasureString(textString1);
            var position = new Vector2((GameGlobals.Instance.GameScreen.X - textSize1.X) / 2, 0);
            //position.X += (string.X - font.MeasureString(string).X) / 2;

            spriteBatch.Draw(_heartTexture, new Vector2(position.X - 32f, 0f) + _hudTopLeftCorner, null
                , Color.White, 0f, Vector2.Zero, 0.75f, SpriteEffects.None, 0f);

            spriteBatch.DrawString(_fonts[0], textString1, new Vector2(position.X, 0f) + _hudTopLeftCorner, Color.White);

            if (InventoryManager.Instance.InventoryBag.TryGetValue("0", out InventoryItemData value_0))
            {
                var _transform = new Transform2
                {
                    Scale = new Vector2(0.75f, 0.75f),
                    Rotation = 0f,
                    Position = new Vector2(position.X + 400f, 0f) + _hudTopLeftCorner
                };

                _spriteItemPack.Play("0");
                _spriteItemPack.Update(_deltaSeconds);

                spriteBatch.Draw(_spriteItemPack, _transform);

                spriteBatch.DrawString(_fonts[0], $" {value_0.Count}"
                    , new Vector2(position.X + 400f + 32f, 0f) + _hudTopLeftCorner, Color.White);
            }

            if (InventoryManager.Instance.InventoryBag.TryGetValue("1", out InventoryItemData value_1))
            {
                var _transform = new Transform2
                {
                    Scale = new Vector2(0.75f, 0.75f),
                    Rotation = 0f,
                    Position = new Vector2(position.X + 480f, 0f) + _hudTopLeftCorner
                };

                _spriteItemPack.Play("1");
                _spriteItemPack.Update(_deltaSeconds);

                spriteBatch.Draw(_spriteItemPack, _transform);

                spriteBatch.DrawString(_fonts[0], $" {value_1.Count}"
                    , new Vector2(position.X + 480f + 32f, 0f) + _hudTopLeftCorner, Color.White);
            }

            if (InventoryManager.Instance.InventoryBag.TryGetValue("312", out InventoryItemData value_2))
            {
                var _transform = new Transform2
                {
                    Scale = new Vector2(0.75f, 0.75f),
                    Rotation = 0f,
                    Position = new Vector2(position.X + 560f, 0f) + _hudTopLeftCorner
                };

                _spriteItemPack.Play("312");
                _spriteItemPack.Update(_deltaSeconds);

                spriteBatch.Draw(_spriteItemPack, _transform);

                spriteBatch.DrawString(_fonts[0], $" {value_2.Count}"
                    , new Vector2(position.X + 560f + 32f, 0f) + _hudTopLeftCorner, Color.White);
            }

            spriteBatch.Draw(_coinTexture, new Vector2(position.X + 640f, 0f) + _hudTopLeftCorner, null
                , Color.White, 0f, Vector2.Zero, 0.75f, SpriteEffects.None, 0f);

            spriteBatch.DrawString(_fonts[0], $" {InventoryManager.Instance.GoldCoin}"
                , new Vector2(position.X + 640f + 32f, 0f) + _hudTopLeftCorner, Color.White);
        }

        private void DrawPressF(SpriteBatch spriteBatch)
        {
            if (GameGlobals.Instance.IsDetectedGameObject)
            {
                spriteBatch.Draw(_pressFTexture, new Vector2(930f, 550f) + _hudTopLeftCorner, null
                    , Color.White, 0f, Vector2.Zero, 0.75f, SpriteEffects.None, 0f);
            }
        }

        private void DrawInsufficientSign(SpriteBatch spriteBatch)
        {
            if (GameGlobals.Instance.ShowInsufficientSign)
            {
                spriteBatch.Draw(_insufficient, PlayerManager.Instance.Player.Position
                    + new Vector2(25, -((PlayerManager.Instance.Player.Sprite.TextureRegion.Height / 2) + 25))
                    , null, Color.White, 0f, Vector2.Zero, 0.40f, SpriteEffects.None, 0f);
            }
        }

        private void DrawHPMobs(SpriteBatch spriteBatch)
        {
            var entities = EntityManager.Instance.Entities;

            foreach (var entity in entities.Where(e => !e.IsDestroyed))
            {
                var entityPos = entity.Position;
                var posX = entity.Sprite.TextureRegion.Width / 5f;
                var posY = entity.Sprite.TextureRegion.Height / 2f;

                spriteBatch.DrawString(_fonts[0], $"{entity.HP}", entityPos - new Vector2(posX, posY)
                    , Color.DarkRed, 0f, Vector2.Zero, 1.5f, SpriteEffects.None, 0f);
            }
        }

        private void DrawCombatNumbers(SpriteBatch spriteBatch)
        {
            var entities = EntityManager.Instance.Entities;

            foreach (var entity in entities.Where(e => !e.IsDestroyed))
            {
                if (entity.IsAttacked) entity.DrawCombatNumbers(spriteBatch, entity.CombatNumCase);
            }

            if (PlayerManager.Instance.Player.IsAttacked) PlayerManager.Instance.Player.DrawCombatNumbers(spriteBatch
                , PlayerManager.Instance.Player.CombatNumCase);
        }

        private void DrawCollectedItem(SpriteBatch spriteBatch) 
        {
            if (GameGlobals.Instance.CollectedItemFeed.Count != 0)
            {
                int n;
                if (GameGlobals.Instance.CollectedItemFeed.Count >= GameGlobals.Instance.MaximumItemFeed)
                {
                    n = GameGlobals.Instance.MaximumItemFeed;
                }
                else n = GameGlobals.Instance.CollectedItemFeed.Count;

                for (int i = 0; i < n; i++)
                {
                    var referId = GameGlobals.Instance.CollectedItemFeed.ElementAt(i).ItemId;
                    var amount = GameGlobals.Instance.CollectedItemFeed.ElementAt(i).Count;
                    var offsetX = GameGlobals.Instance.HUDPosition.X;
                    var offsetY = GameGlobals.Instance.HUDPosition.Y;

                    var itemData = GameGlobals.Instance.ItemsDatas.Where(i => i.ItemId.Equals(referId));

                    spriteBatch.FillRectangle(355f + offsetX, 496f + (i * 40) + offsetY, 120, 28, Color.Black * 0.4f);

                    var _transform = new Transform2
                    {
                        Scale = new Vector2(0.75f, 0.75f),
                        Rotation = 0f,
                        Position = new Vector2(370f, 510f + (i * 40)) + _hudTopLeftCorner
                    };

                    _spriteItemPack.Play(referId.ToString());
                    _spriteItemPack.Update(_deltaSeconds);

                    spriteBatch.Draw(_spriteItemPack, _transform);

                    spriteBatch.DrawString(_fonts[2], $"{itemData.ElementAt(0).Name} x {amount}"
                        , new Vector2(360f + 32f, 495f + (i * 40)) + _hudTopLeftCorner, Color.White);
                }

                if (_nextFeed)
                {
                    if (GameGlobals.Instance.CollectedItemFeed.Count >= GameGlobals.Instance.MaximumItemFeed)
                    {
                        GameGlobals.Instance.CollectedItemFeed.RemoveRange(0, GameGlobals.Instance.MaximumItemFeed);
                    }
                    else GameGlobals.Instance.CollectedItemFeed.RemoveRange(0, GameGlobals.Instance.CollectedItemFeed.Count);
                }
            }
        }

        private void DrawQuestList(SpriteBatch spriteBatch)
        {
            // TODO :
        }

        public static void ShowInsufficientSign()
        {
            GameGlobals.Instance.ShowInsufficientSign = true;
        }

        public static void AddFeed(int referId, int amount)
        {
            GameGlobals.Instance.CollectedItemFeed.Add(new InventoryItemData() {
                ItemId = referId,
                Count = amount
            });

            GameGlobals.Instance.DisplayFeedTime = GameGlobals.Instance.MaximumDisplayFeedTime;
        }
    }
}
