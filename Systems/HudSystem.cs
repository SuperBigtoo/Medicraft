using FontStashSharp;
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
    public class HUDSystem
    {
        private Vector2 _hudTopLeftCorner;

        private readonly AnimatedSprite _spriteItemPack;

        private readonly float _insufficientTime = 3f;

        private float _deltaSeconds, _insufficientTimer;

        private bool _nextFeed;      

        public HUDSystem()
        {
            _spriteItemPack = new AnimatedSprite(GameGlobals.Instance.ItemsPackSprites);

            _nextFeed = false;
            _insufficientTimer = _insufficientTime;
        }

        public void Update(GameTime gameTime)
        {
            _deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Check for the next feed to roll in
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

            // Check for a notification 
            if (GameGlobals.Instance.ShowInsufficientSign)
            {
                _insufficientTimer -= _deltaSeconds;

                if (_insufficientTimer <= 0)
                {
                    GameGlobals.Instance.ShowInsufficientSign = false;
                    _insufficientTimer = _insufficientTime;
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
                samplerState: SamplerState.LinearClamp,
                blendState: BlendState.AlphaBlend,
                depthStencilState: DepthStencilState.None,
                rasterizerState: RasterizerState.CullCounterClockwise,
                transformMatrix: ScreenManager.Instance.Camera.GetTransform(
                    _graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height)
            );
               
            // Draw HP mobs
            DrawHealthPointMobs(spriteBatch);

            // Draw combat numbers mobs & player
            DrawCombatNumbers(spriteBatch);

            // Draw Press F Sign
            DrawInteractionSigh(spriteBatch);

            // Draw Insufficient Sign
            DrawInsufficientSign(spriteBatch);

            // Draw Feed Items
            DrawCollectedItem(spriteBatch);

            // Draw Quest List
            DrawQuestList(spriteBatch);

            // Draw HUD Bar
            DrawMainHUD(spriteBatch);
        }

        private void DrawMainHUD(SpriteBatch spriteBatch)
        {
            DrawHealthBarGUI(spriteBatch);

            DrawCompanionHealthBarGUI(spriteBatch);

            DrawBossHealthBarGUI(spriteBatch);

            DrawItemBar(spriteBatch);


            if (GameGlobals.Instance.IsDebugMode)
            {
                spriteBatch.FillRectangle(_hudTopLeftCorner.X, _hudTopLeftCorner.Y
                , ScreenManager.Instance.GraphicsDevice.Viewport.Width, 26, Color.Black * 0.4f);

                var FontSensation = GameGlobals.Instance.FontSensation;

                spriteBatch.DrawString(FontSensation, $" Mobs: {EntityManager.Instance.entities.Count}"
                    , Vector2.Zero + _hudTopLeftCorner, Color.White);

                spriteBatch.DrawString(FontSensation, $"Time Spawn: {(int)EntityManager.Instance.spawnTime}"
                    , new Vector2(100f, 0f) + _hudTopLeftCorner, Color.White);

                spriteBatch.DrawString(FontSensation, $"X: {(int)PlayerManager.Instance.Player.Position.X}"
                    , new Vector2(240f, 0f) + _hudTopLeftCorner, Color.White);

                spriteBatch.DrawString(FontSensation, $"Y: {(int)PlayerManager.Instance.Player.Position.Y}"
                    , new Vector2(320f, 0f) + _hudTopLeftCorner, Color.White);

                spriteBatch.Draw(GameGlobals.Instance.GetGuiTexture(GameGlobals.GuiTextureName.gold_coin)
                    , new Vector2(400f, 0f) + _hudTopLeftCorner, null
                    , Color.White, 0f, Vector2.Zero, 0.75f, SpriteEffects.None, 0f);

                spriteBatch.DrawString(FontSensation, $" {InventoryManager.Instance.GoldCoin}"
                    , new Vector2(400f + 32f, 0f) + _hudTopLeftCorner, Color.White);

                spriteBatch.DrawString(FontSensation, $"MouseX: {(int)GameGlobals.Instance.MousePosition.X}"
                    , new Vector2(490f, 0f) + _hudTopLeftCorner, Color.White);

                spriteBatch.DrawString(FontSensation, $"MouseY: {(int)GameGlobals.Instance.MousePosition.Y}"
                    , new Vector2(610f, 0f) + _hudTopLeftCorner, Color.White);

                spriteBatch.DrawString(FontSensation, $"Total PlayTime: {(int)GameGlobals.Instance.TotalPlayTime}"
                    , new Vector2(690f, 0f) + _hudTopLeftCorner, Color.White);

                var rect = new Rectangle((int)_hudTopLeftCorner.X + 50, (int)_hudTopLeftCorner.Y + 255, 300, 300);

                var FontTA8BitBold = GameGlobals.Instance.FontTA8BitBold;

                spriteBatch.DrawString(FontTA8BitBold, $"Player ATK: {PlayerManager.Instance.Player.ATK}"
                    , new Vector2(55f, 260f) + _hudTopLeftCorner, Color.White, clippingRectangle: rect);

                spriteBatch.DrawString(FontTA8BitBold, $"Player Crit: {PlayerManager.Instance.Player.Crit_Percent}"
                    , new Vector2(55f, 275f) + _hudTopLeftCorner, Color.White, clippingRectangle: rect);

                spriteBatch.DrawString(FontTA8BitBold, $"Player CritDMG: {PlayerManager.Instance.Player.CritDMG_Percent}"
                    , new Vector2(55f, 290f) + _hudTopLeftCorner, Color.White, clippingRectangle: rect);

                spriteBatch.DrawString(FontTA8BitBold, $"Player DEF: {PlayerManager.Instance.Player.DEF_Percent}"
                    , new Vector2(55f, 305f) + _hudTopLeftCorner, Color.White, clippingRectangle: rect);

                spriteBatch.DrawString(FontTA8BitBold, $"Cooldown Normal Skill: {PlayerManager.Instance.Player.NormalCooldownTimer}"
                    , new Vector2(55f, 335f) + _hudTopLeftCorner, Color.White);

                spriteBatch.DrawString(FontTA8BitBold, $"Cooldown Normal Skill: {PlayerManager.Instance.Player.BurstCooldownTimer}"
                    , new Vector2(55f, 350f) + _hudTopLeftCorner, Color.White);

                spriteBatch.DrawString(FontTA8BitBold, $"Cooldown Normal Skill: {PlayerManager.Instance.Player.PassiveCooldownTimer}"
                    , new Vector2(55f, 365f) + _hudTopLeftCorner, Color.White);

                spriteBatch.DrawString(FontTA8BitBold, $"Normal Skill Time: {PlayerManager.Instance.Player.NormalActivatedTimer}"
                    , new Vector2(55f, 400f) + _hudTopLeftCorner, Color.White);

                spriteBatch.DrawString(FontTA8BitBold, $"Passive Skill Time: {PlayerManager.Instance.Player.PassiveActivatedTimer}"
                    , new Vector2(55f, 415f) + _hudTopLeftCorner, Color.White);
            }
        }

        private void DrawHealthBarGUI(SpriteBatch spriteBatch)
        {
            // Health Bar Alpha
            spriteBatch.Draw(GameGlobals.Instance.GetGuiTexture(GameGlobals.GuiTextureName.health_bar_alpha)
                , new Vector2(35f, 15f) + _hudTopLeftCorner, null
                , Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

            // Player Profile
            spriteBatch.Draw(GameGlobals.Instance.GetGuiTexture(GameGlobals.GuiTextureName.noah_profile)
                , new Vector2(38f, 18f) + _hudTopLeftCorner, null
                , Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

            // HP gauge
            var curHealthPoint = PlayerManager.Instance.Player.GetCurrentHealthPercentage();
            var hpGaugeTexture = GameGlobals.Instance.GetGuiTexture(GameGlobals.GuiTextureName.healthpoints_gauge);
            var hpGaugeSourceRec = new Rectangle(0, 0, (int)(hpGaugeTexture.Width * curHealthPoint), hpGaugeTexture.Height);
            var hpGaugeRec = new Rectangle((int)(166 + _hudTopLeftCorner.X), (int)(40 + _hudTopLeftCorner.Y)
                , (int)(hpGaugeTexture.Width * curHealthPoint), hpGaugeTexture.Height);

            spriteBatch.Draw(hpGaugeTexture, hpGaugeRec, hpGaugeSourceRec
                , Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);

            // Mana gauge
            var curManaPoint = PlayerManager.Instance.Player.GetCurrentManaPercentage();
            var manaGaugeTexture = GameGlobals.Instance.GetGuiTexture(GameGlobals.GuiTextureName.mana_gauge);
            var manaGaugeSourceRec = new Rectangle(0, 0, (int)(manaGaugeTexture.Width * curManaPoint), manaGaugeTexture.Height);
            var manaGaugeRec = new Rectangle((int)(167 + _hudTopLeftCorner.X), (int)(84 + _hudTopLeftCorner.Y)
                , (int)(manaGaugeTexture.Width * curManaPoint), manaGaugeTexture.Height);

            spriteBatch.Draw(manaGaugeTexture, manaGaugeRec, manaGaugeSourceRec
                , Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);

            // Health Bar GUI
            spriteBatch.Draw(GameGlobals.Instance.GetGuiTexture(GameGlobals.GuiTextureName.health_bar)
                , new Vector2(35f, 15f) + _hudTopLeftCorner, null
                , Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

            // Text Health Point
            var textHP = $"{PlayerManager.Instance.Player.HP}/{PlayerManager.Instance.Player.MaximumHP}";
            var textSizeHP = GameGlobals.Instance.FontTA8BitBold.MeasureString(textHP);
            var positionHP = new Vector2(
                (166 + hpGaugeTexture.Width / 2) - (textSizeHP.Width / 2),
                (40 + hpGaugeTexture.Height / 2) - (textSizeHP.Height / 2)) + _hudTopLeftCorner;

            DrawTextWithStroke(spriteBatch, GameGlobals.Instance.FontTA8BitBold
                , textHP, positionHP, Color.White, 1f, Color.Black, 2);

            // Text Mana Point
            var textMana = $"{(int)PlayerManager.Instance.Player.Mana}/{(int)PlayerManager.Instance.Player.MaximumMana}";
            //var textSizeMana = GameGlobals.Instance.FontTA8Bit.MeasureString(textMana);
            var positionMana = new Vector2(
                (166 + hpGaugeTexture.Width / 2) - (textSizeHP.Width / 2), 80) + _hudTopLeftCorner;

            DrawTextWithStroke(spriteBatch, GameGlobals.Instance.FontTA8Bit
                , textMana, positionMana, Color.White, 1f, Color.Black, 2);
        }

        private void DrawCompanionHealthBarGUI(SpriteBatch spriteBatch)
        {
            // Health Bar Companion Alpha
            spriteBatch.Draw(GameGlobals.Instance.GetGuiTexture(GameGlobals.GuiTextureName.health_bar_companion_alpha)
                , new Vector2(103f, 128f) + _hudTopLeftCorner, null
                , Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

            // Companion Profile
            spriteBatch.Draw(GameGlobals.Instance.GetGuiTexture(GameGlobals.GuiTextureName.companion_profile)
                , new Vector2(105f, 130f) + _hudTopLeftCorner, null
                , Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

            // HP gauge
            //var curHealthPoint = PlayerManager.Instance.Player.GetCurrentHealthPercentage();
            var hpGaugeTexture = GameGlobals.Instance.GetGuiTexture(GameGlobals.GuiTextureName.healthpoints_gauge_companion);
            var hpGaugeSourceRec = new Rectangle(0, 0, (int)(hpGaugeTexture.Width * 1), hpGaugeTexture.Height);
            var hpGaugeRec = new Rectangle((int)(178 + _hudTopLeftCorner.X), (int)(146 + _hudTopLeftCorner.Y)
                , (int)(hpGaugeTexture.Width * 1), hpGaugeTexture.Height);

            spriteBatch.Draw(hpGaugeTexture, hpGaugeRec, hpGaugeSourceRec
                , Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);

            // Health Bar Companion GUI
            spriteBatch.Draw(GameGlobals.Instance.GetGuiTexture(GameGlobals.GuiTextureName.health_bar_companion)
                , new Vector2(103f, 128f) + _hudTopLeftCorner, null
                , Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        }

        private void DrawBossHealthBarGUI(SpriteBatch spriteBatch)
        {
            // Health Bar Boss Alpha
            spriteBatch.Draw(GameGlobals.Instance.GetGuiTexture(GameGlobals.GuiTextureName.health_bar_boss_alpha)
                , new Vector2(506f, 92f) + _hudTopLeftCorner, null
                , Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

            // HP gauge
            //var curHealthPoint = PlayerManager.Instance.Player.GetCurrentHealthPercentage();
            var hpGaugeTexture = GameGlobals.Instance.GetGuiTexture(GameGlobals.GuiTextureName.boss_gauge);
            var hpGaugeSourceRec = new Rectangle(0, 0, (int)(hpGaugeTexture.Width * 1), hpGaugeTexture.Height);
            var hpGaugeRec = new Rectangle((int)(541 + _hudTopLeftCorner.X), (int)(123 + _hudTopLeftCorner.Y)
                , (int)(hpGaugeTexture.Width * 1), hpGaugeTexture.Height);

            spriteBatch.Draw(hpGaugeTexture, hpGaugeRec, hpGaugeSourceRec
                , Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);

            // Health Bar Boss GUI
            spriteBatch.Draw(GameGlobals.Instance.GetGuiTexture(GameGlobals.GuiTextureName.health_bar_boss)
                , new Vector2(506f, 92f) + _hudTopLeftCorner, null
                , Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

            // Text Name
            var text = $"Boss Name is So Cool";
            var textSize = GameGlobals.Instance.FontTA16Bit.MeasureString(text);
            var position = new Vector2(GameGlobals.Instance.GameScreenCenter.X
                - (textSize.Width / 2), 90)
                + _hudTopLeftCorner;

            DrawTextWithStroke(spriteBatch, GameGlobals.Instance.FontTA16Bit
                , text, position, Color.DarkRed, 1f, Color.Black, 1);
        }

        private void DrawItemBar(SpriteBatch spriteBatch)
        {
            // Item Bar
            spriteBatch.Draw(GameGlobals.Instance.GetGuiTexture(GameGlobals.GuiTextureName.item_bar)
                , new Vector2(499f, 814f) + _hudTopLeftCorner, null
                , Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

            // Item Slot
            for (int i = 0; i < 8; i++ )
            {
                spriteBatch.Draw(GameGlobals.Instance.GetGuiTexture(GameGlobals.GuiTextureName.item_slot)
                , new Vector2(515f + (52 * i), 824f) + _hudTopLeftCorner, null
                , Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            }
        }

        private void DrawInteractionSigh(SpriteBatch spriteBatch)
        {
            if (GameGlobals.Instance.IsDetectedGameObject)
            {
                spriteBatch.Draw(GameGlobals.Instance.GetGuiTexture(GameGlobals.GuiTextureName.press_f)
                    , new Vector2(1055f, 560f) + _hudTopLeftCorner, null
                    , Color.White, 0f, Vector2.Zero, 0.75f, SpriteEffects.None, 0f);
            }
        }

        private static void DrawInsufficientSign(SpriteBatch spriteBatch)
        {
            if (GameGlobals.Instance.ShowInsufficientSign)
            {
                spriteBatch.Draw(GameGlobals.Instance.GetGuiTexture(GameGlobals.GuiTextureName.insufficient)
                    , PlayerManager.Instance.Player.Position
                    + new Vector2(25, -((PlayerManager.Instance.Player.Sprite.TextureRegion.Height / 2) + 25))
                    , null, Color.White, 0f, Vector2.Zero, 0.40f, SpriteEffects.None, 0f);
            }
        }

        private static void DrawHealthPointMobs(SpriteBatch spriteBatch)
        {
            var entities = EntityManager.Instance.Entities;
            var FontSensation = GameGlobals.Instance.FontSensation;

            foreach (var entity in entities.Where(e => !e.IsDestroyed 
                && e.EntityType == Entities.Entity.EntityTypes.Hostile && e.HP > 0))
            {
                var entityPos = entity.Position;
                var text = $"{entity.HP}";
                var textSize = FontSensation.MeasureString(text);
                var position = entityPos - new Vector2(textSize.Width / 2
                    , (textSize.Height / 2) + (entity.Sprite.TextureRegion.Height / 2));

                DrawTextWithStroke(spriteBatch, FontSensation, text, position, Color.DarkRed, 1f, Color.Black, 1);
            }
        }

        private static void DrawCombatNumbers(SpriteBatch spriteBatch)
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
                    var offsetX = GameGlobals.Instance.FeedPoint.X;
                    var offsetY = GameGlobals.Instance.FeedPoint.Y;

                    var itemData = GameGlobals.Instance.ItemsDatas.Where(i => i.ItemId.Equals(referId));

                    var _transform = new Transform2
                    {
                        Scale = new Vector2(0.75f, 0.75f),
                        Rotation = 0f,
                        Position = new Vector2(offsetX, offsetY + (i * 40)) + _hudTopLeftCorner
                    };

                    _spriteItemPack.Play(referId.ToString());
                    _spriteItemPack.Update(_deltaSeconds);

                    spriteBatch.Draw(_spriteItemPack, _transform);

                    var text = $"{itemData.ElementAt(0).Name} x {amount}";
                    var position = new Vector2(
                        offsetX + 22f,
                        (offsetY - 15f) + (i * 40)) + _hudTopLeftCorner;

                    DrawTextWithStroke(spriteBatch, GameGlobals.Instance.FontTA8Bit
                        , text, position, Color.White, 1f, Color.Black, 2);
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

        public static void DrawTextWithStroke(SpriteBatch spriteBatch, BitmapFont font, string text
            , Vector2 position, Color textColor, float textScale, Color strokeColor, int strokeWidth)
        {
            // Draw the text with a stroke
            for (int x = -strokeWidth; x <= strokeWidth; x++)
            {
                for (int y = -strokeWidth; y <= strokeWidth; y++)
                {
                    spriteBatch.DrawString(font, text, position + new Vector2(x, y), strokeColor, 0f, Vector2.Zero, textScale, SpriteEffects.None, 0f);
                }
            }

            // Draw the original text on top
            spriteBatch.DrawString(font, text, position, textColor, 0f, Vector2.Zero, textScale, SpriteEffects.None, 0f);
        }

        public static void ShowInsufficientSign()
        {
            GameGlobals.Instance.ShowInsufficientSign = true;
        }

        public static void AddFeedItem(int referId, int amount)
        {
            GameGlobals.Instance.CollectedItemFeed.Add(new InventoryItemData() {
                ItemId = referId,
                Count = amount
            });

            GameGlobals.Instance.DisplayFeedTime = GameGlobals.Instance.MaximumDisplayFeedTime;
        }
    }
}
