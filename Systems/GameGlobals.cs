using GeonBit.UI;
using Medicraft.Data;
using Medicraft.Data.Models;
using Medicraft.Systems.Managers;
using Medicraft.Systems.PathFinding;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Content;
using MonoGame.Extended.Serialization;
using MonoGame.Extended.Sprites;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Medicraft.Systems
{
    public class GameGlobals
    {
        public ContentManager Content { private set; get; }

        // Global Variables
        public Keys SaveGameKeyForTest { private set; get; } = Keys.M;
        public Keys PauseMenuKey { private set; get; } = Keys.Escape;
        public Keys OpenInventoryKey { private set; get; } = Keys.I;
        public Keys OpenCraftingKey { private set; get; } = Keys.O;
        public Keys OpenInspectKey { private set; get; } = Keys.C;
        public Keys DebugModeKey { private set; get; } = Keys.F1;
        public Keys ShowPathFindingKey { private set; get; } = Keys.F2;
        public Keys RecallCompanionKey { private set; get; } = Keys.R;
        public MouseState PrevMouse { set; get; }
        public MouseState CurMouse { set; get; }
        public KeyboardState PrevKeyboard { set; get; }
        public KeyboardState CurKeyboard { set; get; }
        public Point2 MousePosition { private set; get; }
        public Vector2 TopLeftCornerPos { set; get; }
        public Vector2 InitialCameraPos { set; get; }
        public Vector2 AddingCameraPos { set; get; }
        public Vector2 GameScreen { private set; get; }
        public Vector2 GameScreenCenter { private set; get; }
        public Vector2 DefaultAdapterViewport { private set; get; }
        public bool IsGameActive { set; get; }
        public bool IsGamePause { set; get; }      
        public bool SwitchDebugMode { set; get; }
        public bool IsDebugMode { set; get; }
        public bool SwitchShowPath { set; get; }
        public bool ShowInsufficientSign { set; get; }
        public bool ShowMapNameSign { set; get; }
        public int CaseMapNameSign { set; get; }
        public string MapNameSign { set; get; }
        public bool IsFullScreen { set; get; }
        public bool IsShowPath { set; get; }  
        public bool IsEnteringBossFight { set; get; }
        public float TotalPlayTime { set; get; }
        public int MaxLevel { set; get; }


        // UI MainMenu & PlayScreen
        public BuiltinThemes BuiltinTheme { set; get; }
        public bool IsOpenGUI { set; get; }
        public bool IsPauseMenuAllowed { set; get; }
        public bool IsOpenMainMenu { set; get; }
        public bool IsMainBGEnding { set; get; }
        public bool IsRefreshPlayScreenUI { set; get; }

        // Pause Menu
        public bool SwitchOpenPauseMenuPanel { set; get; }
        public bool IsOpenPauseMenu { set; get; }

        // Sound & Music
        public float SoundEffectVolume { set; get; }
        public float BackgroundMusicVolume { set; get; }
        public List<MusicData> CurrentMapMusics { private set; get; }

        // Hotbar Slot Numbers
        public bool SwitchSlot_1 { set; get; }
        public bool SwitchSlot_2 { set; get; }
        public bool SwitchSlot_3 { set; get; }
        public bool SwitchSlot_4 { set; get; }
        public bool SwitchSlot_5 { set; get; }
        public bool SwitchSlot_6 { set; get; }
        public bool SwitchSlot_7 { set; get; }
        public bool SwitchSlot_8 { set; get; }

        // TestMap
        public float MobsTestSpawnTime { private set; get; }
        public float MobsTestSpawnTimer { set; get; }
        public bool IsBossTestDead { set; get; }
        public float BossTestSpawnTime { private set; get; }
        public float BossTestSpawnTimer { set; get; }
        public float ObjectTestSpawnTime { private set; get; }
        public float ObjectTestSpawnTimer { set; get; }

        // Chapter 1
        public float MobsOneSpawnTime { private set; get; }
        public float MobsOneSpawnTimer { set; get; }
        public bool IsBossOneDead { set; get; }
        public float BossOneSpawnTime { private set; get; }
        public float BossOneSpawnTimer { set; get; }
        public float ObjectOneSpawnTime { private set; get; }
        public float ObjectOneSpawnTimer { set; get; }

        // Chapter 2
        // Chapter 3
        // Chapter 4
        // Chapter 5
        // Chapter 6

        // GameSave & Config
        public int SelectedGameSaveIndex { set; get; }
        public int MaxGameSaveSlot { private set; get; }
        public string GameSavePath { private set; get; }
        public List<GameSaveData> GameSave { private set; get; }
        public string GameConfigPath { private set; get; }
        public GameConfigData GameConfig { private set; get; }

        // Inventory
        public int MaximunInventorySlot { private set; get; }
        public int MaximunItemCount { private set; get; }
        public int DefaultInventorySlot { private set; get; }
        public int MaxItemBarSlot { private set; get; }
        public int CurrentHotbarSelect { set; get; }
        public bool SwitchOpenInventoryPanel { set; get; }
        public bool IsOpenInventoryPanel { set; get; }

        // Crafting
        public bool SwitchOpenCraftingPanel { set; get; }
        public bool IsOpenCraftingPanel { set; get; }

        // Inspecting
        public bool SwitchOpenInspectPanel { set; get; }
        public bool IsOpenInspectPanel { set; get; }

        // Save Menu
        public bool SwitchOpenSaveMenuPanel { set; get; }
        public bool IsOpenSaveMenuPanel { set; get; }

        // Trading Menu
        public bool SwitchOpenTradingPanel { set; get; }
        public bool IsOpenTradingPanel { set; get; }

        // Warp Menu
        public bool SwitchOpenWarpPointPanel { set; get; }
        public bool IsOpenWarpPointPanel { set; get; }

        // Game Datas
        public PlayerData InitialPlayerData { private set; get; }
        public SpriteSheet PlayerSpriteSheet { private set; get; }
        public List<SpriteSheet> CompanionSpriteSheet { private set; get; }
        public BitmapFont FontSensation { private set; get; }
        public BitmapFont FontMinecraft { private set; get; }
        public BitmapFont FontTA8Bit { private set; get; }
        public BitmapFont FontTA8BitBold { private set; get; }
        public BitmapFont FontTA16Bit { private set; get; }
        public List<Texture2D> GuiTextures { private set; get; }
        public List<Texture2D> AbilityTextures { private set; get; }
        public List<ExperienceCapacityData> ExperienceCapacityDatas { private set; get; }
        public List<MapLocationPointData> MapLocationPointDatas { private set; get; }         // All Point Loaction of Maps
        public List<ItemData> ItemsDatas { private set; get; }       // All items data
        public List<EquipmentStatsData> EquipmentStatsDatas { private set; get; }
        public List<ItemEffectData> ItemEffectDatas { private set; get; }
        public List<CraftingRecipeData> CraftingRecipeDatas { private set; get; }       // All Crafting Recipe data
        public List<CharacterData> CharacterDatas { private set; get; }                 // All Character data
        public List<ChapterItemData> ChapterItemDatas { private set; get; }
        public List<SkillDescriptionData> SkillDescriptionDatas { private set; get; }
        public List<MedicineDescriptionData> MedicineDescriptionDatas { private set; get; }
        public SpriteSheet ItemsPackSprites { private set; get; }   // All Item sprite
        public SpriteSheet UIBooksIconHUD { private set; get; }
        public SpriteSheet WarpPointSprite { private set; get; }
        public SpriteSheet HitSpriteEffect { private set; get; }
        public SpriteSheet HitSkillSpriteEffect { private set; get; }
        public SpriteSheet BossSpriteEffect { private set; get; }
        public SpriteSheet StatesSpriteEffect { private set; get; }
        public SpriteSheet MagicCircleEffect { private set; get; }
        public List<Texture2D> ShadowTextures { private set; get; }
        public List<SoundEffect> SoundEffects { private set; get; }
        public List<string> BackgroundMusicPath { private set; get; }

        // Collecting Item Feed
        public List<InventoryItemData> CollectedItemFeed { private set; get; }      // Feed collected item
        public Point2 FeedPoint { private set; get; }
        public int MaximumItemFeed { private set; get; }
        public float DisplayFeedTime { set; get; }
        public float MaximumDisplayFeedTime { private set; get; }

        // Tilemap, Objective Area & Layer Depth
        public List<RectangleF> CollistionObject { private set; get; }       // "Collision"
        public List<RectangleF> TopLayerObject { private set; get; }
        public List<RectangleF> MiddleLayerObject { private set; get; }
        public List<RectangleF> BottomLayerObject { private set; get; }
        public List<AreaData> EnteringZoneArea { private set; get; }
        public List<AreaData> MobPartrolArea { private set; get; }
        public float TopEntityDepth { private set; get; }
        public float MiddleEntityDepth { private set; get; }
        public float BottomEntityDepth { private set; get; }
        public float TopObjectDepth { private set; get; }
        public float MiddleObjectDepth { private set; get; }
        public float BottomObjectDepth { private set; get; }
        public float BackgroundDepth { private set; get; }
        public int TILE_SIZE { set; get; }
        public int NUM_ROWS { set; get; }
        public int NUM_COLUMNS { set; get; }
        public int[,] TILEMAP { set; get; }

        public enum ShadowTextureName
        {
            shadow_1,
            shadow_2
        }
        private readonly Dictionary<ShadowTextureName, int> shadowTextureIndices = [];

        public enum GuiTextureName
        {
            logo_wakeup,
            press_f,
            press_interacting,
            press_collecting,
            insufficient,
            health_bar,
            healthpoints_gauge,
            mana_gauge,
            health_bar_companion,
            healthpoints_gauge_companion,
            noah_profile,
            companion_profile,
            health_bar_boss,
            boss_gauge,
            item_bar,
            item_slot,
            gold_coin,
            heart,
            selected_slot,
            health_bar_alpha,
            health_bar_companion_alpha,
            health_bar_boss_alpha,
            quest_stamp,
            level_gui,
            exp_bar,
            exp_bar_alpha,
            exp_gauge,
            burst_skill_gui,
            burst_skill_gui_alpha,
            burst_skill_pic,
            normal_skill_gui,
            normal_skill_gui_alpha,
            normal_skill_pic,
            passive_skill_gui,
            passive_skill_gui_alpha,
            passive_skill_pic,
            transition_texture,
            game_name,
            skill_point,
            mob_gauge,
            health_bar_mob,
            arrow_right,
            arrow_left,
            arrow_up,
            arrow_down,
            Inspect_Menu,
            Inventory_Menu,
            Pause_Menu,
            dialog_tmp,
            book_desciption,
            safezone_map_sign,
            battlezone_map_sign,
            Alpha_BG
        }
        private readonly Dictionary<GuiTextureName, int> guiTextureIndices = [];

        public enum AbilityTextureName
        {
            Ability_Ive_got_the_Scent,
            Ability_Noah_Strike,
            Ability_Survivalist,
            Ability_FrostBolt,
            Ability_FrostNova,
            Ability_BrillianceAura
        }
        private readonly Dictionary<AbilityTextureName, int> abilityTextureIndices = [];

        public enum Sound
        {
            damage01,
            damage02,
            Dead,
            ItemPurchase1,
            quest,
            Bingo,
            Bite,
            Cancel1,
            Cancel2,
            Claw,
            Click1,
            ClickPlayGame,
            Crafting1,
            CraftingPotion1,
            CraftingPotion2,
            CraftingPotion3,
            CraftingPotion4,
            damageCrit1,
            damageCrit2,
            Debuff1,
            door_1,
            door_2,
            dullSwoosh1,
            dullSwoosh2,
            dullSwoosh3,
            dullSwoosh4,
            Equip1,
            magicSwoosh1,
            metalSwoosh1,
            metalSwoosh2,
            metalSwoosh3,
            metalSwoosh4,
            Miss1,
            Miss2,
            Miss3,
            Onmtp_Inspiration08_1,
            Parry,
            Pause,
            Powerup,
            Recovery1,
            Recovery2,
            Save,
            Skill_Fire_explosion,
            Skill_Ice_explosion,
            Skill_Wind,
            Skill1,
            Skill2,
            Slash,
            Step_grass,
            Unequip1,
            Unpause,
            UseItem,
            Warp,
            ColossusSmash_Impact_01,
            frostbolt_1,
            FrostNova,
            GreaterHeal_Low_Base,
            Heal_Low_Base,
            LevelUp,
            Shield_Bash_04,
            PickUpBag,
            PickUpCoin,
            PickUpGeneric,
            PickUpHerb
        }
        private readonly Dictionary<Sound, int> soundEffectIndices = [];

        public enum Music
        {
            ch_1_Town,
            ch_1_Mon,
            ch_1_Boss,
            ch_2_Town,
            ch_2_Mon,
            ch_2_Boss,
            ch_3_Town,
            ch_3_Mon,
            ch_3_Boss,
            ch_4_Town,
            ch_4_Mon,
            ch_4_Boss,
            ch_5_Town,
            ch_5_Mon,
            ch_5_Boss,
            ch_6_Town,
            ch_6_Mon,
            ch_6_Boss,
            dova_action_battle,
            dova_gogonoukurere,
            dova_Good_night,
            dova_ikirusinrin,
            dova_Moonlight,
            dova_pastel_green,
            dova_wagaya,
            FinalStorm,
            Izanai,
            KBF_Town_Village_01,
            kokoro_hiraite,
            LRPG_Tale_of_Aurora_D,
            m308_unmei,
            Morinonakanoseirei,
            winered,
            Gag_dead
        }
        private readonly Dictionary<Music, int> musicBGIndices = [];

        // For Testing
        public int TestInt { set; get; }
        public Texture2D TestIcon { set; get; }


        private float _deltaSeconds;

        private static GameGlobals instance;

        private GameGlobals()
        {
            GameScreen = new Vector2(1440, 900);
            GameScreenCenter = GameScreen / 2;
            DefaultAdapterViewport = Vector2.Zero;
            TopLeftCornerPos = Vector2.Zero;
            InitialCameraPos = Vector2.Zero;
            AddingCameraPos = Vector2.Zero;
            IsFullScreen = false;
            IsGamePause = false;

            // ui
            BuiltinTheme = BuiltinThemes.hd;
            IsPauseMenuAllowed = true;
            IsOpenGUI = false;
            IsOpenMainMenu = false;
            IsMainBGEnding = false;
            IsRefreshPlayScreenUI = false;

            // pause menu
            SwitchOpenPauseMenuPanel = false;
            IsOpenPauseMenu = false;

            // inventory
            SwitchOpenInventoryPanel = false;
            IsOpenInventoryPanel = false;

            // crafting
            SwitchOpenCraftingPanel = false;
            IsOpenCraftingPanel = false;

            // inspecting
            SwitchOpenInspectPanel = false;
            IsOpenInspectPanel = false;

            // save menu
            SwitchOpenSaveMenuPanel = false;
            IsOpenSaveMenuPanel = false;

            // trading menu
            SwitchOpenTradingPanel = false;
            IsOpenTradingPanel = false;

            // warp menu
            SwitchOpenWarpPointPanel = false;
            IsOpenWarpPointPanel = false;

            SwitchDebugMode = false;
            IsDebugMode = false;

            SwitchShowPath = false;
            IsShowPath = false;

            ShowInsufficientSign = false;
            ShowMapNameSign = false;

            // sound & music
            SoundEffectVolume = 0.5f;
            BackgroundMusicVolume = 0.5f;
            CurrentMapMusics = [];

            // hotbar switch
            SwitchSlot_1 = false;
            SwitchSlot_2 = false;
            SwitchSlot_3 = false;
            SwitchSlot_4 = false;
            SwitchSlot_5 = false;
            SwitchSlot_6 = false;
            SwitchSlot_7 = false;
            SwitchSlot_8 = false;

            // boss
            IsEnteringBossFight = false;

            // TestMap
            MobsTestSpawnTime = 10f;
            MobsTestSpawnTimer = MobsTestSpawnTime;
            IsBossTestDead = false;
            BossTestSpawnTime = 15f;
            BossTestSpawnTimer = BossTestSpawnTime;
            ObjectTestSpawnTime = 10f;
            ObjectTestSpawnTimer = ObjectTestSpawnTime;

            // Chapter 1
            MobsOneSpawnTime = 60f;
            MobsOneSpawnTimer = MobsTestSpawnTime;
            IsBossOneDead = false;
            BossOneSpawnTime = 300f;
            BossOneSpawnTimer = BossOneSpawnTime;
            ObjectOneSpawnTime = 180f;
            ObjectOneSpawnTimer = ObjectOneSpawnTime;

            // gamesave
            MaxLevel = 30;
            TotalPlayTime = 0;
            CompanionSpriteSheet = [];
            GameSave = [];
            SelectedGameSaveIndex = 0;
            MaxGameSaveSlot = 4;
            GameSavePath = "save/gamesaves.json";
            GameConfigPath = "config/gameoptions.json";

            // invnetory
            MaximunInventorySlot = 64;
            MaximunItemCount = 999;
            DefaultInventorySlot = 999;
            MaxItemBarSlot = 8;
            CurrentHotbarSelect = 0;

            // data & resource
            SoundEffects = [];
            BackgroundMusicPath = [];
            ShadowTextures = [];
            GuiTextures = [];
            AbilityTextures = [];
            ExperienceCapacityDatas = [];
            MapLocationPointDatas = [];
            ItemsDatas = [];
            CharacterDatas = [];
            CraftingRecipeDatas = [];

            // feed
            FeedPoint = new(200f, 380f);
            CollectedItemFeed = [];
            MaximumItemFeed = 6;
            DisplayFeedTime = 0;
            MaximumDisplayFeedTime = 6f;

            // tilemap
            TILE_SIZE = 32;
            CollistionObject = [];
            TopLayerObject = [];
            MiddleLayerObject = [];
            BottomLayerObject = [];
            EnteringZoneArea = [];
            MobPartrolArea = [];

            TopEntityDepth = 0.2f;
            MiddleEntityDepth = 0.4f;
            BottomEntityDepth = 0.6f;

            TopObjectDepth = 0.3f;
            MiddleObjectDepth = 0.5f;
            BottomObjectDepth = 0.7f;
            BackgroundDepth = 0.9f;
        }

        public void Initialize(ContentManager Content)
        {
            this.Content = Content;

            var viewport = new Vector2(
                GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width,
                GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height);

            DefaultAdapterViewport = new Vector2(viewport.X, viewport.Y);
        }  

        public void LoadContent()
        {
            //System.Diagnostics.Debug.WriteLine($"totalFrames : {}");

            // Load GameSave and Config
            var gameSave = JsonFileManager.LoadGameSave(GameSavePath);
            if (gameSave.Count != 0)
            {
                foreach (var save in gameSave)
                {
                    GameSave.Add(save);
                }
            }
            GameConfig = JsonFileManager.LoadGameConfig(GameConfigPath);
            IsFullScreen = GameConfig.IsFullScreen;
            ScreenManager.ToggleFullScreen();
            SoundEffectVolume = (float)GameConfig.SFXVolume;
            BackgroundMusicVolume = (float)GameConfig.BGMusicVolume;

            // Load Map Position Data
            MapLocationPointDatas = Content.Load<List<MapLocationPointData>>("data/models/map_locations_point");

            // Load Chapter Item Drop Data
            ChapterItemDatas = Content.Load<List<ChapterItemData>>("data/models/chapter_item");

            // Load EXP Cap Data
            ExperienceCapacityDatas = Content.Load<List<ExperienceCapacityData>>("data/models/exp_capacity");

            // Load Player Data
            InitialPlayerData = Content.Load<PlayerData>("data/models/playerdata");
            PlayerSpriteSheet = Content.Load<SpriteSheet>("entity/mc/mc_animation.sf", new JsonContentLoader());

            // Load Companions Sprite Sheet
            // Violet = 0
            CompanionSpriteSheet.Add(Content.Load<SpriteSheet>("entity/companions/violet/violet_animation.sf", new JsonContentLoader()));

            // Load Object Sprite Sheet
            ItemsPackSprites = Content.Load<SpriteSheet>("item/itemspack_spritesheet.sf", new JsonContentLoader());
            WarpPointSprite = Content.Load<SpriteSheet>("entity/objects/Warp_Point_animation.sf", new JsonContentLoader());

            // Load Item Data
            ItemsDatas = Content.Load<List<ItemData>>("data/models/items");
            ItemEffectDatas = Content.Load<List<ItemEffectData>>("data/models/item_effects");
            EquipmentStatsDatas = Content.Load<List<EquipmentStatsData>>("data/models/equipment_stats");

            // Load Crafting Recipes Data
            CraftingRecipeDatas = Content.Load<List<CraftingRecipeData>>("data/models/crafting_recipes");

            // Load Character Datas
            CharacterDatas = Content.Load<List<CharacterData>>("data/models/characters");

            // Load Character Skill Description
            SkillDescriptionDatas = Content.Load<List<SkillDescriptionData>>("data/models/character_skills");

            // Load Thai Traditional Medicine
            MedicineDescriptionDatas = Content.Load<List<MedicineDescriptionData>>("data/models/medicine_descriptions");

            // Load Effect Sprite Sheet
            HitSpriteEffect = Content.Load<SpriteSheet>("effect/hit_effect.sf", new JsonContentLoader());
            HitSkillSpriteEffect = Content.Load<SpriteSheet>("effect/hit_skill_effect.sf", new JsonContentLoader());
            BossSpriteEffect = Content.Load<SpriteSheet>("effect/boss_effect.sf", new JsonContentLoader());
            StatesSpriteEffect = Content.Load<SpriteSheet>("effect/states.sf", new JsonContentLoader());
            MagicCircleEffect = Content.Load<SpriteSheet>("effect/Magic_Circle.sf", new JsonContentLoader());

            // Load Bitmap Font          
            FontSensation = Content.Load<BitmapFont>("fonts/Sensation/Sensation");
            FontMinecraft = Content.Load<BitmapFont>("fonts/Mincraft_Ten/Mincraft_Ten");
            FontTA8Bit = Content.Load<BitmapFont>("fonts/TA_8_Bit/TA_8_Bit");
            FontTA8BitBold = Content.Load<BitmapFont>("fonts/TA_8_Bit_Bold/TA_8_Bit_Bold");
            FontTA16Bit = Content.Load<BitmapFont>("fonts/TA_16_Bit/TA_16_Bit");

            // Load Shadow Effect
            ShadowTextures.Add(Content.Load<Texture2D>("effect/shadow_1"));
            ShadowTextures.Add(Content.Load<Texture2D>("effect/shadow_2"));

            // Load GUI Textures
            GuiTextures.Add(Content.Load<Texture2D>("gui/logo_wakeup"));
            GuiTextures.Add(Content.Load<Texture2D>("gui/press_f"));
            GuiTextures.Add(Content.Load<Texture2D>("gui/press_interacting"));
            GuiTextures.Add(Content.Load<Texture2D>("gui/press_collecting"));
            GuiTextures.Add(Content.Load<Texture2D>("gui/insufficient"));
            GuiTextures.Add(Content.Load<Texture2D>("gui/health_bar"));
            GuiTextures.Add(Content.Load<Texture2D>("gui/healthpoints_gauge"));
            GuiTextures.Add(Content.Load<Texture2D>("gui/mana_gauge"));
            GuiTextures.Add(Content.Load<Texture2D>("gui/health_bar_companion"));
            GuiTextures.Add(Content.Load<Texture2D>("gui/healthpoints_gauge_companion"));
            GuiTextures.Add(Content.Load<Texture2D>("gui/profile/noah_profile"));
            GuiTextures.Add(Content.Load<Texture2D>("gui/profile/violet_profile"));
            GuiTextures.Add(Content.Load<Texture2D>("gui/health_bar_boss"));
            GuiTextures.Add(Content.Load<Texture2D>("gui/boss_gauge"));
            GuiTextures.Add(Content.Load<Texture2D>("gui/item_bar"));
            GuiTextures.Add(Content.Load<Texture2D>("gui/item_slot"));
            GuiTextures.Add(Content.Load<Texture2D>("gui/gold_coin"));
            GuiTextures.Add(Content.Load<Texture2D>("gui/heart"));
            GuiTextures.Add(Content.Load<Texture2D>("gui/selected_slot"));
            GuiTextures.Add(Content.Load<Texture2D>("gui/health_bar_alpha"));
            GuiTextures.Add(Content.Load<Texture2D>("gui/health_bar_companion_alpha"));
            GuiTextures.Add(Content.Load<Texture2D>("gui/health_bar_boss_alpha"));
            GuiTextures.Add(Content.Load<Texture2D>("gui/quest_stamp"));
            GuiTextures.Add(Content.Load<Texture2D>("gui/level_gui"));
            GuiTextures.Add(Content.Load<Texture2D>("gui/exp_bar"));
            GuiTextures.Add(Content.Load<Texture2D>("gui/exp_bar_alpha"));
            GuiTextures.Add(Content.Load<Texture2D>("gui/exp_gauge"));
            GuiTextures.Add(Content.Load<Texture2D>("gui/burst_skill_gui"));
            GuiTextures.Add(Content.Load<Texture2D>("gui/burst_skill_gui_alpha"));
            GuiTextures.Add(Content.Load<Texture2D>("gui/ability/burst_skill_pic"));
            GuiTextures.Add(Content.Load<Texture2D>("gui/normal_skill_gui"));
            GuiTextures.Add(Content.Load<Texture2D>("gui/normal_skill_gui_alpha"));
            GuiTextures.Add(Content.Load<Texture2D>("gui/ability/normal_skill_pic"));
            GuiTextures.Add(Content.Load<Texture2D>("gui/passive_skill_gui"));
            GuiTextures.Add(Content.Load<Texture2D>("gui/passive_skill_gui_alpha"));
            GuiTextures.Add(Content.Load<Texture2D>("gui/ability/passive_skill_pic"));
            GuiTextures.Add(Content.Load<Texture2D>("gui/transition_texture"));                 
            GuiTextures.Add(Content.Load<Texture2D>("gui/game_name"));
            GuiTextures.Add(Content.Load<Texture2D>("gui/skill_point"));
            GuiTextures.Add(Content.Load<Texture2D>("gui/mob_gauge"));
            GuiTextures.Add(Content.Load<Texture2D>("gui/health_bar_mob"));
            GuiTextures.Add(Content.Load<Texture2D>("gui/arrow_right"));
            GuiTextures.Add(Content.Load<Texture2D>("gui/arrow_left"));
            GuiTextures.Add(Content.Load<Texture2D>("gui/arrow_up"));
            GuiTextures.Add(Content.Load<Texture2D>("gui/arrow_down"));
            GuiTextures.Add(Content.Load<Texture2D>("gui/Inspect_Menu"));
            GuiTextures.Add(Content.Load<Texture2D>("gui/Inventory_Menu"));
            GuiTextures.Add(Content.Load<Texture2D>("gui/Pause_Menu"));
            GuiTextures.Add(Content.Load<Texture2D>("gui/dialog_tmp"));
            GuiTextures.Add(Content.Load<Texture2D>("gui/book_desciption"));
            GuiTextures.Add(Content.Load<Texture2D>("gui/safezone_map_sign"));
            GuiTextures.Add(Content.Load<Texture2D>("gui/battlezone_map_sign"));
            GuiTextures.Add(Content.Load<Texture2D>("gui/Alpha_BG"));

            // UI_books_icon_hud
            UIBooksIconHUD = Content.Load<SpriteSheet>("gui/UI_books_icon_hud.sf", new JsonContentLoader());

            // Load Ability Textures
            AbilityTextures.Add(Content.Load<Texture2D>("gui/ability/Ability_I've_got_the_Scent!"));
            AbilityTextures.Add(Content.Load<Texture2D>("gui/ability/Ability_Noah_Strike"));
            AbilityTextures.Add(Content.Load<Texture2D>("gui/ability/Ability_Survivalist"));
            AbilityTextures.Add(Content.Load<Texture2D>("gui/ability/Ability_FrostBolt"));
            AbilityTextures.Add(Content.Load<Texture2D>("gui/ability/Ability_FrostNova"));
            AbilityTextures.Add(Content.Load<Texture2D>("gui/ability/Ability_BrillianceAura"));

            // Load Sound Effects
            SoundEffects.Add(Content.Load<SoundEffect>("sound/damage01"));
            SoundEffects.Add(Content.Load<SoundEffect>("sound/damage02"));
            SoundEffects.Add(Content.Load<SoundEffect>("sound/Dead"));
            SoundEffects.Add(Content.Load<SoundEffect>("sound/ItemPurchase1"));
            SoundEffects.Add(Content.Load<SoundEffect>("sound/quest"));
            SoundEffects.Add(Content.Load<SoundEffect>("sound/Bingo"));
            SoundEffects.Add(Content.Load<SoundEffect>("sound/Bite"));
            SoundEffects.Add(Content.Load<SoundEffect>("sound/Cancel1"));
            SoundEffects.Add(Content.Load<SoundEffect>("sound/Cancel2"));
            SoundEffects.Add(Content.Load<SoundEffect>("sound/Claw"));
            SoundEffects.Add(Content.Load<SoundEffect>("sound/Click1"));
            SoundEffects.Add(Content.Load<SoundEffect>("sound/ClickPlayGame"));
            SoundEffects.Add(Content.Load<SoundEffect>("sound/Crafting1"));
            SoundEffects.Add(Content.Load<SoundEffect>("sound/CraftingPotion1"));
            SoundEffects.Add(Content.Load<SoundEffect>("sound/CraftingPotion2"));
            SoundEffects.Add(Content.Load<SoundEffect>("sound/CraftingPotion3"));
            SoundEffects.Add(Content.Load<SoundEffect>("sound/CraftingPotion4"));
            SoundEffects.Add(Content.Load<SoundEffect>("sound/damageCrit1"));
            SoundEffects.Add(Content.Load<SoundEffect>("sound/damageCrit2"));
            SoundEffects.Add(Content.Load<SoundEffect>("sound/Debuff1"));
            SoundEffects.Add(Content.Load<SoundEffect>("sound/door_1"));
            SoundEffects.Add(Content.Load<SoundEffect>("sound/door_2"));
            SoundEffects.Add(Content.Load<SoundEffect>("sound/dullSwoosh1"));
            SoundEffects.Add(Content.Load<SoundEffect>("sound/dullSwoosh2"));
            SoundEffects.Add(Content.Load<SoundEffect>("sound/dullSwoosh3"));
            SoundEffects.Add(Content.Load<SoundEffect>("sound/dullSwoosh4"));
            SoundEffects.Add(Content.Load<SoundEffect>("sound/Equip1"));
            SoundEffects.Add(Content.Load<SoundEffect>("sound/magicSwoosh1"));
            SoundEffects.Add(Content.Load<SoundEffect>("sound/metalSwoosh1"));
            SoundEffects.Add(Content.Load<SoundEffect>("sound/metalSwoosh2"));
            SoundEffects.Add(Content.Load<SoundEffect>("sound/metalSwoosh3"));
            SoundEffects.Add(Content.Load<SoundEffect>("sound/metalSwoosh4"));
            SoundEffects.Add(Content.Load<SoundEffect>("sound/Miss1"));
            SoundEffects.Add(Content.Load<SoundEffect>("sound/Miss2"));
            SoundEffects.Add(Content.Load<SoundEffect>("sound/Miss3"));
            SoundEffects.Add(Content.Load<SoundEffect>("sound/Onmtp_Inspiration08_1"));
            SoundEffects.Add(Content.Load<SoundEffect>("sound/Parry"));
            SoundEffects.Add(Content.Load<SoundEffect>("sound/Pause"));
            SoundEffects.Add(Content.Load<SoundEffect>("sound/Powerup"));
            SoundEffects.Add(Content.Load<SoundEffect>("sound/Recovery1"));
            SoundEffects.Add(Content.Load<SoundEffect>("sound/Recovery2"));
            SoundEffects.Add(Content.Load<SoundEffect>("sound/Save"));
            SoundEffects.Add(Content.Load<SoundEffect>("sound/Skill_Fire_explosion"));
            SoundEffects.Add(Content.Load<SoundEffect>("sound/Skill_Ice_explosion"));
            SoundEffects.Add(Content.Load<SoundEffect>("sound/Skill_Wind"));
            SoundEffects.Add(Content.Load<SoundEffect>("sound/Skill1"));
            SoundEffects.Add(Content.Load<SoundEffect>("sound/Skill2"));
            SoundEffects.Add(Content.Load<SoundEffect>("sound/Slash"));
            SoundEffects.Add(Content.Load<SoundEffect>("sound/Step_grass"));
            SoundEffects.Add(Content.Load<SoundEffect>("sound/Unequip1"));
            SoundEffects.Add(Content.Load<SoundEffect>("sound/Unpause"));
            SoundEffects.Add(Content.Load<SoundEffect>("sound/UseItem"));
            SoundEffects.Add(Content.Load<SoundEffect>("sound/Warp"));
            SoundEffects.Add(Content.Load<SoundEffect>("sound/ColossusSmash_Impact_01"));
            SoundEffects.Add(Content.Load<SoundEffect>("sound/frostbolt_1"));
            SoundEffects.Add(Content.Load<SoundEffect>("sound/FrostNova"));
            SoundEffects.Add(Content.Load<SoundEffect>("sound/GreaterHeal_Low_Base"));
            SoundEffects.Add(Content.Load<SoundEffect>("sound/Heal_Low_Base"));
            SoundEffects.Add(Content.Load<SoundEffect>("sound/LevelUp"));
            SoundEffects.Add(Content.Load<SoundEffect>("sound/Shield_Bash_04"));
            SoundEffects.Add(Content.Load<SoundEffect>("sound/PickUpBag"));
            SoundEffects.Add(Content.Load<SoundEffect>("sound/PickUpCoin"));
            SoundEffects.Add(Content.Load<SoundEffect>("sound/PickUpGeneric"));
            SoundEffects.Add(Content.Load<SoundEffect>("sound/PickUpHerb"));

            // Music Background Path
            BackgroundMusicPath.Add("music/ch-1/Town");
            BackgroundMusicPath.Add("music/ch-1/Mon");
            BackgroundMusicPath.Add("music/ch-1/Boss");
            BackgroundMusicPath.Add("music/ch-2/Town");
            BackgroundMusicPath.Add("music/ch-2/Mon");
            BackgroundMusicPath.Add("music/ch-2/Boss");
            BackgroundMusicPath.Add("music/ch-3/Town");
            BackgroundMusicPath.Add("music/ch-3/Mon");
            BackgroundMusicPath.Add("music/ch-3/Boss");
            BackgroundMusicPath.Add("music/ch-4/Town");
            BackgroundMusicPath.Add("music/ch-4/Mon");
            BackgroundMusicPath.Add("music/ch-4/Boss");
            BackgroundMusicPath.Add("music/ch-5/Town");
            BackgroundMusicPath.Add("music/ch-5/Mon");
            BackgroundMusicPath.Add("music/ch-5/Boss");
            BackgroundMusicPath.Add("music/ch-6/Town");
            BackgroundMusicPath.Add("music/ch-6/Mon");
            BackgroundMusicPath.Add("music/ch-6/Boss");
            BackgroundMusicPath.Add("music/dova-action_battle");
            BackgroundMusicPath.Add("music/dova-gogonoukurere");
            BackgroundMusicPath.Add("music/dova-Good_night");
            BackgroundMusicPath.Add("music/dova-ikirusinrin");
            BackgroundMusicPath.Add("music/dova-Moonlight");
            BackgroundMusicPath.Add("music/dova-pastel_green");
            BackgroundMusicPath.Add("music/dova-wagaya");
            BackgroundMusicPath.Add("music/FinalStorm");
            BackgroundMusicPath.Add("music/Izanai");
            BackgroundMusicPath.Add("music/KBF_Town_Village_01");
            BackgroundMusicPath.Add("music/kokoro_hiraite");
            BackgroundMusicPath.Add("music/LRPG_Tale_of_Aurora_D");
            BackgroundMusicPath.Add("music/m308_unmei");
            BackgroundMusicPath.Add("music/Morinonakanoseirei");
            BackgroundMusicPath.Add("music/winered");
            BackgroundMusicPath.Add("music/Gag_dead");

            // Initialize Indices
            // ShadowTextureName
            for (int i = 0; i < CountEnums<ShadowTextureName>(); i++)
            {
                shadowTextureIndices.Add(GetEnumByIndex<ShadowTextureName>(i), i);
            }

            // GuiTextureName
            for (int i = 0; i < CountEnums<GuiTextureName>(); i++)
            {
                guiTextureIndices.Add(GetEnumByIndex<GuiTextureName>(i), i);
            }

            // AbilityTextureName
            for (int i = 0; i < CountEnums<AbilityTextureName>(); i++)
            {
                abilityTextureIndices.Add(GetEnumByIndex<AbilityTextureName>(i), i);
            }

            // Sound
            for (int i = 0; i < CountEnums<Sound>(); i++)
            {
                soundEffectIndices.Add(GetEnumByIndex<Sound>(i), i);
            }

            // Music
            for (int i = 0; i < CountEnums<Music>(); i++)
            {
                musicBGIndices.Add(GetEnumByIndex<Music>(i), i);
            }

            // Initialize GUI Panels
            UIManager.Instance.InitializeThemeAndUI(BuiltinTheme);

            // test
            //Test();
        }

        // Count the number of enums
        static int CountEnums<T>()
        {
            return Enum.GetNames(typeof(T)).Length;
        }

        // Get enum by index number
        static T GetEnumByIndex<T>(int index)
        {
            string[] enumNames = Enum.GetNames(typeof(T));
            if (index >= 0 && index < enumNames.Length)
            {
                string enumName = enumNames[index];
                return (T)Enum.Parse(typeof(T), enumName);
            }
            else
            {
                throw new IndexOutOfRangeException($"Index {index} is out of range for enum {typeof(T)}");
            }
        }

        public void Update(GameTime gameTime)
        {
            _deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            TotalPlayTime += _deltaSeconds;

            // Spawn Timer: Testmap
            // Mobs
            if (MobsTestSpawnTimer > 0)
            {
                MobsTestSpawnTimer -= _deltaSeconds;
            }
            else MobsTestSpawnTimer = MobsTestSpawnTime;
            // Object
            if (ObjectTestSpawnTimer > 0)
            {
                ObjectTestSpawnTimer -= _deltaSeconds;
            }
            else ObjectTestSpawnTimer = ObjectTestSpawnTime;
            // Boss
            if (IsBossTestDead)
            {
                BossTestSpawnTimer -= _deltaSeconds;

                if (BossTestSpawnTimer < 0)
                {
                    BossTestSpawnTimer = Instance.BossTestSpawnTime;
                    Instance.IsBossTestDead = false;
                }
            }
            // Spawn Timer: Chapter 1
            // Mobs
            if (MobsOneSpawnTimer > 0)
            {
                MobsOneSpawnTimer -= _deltaSeconds;
            }
            else MobsOneSpawnTimer = MobsOneSpawnTime;
            // Object
            if (ObjectOneSpawnTimer > 0)
            {
                ObjectOneSpawnTimer -= _deltaSeconds;
            }
            else ObjectOneSpawnTimer = ObjectOneSpawnTime;
            // Boss
            if (Instance.IsBossOneDead)
            {
                Instance.BossOneSpawnTimer -= _deltaSeconds;

                if (Instance.BossOneSpawnTimer < 0)
                {
                    Instance.BossOneSpawnTimer = Instance.BossOneSpawnTime;
                    Instance.IsBossOneDead = false;
                }
            }

            var mouseState = Mouse.GetState();
            MousePosition = new Point2(mouseState.X, mouseState.Y);
        }

        public void SetGameScreen(Viewport viewport)
        {
            GameScreen = new Vector2(viewport.Width, viewport.Height);
            GameScreenCenter = GameScreen / 2;
        }

        public static Texture2D GetShadowTexture(ShadowTextureName shadowTextureName)
        {
            if (Instance.shadowTextureIndices.TryGetValue(shadowTextureName, out int index) && index < Instance.ShadowTextures.Count)
            {
                return Instance.ShadowTextures.ElementAt(index);
            }

            return null;
        }

        public static Texture2D GetGuiTexture(GuiTextureName guiTextureName)
        {
            if (Instance.guiTextureIndices.TryGetValue(guiTextureName, out int index) && index < Instance.GuiTextures.Count)
            {
                return Instance.GuiTextures.ElementAt(index);
            }

            return null;
        }

        public static Texture2D GetAbilityTexture(AbilityTextureName abilityTextureName)
        {
            if (Instance.abilityTextureIndices.TryGetValue(abilityTextureName, out int index) && index < Instance.AbilityTextures.Count)
            {
                return Instance.AbilityTextures.ElementAt(index);
            }

            return null;
        }

        public static SoundEffect GetSoundEffect(Sound soundEffectName)
        {
            if (Instance.soundEffectIndices.TryGetValue(soundEffectName, out int index) && index < Instance.SoundEffects.Count)
            {
                return Instance.SoundEffects.ElementAt(index);
            }

            return null;
        }

        public static void PlaySoundEffect(Sound soundEffectName)
        {
            var soundEffect = GetSoundEffect(soundEffectName);

            if (soundEffect == null) return;

            var soundEffectInstance = soundEffect.CreateInstance();
            soundEffectInstance.Volume = Instance.SoundEffectVolume;
            soundEffectInstance.Play();
        }

        public static void PlaySoundEffect(Sound[] soundEffectName)
        {
            int randomIndex = new Random().Next(soundEffectName.Length);

            var soundEffect = GetSoundEffect(soundEffectName[randomIndex]);

            if (soundEffect == null) return;

            var soundEffectInstance = soundEffect.CreateInstance();
            soundEffectInstance.Volume = Instance.SoundEffectVolume;
            soundEffectInstance.Play();
        }

        public static string GetBackgroundMusicPath(Music musicBG)
        {
            if (Instance.musicBGIndices.TryGetValue(musicBG, out int index) && index < Instance.BackgroundMusicPath.Count)
            {
                return Instance.BackgroundMusicPath.ElementAt(index);
            }

            return null;
        }

        public static Song AddCurrentMapMusic(Music musicBG, ContentManager content)
        {
            var path = GetBackgroundMusicPath(musicBG);
            var song = content.Load<Song>(path);

            Instance.CurrentMapMusics.Add(new MusicData()
            {
                Name = musicBG,
                Path = path,
                Song = song
            });

            return song;
        }

        public static void PlayBackgroundMusic(Song song, bool isRepeating, float volumeScale)
        {
            if (song == null) return;

            MediaPlayer.IsRepeating = isRepeating;
            MediaPlayer.Volume = Instance.BackgroundMusicVolume * volumeScale;
            MediaPlayer.Play(song);
        }

        public static void UpdateMediaPlayerVolumeScale(float volumeScale)
        {
            MediaPlayer.Volume = Instance.BackgroundMusicVolume * volumeScale;
        }

        public static Texture2D GetItemTexture(int itemId)
        {
            // Set item
            var itemSprite = new AnimatedSprite(Instance.ItemsPackSprites);

            itemSprite.Play(itemId.ToString());
            itemSprite.Update(Instance._deltaSeconds);

            var texture = itemSprite.TextureRegion.Texture;
            var bounds = itemSprite.TextureRegion.Bounds;

            // Create a new texture with new bounds
            Texture2D newTexture = new(ScreenManager.Instance.GraphicsDevice, bounds.Width, bounds.Height);

            // Get the data from the original texture
            Color[] data = new Color[bounds.Width * bounds.Height];
            texture.GetData(0, bounds, data, 0, data.Length);

            // Set the data to the new texture
            newTexture.SetData(data);

            return newTexture;
        }

        public static int RandomItemDrop()
        {
            var currentMap = ScreenManager.Instance.CurrentMap;

            ChapterItemData chapterItemData = null;

            switch (currentMap)
            {
                case "Test":
                    chapterItemData = Instance.ChapterItemDatas.FirstOrDefault(c => c.Name.Equals("Test"));               
                    break;

                case "map_1":

                case "battlezone_1":

                case "dungeon_1":
                    chapterItemData = Instance.ChapterItemDatas.FirstOrDefault(c => c.Name.Equals("chapter_1"));
                    break;

                case "map_2":

                case "battlezone_2":

                case "dungeon_2":
                    chapterItemData = Instance.ChapterItemDatas.FirstOrDefault(c => c.Name.Equals("chapter_2"));
                    break;

                case "map_3":

                case "battlezone_3":

                case "dungeon_3":
                    chapterItemData = Instance.ChapterItemDatas.FirstOrDefault(c => c.Name.Equals("chapter_3"));
                    break;

                case "map_4":

                case "battlezone_4":

                case "dungeon_4":
                    chapterItemData = Instance.ChapterItemDatas.FirstOrDefault(c => c.Name.Equals("chapter_4"));
                    break;

                case "map_5":

                case "battlezone_5":

                case "dungeon_5":
                    chapterItemData = Instance.ChapterItemDatas.FirstOrDefault(c => c.Name.Equals("chapter_5"));
                    break;

                case "map_6":

                case "battlezone_6":

                case "dungeon_6":
                    chapterItemData = Instance.ChapterItemDatas.FirstOrDefault(c => c.Name.Equals("chapter_6"));
                    break;
            }

            Random random = new();
  
            return chapterItemData.ItemDropId[random.Next(chapterItemData.ItemDropId.Length)];
        }

        public static int RandomItemQuantityDrop(int itemId)
        {
            var itemData = Instance.ItemsDatas.FirstOrDefault(i => i.ItemId.Equals(itemId));
            int minValue = 1, maxValue = 1;

            if (itemData != null && itemData.QuantityDropRange != null && itemData.QuantityDropRange.Length != 0)
            {
                var quantityDropRange = itemData.QuantityDropRange;
                minValue = quantityDropRange[0] > 0 ? quantityDropRange[0] : 1;
                maxValue = quantityDropRange[1] > 0 ? quantityDropRange[1] : 1;
            }

            Random random = new();

            return random.Next(minValue, maxValue + 1);
        }

        public static bool IsUsableItem(int itemId)
        {
            var itemData = Instance.ItemsDatas.FirstOrDefault(i => i.ItemId.Equals(itemId));

            if (!itemData.IsUsable)
            {
                return false;
            }

            return true;
        }

        public static string GetItemName(int itemId)
        {
            var itemData = Instance.ItemsDatas.FirstOrDefault(i => i.ItemId.Equals(itemId));

            return itemData != null ? itemData.Name : "";
        }

        public static string GetItemCategory(int itemId)
        {
            var itemData = Instance.ItemsDatas.FirstOrDefault(i => i.ItemId.Equals(itemId));

            return itemData != null ? itemData.Category : "";
        }

        public static void PrintMap(int[,] map)
        {
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    System.Diagnostics.Debug.Write(map[i, j] + " ");
                }
                System.Diagnostics.Debug.WriteLine(" ");
            }
        }

        private void Test()
        {

            foreach (var item in CraftingRecipeDatas)
            {
                var name = item.ResultItemName;
                var count = item.ResultQuantity;

                var totalCost = 0;
                var finalPrice = 0;

                foreach (var ingre in item.Ingredients)
                {
                    var ingreData = ItemsDatas.FirstOrDefault(e => e.ItemId.Equals(ingre.ItemId));
                    var ingreCost = ingreData.SellingPrice * ingre.Quantity;

                    totalCost += ingreCost;
                }

                finalPrice = totalCost / count;

                System.Diagnostics.Debug.WriteLine($"{name} : {finalPrice}");
            }

            //int ROAD = 1;
            //int BLOCK = 2;
            //int WALL = 3;

            //int[,] originalMap = new int[,]
            //{
            //    { ROAD, ROAD, ROAD, ROAD, ROAD },
            //    { ROAD, WALL, ROAD, ROAD, ROAD },
            //    { ROAD, ROAD, BLOCK, ROAD, ROAD },
            //    { ROAD, ROAD, ROAD, WALL, ROAD },
            //    { ROAD, ROAD, ROAD, ROAD, ROAD },
            //};

            //int centerRow = 2;
            //int centerCol = 2;
            //int radius = 1;

            //int[,] clonedMap = new AStar().CloneMapAroundCenter(originalMap, centerRow, centerCol, radius);

            //System.Diagnostics.Debug.WriteLine("originalMap");
            //PrintMap(originalMap);
            //System.Diagnostics.Debug.WriteLine("");
            //System.Diagnostics.Debug.WriteLine("clonedMap");
            //PrintMap(clonedMap);
        }

        public static GameGlobals Instance
        {
            get
            {
                instance ??= new GameGlobals();
                return instance;
            }
        }
    }
}
