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
        private Vector2 _topLeftCorner;
        private readonly float _insufficientTime = 3f;
        private float _insufficientTimer;
        public static float _deltaSeconds;

        private bool _nextFeed;

        private readonly AnimatedSprite _spriteItemPack = new(GameGlobals.Instance.ItemsPackSprites);         
            
        public HUDSystem()
        {
            _nextFeed = false;
            _insufficientTimer = _insufficientTime;
        }

        public void Update(GameTime gameTime)
        {
            _deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _topLeftCorner = GameGlobals.Instance.TopLeftCornerPos;

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
            spriteBatch.End();

            var graphicsDevice = ScreenManager.Instance.GraphicsDevice;

            spriteBatch.Begin(
                SpriteSortMode.Deferred,
                samplerState: SamplerState.LinearClamp,
                blendState: BlendState.AlphaBlend,
                depthStencilState: DepthStencilState.None,
                rasterizerState: RasterizerState.CullCounterClockwise,
                transformMatrix: ScreenManager.Camera.GetTransform(
                    graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height)
            );

            if (!GameGlobals.Instance.IsGamePause)
            {
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
        }

        private void DrawMainHUD(SpriteBatch spriteBatch)
        {
            DrawHealthBarGUI(spriteBatch);

            if (!PlayerManager.Instance.IsCompanionDead && PlayerManager.Instance.IsCompanionSummoned)
                DrawCompanionHealthBarGUI(spriteBatch);

            if (GameGlobals.Instance.IsEnteringBossFight)
                DrawBossHealthBarGUI(spriteBatch);

            DrawItemBarGUI(spriteBatch);

            DrawLevelGUI(spriteBatch);

            DrawExpBarGUI(spriteBatch);

            DrawSkillGUI(spriteBatch);

            // Draw Gold Coin
            spriteBatch.Draw(GameGlobals.Instance.GetGuiTexture(GameGlobals.GuiTextureName.gold_coin)
                    , new Vector2(90f, 210f) + _topLeftCorner, null
                    , Color.White, 0f, Vector2.Zero, 0.75f, SpriteEffects.None, 0f);

            DrawTextWithStroke(spriteBatch, GameGlobals.Instance.FontTA8BitBold
                , $" {InventoryManager.Instance.GoldCoin}"
                , new Vector2(90f + 32f, 205f) + _topLeftCorner, Color.Wheat, 1f, Color.Black, 2);

            if (GameGlobals.Instance.IsDebugMode)
            {
                spriteBatch.FillRectangle(_topLeftCorner.X, _topLeftCorner.Y
                    , ScreenManager.Instance.GraphicsDevice.Viewport.Width, 26, Color.Black * 0.4f);

                var FontSensation = GameGlobals.Instance.FontSensation;

                spriteBatch.DrawString(FontSensation, $" Mobs: {EntityManager.Instance.entities.Count}"
                    , Vector2.Zero + _topLeftCorner, Color.White);

                spriteBatch.DrawString(FontSensation, $"Time Spawn: {(int)EntityManager.Instance.SpawnTime}"
                    , new Vector2(100f, 0f) + _topLeftCorner, Color.White);

                spriteBatch.DrawString(FontSensation, $"X: {(int)PlayerManager.Instance.Player.Position.X}"
                    , new Vector2(240f, 0f) + _topLeftCorner, Color.White);

                spriteBatch.DrawString(FontSensation, $"Y: {(int)PlayerManager.Instance.Player.Position.Y}"
                    , new Vector2(320f, 0f) + _topLeftCorner, Color.White);

                spriteBatch.DrawString(FontSensation, $"MouseX: {(int)GameGlobals.Instance.MousePosition.X}"
                    , new Vector2(490f, 0f) + _topLeftCorner, Color.White);

                spriteBatch.DrawString(FontSensation, $"MouseY: {(int)GameGlobals.Instance.MousePosition.Y}"
                    , new Vector2(610f, 0f) + _topLeftCorner, Color.White);

                spriteBatch.DrawString(FontSensation, $"Total PlayTime: {(int)GameGlobals.Instance.TotalPlayTime}"
                    , new Vector2(760f, 0f) + _topLeftCorner, Color.White);

                var rect = new Rectangle((int)_topLeftCorner.X + 50, (int)_topLeftCorner.Y + 220, 300, 500);

                var FontTA8BitBold = GameGlobals.Instance.FontTA8BitBold;

                spriteBatch.DrawString(FontTA8BitBold, $"Level: {PlayerManager.Instance.Player.Level}"
                    , new Vector2(55f, 225f) + _topLeftCorner, Color.Magenta, clippingRectangle: rect);

                spriteBatch.DrawString(FontTA8BitBold
                    , $"EXP: {PlayerManager.Instance.Player.EXP} | EXPMaxCap: {PlayerManager.Instance.Player.EXPMaxCap}"
                    , new Vector2(55f, 240f) + _topLeftCorner, Color.Magenta, clippingRectangle: rect);

                spriteBatch.DrawString(FontTA8BitBold, $"Player ATK: {PlayerManager.Instance.Player.ATK}"
                    , new Vector2(55f, 260f) + _topLeftCorner, Color.Magenta, clippingRectangle: rect);

                spriteBatch.DrawString(FontTA8BitBold, $"Player Crit: {PlayerManager.Instance.Player.Crit}"
                    , new Vector2(55f, 275f) + _topLeftCorner, Color.Magenta, clippingRectangle: rect);

                spriteBatch.DrawString(FontTA8BitBold, $"Player CritDMG: {PlayerManager.Instance.Player.CritDMG}"
                    , new Vector2(55f, 290f) + _topLeftCorner, Color.Magenta, clippingRectangle: rect);

                spriteBatch.DrawString(FontTA8BitBold, $"Player DEF: {PlayerManager.Instance.Player.DEF}"
                    , new Vector2(55f, 305f) + _topLeftCorner, Color.Magenta, clippingRectangle: rect);

                spriteBatch.DrawString(FontTA8BitBold, $"Cooldown Normal Skill: {PlayerManager.Instance.Player.NormalCooldownTimer}"
                    , new Vector2(55f, 335f) + _topLeftCorner, Color.Magenta);

                spriteBatch.DrawString(FontTA8BitBold, $"Cooldown Normal Skill: {PlayerManager.Instance.Player.BurstCooldownTimer}"
                    , new Vector2(55f, 350f) + _topLeftCorner, Color.Magenta);

                spriteBatch.DrawString(FontTA8BitBold, $"Cooldown Normal Skill: {PlayerManager.Instance.Player.PassiveCooldownTimer}"
                    , new Vector2(55f, 365f) + _topLeftCorner, Color.Magenta);

                spriteBatch.DrawString(FontTA8BitBold, $"Normal Skill Time: {PlayerManager.Instance.Player.NormalActivatedTimer}"
                    , new Vector2(55f, 400f) + _topLeftCorner, Color.Magenta);

                spriteBatch.DrawString(FontTA8BitBold, $"Passive Skill Time: {PlayerManager.Instance.Player.PassiveActivatedTimer}"
                    , new Vector2(55f, 415f) + _topLeftCorner, Color.Magenta);
            }
        }

        public static void DrawOnTopUI(SpriteBatch spriteBatch)
        {
            switch (GUIManager.Instance.CurrentGUI)
            {
                case GUIManager.PlayScreen:
                    // Selected Slot
                    var selectedSlot = GameGlobals.Instance.CurrentHotbarSelect;
                    spriteBatch.Draw(GameGlobals.Instance.GetGuiTexture(GameGlobals.GuiTextureName.selected_slot)
                        , new Vector2(511f + (52 * selectedSlot), 820f) + GameGlobals.Instance.TopLeftCornerPos, null
                        , Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                    break;

                case GUIManager.InspectPanel:
                    // Character Sprite
                    if (GameGlobals.Instance.IsOpenInspectPanel && GUIManager.Instance.IsCharacterTabSelected
                        && !GUIManager.Instance.IsShowConfirmBox)
                    {
                        var playerSprite = GUIManager.Instance.PlayerSprite;

                        //playerSprite.Depth = 0.1f;
                        playerSprite.Play("default_idle");
                        playerSprite.Update(_deltaSeconds);

                        var transform = new Transform2()
                        {
                            Scale = new Vector2(1.5f, 1.5f),
                            Rotation = 0f,
                            Position = PlayerManager.Instance.Player.Position - new Vector2(200f, 50f)
                        };
                        spriteBatch.Draw(playerSprite, transform);
                    }                  
                    break;
            }              
        }

        private void DrawHealthBarGUI(SpriteBatch spriteBatch)
        {
            // Health Bar Alpha
            spriteBatch.Draw(GameGlobals.Instance.GetGuiTexture(GameGlobals.GuiTextureName.health_bar_alpha)
                , new Vector2(35f, 15f) + _topLeftCorner, null
                , Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

            // Player Profile
            spriteBatch.Draw(GameGlobals.Instance.GetGuiTexture(GameGlobals.GuiTextureName.noah_profile)
                , new Vector2(38f, 18f) + _topLeftCorner, null
                , Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

            // HP gauge
            var curHealthPoint = PlayerManager.Instance.Player.GetCurrentHealthPercentage();
            var hpGaugeTexture = GameGlobals.Instance.GetGuiTexture(GameGlobals.GuiTextureName.healthpoints_gauge);
            var hpGaugeSourceRec = new Rectangle(0, 0
                , (int)(hpGaugeTexture.Width * curHealthPoint), hpGaugeTexture.Height);
            var hpGaugeRec = new Rectangle((int)(166 + _topLeftCorner.X), (int)(40 + _topLeftCorner.Y)
                , (int)(hpGaugeTexture.Width * curHealthPoint), hpGaugeTexture.Height);

            spriteBatch.Draw(hpGaugeTexture, hpGaugeRec, hpGaugeSourceRec
                , Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);

            // Mana gauge
            var curManaPoint = PlayerManager.Instance.Player.GetCurrentManaPercentage();
            var manaGaugeTexture = GameGlobals.Instance.GetGuiTexture(GameGlobals.GuiTextureName.mana_gauge);
            var manaGaugeSourceRec = new Rectangle(0, 0
                , (int)(manaGaugeTexture.Width * curManaPoint), manaGaugeTexture.Height);
            var manaGaugeRec = new Rectangle((int)(167 + _topLeftCorner.X), (int)(84 + _topLeftCorner.Y)
                , (int)(manaGaugeTexture.Width * curManaPoint), manaGaugeTexture.Height);

            spriteBatch.Draw(manaGaugeTexture, manaGaugeRec, manaGaugeSourceRec
                , Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);

            // Health Bar GUI
            spriteBatch.Draw(GameGlobals.Instance.GetGuiTexture(GameGlobals.GuiTextureName.health_bar)
                , new Vector2(35f, 15f) + _topLeftCorner, null
                , Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

            // Text Health Point
            var textHP = $"{(int)PlayerManager.Instance.Player.HP}/{(int)PlayerManager.Instance.Player.MaxHP}";
            var textSizeHP = GameGlobals.Instance.FontTA8BitBold.MeasureString(textHP);
            var positionHP = new Vector2(
                (166f + hpGaugeTexture.Width / 2) - (textSizeHP.Width / 2),
                (40f + hpGaugeTexture.Height / 2) - (textSizeHP.Height / 2)) + _topLeftCorner;

            DrawTextWithStroke(spriteBatch, GameGlobals.Instance.FontTA8BitBold
                , textHP, positionHP, Color.White, 1f, Color.Black, 2);

            // Text Mana Point
            var textMana = $"{(int)PlayerManager.Instance.Player.Mana}/{(int)PlayerManager.Instance.Player.MaxMana}";
            //var textSizeMana = GameGlobals.Instance.FontTA8Bit.MeasureString(textMana);
            var positionMana = new Vector2(
                (166f + hpGaugeTexture.Width / 2) - (textSizeHP.Width / 2), 80f) + _topLeftCorner;

            DrawTextWithStroke(spriteBatch, GameGlobals.Instance.FontTA8Bit
                , textMana, positionMana, Color.White, 1f, Color.Black, 2);
        }

        private void DrawCompanionHealthBarGUI(SpriteBatch spriteBatch)
        {
            var companion = PlayerManager.Instance.Companions[PlayerManager.Instance.CurrCompaIndex];

            // Health Bar Companion Alpha
            spriteBatch.Draw(GameGlobals.Instance.GetGuiTexture(GameGlobals.GuiTextureName.health_bar_companion_alpha)
                , new Vector2(127f, 106f) + _topLeftCorner, null
                , Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

            // Companion Profile
            spriteBatch.Draw(GameGlobals.Instance.GetGuiTexture(GameGlobals.GuiTextureName.companion_profile)
                , new Vector2(129f, 108f) + _topLeftCorner, null
                , Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

            // HP gauge
            var curHealthPoint = companion.GetCurrentHealthPercentage();
            var hpGaugeTexture = GameGlobals.Instance.GetGuiTexture(GameGlobals.GuiTextureName.healthpoints_gauge_companion);
            var hpGaugeSourceRec = new Rectangle(0, 0
                , (int)(hpGaugeTexture.Width * curHealthPoint), hpGaugeTexture.Height);
            var hpGaugeRec = new Rectangle((int)(202f + _topLeftCorner.X), (int)(124f + _topLeftCorner.Y)
                , (int)(hpGaugeTexture.Width * curHealthPoint), hpGaugeTexture.Height);

            spriteBatch.Draw(hpGaugeTexture, hpGaugeRec, hpGaugeSourceRec
                , Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);

            // Text Health Point
            var textHP = $"{(int)companion.HP}/{(int)companion.MaxHP}";
            var textSizeHP = GameGlobals.Instance.FontTA8BitBold.MeasureString(textHP);
            var positionHP = new Vector2(
                (202f + hpGaugeTexture.Width / 2) - ((textSizeHP.Width * 0.8f) / 2),
                (124f + hpGaugeTexture.Height / 2) - ((textSizeHP.Height * 0.8f) / 2)) + _topLeftCorner;

            DrawTextWithStroke(spriteBatch, GameGlobals.Instance.FontTA8BitBold
                , textHP, positionHP, Color.White, 0.8f, Color.Black, 2);

            // Health Bar Companion GUI
            spriteBatch.Draw(GameGlobals.Instance.GetGuiTexture(GameGlobals.GuiTextureName.health_bar_companion)
                , new Vector2(127f, 106f) + _topLeftCorner, null
                , Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        }

        private void DrawBossHealthBarGUI(SpriteBatch spriteBatch)
        {
            // Health Bar Boss Alpha
            spriteBatch.Draw(GameGlobals.Instance.GetGuiTexture(GameGlobals.GuiTextureName.health_bar_boss_alpha)
                , new Vector2(506f, 92f) + _topLeftCorner, null
                , Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

            // HP gauge
            //var curHealthPoint = PlayerManager.Instance.Player.GetCurrentHealthPercentage();
            var hpGaugeTexture = GameGlobals.Instance.GetGuiTexture(GameGlobals.GuiTextureName.boss_gauge);
            var hpGaugeSourceRec = new Rectangle(0, 0
                , (int)(hpGaugeTexture.Width * 1), hpGaugeTexture.Height);
            var hpGaugeRec = new Rectangle((int)(541 + _topLeftCorner.X), (int)(123 + _topLeftCorner.Y)
                , (int)(hpGaugeTexture.Width * 1), hpGaugeTexture.Height);

            spriteBatch.Draw(hpGaugeTexture, hpGaugeRec, hpGaugeSourceRec
                , Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);

            // Health Bar Boss GUI
            spriteBatch.Draw(GameGlobals.Instance.GetGuiTexture(GameGlobals.GuiTextureName.health_bar_boss)
                , new Vector2(506f, 92f) + _topLeftCorner, null
                , Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

            // Text Name
            var text = $"Boss Name is So Cool";
            var textSize = GameGlobals.Instance.FontTA16Bit.MeasureString(text);
            var position = new Vector2(GameGlobals.Instance.GameScreenCenter.X
                - (textSize.Width / 2), 90)
                + _topLeftCorner;

            DrawTextWithStroke(spriteBatch, GameGlobals.Instance.FontTA16Bit
                , text, position, Color.DarkRed, 1f, Color.Black, 1);
        }

        private void DrawItemBarGUI(SpriteBatch spriteBatch)
        {
            // Item Bar
            spriteBatch.Draw(GameGlobals.Instance.GetGuiTexture(GameGlobals.GuiTextureName.item_bar)
                , new Vector2(499f, 814f) + _topLeftCorner, null
                , Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

            // Item Slot
            for (int i = 0; i < 8; i++ )
            {
                spriteBatch.Draw(GameGlobals.Instance.GetGuiTexture(GameGlobals.GuiTextureName.item_slot)
                    , new Vector2(515f + (52 * i), 824f) + _topLeftCorner, null
                    , Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);          
            }
        }
   
        private void DrawLevelGUI(SpriteBatch spriteBatch)
        {
            // Level Gui
            var LevelGuiTexture = GameGlobals.Instance.GetGuiTexture(GameGlobals.GuiTextureName.level_gui);

            spriteBatch.Draw(LevelGuiTexture
                , new Vector2(25f, 10f) + _topLeftCorner, null
                , Color.White, 0f, Vector2.Zero, 1.25f, SpriteEffects.None, 0f);

            // Text Level
            var textLevel = $"{PlayerManager.Instance.Player.Level}";
            var textSizeLevel = GameGlobals.Instance.FontTA8BitBold.MeasureString(textLevel);
            var position = new Vector2(
                (29f + LevelGuiTexture.Width / 2) - (textSizeLevel.Width / 2),
                (14f + LevelGuiTexture.Height / 2) - (textSizeLevel.Height / 2)) + _topLeftCorner;

            DrawTextWithStroke(spriteBatch, GameGlobals.Instance.FontTA8BitBold
                , textLevel, position, Color.White, 1f, Color.Black, 1);
        }

        private void DrawExpBarGUI(SpriteBatch spriteBatch)
        {
            var position = new Vector2(508f, 792f);

            // Experience Alpha
            spriteBatch.Draw(GameGlobals.Instance.GetGuiTexture(GameGlobals.GuiTextureName.exp_bar_alpha)
                , position + _topLeftCorner, null
                , Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

            // Experience gauge
            var curEXP = PlayerManager.Instance.Player.GetCurrentEXPPercentage();
            var expGaugeTexture = GameGlobals.Instance.GetGuiTexture(GameGlobals.GuiTextureName.exp_gauge);
            var expGaugeSourceRec = new Rectangle(0, 0
                , (int)(expGaugeTexture.Width * curEXP), expGaugeTexture.Height);
            var expGaugeRec = new Rectangle((int)(508f + _topLeftCorner.X), (int)(792f + _topLeftCorner.Y)
                , (int)(expGaugeTexture.Width * curEXP), expGaugeTexture.Height);

            spriteBatch.Draw(expGaugeTexture, expGaugeRec, expGaugeSourceRec
                , Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);

            // Experience Bar
            spriteBatch.Draw(GameGlobals.Instance.GetGuiTexture(GameGlobals.GuiTextureName.exp_bar)
                , position + _topLeftCorner, null
                , Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

            // Text Experience Point
            var textLevel = $"{PlayerManager.Instance.Player.Level}";
            var textSizeLevel = GameGlobals.Instance.FontTA8BitBold.MeasureString(textLevel);
            var positionLevel = new Vector2(
                (508f + expGaugeTexture.Width / 2) - (textSizeLevel.Width / 2),
                (792f + expGaugeTexture.Height / 2) - (textSizeLevel.Height / 2)) + _topLeftCorner;

            DrawTextWithStroke(spriteBatch, GameGlobals.Instance.FontTA8BitBold
                , textLevel, positionLevel, Color.White, 1f, new Color(48, 15, 61), 2);
        }

        private void DrawSkillGUI(SpriteBatch spriteBatch)
        {
            //  Burst Skill
            var positionBurst = new Vector2(1282f, 726f);
            var burstTexture = GameGlobals.Instance.GetGuiTexture(GameGlobals.GuiTextureName.burst_skill_pic);

            spriteBatch.Draw(burstTexture
                , positionBurst + _topLeftCorner, null
                , Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

            if (PlayerManager.Instance.Player.IsBurstSkillCooldown)
            {
                // Burst Skill Cooldown
                var cooldownPercentage = PlayerManager.Instance.Player.BurstCooldownTimer / PlayerManager.Instance.Player.BurstCooldownTime;
                var cooldownTexture = GameGlobals.Instance.GetGuiTexture(GameGlobals.GuiTextureName.burst_skill_gui_alpha);
                var cooldownSourceRec = new Rectangle(0, 0
                    , cooldownTexture.Width, (int)(cooldownTexture.Height * cooldownPercentage));

                var cooldownRec = new Rectangle((int)(positionBurst.X + _topLeftCorner.X)
                    , (int)(positionBurst.Y + _topLeftCorner.Y)
                    , cooldownTexture.Width
                    , (int)(cooldownTexture.Height * cooldownPercentage));

                spriteBatch.Draw(cooldownTexture, cooldownRec, cooldownSourceRec
                    , Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);

                // Text Burst Skill Cooldown
                var text = $"{(int)PlayerManager.Instance.Player.BurstCooldownTimer}";
                var textSize = GameGlobals.Instance.FontTA8BitBold.MeasureString(text);
                var positionText = new Vector2(
                    (positionBurst.X + cooldownTexture.Width / 2) - ((textSize.Width * 1.5f) / 2),
                    (positionBurst.Y + cooldownTexture.Height / 2) - ((textSize.Height * 1.5f) / 2)) + _topLeftCorner;

                DrawTextWithStroke(spriteBatch, GameGlobals.Instance.FontTA8BitBold
                    , text, positionText, Color.White, 1.5f, Color.Black, 2);
            }

            // Text Burst Skill Q
            {
                var text = $"Q";
                var textSize = GameGlobals.Instance.FontTA8BitBold.MeasureString(text);
                var positionText = new Vector2(
                    (positionBurst.X + burstTexture.Width / 2) - ((textSize.Width * 1.5f) / 2),
                    (positionBurst.Y + burstTexture.Height / 0.8f) - ((textSize.Height * 1.5f) / 2)) + _topLeftCorner;

                DrawTextWithStroke(spriteBatch, GameGlobals.Instance.FontTA8BitBold
                    , text, positionText, Color.White, 1.5f, Color.Black, 2);
            }

            spriteBatch.Draw(GameGlobals.Instance.GetGuiTexture(GameGlobals.GuiTextureName.burst_skill_gui)
                , positionBurst + _topLeftCorner, null
                , Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

            // Normal Skill
            var positionNormal = new Vector2(1186f, 771f);
            var normalTexture = GameGlobals.Instance.GetGuiTexture(GameGlobals.GuiTextureName.normal_skill_pic);

            spriteBatch.Draw(normalTexture
                , positionNormal + _topLeftCorner, null
                , Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

            if (PlayerManager.Instance.Player.IsNormalSkillCooldown)
            {
                // Normal Skill Cooldown
                var cooldownPercentage = PlayerManager.Instance.Player.NormalCooldownTimer / PlayerManager.Instance.Player.NormalCooldownTime;
                var cooldownTexture = GameGlobals.Instance.GetGuiTexture(GameGlobals.GuiTextureName.normal_skill_gui_alpha);
                var cooldownSourceRec = new Rectangle(0, 0
                    , cooldownTexture.Width, (int)(cooldownTexture.Height * cooldownPercentage));

                var cooldownRec = new Rectangle((int)(positionNormal.X + _topLeftCorner.X)
                    , (int)(positionNormal.Y + _topLeftCorner.Y)
                    , cooldownTexture.Width
                    , (int)(cooldownTexture.Height * cooldownPercentage));

                spriteBatch.Draw(cooldownTexture, cooldownRec, cooldownSourceRec
                    , Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);

                // Text Normal Skill Cooldown
                var text = $"{(int)PlayerManager.Instance.Player.NormalCooldownTimer}";
                var textSize = GameGlobals.Instance.FontTA8BitBold.MeasureString(text);
                var positionText = new Vector2(
                    (positionNormal.X + cooldownTexture.Width / 2) - ((textSize.Width * 1.25f) / 2),
                    (positionNormal.Y + cooldownTexture.Height / 2) - ((textSize.Height * 1.25f) / 2)) + _topLeftCorner;

                DrawTextWithStroke(spriteBatch, GameGlobals.Instance.FontTA8BitBold
                    , text, positionText, Color.White, 1.25f, Color.Black, 2);
            }

            // Text Noraml Skill E
            {
                var text = $"E";
                var textSize = GameGlobals.Instance.FontTA8BitBold.MeasureString(text);
                var positionText = new Vector2(
                    (positionNormal.X + normalTexture.Width / 2) - ((textSize.Width * 1.5f) / 2),
                    (positionNormal.Y + normalTexture.Height / 0.8f) - ((textSize.Height * 1.5f) / 2)) + _topLeftCorner;

                DrawTextWithStroke(spriteBatch, GameGlobals.Instance.FontTA8BitBold
                    , text, positionText, Color.White, 1.5f, Color.Black, 2);
            }

            spriteBatch.Draw(GameGlobals.Instance.GetGuiTexture(GameGlobals.GuiTextureName.normal_skill_gui)
                , positionNormal + _topLeftCorner, null
                , Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

            // Passive Skill
            var positionPassive = new Vector2(1324, 653f);

            spriteBatch.Draw(GameGlobals.Instance.GetGuiTexture(GameGlobals.GuiTextureName.passive_skill_pic)
                , positionPassive + _topLeftCorner, null
                , Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

            if (PlayerManager.Instance.Player.IsPassiveSkillCooldown)
            {
                // Passive Skill Cooldown
                var cooldownPercentage = PlayerManager.Instance.Player.PassiveCooldownTimer / PlayerManager.Instance.Player.PassiveCooldownTime;
                var cooldownTexture = GameGlobals.Instance.GetGuiTexture(GameGlobals.GuiTextureName.passive_skill_gui_alpha);
                var cooldownSourceRec = new Rectangle(0, 0
                    , cooldownTexture.Width, (int)(cooldownTexture.Height * cooldownPercentage));

                var cooldownRec = new Rectangle((int)(positionPassive.X + _topLeftCorner.X)
                    , (int)(positionPassive.Y + _topLeftCorner.Y)
                    , cooldownTexture.Width
                    , (int)(cooldownTexture.Height * cooldownPercentage));

                spriteBatch.Draw(cooldownTexture, cooldownRec, cooldownSourceRec
                    , Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);

                // Text Passive Skill Cooldown
                var text = $"{(int)PlayerManager.Instance.Player.PassiveCooldownTimer}";
                var textSize = GameGlobals.Instance.FontTA8BitBold.MeasureString(text);
                var positionText = new Vector2(
                    (positionPassive.X + cooldownTexture.Width / 2) - (textSize.Width / 2),
                    (positionPassive.Y + cooldownTexture.Height / 2) - (textSize.Height / 2)) + _topLeftCorner;

                DrawTextWithStroke(spriteBatch, GameGlobals.Instance.FontTA8BitBold
                    , text, positionText, Color.White, 1f, Color.Black, 2);
            }

            spriteBatch.Draw(GameGlobals.Instance.GetGuiTexture(GameGlobals.GuiTextureName.passive_skill_gui)
                , positionPassive + _topLeftCorner, null
                , Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        }

        private void DrawInteractionSigh(SpriteBatch spriteBatch)
        {
            if (GameGlobals.Instance.IsDetectedGameObject)
            {
                spriteBatch.Draw(GameGlobals.Instance.GetGuiTexture(GameGlobals.GuiTextureName.press_f)
                    , new Vector2(1055f, 560f) + _topLeftCorner, null
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

            foreach (var entity in entities.Where(e => !e.IsDestroyed && e.IsAggro
                && e.EntityType == Entities.Entity.EntityTypes.Hostile && e.HP > 0))
            {
                var entityPos = entity.Position;
                var text = $"{(int)entity.HP}";
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
                entity.DrawCombatNumbers(spriteBatch);
            }

            if (PlayerManager.Instance.IsCompanionSummoned)
                PlayerManager.Instance.Companions[PlayerManager.Instance.CurrCompaIndex].DrawCombatNumbers(spriteBatch);

            PlayerManager.Instance.Player.DrawCombatNumbers(spriteBatch);
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
                        Position = new Vector2(offsetX, offsetY + (i * 40)) + _topLeftCorner
                    };

                    _spriteItemPack.Play(referId.ToString());
                    _spriteItemPack.Update(_deltaSeconds);

                    spriteBatch.Draw(_spriteItemPack, _transform);

                    var text = $"{itemData.ElementAt(0).Name} x {amount}";
                    var position = new Vector2(
                        offsetX + 22f,
                        (offsetY - 15f) + (i * 40)) + _topLeftCorner;

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
