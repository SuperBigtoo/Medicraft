﻿using Medicraft.Data.Models;
using Medicraft.GameObjects;
using Medicraft.Systems.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Sprites;
using System.Linq;
using static Medicraft.GameObjects.Crop;
using static Medicraft.Systems.GameGlobals;
using static System.Net.Mime.MediaTypeNames;

namespace Medicraft.Systems
{
    public class HUDSystem
    {
        public static float _deltaSeconds;

        private Vector2 _topLeftCorner;
        
        private readonly float _insufficientTime = 3f;
        private float _insufficientTimer;

        private static bool _isAlphaScaleIncrease = true;
        private readonly float _showSignTime = 6f;
        private static float _showSignTimer;
        private static float _alphaScale;

        private bool _nextFeed = false;

        private readonly AnimatedSprite _spriteItemPack = new(Instance.ItemsPackSprites);
        private readonly AnimatedSprite _uiBooksIconHUD = new(Instance.UIBooksIconHUD);

        public HUDSystem()
        {
            _insufficientTimer = _insufficientTime;
            _showSignTimer = 0;
            _alphaScale = 0;
            Instance.ShowMapNameSign = false;
        }

        public void Update(GameTime gameTime)
        {
            _deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _topLeftCorner = Instance.TopLeftCornerPos;

            // Check for the next feed to roll in
            if (Instance.CollectedItemFeed.Count != 0)
            {
                Instance.DisplayFeedTime -= _deltaSeconds;

                if (Instance.DisplayFeedTime <= 0)
                {
                    _nextFeed = true;
                    Instance.DisplayFeedTime = Instance.MaximumDisplayFeedTime;
                }
                else _nextFeed = false;
            }

            // Check for a notification InsufficientSign
            if (Instance.ShowInsufficientSign)
            {
                _insufficientTimer -= _deltaSeconds;

                if (_insufficientTimer <= 0)
                {
                    Instance.ShowInsufficientSign = false;
                    _insufficientTimer = _insufficientTime;
                }
            }

            // Check for showing Map name
            if (Instance.ShowMapNameSign)
            {
                if (_isAlphaScaleIncrease)
                {
                    _showSignTimer += _deltaSeconds;

                    if (_alphaScale < 1f && _showSignTimer >= 2f)
                        _alphaScale += _deltaSeconds * 0.5f;

                    if (_showSignTimer >= _showSignTime)
                        _isAlphaScaleIncrease = false;
                }
                else
                {
                    _showSignTimer -= _deltaSeconds;

                    if (_alphaScale > 0f && _showSignTimer <= _showSignTime - 2f)
                        _alphaScale -= _deltaSeconds;

                    if (_showSignTimer <= 0f)
                    {
                        _alphaScale = 0f;
                        _showSignTimer = 0f;
                        _isAlphaScaleIncrease = true;
                        Instance.ShowMapNameSign = false;
                    }
                }
            }

            if (Instance.ShowQuestObjectiveDescription)
            {
                if (_isAlphaScaleIncrease)
                {
                    _showSignTimer += _deltaSeconds;

                    if (_alphaScale < 1f)
                        _alphaScale += _deltaSeconds * 3f;

                    if (_showSignTimer >= _showSignTime - 3f)
                        _isAlphaScaleIncrease = false;
                }
                else
                {
                    _showSignTimer -= _deltaSeconds;

                    if (_alphaScale > 0f && _showSignTimer <= _showSignTime - 4f)
                    {
                        _alphaScale -= _deltaSeconds;
                    }
                    else if(_alphaScale <= 0f)
                    {
                        _alphaScale = 0f;
                        _showSignTimer = 0f;
                        _isAlphaScaleIncrease = true;
                        Instance.ShowQuestObjectiveDescription = false;
                    }
                }
            }

            // Check for showing Quest stamp
            if (Instance.ShowQuestComplete)
            {
                if (_isAlphaScaleIncrease)
                {
                    _showSignTimer += _deltaSeconds;

                    if (_alphaScale < 1f)
                        _alphaScale += _deltaSeconds * 5f;

                    if (_showSignTimer >= _showSignTime - 3.5f)
                        _isAlphaScaleIncrease = false;
                }
                else
                {
                    _showSignTimer -= _deltaSeconds;

                    if (_alphaScale > 0f && _showSignTimer <= _showSignTime - 5f)
                    {
                        _alphaScale -= _deltaSeconds * 0.5f;
                    }
                    else if (_alphaScale <= 0f)
                    {
                        _alphaScale = 0f;
                        _showSignTimer = 0f;
                        _isAlphaScaleIncrease = true;
                        Instance.ShowQuestComplete = false;
                    }
                }
            }

            // ShowMiniGameScoreEnd
            if (Instance.ShowMiniGameScoreEnd)
            {
                if (_isAlphaScaleIncrease)
                {
                    _showSignTimer += _deltaSeconds;

                    if (_alphaScale < 1f)
                        _alphaScale += _deltaSeconds * 5f;

                    if (_showSignTimer >= _showSignTime - 3.5f)
                        _isAlphaScaleIncrease = false;
                }
                else
                {
                    _showSignTimer -= _deltaSeconds;

                    if (_alphaScale > 0f && _showSignTimer <= _showSignTime - 5f)
                    {
                        _alphaScale -= _deltaSeconds * 0.75f;
                    }
                    else if (_alphaScale <= 0f)
                    {
                        _alphaScale = 0f;
                        _showSignTimer = 0f;
                        _isAlphaScaleIncrease = true;
                        Instance.ShowMiniGameScoreEnd = false;
                    }
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

            if (!Instance.IsGamePause)
            {
                // Draw HP mobs
                DrawHealthPointMobs(spriteBatch);

                // Draw combat numbers mobs & player
                DrawCombatNumbers(spriteBatch);

                // Draw Crop Timer
                DrawCropTimer(spriteBatch);

                // Draw Press F Sign
                DrawInteractionSigh(spriteBatch);

                // Draw Sign Show
                DrawInsufficientSign(spriteBatch);
                DrawMapNameSign(spriteBatch);
                DrawQuestSign(spriteBatch);
                DrawQuestComplete(spriteBatch);
                DrawEntranceMapSign(spriteBatch);
                DrawMiniGameScoreEnd(spriteBatch);
                DrawMiniGameScoreUI(spriteBatch);

                // Draw Feed Items
                DrawCollectedItem(spriteBatch);

                // Draw HUD Bar
                DrawMainHUD(spriteBatch);
            }
        }

        private void DrawMainHUD(SpriteBatch spriteBatch)
        {
            DrawHealthBarGUI(spriteBatch);

            if (!PlayerManager.Instance.IsCompanionDead && PlayerManager.Instance.IsCompanionSummoned)
                DrawCompanionHealthBarGUI(spriteBatch);

            if (Instance.IsEnteringBossFight)
                DrawBossHealthBarGUI(spriteBatch);

            DrawItemBarGUI(spriteBatch);

            DrawLevelGUI(spriteBatch);

            DrawExpBarGUI(spriteBatch);

            DrawSkillGUI(spriteBatch);

            if (Instance.IsDebugMode)
            {
                spriteBatch.FillRectangle(_topLeftCorner.X, _topLeftCorner.Y
                    , ScreenManager.Instance.GraphicsDevice.Viewport.Width, 26, Color.Black * 0.4f);

                var FontSensation = Instance.FontSensation;

                spriteBatch.DrawString(FontSensation, $" Mobs: {EntityManager.Instance.entities.Count}"
                    , Vector2.Zero + _topLeftCorner, Color.White);

                spriteBatch.DrawString(FontSensation, $"Map: {ScreenManager.Instance.CurrentMap}"
                    , new Vector2(100f, 0f) + _topLeftCorner, Color.White);

                spriteBatch.DrawString(FontSensation, $"Time Spawn: {(int)EntityManager.Instance.SpawnTimer}"
                    , new Vector2(270f, 0f) + _topLeftCorner, Color.White);

                spriteBatch.DrawString(FontSensation, $"X: {(int)PlayerManager.Instance.Player.Position.X}"
                    , new Vector2(410f, 0f) + _topLeftCorner, Color.White);

                spriteBatch.DrawString(FontSensation, $"Y: {(int)PlayerManager.Instance.Player.Position.Y}"
                    , new Vector2(490f, 0f) + _topLeftCorner, Color.White);

                spriteBatch.DrawString(FontSensation, $"MouseX: {(int)Instance.MousePosition.X}"
                    , new Vector2(600f, 0f) + _topLeftCorner, Color.White);

                spriteBatch.DrawString(FontSensation, $"MouseY: {(int)Instance.MousePosition.Y}"
                    , new Vector2(720f, 0f) + _topLeftCorner, Color.White);

                spriteBatch.DrawString(FontSensation, $"Total PlayTime: {(int)Instance.TotalPlayTime}"
                    , new Vector2(870f, 0f) + _topLeftCorner, Color.White);

                var rect = new Rectangle((int)_topLeftCorner.X + 50, (int)_topLeftCorner.Y + 220, 300, 500);

                var FontTA8BitBold = Instance.FontTA8BitBold;

                // Player Stats
                {
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

                // CloseEnemyToCompanion
                if (EntityManager.Instance.ClosestEnemyToCompanion != null) {

                    var closestMob = EntityManager.Instance.ClosestEnemyToCompanion;

                    spriteBatch.DrawString(FontTA8BitBold, $"Mob Name: {closestMob.Name}"
                    , new Vector2(55f, 225f + 250) + _topLeftCorner, Color.Magenta);

                    spriteBatch.DrawString(FontTA8BitBold, $"Level: {closestMob.Level}"
                        , new Vector2(55f, 240f + 250) + _topLeftCorner, Color.Magenta);

                    spriteBatch.DrawString(FontTA8BitBold, $"ATK: {closestMob.ATK}"
                        , new Vector2(55f, 260f + 250) + _topLeftCorner, Color.Magenta);

                    spriteBatch.DrawString(FontTA8BitBold, $"Crit: {closestMob.Crit}"
                        , new Vector2(55f, 275f + 250) + _topLeftCorner, Color.Magenta);

                    spriteBatch.DrawString(FontTA8BitBold, $"CritDMG: {closestMob.CritDMG}"
                        , new Vector2(55f, 290f + 250) + _topLeftCorner, Color.Magenta);

                    spriteBatch.DrawString(FontTA8BitBold, $"DEF: {closestMob.DEF}"
                        , new Vector2(55f, 305f + 250) + _topLeftCorner, Color.Magenta);

                    spriteBatch.DrawString(FontTA8BitBold, $"Evasion: {closestMob.Evasion}"
                        , new Vector2(55f, 335f + 250) + _topLeftCorner, Color.Magenta);

                    spriteBatch.DrawString(FontTA8BitBold, $"Speed: {closestMob.Speed}"
                        , new Vector2(55f, 350f + 250) + _topLeftCorner, Color.Magenta);
                }
            }
        }

        public static void DrawOnTopUI(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            spriteBatch.Begin
                (
                    SpriteSortMode.Deferred,
                    samplerState: SamplerState.LinearClamp,
                    blendState: BlendState.AlphaBlend,
                    depthStencilState: DepthStencilState.None,
                    rasterizerState: RasterizerState.CullCounterClockwise,
                    transformMatrix: ScreenManager.Camera.GetTransform(
                        graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height)
                );

            var topLeftCornerPos = Instance.TopLeftCornerPos;
            var font = Instance.FontTA8BitBold;

            // GoldCoin
            var goldCoinTexture = GetGuiTexture(GuiTextureName.gold_coin);
            var goldCoinText = $" {InventoryManager.Instance.GoldCoin}";
            var goldCoinTextSize = font.MeasureString(goldCoinText);
            var hudGoldCoinSize = new Vector2(
                (goldCoinTexture.Width * 0.8f) + goldCoinTextSize.Width,
                goldCoinTexture.Height * 0.8f);

            switch (UIManager.Instance.CurrentUI)
            {
                case UIManager.PlayScreen:
                    // Draw Gold Coin
                    spriteBatch.Draw(goldCoinTexture
                            , new Vector2(
                                720 - (hudGoldCoinSize.X / 2),
                                80 - (hudGoldCoinSize.Y / 2)) + topLeftCornerPos, null
                            , Color.White, 0f, Vector2.Zero, 0.8f, SpriteEffects.None, 0f);

                    DrawTextWithStroke(spriteBatch, font, goldCoinText
                        , new Vector2(
                            720 - (hudGoldCoinSize.X / 2) + 32f,
                            80 - (goldCoinTextSize.Height / 2)) + topLeftCornerPos
                        , Color.Gold, 1f, Color.Black, 2);

                    // Selected Slot
                    var selectedSlot = Instance.CurrentHotbarSelect;
                    spriteBatch.Draw(GetGuiTexture(GuiTextureName.selected_slot)
                        , new Vector2(511f + (52 * selectedSlot), 820f) + Instance.TopLeftCornerPos, null
                        , Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

                    // C
                    {
                        var text = "C";
                        var textSize = font.MeasureString(text);

                        var positionText = new Vector2(
                            1113 - (textSize.Width * 1.1f / 2),
                            122 - (textSize.Height * 1.1f / 2)) + topLeftCornerPos;

                        DrawTextWithStroke(spriteBatch, Instance.FontTA8BitBold
                            , text, positionText, Color.White, 1.1f, Color.Black, 2);
                    }

                    // I
                    {
                        var text = "I";
                        var textSize = font.MeasureString(text);

                        var positionText = new Vector2(
                            1234 - (textSize.Width * 1.1f / 2),
                            122 - (textSize.Height * 1.1f / 2)) + topLeftCornerPos;

                        DrawTextWithStroke(spriteBatch, Instance.FontTA8BitBold
                            , text, positionText, Color.White, 1.1f, Color.Black, 2);
                    }

                    // Esc
                    {
                        var text = "Esc";
                        var textSize = font.MeasureString(text);

                        var positionText = new Vector2(
                            1357 - (textSize.Width * 1.1f / 2),
                            122 - (textSize.Height * 1.1f / 2)) + topLeftCornerPos;

                        DrawTextWithStroke(spriteBatch, Instance.FontTA8BitBold
                            , text, positionText, Color.White, 1.1f, Color.Black, 2);
                    }

                    // Show Quest Objective Clear
                    {
                        DrawQuestObjectiveClearStamp(spriteBatch);
                    }
                    break;

                case UIManager.TradingPanel:
                    // Draw Gold Coin
                    spriteBatch.Draw(goldCoinTexture
                            , new Vector2(
                                720 - (hudGoldCoinSize.X / 2),
                                200 - (hudGoldCoinSize.Y / 2)) + topLeftCornerPos, null
                            , Color.White, 0f, Vector2.Zero, 0.8f, SpriteEffects.None, 0f);

                    DrawTextWithStroke(spriteBatch, font, goldCoinText
                        , new Vector2(
                            720 - (hudGoldCoinSize.X / 2) + 32f,
                            200 - (goldCoinTextSize.Height / 2)) + topLeftCornerPos
                        , Color.Gold, 1f, Color.Black, 2);
                    break;

                case UIManager.InventoryPanel:
                    // Draw Gold Coin
                    spriteBatch.Draw(goldCoinTexture
                            , new Vector2(
                                720 - (hudGoldCoinSize.X / 2),
                                80 - (hudGoldCoinSize.Y / 2)) + topLeftCornerPos, null
                            , Color.White, 0f, Vector2.Zero, 0.8f, SpriteEffects.None, 0f);

                    DrawTextWithStroke(spriteBatch, font, goldCoinText
                        , new Vector2(
                            720 - (hudGoldCoinSize.X / 2) + 32f,
                            80 - (goldCoinTextSize.Height / 2)) + topLeftCornerPos
                        , Color.Gold, 1f, Color.Black, 2);
                    break;

                case UIManager.InspectPanel:
                    // Character Sprite
                    if (Instance.IsOpenInspectPanel && UIManager.Instance.IsCharacterTabSelected
                        && !UIManager.Instance.IsShowConfirmBox)
                    {
                        var charSprite = UIManager.Instance.CharacterSprite;

                        //Sprite.Depth = 0.1f;
                        charSprite.Play("default_idle");
                        charSprite.Update(_deltaSeconds);

                        var positon = PlayerManager.Instance.Player.Position - new Vector2(200f, 50f);
                        var transform = new Transform2()
                        {
                            Scale = new Vector2(1.5f, 1.5f),
                            Rotation = 0f,
                            Position = positon
                        };
                        spriteBatch.Draw(charSprite, transform);

                        if (UIManager.Instance.CharacterType.Equals("Companion"))
                        {
                            if (UIManager.Instance.SelectedCompanion.IsDead)
                            {
                                var text = $"DEAD";
                                var textSize = font.MeasureString(text);

                                DrawTextWithStroke(spriteBatch, font, text
                                    , positon - new Vector2((textSize.Width * 1.5f) / 2, (textSize.Height * 1.5f) / 2)
                                    , Color.DarkRed, 1.5f, Color.Black, 2);
                            }
                            else if (!UIManager.Instance.SelectedCompanion.CompanionData.IsSummoned)
                            {
                                var text = $"INACTIVE";
                                var textSize = font.MeasureString(text);

                                DrawTextWithStroke(spriteBatch, font, text
                                    , positon - new Vector2((textSize.Width * 1.5f) / 2, (textSize.Height * 1.5f) / 2)
                                    , Color.DarkGray, 1.5f, Color.Black, 2);
                            }
                        }
                    }                  
                    break;
            }

            spriteBatch.End();
        }

        private void DrawHealthBarGUI(SpriteBatch spriteBatch)
        {
            // Health Bar Alpha
            spriteBatch.Draw(GetGuiTexture(GuiTextureName.health_bar_alpha)
                , new Vector2(35f, 15f) + _topLeftCorner, null
                , Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

            // Player Profile
            spriteBatch.Draw(GetGuiTexture(GuiTextureName.noah_profile)
                , new Vector2(38f, 18f) + _topLeftCorner, null
                , Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

            // HP gauge
            var curHealthPoint = PlayerManager.Instance.Player.GetCurrentHealthPercentage();
            var hpGaugeTexture = GetGuiTexture(GuiTextureName.healthpoints_gauge);
            var hpGaugeSourceRec = new Rectangle(0, 0
                , (int)(hpGaugeTexture.Width * curHealthPoint), hpGaugeTexture.Height);
            var hpGaugeRec = new Rectangle((int)(166 + _topLeftCorner.X), (int)(40 + _topLeftCorner.Y)
                , (int)(hpGaugeTexture.Width * curHealthPoint), hpGaugeTexture.Height);

            spriteBatch.Draw(hpGaugeTexture, hpGaugeRec, hpGaugeSourceRec
                , Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);

            // Mana gauge
            var curManaPoint = PlayerManager.Instance.Player.GetCurrentManaPercentage();
            var manaGaugeTexture = GetGuiTexture(GuiTextureName.mana_gauge);
            var manaGaugeSourceRec = new Rectangle(0, 0
                , (int)(manaGaugeTexture.Width * curManaPoint), manaGaugeTexture.Height);
            var manaGaugeRec = new Rectangle((int)(167 + _topLeftCorner.X), (int)(84 + _topLeftCorner.Y)
                , (int)(manaGaugeTexture.Width * curManaPoint), manaGaugeTexture.Height);

            spriteBatch.Draw(manaGaugeTexture, manaGaugeRec, manaGaugeSourceRec
                , Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);

            // Health Bar GUI
            spriteBatch.Draw(GetGuiTexture(GuiTextureName.health_bar)
                , new Vector2(35f, 15f) + _topLeftCorner, null
                , Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

            // Text Health Point
            var textHP = $"{(int)PlayerManager.Instance.Player.HP}/{(int)PlayerManager.Instance.Player.MaxHP}";
            var textSizeHP = Instance.FontTA8BitBold.MeasureString(textHP);
            var positionHP = new Vector2(
                (166f + hpGaugeTexture.Width / 2) - (textSizeHP.Width / 2),
                (40f + hpGaugeTexture.Height / 2) - (textSizeHP.Height / 2)) + _topLeftCorner;

            DrawTextWithStroke(spriteBatch, Instance.FontTA8BitBold
                , textHP, positionHP, Color.White, 1f, Color.Black, 2);

            // Text Mana Point
            var textMana = $"{(int)PlayerManager.Instance.Player.Mana}/{(int)PlayerManager.Instance.Player.MaxMana}";
            //var textSizeMana = GameGlobals.Instance.FontTA8Bit.MeasureString(textMana);
            var positionMana = new Vector2(
                (166f + hpGaugeTexture.Width / 2) - (textSizeHP.Width / 2), 80f) + _topLeftCorner;

            DrawTextWithStroke(spriteBatch, Instance.FontTA8Bit
                , textMana, positionMana, Color.White, 1f, Color.Black, 2);
        }

        private void DrawCompanionHealthBarGUI(SpriteBatch spriteBatch)
        {
            var companion = PlayerManager.Instance.Companions[PlayerManager.Instance.CurrCompaIndex];
            var compaProfile = GetGuiTexture(GuiTextureName.companion_profile);

            // Health Bar Companion Alpha
            spriteBatch.Draw(GetGuiTexture(GuiTextureName.health_bar_companion_alpha)
                , new Vector2(127f, 106f) + _topLeftCorner, null
                , Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

            // Companion Profile
            spriteBatch.Draw(compaProfile
                , new Vector2(129f, 108f) + _topLeftCorner, null
                , Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

            // HP gauge
            var curHealthPoint = companion.GetCurrentHealthPercentage();
            var hpGaugeTexture = GetGuiTexture(GuiTextureName.healthpoints_gauge_companion);
            var hpGaugeSourceRec = new Rectangle(0, 0
                , (int)(hpGaugeTexture.Width * curHealthPoint), hpGaugeTexture.Height);
            var hpGaugeRec = new Rectangle((int)(202f + _topLeftCorner.X), (int)(124f + _topLeftCorner.Y)
                , (int)(hpGaugeTexture.Width * curHealthPoint), hpGaugeTexture.Height);

            spriteBatch.Draw(hpGaugeTexture, hpGaugeRec, hpGaugeSourceRec
                , Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);

            // Text Health Point
            var textHP = $"{(int)companion.HP}/{(int)companion.MaxHP}";
            var textSizeHP = Instance.FontTA8BitBold.MeasureString(textHP);
            var positionHP = new Vector2(
                (202f + hpGaugeTexture.Width / 2) - ((textSizeHP.Width * 0.8f) / 2),
                (124f + hpGaugeTexture.Height / 2) - ((textSizeHP.Height * 0.8f) / 2)) + _topLeftCorner;

            DrawTextWithStroke(spriteBatch, Instance.FontTA8BitBold
                , textHP, positionHP, Color.White, 0.8f, Color.Black, 2);

            // Health Bar Companion GUI
            spriteBatch.Draw(GetGuiTexture(GuiTextureName.health_bar_companion)
                , new Vector2(127f, 106f) + _topLeftCorner, null
                , Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        }

        private void DrawBossHealthBarGUI(SpriteBatch spriteBatch)
        {
            if (EntityManager.Instance.Boss == null) return;

            var boss = EntityManager.Instance.Boss;

            // Health Bar Boss Alpha
            spriteBatch.Draw(GetGuiTexture(GuiTextureName.health_bar_boss_alpha)
                , new Vector2(506f, 92f) + _topLeftCorner, null
                , Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

            // HP gauge
            var curHealthPoint = boss.GetCurrentHealthPercentage();
            var hpGaugeTexture = GetGuiTexture(GuiTextureName.boss_gauge);
            var hpGaugeSourceRec = new Rectangle(0, 0
                , (int)(hpGaugeTexture.Width * curHealthPoint), hpGaugeTexture.Height);
            var hpGaugeRec = new Rectangle((int)(541 + _topLeftCorner.X), (int)(123 + _topLeftCorner.Y)
                , (int)(hpGaugeTexture.Width * curHealthPoint), hpGaugeTexture.Height);

            spriteBatch.Draw(hpGaugeTexture, hpGaugeRec, hpGaugeSourceRec
                , Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);

            // Health Bar Boss GUI
            spriteBatch.Draw(GetGuiTexture(GuiTextureName.health_bar_boss)
                , new Vector2(506f, 92f) + _topLeftCorner, null
                , Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

            // Text Name
            var text = boss.Name;
            var textSize = Instance.FontTA16Bit.MeasureString(text);
            var position = new Vector2(Instance.GameScreenCenter.X
                - (textSize.Width / 2), 90)
                + _topLeftCorner;

            DrawTextWithStroke(spriteBatch, Instance.FontTA16Bit
                , text, position, Color.DarkRed, 1f, Color.Black, 1);
        }

        private void DrawItemBarGUI(SpriteBatch spriteBatch)
        {
            // Item Bar
            spriteBatch.Draw(GetGuiTexture(GuiTextureName.item_bar)
                , new Vector2(499f, 814f) + _topLeftCorner, null
                , Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

            // Item Slot
            for (int i = 0; i < 8; i++ )
            {
                spriteBatch.Draw(GetGuiTexture(GuiTextureName.item_slot)
                    , new Vector2(515f + (52 * i), 824f) + _topLeftCorner, null
                    , Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);          
            }
        }
   
        private void DrawLevelGUI(SpriteBatch spriteBatch)
        {
            // Level Gui
            var LevelGuiTexture = GetGuiTexture(GuiTextureName.level_gui);

            spriteBatch.Draw(LevelGuiTexture
                , new Vector2(25f, 10f) + _topLeftCorner, null
                , Color.White, 0f, Vector2.Zero, 1.25f, SpriteEffects.None, 0f);

            // Text Level
            var textLevel = $"{PlayerManager.Instance.Player.Level}";
            var textSizeLevel = Instance.FontTA8BitBold.MeasureString(textLevel);
            var position = new Vector2(
                (29f + LevelGuiTexture.Width / 2) - (textSizeLevel.Width / 2),
                (14f + LevelGuiTexture.Height / 2) - (textSizeLevel.Height / 2)) + _topLeftCorner;

            DrawTextWithStroke(spriteBatch, Instance.FontTA8BitBold
                , textLevel, position, Color.White, 1f, Color.Black, 1);
        }

        private void DrawExpBarGUI(SpriteBatch spriteBatch)
        {
            var position = new Vector2(508f, 792f);

            // Experience Alpha
            spriteBatch.Draw(GetGuiTexture(GuiTextureName.exp_bar_alpha)
                , position + _topLeftCorner, null
                , Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

            // Experience gauge
            var curEXP = PlayerManager.Instance.Player.GetCurrentEXPPercentage();
            var expGaugeTexture = GetGuiTexture(GuiTextureName.exp_gauge);
            var expGaugeSourceRec = new Rectangle(0, 0
                , (int)(expGaugeTexture.Width * curEXP), expGaugeTexture.Height);
            var expGaugeRec = new Rectangle((int)(508f + _topLeftCorner.X), (int)(792f + _topLeftCorner.Y)
                , (int)(expGaugeTexture.Width * curEXP), expGaugeTexture.Height);

            spriteBatch.Draw(expGaugeTexture, expGaugeRec, expGaugeSourceRec
                , Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);

            // Experience Bar
            spriteBatch.Draw(GetGuiTexture(GuiTextureName.exp_bar)
                , position + _topLeftCorner, null
                , Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

            // Text Experience Point
            var textLevel = $"{PlayerManager.Instance.Player.Level}";
            var textSizeLevel = Instance.FontTA8BitBold.MeasureString(textLevel);
            var positionLevel = new Vector2(
                (508f + expGaugeTexture.Width / 2) - (textSizeLevel.Width / 2),
                (792f + expGaugeTexture.Height / 2) - (textSizeLevel.Height / 2)) + _topLeftCorner;

            DrawTextWithStroke(spriteBatch, Instance.FontTA8BitBold
                , textLevel, positionLevel, Color.White, 1f, new Color(48, 15, 61), 2);
        }

        private void DrawSkillGUI(SpriteBatch spriteBatch)
        {
            //  Burst Skill
            var positionBurst = new Vector2(1282f, 726f);
            var burstTexture = GetGuiTexture(GuiTextureName.burst_skill_pic);

            spriteBatch.Draw(burstTexture
                , positionBurst + _topLeftCorner, null
                , Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

            if (PlayerManager.Instance.Player.IsBurstSkillCooldown)
            {
                // Burst Skill Cooldown
                var cooldownPercentage = PlayerManager.Instance.Player.BurstCooldownTimer / PlayerManager.Instance.Player.BurstCooldownTime;
                var cooldownTexture = GetGuiTexture(GuiTextureName.burst_skill_gui_alpha);
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
                var textSize = Instance.FontTA8BitBold.MeasureString(text);
                var positionText = new Vector2(
                    (positionBurst.X + cooldownTexture.Width / 2) - ((textSize.Width * 1.5f) / 2),
                    (positionBurst.Y + cooldownTexture.Height / 2) - ((textSize.Height * 1.5f) / 2)) + _topLeftCorner;

                DrawTextWithStroke(spriteBatch, Instance.FontTA8BitBold
                    , text, positionText, Color.White, 1.5f, Color.Black, 2);
            }

            // Text Burst Skill Q
            {
                var text = $"Q";
                var textSize = Instance.FontTA8BitBold.MeasureString(text);
                var positionText = new Vector2(
                    (positionBurst.X + burstTexture.Width / 2) - ((textSize.Width * 1.5f) / 2),
                    (positionBurst.Y + burstTexture.Height / 0.8f) - ((textSize.Height * 1.5f) / 2)) + _topLeftCorner;

                DrawTextWithStroke(spriteBatch, Instance.FontTA8BitBold
                    , text, positionText, Color.White, 1.5f, Color.Black, 2);
            }

            spriteBatch.Draw(GetGuiTexture(GuiTextureName.burst_skill_gui)
                , positionBurst + _topLeftCorner, null
                , Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

            // Normal Skill
            var positionNormal = new Vector2(1186f, 771f);
            var normalTexture = GetGuiTexture(GuiTextureName.normal_skill_pic);

            spriteBatch.Draw(normalTexture
                , positionNormal + _topLeftCorner, null
                , Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

            if (PlayerManager.Instance.Player.IsNormalSkillCooldown)
            {
                // Normal Skill Cooldown
                var cooldownPercentage = PlayerManager.Instance.Player.NormalCooldownTimer / PlayerManager.Instance.Player.NormalCooldownTime;
                var cooldownTexture = GetGuiTexture(GuiTextureName.normal_skill_gui_alpha);
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
                var textSize = Instance.FontTA8BitBold.MeasureString(text);
                var positionText = new Vector2(
                    (positionNormal.X + cooldownTexture.Width / 2) - ((textSize.Width * 1.25f) / 2),
                    (positionNormal.Y + cooldownTexture.Height / 2) - ((textSize.Height * 1.25f) / 2)) + _topLeftCorner;

                DrawTextWithStroke(spriteBatch, Instance.FontTA8BitBold
                    , text, positionText, Color.White, 1.25f, Color.Black, 2);
            }

            // Text Noraml Skill E
            {
                var text = $"E";
                var textSize = Instance.FontTA8BitBold.MeasureString(text);
                var positionText = new Vector2(
                    (positionNormal.X + normalTexture.Width / 2) - ((textSize.Width * 1.5f) / 2),
                    (positionNormal.Y + normalTexture.Height / 0.8f) - ((textSize.Height * 1.5f) / 2)) + _topLeftCorner;

                DrawTextWithStroke(spriteBatch, Instance.FontTA8BitBold
                    , text, positionText, Color.White, 1.5f, Color.Black, 2);
            }

            spriteBatch.Draw(GetGuiTexture(GuiTextureName.normal_skill_gui)
                , positionNormal + _topLeftCorner, null
                , Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

            // Passive Skill
            var positionPassive = new Vector2(1324, 653f);

            spriteBatch.Draw(GetGuiTexture(GuiTextureName.passive_skill_pic)
                , positionPassive + _topLeftCorner, null
                , Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

            if (PlayerManager.Instance.Player.IsPassiveSkillCooldown)
            {
                // Passive Skill Cooldown
                var cooldownPercentage = PlayerManager.Instance.Player.PassiveCooldownTimer / PlayerManager.Instance.Player.PassiveCooldownTime;
                var cooldownTexture = GetGuiTexture(GuiTextureName.passive_skill_gui_alpha);
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
                var textSize = Instance.FontTA8BitBold.MeasureString(text);
                var positionText = new Vector2(
                    (positionPassive.X + cooldownTexture.Width / 2) - (textSize.Width / 2),
                    (positionPassive.Y + cooldownTexture.Height / 2) - (textSize.Height / 2)) + _topLeftCorner;

                DrawTextWithStroke(spriteBatch, Instance.FontTA8BitBold
                    , text, positionText, Color.White, 1f, Color.Black, 2);
            }

            spriteBatch.Draw(GetGuiTexture(GuiTextureName.passive_skill_gui)
                , positionPassive + _topLeftCorner, null
                , Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        }

        private void DrawInteractionSigh(SpriteBatch spriteBatch)
        {
            var position = new Vector2(1055f, 560f);

            if (PlayerManager.Instance.IsDetectedObject)
            {
                Texture2D texture = null;
                bool isDraw = true;

                switch (PlayerManager.Instance.DetectedObject.ObjectType)
                {
                    case GameObject.GameObjectType.Item:
                    case GameObject.GameObjectType.QuestObject:
                        texture = GetGuiTexture(GuiTextureName.press_collecting);
                        break;

                    default:
                    case GameObject.GameObjectType.CraftingTable:
                    case GameObject.GameObjectType.SavingTable:
                    case GameObject.GameObjectType.WarpPoint:
                    case GameObject.GameObjectType.RestPoint:
                        texture = GetGuiTexture(GuiTextureName.press_interacting);
                        break;

                    case GameObject.GameObjectType.Crop:
                        Crop crop = PlayerManager.Instance.DetectedObject as Crop;

                        if (crop.CropStage == Crop.CropStages.Harvestable)
                        {
                            texture = GetGuiTexture(GuiTextureName.press_collecting);
                        }
                        else if (crop.CropStage == Crop.CropStages.Empty)
                        {
                            texture = GetGuiTexture(GuiTextureName.press_interacting);
                        }
                        else isDraw = false;

                        break;
                }

                if (!isDraw) return;

                spriteBatch.Draw(texture
                    , position + _topLeftCorner, null
                    , Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

                var text = "F";
                var textSize = Instance.FontTA8BitBold.MeasureString(text);

                var positionText = new Vector2(
                    (position.X + texture.Width * 0.5f) - (textSize.Width * 1.25f / 2),
                    (position.Y + texture.Height * 1.25f) - (textSize.Height * 1.25f / 2)) + _topLeftCorner;

                DrawTextWithStroke(spriteBatch, Instance.FontTA8BitBold
                    , text, positionText, Color.White, 1.25f, Color.Black, 2);
            }
            else if (PlayerManager.Instance.IsDetectedInteractableMob)
            {
                Texture2D texture = GetGuiTexture(GuiTextureName.press_interacting);

                spriteBatch.Draw(texture
                    , position + _topLeftCorner, null
                    , Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

                var text = "F";
                var textSize = Instance.FontTA8BitBold.MeasureString(text);

                var positionText = new Vector2(
                    (position.X + texture.Width * 0.5f) - (textSize.Width * 1.25f / 2),
                    (position.Y + texture.Height * 1.25f) - (textSize.Height * 1.25f / 2)) + _topLeftCorner;

                DrawTextWithStroke(spriteBatch, Instance.FontTA8BitBold
                    , text, positionText, Color.White, 1.25f, Color.Black, 2);
            }
        }

        private void DrawEntranceMapSign(SpriteBatch spriteBatch)
        {
            var enteringZoneArea = Instance.EnteringZoneArea;

            foreach (var areaData in enteringZoneArea)
            {
                if (areaData.SpriteUI != string.Empty)
                {
                    var pos = areaData.Bounds.Center;

                    var transform = new Transform2
                    {
                        Scale = Vector2.One,
                        Rotation = 0f,
                        Position = new Vector2(pos.X, pos.Y)
                    };

                    _uiBooksIconHUD.Play(areaData.SpriteUI);
                    _uiBooksIconHUD.Depth = 0;
                    _uiBooksIconHUD.Update(_deltaSeconds);

                    spriteBatch.Draw(_uiBooksIconHUD, transform);
                }
            }
        }

        private void DrawHealthPointMobs(SpriteBatch spriteBatch)
        {
            var entities = EntityManager.Instance.Entities;
            var FontSensation = Instance.FontSensation;
            var FontTA8BitBold = Instance.FontTA8BitBold;

            foreach (var entity in entities.Where(e => !e.IsDestroyed && e.IsAggro
                && e.EntityType == Entities.Entity.EntityTypes.HostileMob && e.HP > 0))
            {
                var entityPos = entity.Position;
                //var text = $"{(int)entity.HP}";
                //var textSize = FontSensation.MeasureString(text);
                //var position = entityPos - new Vector2(textSize.Width / 2
                //    , (textSize.Height / 2) + (entity.Sprite.TextureRegion.Height / 2));

                //DrawTextWithStroke(spriteBatch, FontSensation, text, position, Color.DarkRed, 1f, Color.Black, 1);

                var mobHP = GetGuiTexture(GuiTextureName.health_bar_mob);
                var mobGauge = GetGuiTexture(GuiTextureName.mob_gauge);
                var offSet = new Vector2(mobHP.Width / 2, (mobHP.Height / 2) + (entity.Sprite.TextureRegion.Height / 2));
                var position = entityPos - offSet;

                // HP gauge
                var curHealthPoint = entity.GetCurrentHealthPercentage();
                
                var hpGaugeSourceRec = new Rectangle(
                    0, 0,
                    (int)(mobGauge.Width * curHealthPoint),
                    mobGauge.Height);

                var hpGaugeRec = new Rectangle(
                    (int)(position.X),
                    (int)(position.Y),
                    (int)(mobGauge.Width * curHealthPoint),
                    mobGauge.Height);

                spriteBatch.Draw(mobGauge, hpGaugeRec, hpGaugeSourceRec
                    , Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);

                // Text Health Point
                var textHP = $"{(int)entity.HP}/{(int)entity.MaxHP}";
                var textSizeHP = FontTA8BitBold.MeasureString(textHP);
                var positionHP = new Vector2(
                    (position.X + mobGauge.Width / 2) - ((textSizeHP.Width * 0.8f) / 2),
                    (position.Y + mobGauge.Height / 2) - ((textSizeHP.Height * 0.8f) / 2));

                DrawTextWithStroke(spriteBatch, FontTA8BitBold
                    , textHP, positionHP, Color.White, 0.8f, Color.Black, 2);

                // Health Bar mob GUI
                spriteBatch.Draw(mobHP, position, null
                    , Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
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

        private void DrawCropTimer(SpriteBatch spriteBatch)
        {
            var crops = ObjectManager.Instance.Crops.Where(e => e.IsVisible);

            foreach (var crop in crops)
            {
                if (crop.CropStage == CropStages.Growing)
                    crop.DrawGrowingTimer(spriteBatch);
            }
        }

        private void DrawCollectedItem(SpriteBatch spriteBatch) 
        {
            if (Instance.CollectedItemFeed.Count != 0)
            {
                int n;
                if (Instance.CollectedItemFeed.Count >= Instance.MaximumItemFeed)
                {
                    n = Instance.MaximumItemFeed;
                }
                else n = Instance.CollectedItemFeed.Count;

                for (int i = 0; i < n; i++)
                {
                    var referId = Instance.CollectedItemFeed.ElementAt(i).ItemId;
                    var amount = Instance.CollectedItemFeed.ElementAt(i).Count;
                    var offsetX = Instance.FeedPoint.X;
                    var offsetY = Instance.FeedPoint.Y;

                    var itemData = Instance.ItemsDatas.Where(i => i.ItemId.Equals(referId));

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

                    DrawTextWithStroke(spriteBatch, Instance.FontTA8Bit
                        , text, position, Color.White, 1f, Color.Black, 2);
                }

                if (_nextFeed)
                {
                    if (Instance.CollectedItemFeed.Count >= Instance.MaximumItemFeed)
                    {
                        Instance.CollectedItemFeed.RemoveRange(0, Instance.MaximumItemFeed);
                    }
                    else Instance.CollectedItemFeed.RemoveRange(0, Instance.CollectedItemFeed.Count);
                }
            }
        }

        public static void AddFeedCollectedItem(int referId, int amount)
        {
            Instance.CollectedItemFeed.Add(new InventoryItemData()
            {
                ItemId = referId,
                Count = amount
            });

            Instance.DisplayFeedTime = Instance.MaximumDisplayFeedTime;
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

        private static void DrawInsufficientSign(SpriteBatch spriteBatch)
        {
            if (Instance.ShowInsufficientSign)
            {
                spriteBatch.Draw(GetGuiTexture(GuiTextureName.insufficient)
                    , PlayerManager.Instance.Player.Position
                        + new Vector2(25, -((PlayerManager.Instance.Player.Sprite.TextureRegion.Height / 2) + 25))
                    , null, Color.White, 0f, Vector2.Zero, 0.40f, SpriteEffects.None, 0f);
            }
        }

        private static void DrawMapNameSign(SpriteBatch spriteBatch)
        {
            if (Instance.ShowMapNameSign)
            {
                var topLeftCornerPos = Instance.TopLeftCornerPos;

                Texture2D texture = Instance.CaseMapNameSign switch
                {
                    1 => GetGuiTexture(GuiTextureName.battlezone_map_sign),
                    _ => GetGuiTexture(GuiTextureName.safezone_map_sign),
                };

                var position = new Vector2(439, 125);

                spriteBatch.Draw(texture
                    , position + topLeftCornerPos
                    , null, Color.White * _alphaScale, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

                var text = Instance.MapNameSign;
                var textSize = Instance.FontTA8BitBold.MeasureString(text);

                var positionText = new Vector2(
                    (position.X + texture.Width / 2) - (textSize.Width * 1.5f / 2),
                    (position.Y + texture.Height / 1.35f) - (textSize.Height * 1.5f / 2)) + topLeftCornerPos;

                DrawTextWithStroke(spriteBatch, Instance.FontTA8BitBold
                    , text, positionText, Color.White * _alphaScale, 1.5f, Color.Black * _alphaScale, 2);
            }
        }

        private void DrawMiniGameScoreUI(SpriteBatch spriteBatch)
        {
            if (Instance.IsMiniGameStart)
            {
                var topLeftCornerPos = Instance.TopLeftCornerPos;

                Texture2D textureTime = GetGuiTexture(GuiTextureName.timewindow);

                var position = new Vector2(500, 100);

                spriteBatch.Draw(textureTime
                    , position + topLeftCornerPos
                    , null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

                var text = $"{(int)Instance.MiniGameTimer}";
                var textSize = Instance.FontTA8BitBold.MeasureString(text);
                var positionText = new Vector2(
                    (position.X + textureTime.Width / 2) - (textSize.Width  / 2),
                    (position.Y + textureTime.Height / 2f) - (textSize.Height / 2)) + topLeftCornerPos;

                DrawTextWithStroke(spriteBatch, Instance.FontTA8BitBold
                    , text, positionText, Color.White, 1f, Color.Black, 2);

                // Score
                Texture2D textureScore = GetGuiTexture(GuiTextureName.scorewindow);

                position = new Vector2(740, 100);

                spriteBatch.Draw(textureScore
                    , position + topLeftCornerPos
                    , null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

                text = $"{Instance.MiniGameScore}";
                textSize = Instance.FontTA8BitBold.MeasureString(text);
                positionText = new Vector2(
                    (position.X + textureScore.Width / 2) - (textSize.Width / 2),
                    (position.Y + textureScore.Height / 2f) - (textSize.Height / 2)) + topLeftCornerPos;

                DrawTextWithStroke(spriteBatch, Instance.FontTA8BitBold
                    , text, positionText, Color.White, 1f, Color.Black, 2);
            }
        }

        private static void DrawMiniGameScoreEnd(SpriteBatch spriteBatch)
        {
            if (Instance.ShowMiniGameScoreEnd)
            {
                Texture2D texture = null;

                switch (Instance.ScoreCase)
                {
                    case 1:
                        texture = GetGuiTexture(GuiTextureName.advrankup_ranklogo_S);
                        break;

                    case 2:
                        texture = GetGuiTexture(GuiTextureName.advrankup_ranklogo_A);
                        break;

                    case 3:
                        texture = GetGuiTexture(GuiTextureName.advrankup_ranklogo_B);
                        break;

                    case 4:
                        texture = GetGuiTexture(GuiTextureName.advrankup_ranklogo_C);
                        break;

                    case 5:
                        texture = GetGuiTexture(GuiTextureName.advrankup_ranklogo_D);
                        break;
                }               

                var position = PlayerManager.Instance.Player.Position - new Vector2(
                    texture.Width / 2, texture.Height * 1.25f);

                spriteBatch.Draw(texture, position
                    , null, Color.White * _alphaScale, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            }
        }

        private static void DrawQuestSign(SpriteBatch spriteBatch)
        {
            if (Instance.ShowQuestObjectiveDescription)
            {
                var topLeftCornerPos = Instance.TopLeftCornerPos;

                Texture2D texture = GetGuiTexture(GuiTextureName.quest_objective_window);

                var position = new Vector2(
                    Instance.GameScreenCenter.X - texture.Width / 2,
                    150);

                spriteBatch.Draw(texture
                    , position + topLeftCornerPos
                    , null, Color.White * _alphaScale, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

                var text = Instance.QuestObjectiveDescription;
                var textSize = Instance.FontTA8BitBold.MeasureString(text);

                var positionText = new Vector2(
                    (position.X + texture.Width / 2) - (textSize.Width * 1.25f / 2),
                    (position.Y + texture.Height / 2) - (textSize.Height * 1.25f / 2));

                DrawTextWithStroke(spriteBatch, Instance.FontTA8BitBold
                    , text, positionText, Color.White * _alphaScale, 1.25f, Color.Black * _alphaScale, 2);
            }
        }

        private static void DrawQuestComplete(SpriteBatch spriteBatch)
        {
            if (Instance.ShowQuestComplete)
            {
                Texture2D texture = GetGuiTexture(GuiTextureName.quest_stamp);

                var position = PlayerManager.Instance.Player.Position - new Vector2(
                    texture.Width / 2, texture.Height * 1.25f);

                spriteBatch.Draw(texture, position
                    , null, Color.White * _alphaScale, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            }
        }

        private static void DrawQuestObjectiveClearStamp(SpriteBatch spriteBatch)
        {
            if (Instance.ShowQuestObjectiveClear)
            {
                var topLeftCornerPos = Instance.TopLeftCornerPos;

                Texture2D texture = GetGuiTexture(GuiTextureName.quest_objective_stamp_cleared);

                var position = new Vector2(1140, 250);

                spriteBatch.Draw(texture, position + topLeftCornerPos
                    , null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            }
        }

        public static void ShowInsufficientSign()
        {
            Instance.ShowInsufficientSign = true;
        }

        public static void ShowMiniGameScoreEnd()
        {
            Instance.ShowMiniGameScoreEnd = true;
        }

        public static void ShowMapNameSign(int caseNumber, string mapName)
        {
            Instance.ShowMapNameSign = true;
            Instance.CaseMapNameSign = caseNumber;
            Instance.MapNameSign = mapName;

            Instance.ShowQuestObjectiveDescription = false;
            Instance.ShowQuestComplete = false;
            _alphaScale = 0f;
            _showSignTimer = 0f;
            _isAlphaScaleIncrease = true;
        }

        public static void ShowQuestObjective(string description)
        {
            Instance.ShowQuestObjectiveDescription = true;
            Instance.QuestObjectiveDescription = description;

            Instance.ShowMapNameSign = false;
            Instance.ShowQuestComplete = false;
            _alphaScale = 0f;
            _showSignTimer = 0f;
            _isAlphaScaleIncrease = true;
        }

        public static void ShowQuestComplete()
        {
            Instance.ShowQuestComplete = true;

            Instance.ShowMapNameSign = false;
            Instance.ShowQuestObjectiveDescription = false;
            _alphaScale = 0f;
            _showSignTimer = 0f;
            _isAlphaScaleIncrease = true;
        }
    }
}
