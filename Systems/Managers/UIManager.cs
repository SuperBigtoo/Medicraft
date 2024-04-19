using GeonBit.UI;
using GeonBit.UI.Animators;
using GeonBit.UI.Entities;
using GeonBit.UI.Utils.Forms;
using Medicraft.Data;
using Medicraft.Data.Models;
using Medicraft.Entities.Companion;
using Medicraft.Entities.Mobs;
using Medicraft.Entities.Mobs.Friendly;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using MonoGame.Extended.Gui;
using MonoGame.Extended.Sprites;
using System;
using System.Collections.Generic;
using System.Linq;
using static Medicraft.Systems.GameGlobals;

namespace Medicraft.Systems.Managers
{
    public class UIManager
    {
        private static UIManager instance;

        private float _deltaSeconds = 0;

        public int PreviosUI {  get; private set; }
        private int currentUI;
        public int CurrentUI
        {
            get => currentUI;
            set
            {
                PreviosUI = currentUI;
                currentUI = value;
            }
        }

        public const int PlayScreen = 0;
        public const int InventoryPanel = 1;
        public const int CraftingPanel = 2;
        public const int InspectPanel = 3;
        public const int MainMenu = 4;
        public const int PauseMenu = 5;
        public const int SaveMenu = 6;
        public const int TradingPanel = 7;
        public const int WarpPointPanel = 8;

        public string CurrentCraftingList { get; set; }

        public const string ThaiTraditionalMedicine = "Thai traditional medicine";
        public const string ConsumableItem = "Consumable item";
        public const string Equipment = "Equipment";

        public bool IsQuickMenuFocus { get; private set; } = false;

        // Dialog
        public bool IsShowDialogUI { get; private set; } = false;
        public FriendlyMob InteractingMob { get; private set; }
        public DialogData Dialog { get; private set; }
        public bool IsForceAccepting { get; private set; } = false;

        private TypeWriterAnimator _animatorDialogue;
        private int _dialogueIndex = 0;
        private QuestStamp _questStamp;

        // Inspect
        public bool IsCharacterTabSelected { get; set; } = false;
        public bool IsShowConfirmBox { get; set; } = false;
        public AnimatedSprite CharacterSprite { get; set; }
        public string CharacterType { get; private set; }

        // Companion character
        public Companion SelectedCompanion { get; private set; }
        public const int VioletSprite = 0;
        private int _selectedCompanionId = 0;

        // Skill selected
        public string NoahSelectedSkill { get; private set; }
        public string CompanionSelectedSkill { get; private set; }

        // Load Save
        public bool IsClickedLoadButton { get; private set; } = false;
        public List<Panel> MainMenuSaveSlots { get; private set; } = [];
        public List<Panel> SaveMenuSaveSlots { get; private set; } = [];
        public Panel SelectedGameSavePanel { get; private set; }

        // Trading
        public Vendor InteractingVendor { get; private set; }
        public Icon SelectedPurchaseItem { get; private set; }

        // WarpPoint
        public const int NoahHome = -1;
        public const int NordlingenTown = 0;
        public const int RothenburgTown = 1;
        public const int TallinnTown = 2;
        public static readonly int[] AvailableWarpPoint = [ NordlingenTown, RothenburgTown, TallinnTown ];
        public int _selectedWarpPointId = 0;

        // UI elements
        private readonly List<Panel> _mainPanels = [];

        private UIManager()
        {
            CurrentUI = MainMenu;
            PreviosUI = MainMenu;
            CurrentCraftingList = ConsumableItem;
        }

        public void UpdateAfterChangeGUI()
        {
            // hide all panels and show current example panel
            foreach (Panel panel in _mainPanels)
                panel.Visible = false;

            _mainPanels[CurrentUI].Visible = true;
        }

        private void UpdateSelectedWarpPoint(bool isNext)
        {
            if (isNext)
            {
                _selectedWarpPointId++;

                if (_selectedWarpPointId >= AvailableWarpPoint.Length)
                    _selectedWarpPointId = -1;
            }
            else
            {
                _selectedWarpPointId--;

                if (_selectedWarpPointId < -1)
                    _selectedWarpPointId = AvailableWarpPoint.Length - 1;
            }

            RefreshWarpPointUI();
        }

        private void UpdateSelectedCompanion(bool isNext)
        {
            if (isNext)
            {
                _selectedCompanionId++;

                if (_selectedCompanionId >= PlayerManager.Instance.Companions.Count)
                    _selectedCompanionId = 0;
            }
            else
            {
                _selectedCompanionId--;

                if (_selectedCompanionId < 0)
                    _selectedCompanionId = PlayerManager.Instance.Companions.Count - 1;
            }

            // Refresh Companion Inspect
            RefreshInspectCompanionDisplay();
            ClearSkillDescription("Companion");
            IsCharacterTabSelected = true;
        }

        private void SetCompanionSelectedSkill(string abilityType)
        {
            var companion = PlayerManager.Instance.Companions[_selectedCompanionId];

            switch (abilityType)
            {
                case "normal":
                    switch (companion.Name)
                    {
                        case "Violet":
                            CompanionSelectedSkill = "Frost Bolt";
                            break;
                    }
                    break;

                case "burst":
                    switch (companion.Name)
                    {
                        case "Violet":
                            CompanionSelectedSkill = "Frost Nova";
                            break;
                    }
                    break;

                case "passive":
                    switch (companion.Name)
                    {
                        case "Violet":
                            CompanionSelectedSkill = "Brilliance Aura";
                            break;
                    }
                    break;
            }
        }

        private void SetCompanionSkillIconTextrue(string companionName, Icon normalSkillIcon
            , Icon burstSkillIcon, Icon passiveSkillIcon)
        {
            CharacterType = "Companion";

            switch (companionName)
            {
                case "Violet":
                    CharacterSprite = new(GameGlobals.Instance.CompanionSpriteSheet[VioletSprite]);
                    normalSkillIcon.Texture = GetAbilityTexture(AbilityTextureName.Ability_FrostBolt);
                    burstSkillIcon.Texture = GetAbilityTexture(AbilityTextureName.Ability_FrostNova);
                    passiveSkillIcon.Texture = GetAbilityTexture(AbilityTextureName.Ability_BrillianceAura);
                    break;
            }
        }

        public void SetInteractingVendor(Vendor vendorMob)
        {
            InteractingVendor = vendorMob;
        }

        public void InitializeThemeAndUI(BuiltinThemes theme)
        {
            // create and init the UI manager
            var content = new ContentManager(
                GameGlobals.Instance.Content.ServiceProvider, "Content");
            UserInterface.Initialize(content, theme);
            UserInterface.Active.UseRenderTarget = true;

            // draw cursor outside the render target
            UserInterface.Active.IncludeCursorInRenderTarget = false;

            // disable Cursor
            UserInterface.Active.ShowCursor = false;

            // Set Click Button Sound
            UserInterface.Active.OnClick = (Entity entity) =>
            {
                if (entity.GetType().Name.Equals("Button") || entity.GetType().Name.Equals("DropDown"))
                {
                    switch (entity.Identifier)
                    {
                        case "backButton":
                        case "closeButton":
                        case "Cancel":
                        case "No":
                            PlaySoundEffect(Sound.Cancel1);
                            break;

                        case "Yes!":
                        case "newGameButton":
                            PlaySoundEffect(Sound.ClickPlayGame);
                            break;

                        case "resumeButton":
                            PlaySoundEffect(Sound.Unpause);
                            break;

                        default:
                            PlaySoundEffect(Sound.Click1);
                            break;
                    }
                }
            };

            // init all ui panel
            // PlayScreen = 0
            InitPlayScreenUI();

            // Inventory = 1
            InitInventoryUI();

            // Crafting = 2
            InitCraftingUI();

            // Inspect = 3
            InitInspectUI();

            // Main Menu = 4
            InitMainMenuUI();

            // Pause Menu = 5
            InitPauseMenuUI();

            // Save Menu = 6
            InitSaveMenuUI();

            // Trading = 7
            InitTradingUI();

            // Warp Point = 8
            InitWarpPointUI();

            // update ui panel
            UpdateAfterChangeGUI();
        }

        /// <summary>
        /// Item Bar and Slot Item
        /// </summary>
        private void InitPlayScreenUI()
        {
            var playScreenUI = new Panel(
                new Vector2(GameGlobals.Instance.GameScreen.X, GameGlobals.Instance.GameScreen.Y),
                PanelSkin.None,
                Anchor.Center)
            { 
                Identifier = "playScreenUI"
            };
            _mainPanels.Add(playScreenUI);
            UserInterface.Active.AddEntity(playScreenUI);

            // Hotbar
            var hotbarSlotPanel = new Panel(new Vector2(500, 50), PanelSkin.None, Anchor.BottomCenter)
            {
                Identifier = "hotbarSlotPanel",
                Offset = new Vector2(0, -25)
            };
            playScreenUI.AddChild(hotbarSlotPanel);

            // Quick Menu
            var quickMenuPanel = new Panel(new Vector2(360, 100), PanelSkin.None, Anchor.TopRight)
            {
                Identifier = "quickMenuPanel"
            };
            playScreenUI.AddChild(quickMenuPanel);

            // Open Inspect Character
            var InspectMenuIcon = new Icon(IconType.None, Anchor.CenterLeft)
            {
                Identifier = "InspectMenuIcon",
                Texture = GetGuiTexture(GuiTextureName.Inspect_Menu),
                Size = new Vector2(64, 64),
                OnClick = (e) =>
                {
                    // Toggle Pause PlayScreen
                    GameGlobals.Instance.IsGamePause = !GameGlobals.Instance.IsGamePause;
                    GameGlobals.Instance.IsOpenGUI = !GameGlobals.Instance.IsOpenGUI;

                    // Toggle IsOpenInspectPanel      
                    GameGlobals.Instance.IsOpenInspectPanel = false;
                    IsCharacterTabSelected = true;
                    if (CurrentUI.Equals(InspectPanel))
                    {
                        CurrentUI = PlayScreen;
                        GameGlobals.Instance.IsRefreshPlayScreenUI = false;
                        ClearSkillDescription("Character");
                        PlaySoundEffect(Sound.Cancel1);
                    }
                    else
                    {
                        Instance.CurrentUI = InspectPanel;
                        PlaySoundEffect(Sound.Click1);
                    }
                },
                OnMouseEnter = (e) => { IsQuickMenuFocus = true; },
                OnMouseLeave = (e) => { IsQuickMenuFocus = false; }
            };
            quickMenuPanel.AddChild(InspectMenuIcon);

            // Open Inventory
            var InventoryMenuIcon = new Icon(IconType.None, Anchor.Center)
            {
                Identifier = "InventoryMenuIcon",
                Texture = GetGuiTexture(GuiTextureName.Inventory_Menu),
                Size = new Vector2(64, 64),
                OnClick = (e) =>
                {
                    // Toggle Pause PlayScreen
                    GameGlobals.Instance.IsGamePause = !GameGlobals.Instance.IsGamePause;
                    GameGlobals.Instance.IsOpenGUI = !GameGlobals.Instance.IsOpenGUI;

                    // Toggle IsOpenInvenoryPanel & refresh inventory item display              
                    GameGlobals.Instance.IsOpenInventoryPanel = false;
                    if (CurrentUI.Equals(InventoryPanel))
                    {
                        CurrentUI = PlayScreen;
                        GameGlobals.Instance.IsRefreshPlayScreenUI = false;
                        PlaySoundEffect(Sound.PickUpBag);
                    }
                    else
                    {
                        CurrentUI = InventoryPanel;
                        PlaySoundEffect(Sound.PickUpBag);
                    }
                },
                OnMouseEnter = (e) => { IsQuickMenuFocus = true; },
                OnMouseLeave = (e) => { IsQuickMenuFocus = false; }
            };
            quickMenuPanel.AddChild(InventoryMenuIcon);

            // Open Pause Menu
            var PauseMenuIcon = new Icon(IconType.None, Anchor.CenterRight)
            {
                Identifier = "PauseMenuIcon",
                Texture = GetGuiTexture(GuiTextureName.Pause_Menu),
                Size = new Vector2(64, 64),
                OnClick = (e) =>
                {
                    // Toggle Pause PlayScreen
                    GameGlobals.Instance.IsGamePause = !GameGlobals.Instance.IsGamePause;
                    GameGlobals.Instance.IsOpenGUI = !GameGlobals.Instance.IsOpenGUI;

                    // Toggle IsOpenPauseMenu              
                    GameGlobals.Instance.IsOpenPauseMenu = false;
                    if (CurrentUI.Equals(PauseMenu))
                    {
                        Instance.CurrentUI = PlayScreen;
                        GameGlobals.Instance.IsRefreshPlayScreenUI = false;
                        PlaySoundEffect(Sound.Unpause);
                    }
                    else
                    {
                        Instance.CurrentUI = PauseMenu;
                        PlaySoundEffect(Sound.Pause);
                    }
                },
                OnMouseEnter = (e) => { IsQuickMenuFocus = true; },
                OnMouseLeave = (e) => { IsQuickMenuFocus = false; }
            };
            quickMenuPanel.AddChild(PauseMenuIcon);

            // Quest 
            var questPanel = new Panel(new Vector2(310, 600), PanelSkin.Simple, Anchor.CenterRight)
            {
                Identifier = "questPanel",
                Visible = false,
                Opacity = 200,
                Offset = new Vector2(0, -75),
                AdjustHeightAutomatically = true
            };
            questPanel.SetCustomSkin(GetGuiTexture(GuiTextureName.Alpha_BG));
            playScreenUI.AddChild(questPanel);

            var headerQuest = new Button("Quest Name", ButtonSkin.Fancy, Anchor.AutoCenter)
            {
                Identifier = "headerQuest",
                Size = new Vector2(325, 40),
                Offset = new Vector2(0, -50),
            };
            questPanel.AddChild(headerQuest);

            var questDescrip = new RichParagraph("Quest Description", Anchor.AutoCenter)
            {
                Identifier = "questDescrip",
                WrapWords = true,
                Size = new Vector2(280, 550)
            };
            questPanel.AddChild(questDescrip);

            // Dialog UI
            {
                var dialogPanel = new Panel(new Vector2(1200, 300), PanelSkin.None, Anchor.BottomCenter)
                {
                    Identifier = "dialogPanel",
                    Visible = false
                };
                playScreenUI.AddChild(dialogPanel);

                // BG dialog
                var dialogTexture = GetGuiTexture(GuiTextureName.dialog_tmp);
                var dialogImageBG = new Image(dialogTexture)
                {
                    Size = new Vector2(dialogTexture.Width, dialogTexture.Height)
                };
                dialogPanel.AddChild(dialogImageBG);

                var entityNamePanel = new Panel(new Vector2(265, 85), PanelSkin.None, Anchor.TopLeft)
                {
                    Identifier = "entityNamePanel",
                    Offset = new Vector2(90, -5)
                };
                dialogPanel.AddChild(entityNamePanel);

                var textName = "TestName";
                var entityName = new RichParagraph(textName, Anchor.Center)
                {
                    Identifier = "entityName",
                    Scale = 1.3f
                };
                entityNamePanel.AddChild(entityName);

                var dialogueText = new RichParagraph(@"", Anchor.TopLeft)
                {
                    Identifier = "dialogueText",
                    WrapWords = true,
                    Size = new Vector2(890, 170),
                    Offset = new Vector2(80, 100)
                };
                dialogPanel.AddChild(dialogueText);

                _animatorDialogue = (TypeWriterAnimator)dialogueText.AttachAnimator(new TypeWriterAnimator()
                {
                    TextToType = @"Text Text"
                });

                dialogPanel.OnClick += (e) =>
                {
                    if (!_animatorDialogue.IsDone)
                    {
                        _animatorDialogue.Finish();
                    }
                };
                dialogueText.OnClick += (e) =>
                {
                    if (!_animatorDialogue.IsDone)
                    {
                        _animatorDialogue.Finish();
                    }
                };

                // Accept button
                var acceptButton = new Button("", ButtonSkin.Default, Anchor.BottomRight)
                {
                    Identifier = "acceptButton",
                    Visible = false,
                    Enabled = false,
                    Size = new Vector2(40, 40),
                    Offset = new Vector2(50, 60)
                };
                acceptButton.ButtonParagraph.SetAnchorAndOffset(Anchor.AutoCenter, new Vector2(0, -40));
                acceptButton.AddChild(new Icon(IconType.None, Anchor.AutoCenter)
                {
                    Texture = GetGuiTexture(GuiTextureName.accept_quest),
                    ClickThrough = true
                }, true);
                dialogPanel.AddChild(acceptButton);

                // Next Dialogue button
                var nextDialogueButton = new Button("", ButtonSkin.Default, Anchor.BottomRight)
                {
                    Identifier = "nextDialogueButton",
                    Size = new Vector2(40, 40),
                    Offset = new Vector2(50, 0)
                };
                nextDialogueButton.ButtonParagraph.SetAnchorAndOffset(Anchor.AutoCenter, new Vector2(0, -40));
                nextDialogueButton.AddChild(new Icon(IconType.None, Anchor.AutoCenter)
                {
                    Texture = GetGuiTexture(GuiTextureName.arrow_down),
                    ClickThrough = true
                }, true);
                dialogPanel.AddChild(nextDialogueButton);

                // Set OnClick
                acceptButton.OnClick = (btn) =>
                {
                    // Add Quest to List
                    _questStamp.IsQuestAccepted = QuestManager.Instance.AddQuest(Dialog.ChapterId, _questStamp.QuestId);

                    nextDialogueButton.Enabled = true;

                    CloseDialog();
                };

                nextDialogueButton.OnClick = (btn) =>
                {
                    if (!_animatorDialogue.IsDone)
                    {
                        _animatorDialogue.Finish();
                        return;
                    }

                    if (_dialogueIndex < Dialog.Dialogues.Count - 1)
                    {
                        _dialogueIndex++;
                        entityName.Text = Dialog.Dialogues[_dialogueIndex].Item1;
                        _animatorDialogue.TextToType = Dialog.Dialogues[_dialogueIndex].Item2;

                        if (_dialogueIndex == Dialog.Dialogues.Count - 1)
                        {
                            acceptButton.Enabled = true;
                            if (IsForceAccepting) nextDialogueButton.Enabled = false;
                        }
                    }
                    else CloseDialog();
                };
            }

            // DeadPanel
            {
                var deadPanel = new Panel()
                {
                    Identifier = "deadPanel",
                    Visible = false,
                    Skin = PanelSkin.None,
                    Anchor = Anchor.Center
                };
                playScreenUI.AddChild(deadPanel);

                deadPanel.AddChild(new Image(GetGuiTexture(GuiTextureName.you_die))
                {
                    Size = new Vector2(288, 55),
                    Anchor = Anchor.AutoCenter
                });
                deadPanel.AddChild(new LineSpace(12));

                var tryAgainButton = new Button("TRY AGAIN!", ButtonSkin.Default, Anchor.AutoCenter)
                {
                    Identifier = "tryAgainButton",
                    Size = new Vector2(200, -1),
                    OnClick = (e) =>
                    {
                        PlayerManager.Instance.IsRespawning = true;
                        hotbarSlotPanel.Visible = true;
                        quickMenuPanel.Visible = true;
                        questPanel.Visible = true;
                        deadPanel.Visible = false;
                    }
                };
                deadPanel.AddChild(tryAgainButton);
            }
        }

        public void ShowDeadPanel()
        {
            var playScreenUI = _mainPanels[PlayScreen];

            var hotbarSlotPanel = playScreenUI.Children.FirstOrDefault
                (p => p.Identifier.Equals("hotbarSlotPanel"));
            var quickMenuPanel = playScreenUI.Children.FirstOrDefault
                (p => p.Identifier.Equals("quickMenuPanel"));
            var questPanel = playScreenUI.Children.FirstOrDefault
                (p => p.Identifier.Equals("questPanel"));
            var deadPanel = playScreenUI.Children.FirstOrDefault
                (p => p.Identifier.Equals("deadPanel"));

            hotbarSlotPanel.Visible = false;
            quickMenuPanel.Visible = false;
            questPanel.Visible = false;
            deadPanel.Visible = true;
        }

        public void UpdateQuestDescription()
        {
            var playScreenUI = _mainPanels[PlayScreen];

            var questPanel = playScreenUI.Children.FirstOrDefault
                (p => p.Identifier.Equals("questPanel"));
            
            var headerQuest = questPanel.Children.OfType<Button>().FirstOrDefault
                (e => e.Identifier.Equals("headerQuest"));
            var questDescrip = questPanel.Children.OfType<RichParagraph>().FirstOrDefault
                (e => e.Identifier.Equals("questDescrip"));

            if (QuestManager.Instance.QuestList.Count == 0 || IsShowDialogUI)
            {
                questPanel.Visible = false;
                headerQuest.Children.OfType<RichParagraph>().FirstOrDefault().Text = "";
                questDescrip.Text = "";
                return;
            }                 

            questPanel.Visible = true;
            var quest = QuestManager.Instance.QuestList.FirstOrDefault();
            var questStamp = quest.QuestStamp;

            headerQuest.Children.OfType<RichParagraph>().FirstOrDefault().Text = quest.QuestData.Name;
            questDescrip.Text = $"{quest.QuestData.Description} - ({questStamp.ObjectiveCount}/{quest.QuestData.ObjectiveValue})";
        }

        public void CreateDialog(DialogData dialogData, string actorName, bool isForceAccepting)
        {
            IsShowDialogUI = true;
            IsForceAccepting = isForceAccepting;

            var playScreenUI = _mainPanels[PlayScreen];

            var hotbarSlotPanel = playScreenUI.Children.FirstOrDefault
                (p => p.Identifier.Equals("hotbarSlotPanel"));
            var quickMenuPanel = playScreenUI.Children.FirstOrDefault
                (p => p.Identifier.Equals("quickMenuPanel"));
            var questPanel = playScreenUI.Children.FirstOrDefault
                (p => p.Identifier.Equals("questPanel"));
            var dialogPanel = playScreenUI.Children.FirstOrDefault
                (p => p.Identifier.Equals("dialogPanel"));
            var entityNamePanel = dialogPanel.Children.FirstOrDefault
                (p => p.Identifier.Equals("entityNamePanel"));

            var entityName = entityNamePanel.Children.OfType<RichParagraph>().FirstOrDefault
                (e => e.Identifier.Equals("entityName"));

            var acceptButton = dialogPanel.Children.FirstOrDefault
                (p => p.Identifier.Equals("acceptButton"));
            acceptButton.Visible = false;
            acceptButton.Enabled = false;

            questPanel.Visible = false;
            hotbarSlotPanel.Visible = false;
            quickMenuPanel.Visible = false;
            dialogPanel.Visible = true;

            Dialog = dialogData;
            _dialogueIndex = 0;
            entityName.Text = Dialog.Dialogues[_dialogueIndex].Item1;
            _animatorDialogue.TextToType = Dialog.Dialogues[_dialogueIndex].Item2;

            if (Dialog.Type.Equals("Quest"))
            {
                var questId = Dialog.QuestId;
                var chapterId = Dialog.ChapterId;

                _questStamp = PlayerManager.Instance.Player.PlayerData.ChapterProgression.FirstOrDefault
                    (e => e.ChapterId.Equals(chapterId)).Quests.FirstOrDefault
                        (e => e.QuestId.Equals(questId));

                if (Dialog.Stage.Equals("onAccept")) acceptButton.Visible = true;
            }
        }

        public void CreateDialog(FriendlyMob friendlyMob)
        {
            if (friendlyMob == null || !friendlyMob.IsInteractable) return;

            InteractingMob = friendlyMob;
            InteractingMob.IsInteracting = true;
            IsShowDialogUI = true;
            IsForceAccepting = false;

            var playScreenUI = _mainPanels[PlayScreen];

            var hotbarSlotPanel = playScreenUI.Children.FirstOrDefault
                (p => p.Identifier.Equals("hotbarSlotPanel"));
            var quickMenuPanel = playScreenUI.Children.FirstOrDefault
                (p => p.Identifier.Equals("quickMenuPanel"));
            var questPanel = playScreenUI.Children.FirstOrDefault
                (p => p.Identifier.Equals("questPanel"));
            var dialogPanel = playScreenUI.Children.FirstOrDefault
                (p => p.Identifier.Equals("dialogPanel"));
            var entityNamePanel = dialogPanel.Children.FirstOrDefault
                (p => p.Identifier.Equals("entityNamePanel"));

            var entityName = entityNamePanel.Children.OfType<RichParagraph>().FirstOrDefault
                (e => e.Identifier.Equals("entityName"));

            var acceptButton = dialogPanel.Children.FirstOrDefault
                (p => p.Identifier.Equals("acceptButton"));
            acceptButton.Enabled = false;

            questPanel.Visible = false;
            hotbarSlotPanel.Visible = false;
            quickMenuPanel.Visible = false;
            dialogPanel.Visible = true;

            // Init Dialog
            _dialogueIndex = 0;
            var dialogList = InteractingMob.DialogData;

            if (InteractingMob.MobType == FriendlyMob.FriendlyMobType.QuestGiver)
            {
                var dialogs = dialogList.Where(e => e.Type.Equals("Quest")).ToList();
                var questId = dialogs.FirstOrDefault().QuestId;
                var chapterId = dialogs.FirstOrDefault().ChapterId;

                _questStamp = PlayerManager.Instance.Player.PlayerData.ChapterProgression.FirstOrDefault
                    (e => e.ChapterId.Equals(chapterId)).Quests.FirstOrDefault
                        (e => e.QuestId.Equals(questId));

                if (_questStamp.IsQuestClear)
                {
                    acceptButton.Visible = false;
                    Dialog = dialogs.FirstOrDefault(e => e.Stage.Equals("onClear"));                   
                }
                else if (!_questStamp.IsQuestAccepted && !_questStamp.IsQuestDone)
                {
                    acceptButton.Visible = true;
                    IsForceAccepting = true;
                    Dialog = dialogs.FirstOrDefault(e => e.Stage.Equals("onAccept"));
                }
                else if (_questStamp.IsQuestAccepted && !_questStamp.IsQuestDone)
                {
                    acceptButton.Visible = false;
                    Dialog = dialogs.FirstOrDefault(e => e.Stage.Equals("onGoing"));
                }
                else if (_questStamp.IsQuestAccepted && _questStamp.IsQuestDone)
                {
                    acceptButton.Visible = false;
                    Dialog = dialogs.FirstOrDefault(e => e.Stage.Equals("onDone"));
                }

                entityName.Text = Dialog.Dialogues[_dialogueIndex].Item1;
                _animatorDialogue.TextToType = Dialog.Dialogues[_dialogueIndex].Item2;
            }
            else
            {
                acceptButton.Visible = false;
                Dialog = dialogList.FirstOrDefault(e => e.Type.Equals("Daily"));

                entityName.Text = Dialog.Dialogues[_dialogueIndex].Item1;
                _animatorDialogue.TextToType = Dialog.Dialogues[_dialogueIndex].Item2;
            }
        }

        private void CloseDialog()
        {
            if (InteractingMob != null)
            {
                InteractingMob.IsInteracting = false;
            }
            IsShowDialogUI = false;

            var playScreenUI = _mainPanels[PlayScreen];

            var hotbarSlotPanel = playScreenUI.Children.FirstOrDefault
                (p => p.Identifier.Equals("hotbarSlotPanel"));
            var quickMenuPanel = playScreenUI.Children.FirstOrDefault
                (p => p.Identifier.Equals("quickMenuPanel"));
            var questPanel = playScreenUI.Children.FirstOrDefault
                (p => p.Identifier.Equals("questPanel"));
            var dialogPanel = playScreenUI.Children.FirstOrDefault
                (p => p.Identifier.Equals("dialogPanel"));

            hotbarSlotPanel.Visible = true;
            quickMenuPanel.Visible = true;
            questPanel.Visible = true;
            dialogPanel.Visible = false;

            // Invoke the event to notify listeners that the dialog is closed
            OnDialogClosed(new DialogClosedEventArgs(Dialog));
        }

        public void RefreshHotbar()
        {
            var playScreenUI = _mainPanels[PlayScreen];
            var hotbarSlotPanel = playScreenUI.Children.FirstOrDefault
                (p => p.Identifier.Equals("hotbarSlotPanel"));

            hotbarSlotPanel.ClearChildren();

            var minSlotNum = InventoryManager.HotbarSlot_1;
            var maxSlotNum = InventoryManager.HotbarSlot_8;

            var offSetX = 22f;

            // Set Item
            for (int i = minSlotNum; i <= maxSlotNum; i++)
            {
                var itemInSlot = InventoryManager.Instance.InventoryBag.FirstOrDefault
                    (item => item.Value.Slot.Equals(i));

                if (itemInSlot.Value != null)
                {
                    var iconItem = new Icon(IconType.None, Anchor.BottomLeft, 0.80f, false, offset: new Vector2(offSetX, 3f))
                    {
                        ItemId = itemInSlot.Value.ItemId,
                        Count = itemInSlot.Value.Count,
                        Slot = itemInSlot.Value.Slot,
                        KeyIndex = itemInSlot.Key,
                        Size = new Vector2(42, 42),
                        Texture = GetItemTexture(itemInSlot.Value.ItemId),
                    };

                    iconItem.AddChild(new Label(iconItem.Count.ToString(), Anchor.BottomRight, offset: new Vector2(-25f, -33))
                    {
                        Size = new Vector2(5, 5), // กำหนดขนาดของ Label
                        Scale = 1f,
                        ClickThrough = true,
                    });

                    hotbarSlotPanel.AddChild(iconItem);
                }
                else
                {
                    var iconNull = new Icon(IconType.None, Anchor.BottomLeft, 0.80f, false, offset: new Vector2(offSetX, 3f))
                    {
                        Locked = true,
                        Enabled = false,
                        Size = new Vector2(42, 42),
                    };

                    hotbarSlotPanel.AddChild(iconNull);
                }

                offSetX += 47f;

                switch (i)
                {
                    case InventoryManager.HotbarSlot_1: offSetX += 5f;
                        break;
                    case InventoryManager.HotbarSlot_2: offSetX += 5.2f;
                        break;
                    case InventoryManager.HotbarSlot_3:
                    case InventoryManager.HotbarSlot_4: offSetX += 5.2f;
                        break;
                    case InventoryManager.HotbarSlot_5: 
                    case InventoryManager.HotbarSlot_6: offSetX += 5.4f;
                        break;
                    case InventoryManager.HotbarSlot_7: offSetX += 5.5f;
                        break;
                    case InventoryManager.HotbarSlot_8: offSetX += 5.6f;
                        break;
                }
            }

            foreach (var iconItem in hotbarSlotPanel.Children.OfType<Icon>())
            {
                iconItem.OnClick = (Entity entity) =>
                {
                    InventoryManager.Instance.UseItemInHotbar(iconItem.KeyIndex);
                };
            }
        }


        /// <summary>
        /// Inventory Panel, Display list of items in inventory and Description
        /// </summary>
        private void InitInventoryUI()
        {
            // สร้าง Panel หลัก
            var inventoryPanel = new Panel(new Vector2(1200, 650), PanelSkin.Fancy, Anchor.Center)
            {
                Identifier = "invenMainPanel"
            };
            _mainPanels.Add(inventoryPanel);
            UserInterface.Active.AddEntity(inventoryPanel);

            // สร้าง Panel สำหรับฝั่งซ้าย
            var leftInvenPanel = new Panel(new Vector2(500, 600), PanelSkin.Simple, Anchor.TopLeft)
            {
                Identifier = "invenLeftPanel"
            };
            leftInvenPanel.SetCustomSkin(GetGuiTexture(GuiTextureName.drake_shop_window));
            inventoryPanel.AddChild(leftInvenPanel);

            var invenDescriptPanel = new Panel(new Vector2(450, 225), PanelSkin.ListBackground, Anchor.AutoCenter)
            {
                Identifier = "invenDescriptPanel",
                PanelOverflowBehavior = PanelOverflowBehavior.VerticalScroll
            };
            invenDescriptPanel.Scrollbar.AdjustMaxAutomatically = true;
            invenDescriptPanel.OnMouseEnter = (e) =>
            {
                UserInterface.Active.ActiveEntity = invenDescriptPanel;
            };

            invenDescriptPanel.AddChild(new Label("iconLabel", Anchor.AutoCenter)
            {
                Scale = 1.25f,
                ClickThrough = true,
                Identifier = "iconLabel"
            });
            invenDescriptPanel.AddChild(new Paragraph("description", Anchor.AutoInline)
            {
                //Size = new Vector2(380, 200), // กำหนดขนาดของ Label
                Scale = 1f,
                WrapWords = true,
                ClickThrough = true,
                Identifier = "description"
            });
            invenDescriptPanel.AddChild(new LineSpace(1));

            leftInvenPanel.AddChild(new Icon(IconType.None, Anchor.AutoCenter, 1, false)
            {
                Size = new Vector2(300, 300),   // ปรับขนาดของไอคอน
                Locked = true,
                Identifier = "itemIcon" 
            });
            leftInvenPanel.AddChild(new LineSpace(1));
            leftInvenPanel.AddChild(invenDescriptPanel);

            // สร้าง Panel สำหรับฝั่งขวา
            var rightInvenPanel = new Panel(new Vector2(600, 600), PanelSkin.None, Anchor.TopRight)
            {
                Identifier = "invenRightPanel"
            };
            inventoryPanel.AddChild(rightInvenPanel);

            rightInvenPanel.AddChild(new Header("IVENTORY"));
            rightInvenPanel.AddChild(new LineSpace(1));

            var listItemPanel = new Panel(new Vector2(550, 450), PanelSkin.ListBackground, Anchor.AutoCenter)
            {
                PanelOverflowBehavior = PanelOverflowBehavior.VerticalScroll,
                Identifier = "listItemPanel",
                Padding = new Vector2(10, 10)
            };
            listItemPanel.Scrollbar.AdjustMaxAutomatically = true;
            listItemPanel.OnMouseEnter = (e) =>
            {
                UserInterface.Active.ActiveEntity = listItemPanel;
            };
            rightInvenPanel.AddChild(listItemPanel);

            // add close button
            var closeButton = new Button("Close", anchor: Anchor.BottomRight
                , size: new Vector2(200, -1), offset: new Vector2(64, -100))
            {
                Identifier = "closeButton",
                Skin = ButtonSkin.Fancy,
                OnClick = (Entity entity) =>
                {
                    // Closing Inventory and reset current gui panel
                    // Pause PlayScreen
                    GameGlobals.Instance.IsGamePause = !GameGlobals.Instance.IsGamePause;
                    GameGlobals.Instance.IsOpenGUI = !GameGlobals.Instance.IsOpenGUI;

                    // Toggle the IsOpenInventory flag
                    GameGlobals.Instance.IsOpenInventoryPanel = false;
                    GameGlobals.Instance.IsRefreshPlayScreenUI = false;
                    CurrentUI = PlayScreen;
                }
            };
            inventoryPanel.AddChild(closeButton);

            var offsetX = 64;

            var useInvenItemButton = new Button("Use", anchor: Anchor.BottomLeft
                , size: new Vector2(200, -1), offset: new Vector2(offsetX, -100))
            {
                Identifier = "invenUseItemButton",
                Enabled = false,
                Skin = ButtonSkin.Fancy
            };
            inventoryPanel.AddChild(useInvenItemButton);         

            offsetX += 216;
            var invenSetHotbarButton = new Button("Setup Hotbar", anchor: Anchor.BottomLeft
                , size: new Vector2(200, -1), offset: new Vector2(offsetX, -100))
            {
                Identifier = "invenSetHotbarButton",
                Enabled = false,
                Skin = ButtonSkin.Fancy 
            };
            inventoryPanel.AddChild(invenSetHotbarButton);

            // Set on Click use item in inventory
            useInvenItemButton.OnClick = (Entity entity) =>
            {
                var currItemInv = InventoryManager.Instance.SelectedItem;

                string notifyText = string.Empty;
                if (currItemInv.Value.GetCategory().Equals("Equipment"))
                {
                    if (currItemInv.Value.Slot != GameGlobals.Instance.DefaultInventorySlot)
                    {
                        notifyText = $"Do you wanna unequip '{currItemInv.Value.GetName()}'?";
                    }
                    else notifyText = $"Do you wanna equip '{currItemInv.Value.GetName()}'?";
                }
                else notifyText = $"Do you wanna use '{currItemInv.Value.GetName()}'?";

                GeonBit.UI.Utils.MessageBox.ShowMsgBox("Use Item?"
                    , notifyText
                    , new GeonBit.UI.Utils.MessageBox.MsgBoxOption[]
                    {
                            new("Ok", () =>
                            {
                                // Use selected item from inventory
                                if (currItemInv.Value.GetCategory().Equals("Equipment")
                                    && currItemInv.Value.Slot != GameGlobals.Instance.DefaultInventorySlot)
                                {
                                    InventoryManager.Instance.UnEquip(currItemInv.Value);
                                    
                                    // refresh display item after selectedItem has been use
                                    RefreshInvenrotyItem(true);
                                }
                                else
                                {
                                    InventoryManager.Instance.UseItem(
                                        PlayerManager.Instance.Player,
                                        currItemInv.Key,
                                        currItemInv.Value);
                                }

                                useInvenItemButton.Enabled = false;
                                invenSetHotbarButton.Enabled = false;
                                return true;
                            }),
                            new("Cancel", () =>
                            {
                                return true;
                            })
                    });
            };

            // Set on Click set Hotbar item
            invenSetHotbarButton.OnClick = (Entity entity) =>
            {
                var currItemInv = InventoryManager.Instance.SelectedItem;

                var hotbarSetupForm = new Form([
                    new FormFieldData(FormFieldType.DropDown, "dropdown_setup_hotbar", "Hotbar Slot")
                        {
                                ToolTipText = "Click to select a Hotbar Slot",
                                Choices = ["Slot 1", "Slot 2", "Slot 3", "Slot 4", "Slot 5", "Slot 6", "Slot 7", "Slot 8"]
                        },
                    ], null);

                GeonBit.UI.Utils.MessageBox.ShowMsgBox("Setup Hotbar Slot"
                    , "", "Done"
                    , extraEntities: [hotbarSetupForm.FormPanel]
                    , onDone: () =>
                    {
                        if (hotbarSetupForm.GetValue("dropdown_setup_hotbar") != null)
                        {
                            GeonBit.UI.Utils.MessageBox.ShowMsgBox("Setup Hotbar Successfully"
                                , $"Set '{currItemInv.Value.GetName()}' on '{hotbarSetupForm.GetValueString("dropdown_setup_hotbar")}'");

                            var output = hotbarSetupForm.GetValueString("dropdown_setup_hotbar");
                            var slotNum = InventoryManager.Instance.GetIHotbarSlot(output);
                            InventoryManager.Instance.SetHotbarItem(currItemInv.Value, slotNum);
                        }
                        else
                        {
                            GeonBit.UI.Utils.MessageBox.ShowMsgBox("Setup Complete", $"Ya didn't select a for '{currItemInv.Value.GetName()}'");
                            RefreshInvenrotyItem(true);
                        }

                        //InventoryManager.Instance.ItemSelected = null;
                        useInvenItemButton.Enabled = false;
                        invenSetHotbarButton.Enabled = false;
                    });
            };
        }

        // Call this after initialize Player's Inventory Data
        public void InitInventoryItemDisplay()
        {
            RefreshInvenrotyItem(false);

            var inventoryPanel = _mainPanels.ElementAt(InventoryPanel);
            var invenRightPanel = inventoryPanel.Children?.FirstOrDefault
                (p => p.Identifier.Equals("invenRightPanel"));
            var listItemPanel = invenRightPanel.Children?.FirstOrDefault
                (p => p.Identifier.Equals("listItemPanel"));
            var invenUseItemButton = inventoryPanel.Children?.FirstOrDefault
                (p => p.Identifier.Equals("invenUseItemButton"));
            var invenSetHotbarButton = inventoryPanel.Children?.FirstOrDefault
                (p => p.Identifier.Equals("invenSetHotbarButton"));

            if (listItemPanel.Children.Count != 0)
            {
                var fisrtItem = listItemPanel.Children.OfType<Icon>().ToList().ElementAt(0);

                SetIconItemInventoryDisplay(fisrtItem);

                // Setup Enable or Disable the button for fisrtItem
                var item = InventoryManager.Instance.InventoryBag.FirstOrDefault
                    (i => i.Key.Equals(fisrtItem.KeyIndex));

                InventoryManager.Instance.SelectedItem = item;

                if (IsUsableItem(fisrtItem.ItemId))
                {
                    invenUseItemButton.Enabled = true;

                    if (!GetItemCategory(fisrtItem.ItemId).Equals("Equipment"))
                    {
                        invenSetHotbarButton.Enabled = true;
                    }
                    else
                    {
                        // In case selected item is Equipment
                        invenSetHotbarButton.Enabled = false;

                        //if (!item.Value.Slot.Equals(GameGlobals.Instance.DefaultInventorySlot))
                        //    invenUseItemButton.Enabled = false;
                    }
                }
                else
                {
                    invenUseItemButton.Enabled = false;
                    invenSetHotbarButton.Enabled = true;
                }
            }
        }

        public void RefreshInvenrotyItem(bool isClearLeftPanel)
        {
            var inventoryPanel = _mainPanels[InventoryPanel];
            var invenLeftPanel = inventoryPanel.Children?.FirstOrDefault
                (p => p.Identifier.Equals("invenLeftPanel"));

            var invenRightPanel = inventoryPanel.Children?.FirstOrDefault
                (p => p.Identifier.Equals("invenRightPanel"));
            
            var listItemPanel = invenRightPanel.Children?.FirstOrDefault
                (p => p.Identifier.Equals("listItemPanel"));
            
            var invenUseItemButton = inventoryPanel.Children?.FirstOrDefault
                (p => p.Identifier.Equals("invenUseItemButton"));
            var invenSetHotbarButton = inventoryPanel.Children?.FirstOrDefault
                (p => p.Identifier.Equals("invenSetHotbarButton"));

            // Clear inventory display
            listItemPanel.ClearChildren();

            // if true den set invisible to all entity in invenLeftPanel
            if (isClearLeftPanel)
                foreach (var itemLeftPanel in invenLeftPanel.Children)
                    itemLeftPanel.Visible = false;

            // Set item Display
            var tempIconItems = new List<Icon>();
            foreach (var item in InventoryManager.Instance.InventoryBag)
            {
                // Item Icon
                var iconItem = new Icon(IconType.None, Anchor.AutoInline, 1, true)
                {
                    ItemId = item.Value.ItemId,
                    Count = item.Value.Count,
                    Slot = item.Value.Slot,
                    KeyIndex = item.Key,
                    Texture = GetItemTexture(item.Value.ItemId),
                };

                // Item text
                var text = iconItem.Count.ToString();
                if (item.Value.GetCategory().Equals("Equipment"))
                {
                    if (iconItem.Slot >= InventoryManager.Sword && iconItem.Slot <= InventoryManager.Ring)
                    {
                        text = "E";
                    }
                    else text = "";
                }

                var iconText = new Label(text, Anchor.BottomRight, offset: new Vector2(-22, -35))
                {
                    Size = new Vector2(10, 10),
                    Scale = 1f,
                    ClickThrough = true,
                };

                iconItem.AddChild(iconText);
                tempIconItems.Add(iconItem);
            }

            // Sort Item Display by Id and add to listItemPanel
            var sortedIconItems = tempIconItems.OrderBy(icon => icon.ItemId).ToList();
            foreach (var sortediconItem in sortedIconItems)
                listItemPanel.AddChild(sortediconItem);

            // Set OnClick
            foreach (var iconItem in listItemPanel.Children.OfType<Icon>())
            {
                iconItem.OnClick = (Entity entity) =>
                {
                    SetIconItemInventoryDisplay(iconItem);

                    foreach (var itemLeftPanel in invenLeftPanel.Children)
                        itemLeftPanel.Visible = true;

                    // Enable or Disable the button for each item icon
                    var item = InventoryManager.Instance.InventoryBag.FirstOrDefault
                        (i => i.Key.Equals(iconItem.KeyIndex));

                    InventoryManager.Instance.SelectedItem = item;

                    if (IsUsableItem(iconItem.ItemId))
                    {
                        invenUseItemButton.Enabled = true;

                        if (!GetItemCategory(iconItem.ItemId).Equals("Equipment"))
                        {
                            invenSetHotbarButton.Enabled = true;
                        }
                        else
                        {
                            // In case selected item is Equipment
                            invenSetHotbarButton.Enabled = false;

                            //if (!item.Value.Slot.Equals(GameGlobals.Instance.DefaultInventorySlot))
                            //    invenUseItemButton.Enabled = false;
                        }
                    }
                    else
                    {
                        invenUseItemButton.Enabled = false;
                        invenSetHotbarButton.Enabled = true;
                    }
                };
            }
        }

        private void SetIconItemInventoryDisplay(Icon icon)
        {
            var itemData = GameGlobals.Instance.ItemsDatas.FirstOrDefault
                (i => i.ItemId.Equals(icon.ItemId));

            var inventoryPanel = _mainPanels.ElementAt(InventoryPanel);
            var invenLeftPanel = inventoryPanel.Children?.FirstOrDefault
                (p => p.Identifier.Equals("invenLeftPanel"));
            var invenDescriptPanel = invenLeftPanel.Children?.FirstOrDefault
                (p => p.Identifier.Equals("invenDescriptPanel"));

            var itemIcon = invenLeftPanel.Children.OfType<Icon>().FirstOrDefault
                (i => i.Identifier.Equals("itemIcon"));
            itemIcon.Texture = icon.Texture;

            var iconLabel = invenDescriptPanel.Children.OfType<Label>().FirstOrDefault
                (i => i.Identifier.Equals("iconLabel"));
            iconLabel.Text = itemData.Name;

            var description = invenDescriptPanel.Children.OfType<Paragraph>().FirstOrDefault
                (i => i.Identifier.Equals("description"));
            description.Text = itemData.Description;                 
        }


        /// <summary>
        /// Crafting Panel, Display list of craftable items that play unlocked
        /// </summary>
        private void InitCraftingUI()
        {
            // สร้าง Panel หลัก
            var craftingPanel = new Panel(new Vector2(1400, 710), PanelSkin.Fancy, Anchor.Center)
            {
                Visible = true,
                Identifier = "craftingMainPanel"
            };
            _mainPanels.Add(craftingPanel);
            UserInterface.Active.AddEntity(craftingPanel);

            // สร้าง Panel สำหรับฝั่งซ้าย
            var leftCraftingPanel = new Panel(new Vector2(650, 660), PanelSkin.Simple, Anchor.TopLeft)
            {
                Identifier = "leftCraftingPanel"
            };
            leftCraftingPanel.SetCustomSkin(GetGuiTexture(GuiTextureName.drake_shop_window));
            craftingPanel.AddChild(leftCraftingPanel);

            var craftingDescriptPanel = new Panel(new Vector2(550, 285), PanelSkin.ListBackground, Anchor.AutoCenter)
            {
                Identifier = "craftingDescriptPanel",
                PanelOverflowBehavior = PanelOverflowBehavior.VerticalScroll
            };
            craftingDescriptPanel.Scrollbar.AdjustMaxAutomatically = true;
            craftingDescriptPanel.OnMouseEnter = (e) =>
            {
                UserInterface.Active.ActiveEntity = craftingDescriptPanel;
            };

            craftingDescriptPanel.AddChild(new Label("iconLabel", Anchor.AutoCenter)
            {
                //Size = new Vector2(250, 30), // กำหนดขนาดของ Label
                Scale = 1.25f,
                ClickThrough = true,
                Identifier = "iconLabel"
            });
            craftingDescriptPanel.AddChild(new Paragraph("description", Anchor.AutoInline)
            {
                //Size = new Vector2(380, 200), // กำหนดขนาดของ Label
                Scale = 1f,
                WrapWords = true,
                ClickThrough = true,
                Identifier = "description"
            });
            craftingDescriptPanel.AddChild(new LineSpace(1));

            leftCraftingPanel.AddChild(new Icon(IconType.None, Anchor.AutoCenter, 1, false)
            {
                Size = new Vector2(300, 300),   // ปรับขนาดของไอคอน
                Locked = true,
                Identifier = "itemIcon"
            });
            leftCraftingPanel.AddChild(new LineSpace(1));
            leftCraftingPanel.AddChild(craftingDescriptPanel);

            // สร้าง Panel สำหรับฝั่งขวา
            var rightCraftingPanel = new Panel(new Vector2(650, 660), PanelSkin.None, Anchor.TopRight)
            {
                Identifier = "rightCraftingPanel"
            };
            craftingPanel.AddChild(rightCraftingPanel);

            rightCraftingPanel.AddChild(new Header("CRAFTING ITEM LIST"));
            rightCraftingPanel.AddChild(new LineSpace(1));

            var listCraftableItemPanel = new Panel(new Vector2(550, 530), PanelSkin.ListBackground, Anchor.AutoCenter)
            {
                PanelOverflowBehavior = PanelOverflowBehavior.VerticalScroll,
                Identifier = "listCraftableItemPanel",
                Padding = new Vector2(15, 15)
            };
            listCraftableItemPanel.Scrollbar.AdjustMaxAutomatically = true;
            rightCraftingPanel.AddChild(listCraftableItemPanel);

            // Crafting Selected Item
            // Panel
            var craftingSelectItemPanel = new Panel(new Vector2(1400, 710), PanelSkin.Fancy, Anchor.Center)
            {
                Identifier = "craftingSelectItemPanel",
                Visible = false,
            };
            UserInterface.Active.AddEntity(craftingSelectItemPanel);

            var leftCraftingSelectItemPanel = new Panel(new Vector2(650, 660), PanelSkin.Simple, Anchor.TopLeft)
            {
                Identifier = "leftCraftingSelectItemPanel"
            };
            leftCraftingSelectItemPanel.SetCustomSkin(GetGuiTexture(GuiTextureName.drake_shop_window));
            craftingSelectItemPanel.AddChild(leftCraftingSelectItemPanel);

            leftCraftingSelectItemPanel.AddChild(new Icon(IconType.None, Anchor.AutoCenter, 1, false)
            {
                Size = new Vector2(300, 300),   // ปรับขนาดของไอคอน
                Locked = true,
                Identifier = "itemIcon"
            });
            leftCraftingSelectItemPanel.AddChild(new LineSpace(1));

            var quantitySelectorPanel = new Panel(new Vector2(500, 200), PanelSkin.None, Anchor.BottomCenter)
            {
                Identifier = "quantitySelectorPanel"
            };

            var quantitySlider = new Slider(1, 10, SliderSkin.Default, Anchor.AutoCenter)
            {
                Identifier = "quantitySlider"
            };

            var quantityLabel = new Label("Quantity: " + quantitySlider.Min, Anchor.AutoCenter)
            {
                Identifier = "quantityLabel",
                Scale = 1.2f,
            };
            quantitySlider.OnValueChange = (Entity entity) =>
            {
                quantityLabel.Text = "Quantity: " + quantitySlider.Value;
            };

            quantitySelectorPanel.AddChild(quantityLabel);
            quantitySelectorPanel.AddChild(quantitySlider);
            leftCraftingSelectItemPanel.AddChild(quantitySelectorPanel);

            var rightCraftingSelectPanel = new Panel(new Vector2(650, 660), PanelSkin.None, Anchor.TopRight)
            {
                Identifier = "rightCraftingSelectPanel"
            };
            craftingSelectItemPanel.AddChild(rightCraftingSelectPanel);

            rightCraftingSelectPanel.AddChild(new Header("INGREDINES"));
            rightCraftingSelectPanel.AddChild(new LineSpace(1));

            var ingredientList = new SelectList(new Vector2(550, 530), Anchor.AutoCenter, skin: PanelSkin.ListBackground)
            {
                Identifier = "ingredientList",
                Padding = new Vector2(40, 16),
                LockSelection = true
            };         

            rightCraftingSelectPanel.AddChild(ingredientList);

            // Close Button Crafting Selected Item
            {
                var closeButton = new Button("Back", anchor: Anchor.BottomRight
                , size: new Vector2(200, -1), offset: new Vector2(64, -100))
                {
                    Identifier = "closeButton",
                    Skin = ButtonSkin.Fancy,
                    OnClick = (Entity entity) =>
                    {
                        craftingPanel.Visible = !craftingPanel.Visible;
                        craftingSelectItemPanel.Visible = !craftingSelectItemPanel.Visible;

                        quantitySlider.Value = 1;
                    }
                };
                craftingSelectItemPanel.AddChild(closeButton);
            } 

            var craftingSelectedButton = new Button("Craft", anchor: Anchor.BottomLeft
                , size: new Vector2(250, -1), offset: new Vector2(200, -100))
            {
                Enabled = false,
                Identifier = "craftingSelectedButton",
                Skin = ButtonSkin.Fancy,
                OnClick = (Entity entity) =>
                {
                    var craftingSelected = CraftingManager.Instance.CraftingItemSelected;

                    GeonBit.UI.Utils.MessageBox.ShowMsgBox("Crafting Item?"
                    , $"Do ya wanna craft '{GetItemName(craftingSelected.ItemId)}'?"
                    , new GeonBit.UI.Utils.MessageBox.MsgBoxOption[]
                    {
                            new("Ok", () =>
                            {
                                var itemQuantity = quantitySlider.Value;
                                var craftableCount = CraftingManager.Instance.GetCraftableNumber(craftingSelected.ItemId);

                                if (craftableCount != 0 && itemQuantity <= craftableCount)
                                {
                                    CraftingManager.Instance.CraftingItem(craftingSelected.ItemId, itemQuantity);
                                }
                                else
                                {
                                    GeonBit.UI.Utils.MessageBox.ShowMsgBox("ERROR", "Insufficient ingredients. \nPlease select a new quantity.");
                                    return true;
                                }

                                GeonBit.UI.Utils.MessageBox.ShowMsgBox("Crafting Item", "Item created successfully");

                                // Reset craft button
                                var craftingSelectItemPanel = UserInterface.Active.Root.Children.FirstOrDefault
                                    (i => i.Identifier.Equals("craftingSelectItemPanel"));
                                var craftingSelectedButton = craftingSelectItemPanel.Children.OfType<Button>().FirstOrDefault
                                    (i => i.Identifier.Equals("craftingSelectedButton"));

                                var maxQuantity = CraftingManager.Instance.GetCraftableNumber(craftingSelected.ItemId);

                                if (maxQuantity > 0)
                                {
                                    quantitySlider.Max = maxQuantity;
                                    quantitySlider.Enabled = true;
                                    craftingSelectedButton.Enabled = true;
                                }
                                else
                                {
                                    quantitySlider.Max = 1;
                                    quantitySlider.Enabled = false;
                                    craftingSelectedButton.Enabled = false;
                                }
                                return true;
                            }),
                            new("Cancel", () =>
                            {
                                return true;
                            })
                    });
                }
            };
            craftingSelectItemPanel.AddChild(craftingSelectedButton);

            // CraftingPanel
            // add close button
            {
                var closeButton = new Button("Close", anchor: Anchor.BottomRight
                , size: new Vector2(200, -1), offset: new Vector2(64, -100))
                {
                    Identifier = "closeButton",
                    Skin = ButtonSkin.Fancy,
                    OnClick = (Entity entity) =>
                    {
                        // Closing Inventory and reset current gui panel
                        // Pause PlayScreen
                        GameGlobals.Instance.IsGamePause = !GameGlobals.Instance.IsGamePause;
                        GameGlobals.Instance.IsOpenGUI = !GameGlobals.Instance.IsOpenGUI;

                        // Toggle the IsOpenCraftingPanel flag
                        GameGlobals.Instance.IsOpenCraftingPanel = false;
                        GameGlobals.Instance.IsRefreshPlayScreenUI = false;
                        CurrentUI = PlayScreen;
                    }
                };
                craftingPanel.AddChild(closeButton);
            }   

            var craftingItemButton = new Button("Craft", anchor: Anchor.BottomLeft
                , size: new Vector2(250, -1), offset: new Vector2(200, -100))
            {
                Identifier = "craftingItemButton",
                Skin = ButtonSkin.Fancy,
                Enabled = false,
                OnClick = (Entity entity) =>
                {
                    // Set display crafting item selected & indredients
                    var itemCraftingSelected = CraftingManager.Instance.CraftingItemSelected;
                    SetCraftingItemSelectedDisplay(itemCraftingSelected);
                    SetItemIngredientDisplay(itemCraftingSelected);

                    // Set craftable quantity
                    var maxQuantity = CraftingManager.Instance.GetCraftableNumber(itemCraftingSelected.ItemId);

                    if (maxQuantity > 0)
                    {
                        quantitySlider.Max = maxQuantity;
                        quantitySlider.Enabled = true;
                        craftingSelectedButton.Enabled = true;
                    }
                    else
                    {
                        quantitySlider.Max = 1;
                        quantitySlider.Enabled = false;
                        craftingSelectedButton.Enabled = false;
                    }

                    craftingPanel.Visible = !craftingPanel.Visible;
                    craftingSelectItemPanel.Visible = !craftingSelectItemPanel.Visible;
                }
            };
            craftingPanel.AddChild(craftingItemButton);
        }

        public void InitCraftableItemDisplay()
        {
            RefreshCraftableItem(CurrentCraftingList);

            var craftingPanel = _mainPanels.ElementAt(CraftingPanel);
            var rightCraftingPanel = craftingPanel.Children?.FirstOrDefault
                (e => e.Identifier.Equals("rightCraftingPanel"));
            var listCraftableItemPanel = rightCraftingPanel.Children?.FirstOrDefault
                (e => e.Identifier.Equals("listCraftableItemPanel"));
            var craftingItemButton = craftingPanel.Children?.FirstOrDefault
                (e => e.Identifier.Equals("craftingItemButton"));
            var leftCraftingPanel = craftingPanel.Children?.FirstOrDefault
                (p => p.Identifier.Equals("leftCraftingPanel"));

            List<CraftableItem> craftableItemList = [];

            switch (CurrentCraftingList)
            {
                case ThaiTraditionalMedicine:
                    craftableItemList = CraftingManager.Instance.CraftableMedicineItem;
                    break;

                case ConsumableItem:
                    craftableItemList = CraftingManager.Instance.CraftableFoodItem;
                    break;

                case Equipment:
                    craftableItemList = CraftingManager.Instance.CraftableEquipmentItem;
                    break;
            }

            if (listCraftableItemPanel.Children.Count != 0)
            {
                var fisrtItem = listCraftableItemPanel.Children.OfType<Icon>().ToList().ElementAt(0);

                SetCraftableItemDisplay(fisrtItem);

                var craftingItemselected = craftableItemList.FirstOrDefault
                    (i => i.ItemId.Equals(fisrtItem.ItemId));
                CraftingManager.Instance.CraftingItemSelected = craftingItemselected;

                // Enable or Disable the button for each item icon
                if (fisrtItem.Enabled) craftingItemButton.Enabled = true;
            }
        }

        public void RefreshCraftableItem(string craftableType)
        {
            var craftingPanel = _mainPanels.ElementAt(CraftingPanel);
            var rightCraftingPanel = craftingPanel.Children?.FirstOrDefault
                (e => e.Identifier.Equals("rightCraftingPanel"));
            var listCraftableItemPanel = rightCraftingPanel.Children?.FirstOrDefault
                (e => e.Identifier.Equals("listCraftableItemPanel"));
            var craftingItemButton = craftingPanel.Children?.FirstOrDefault
                (e => e.Identifier.Equals("craftingItemButton"));
            var leftCraftingPanel = craftingPanel.Children?.FirstOrDefault
                (p => p.Identifier.Equals("leftCraftingPanel"));

            craftingItemButton.Enabled = false;

            // Clear craftable item display
            listCraftableItemPanel.ClearChildren();

            List<CraftableItem> craftableItemList = [];

            switch (craftableType)
            {
                case ThaiTraditionalMedicine:
                    craftableItemList = CraftingManager.Instance.CraftableMedicineItem;
                    break;

                case ConsumableItem:
                    craftableItemList = CraftingManager.Instance.CraftableFoodItem;
                    break;

                case Equipment:
                    craftableItemList = CraftingManager.Instance.CraftableEquipmentItem;
                    break;
            }

            // Set item
            foreach (var item in craftableItemList)
            {
                // Item Icon
                var iconItem = new Icon(IconType.None, Anchor.AutoInline, 1, true)
                {
                    Enabled = item.IsCraftable,
                    ItemId = item.ItemId,
                    Texture = GetItemTexture(item.ItemId),
                };

                listCraftableItemPanel.AddChild(iconItem);
            }

            foreach (var iconItem in listCraftableItemPanel.Children.OfType<Icon>())
            {
                iconItem.OnClick = (Entity entity) =>
                {
                    SetCraftableItemDisplay(iconItem);

                    var craftingItemselected = craftableItemList.FirstOrDefault
                        (i => i.ItemId.Equals(iconItem.ItemId));
                    CraftingManager.Instance.CraftingItemSelected = craftingItemselected;              

                    // Enable or Disable the button for each item icon
                    if (iconItem.Enabled) craftingItemButton.Enabled = true;
                };
            }

            // Set Display fisrt Icon in list
            var firstIcon = listCraftableItemPanel.Children.OfType<Icon>().FirstOrDefault();
            SetCraftableItemDisplay(firstIcon);
            var craftingItemselected = craftableItemList.FirstOrDefault
                        (i => i.ItemId.Equals(firstIcon.ItemId));
            CraftingManager.Instance.CraftingItemSelected = craftingItemselected;

            // Set enable
            if (firstIcon.Enabled)
            {
                craftingItemButton.Enabled = true;
            }
            else craftingItemButton.Enabled = false;
        }

        private void SetCraftableItemDisplay(Icon icon)
        {
            var itemData = GameGlobals.Instance.ItemsDatas.FirstOrDefault
                (i => i.ItemId.Equals(icon.ItemId));

            var craftingPanel = _mainPanels.ElementAt(CraftingPanel);
            var leftCraftingPanel = craftingPanel.Children?.FirstOrDefault
                (p => p.Identifier.Equals("leftCraftingPanel"));
            var craftingDescriptPanel = leftCraftingPanel.Children?.FirstOrDefault
                (p => p.Identifier.Equals("craftingDescriptPanel"));

            // Clone the clicked icon and add it to the left panel
            var itemIcon = leftCraftingPanel.Children.OfType<Icon>().FirstOrDefault
                (i => i.Identifier.Equals("itemIcon"));
            itemIcon.Texture = icon.Texture;

            // สร้าง Label เพื่อแสดงชื่อของไอคอน
            var iconLabel = craftingDescriptPanel.Children.OfType<Label>().FirstOrDefault
                (i => i.Identifier.Equals("iconLabel"));
            iconLabel.Text = itemData.Name;

            // Description item
            var description = craftingDescriptPanel.Children.OfType<Paragraph>().FirstOrDefault
                (i => i.Identifier.Equals("description"));
            description.Text = itemData.Description;
        }

        private void SetCraftingItemSelectedDisplay(CraftableItem itemCrafting)
        {            
            var craftingSelectItemPanel = UserInterface.Active.Root.Children.FirstOrDefault
                (i => i.Identifier.Equals("craftingSelectItemPanel"));
            var leftCraftingSelectItemPanel = craftingSelectItemPanel.Children.FirstOrDefault
                (i => i.Identifier.Equals("leftCraftingSelectItemPanel"));

            var itemIcon = leftCraftingSelectItemPanel.Children.OfType<Icon>().FirstOrDefault
                (i => i.Identifier.Equals("itemIcon"));
            itemIcon.Texture = GetItemTexture(itemCrafting.ItemId);
        }

        private void SetItemIngredientDisplay(CraftableItem itemCrafting)
        {
            var recipeData = GameGlobals.Instance.CraftingRecipeDatas.FirstOrDefault
                (i => i.RecipeId.Equals(itemCrafting.ItemId));

            var craftingSelectItemPanel = UserInterface.Active.Root.Children.FirstOrDefault
                (i => i.Identifier.Equals("craftingSelectItemPanel"));
            var rightCraftingSelectPanel = craftingSelectItemPanel.Children.FirstOrDefault
                (i => i.Identifier.Equals("rightCraftingSelectPanel"));
            var ingredientList = rightCraftingSelectPanel.Children.OfType<SelectList>().FirstOrDefault
                (i => i.Identifier.Equals("ingredientList"));

            ingredientList.ClearItems();

            foreach (var ingredient in recipeData.Ingredients)
            {
                // Add ingredient of selected crafting item
                string ingLabel = "";

                var items = InventoryManager.Instance.InventoryBag.Values.Where
                    (i => i.ItemId.Equals(ingredient.ItemId));

                if (items != null || items.Any())
                {
                    int totalItemCount = 0;
                    foreach (var item in items)
                        totalItemCount += item.Count;

                    if (totalItemCount < ingredient.Quantity)
                    {
                        var count = totalItemCount - ingredient.Quantity;
                        ingLabel = $"{ingredient.Name} x {ingredient.Quantity} ({count})((Red))";
                    }
                    else ingLabel = $"{ingredient.Name} x {ingredient.Quantity} ({totalItemCount})((White))";
                }
                else
                {
                    ingLabel = $"{ingredient.Name} x {ingredient.Quantity} (-{ingredient.Quantity})((Red))";
                }

                ingredientList.AddItem(ingLabel);
                ingredientList.IconsScale *= 1f;
                ingredientList.SetIcon(GetItemTexture(ingredient.ItemId), ingLabel);
            }
        }

        public void InitInspectUI()
        {
            // Main Panel
            var inspectPanel = new Panel(new Vector2(900, 710), PanelSkin.None, Anchor.Center)
            {
                Visible = true,
                Identifier = "inspectPanel"
            };
            _mainPanels.Add(inspectPanel);
            UserInterface.Active.AddEntity(inspectPanel);

            // create panel tabs
            var inspectTabs = new PanelTabs()
            {
                Identifier = "inspectTabs",
                BackgroundSkin = PanelSkin.Fancy
            };
            inspectPanel.AddChild(inspectTabs);

            // Add tab: Character Inspect
            {            
                TabData characterTab = inspectTabs.AddTab("Character");

                // Set true if character tab is clicked
                characterTab.button.OnClick = (Entity entity) =>
                {
                    RefreshInspectCharacterDisplay();
                    ClearSkillDescription("Character");
                    
                    IsCharacterTabSelected = true;
                };

                // Left Panel
                var leftPanel = new Panel(new Vector2(450, 670), PanelSkin.None, Anchor.TopLeft)
                {
                    Identifier = "leftPanel"
                };
                characterTab.panel.AddChild(leftPanel);

                // Display Character
                var displayCharacterPanel = new Panel(new Vector2(425, 400), PanelSkin.ListBackground, Anchor.TopCenter)
                {
                    Identifier = "displayCharacterPanel"
                };
                leftPanel.AddChild(displayCharacterPanel);

                displayCharacterPanel.AddChild(new Header()
                {                
                    Identifier = "characterNameHeader",
                    Scale = 1.5f,
                    Anchor = Anchor.TopCenter
                });

                // Display Skill point
                var skillPointPanel = new Panel()
                {
                    Identifier = "skillPointPanel",
                    Anchor = Anchor.TopLeft,
                    Skin = PanelSkin.None
                };
                displayCharacterPanel.AddChild(skillPointPanel);

                skillPointPanel.AddChild(new Icon(IconType.None)
                {
                    Identifier = "SPIcon",
                    Locked = true,
                    Texture = GetGuiTexture(GuiTextureName.skill_point),
                    Scale = 0.75f,
                    Offset = new Vector2(-35, -35),
                    Anchor = Anchor.AutoInlineNoBreak
                });
                skillPointPanel.AddChild(new Label()
                {
                    Identifier = "skillPoint",
                    Scale = 1.15f,
                    Anchor = Anchor.AutoInlineNoBreak
                });

                // Left Equipment Panel
                var leftEquipmentPanel = new Panel(new Vector2(70, 210), PanelSkin.None, Anchor.CenterLeft)
                {
                    Identifier = "leftEquipmentPanel",
                };
                displayCharacterPanel.AddChild(leftEquipmentPanel);

                // Add Slot Equipment in Left Panel
                for (int i = 0; i < 3; i++)
                {
                    leftEquipmentPanel.AddChild(new Icon(IconType.None, Anchor.AutoInline, 1, true)
                    {
                        Identifier = "Slot_" + i,
                        Offset = new Vector2(-16, 0)
                    });
                }

                // Right Equipment Panel
                var rightEquipmentPanel = new Panel(new Vector2(70, 210), PanelSkin.None, Anchor.CenterRight)
                {
                    Identifier = "rightEquipmentPanel"
                };
                displayCharacterPanel.AddChild(rightEquipmentPanel);

                // Add Slot Equipment in Right Panel
                for (int i = 3; i < 6; i++)
                {
                    rightEquipmentPanel.AddChild(new Icon(IconType.None, Anchor.AutoInline, 1, true)
                    {
                        Identifier = "Slot_" + i,
                        Offset = new Vector2(-16, 0)
                    });
                }

                // Skill Icon Panel
                var skillIconPanel = new Panel(new Vector2(220, 60), PanelSkin.None, Anchor.BottomCenter)
                {
                    Identifier = "skillIconPanel"
                };
                displayCharacterPanel.AddChild(skillIconPanel); 

                leftPanel.AddChild(new LineSpace(1));
                var selectedSkillPanel = new Panel(new Vector2(820, 210), PanelSkin.ListBackground, Anchor.AutoCenter)
                {
                    Identifier = "selectedSkillPanel",
                    Offset = new Vector2((800 / 4) - 5, 0f)
                };
                leftPanel.AddChild(selectedSkillPanel);

                var descripLeftSkill = new Paragraph(@"", Anchor.CenterLeft, Color.White, 0.9f, new Vector2(280, 250))
                {
                    Identifier = "descripLeftSkill",
                    WrapWords = true
                };
                selectedSkillPanel.AddChild(descripLeftSkill);

                // Upgrading Cost
                var skillCostPanel = new Panel(new Vector2(200, 180), PanelSkin.None, Anchor.TopCenter)
                {
                    Identifier = "skillCostPanel",
                    Offset = new Vector2(0, -25),
                    Visible = false
                };
                selectedSkillPanel.AddChild(skillCostPanel);

                skillCostPanel.AddChild(new Label("COST", Anchor.TopCenter)
                {
                    Scale = 1f,
                    Offset = new Vector2(0, -10),
                    FillColor = Color.Yellow,
                });

                // SkillPoint Cost
                var SPCost = new Panel(new Vector2(150, 30), PanelSkin.None, Anchor.Center)
                {
                    Identifier = "SPCost",
                    Offset = new Vector2(0, -40)
                };
                skillCostPanel.AddChild(SPCost);
                SPCost.AddChild(new Image(
                    GetGuiTexture(GuiTextureName.skill_point),
                    new Vector2(20, 20),
                    anchor: Anchor.AutoInlineNoBreak));
                SPCost.AddChild(new Label("SP_Cost", Anchor.AutoInlineNoBreak)
                {
                    Identifier = "SPCostText",
                    Scale = 1f,
                    Offset = new Vector2(10, -5)
                });

                // GoldCoin Cost
                var GoldCost = new Panel(new Vector2(150, 30), PanelSkin.None, Anchor.Center)
                {
                    Identifier = "GoldCost",
                    Offset = new Vector2(0, -10)
                };
                skillCostPanel.AddChild(GoldCost);
                GoldCost.AddChild(new Image(
                    GetGuiTexture(GuiTextureName.gold_coin),
                    new Vector2(20, 20),
                    anchor: Anchor.AutoInlineNoBreak));
                GoldCost.AddChild(new Label("Gold_Cost", Anchor.AutoInlineNoBreak)
                {
                    Identifier = "GoldCostText",
                    Scale = 1f,
                    Offset = new Vector2(10, -5)
                });

                // Button Upgrade Skill
                var upSkillButton = new Button("Lv.Up", ButtonSkin.Fancy, Anchor.BottomCenter, new Vector2(120, 50))
                {
                    Identifier = "upSkillButton",
                    ToolTipText = "Use a 'Skill Point' and 'Gold Coin' to up level skill",
                    Enabled = false,
                    Visible = false
                };
                selectedSkillPanel.AddChild(upSkillButton);

                var descripRightSkill = new Paragraph(@"", Anchor.CenterRight, Color.White, 0.9f, new Vector2(280, 250))
                {
                    Identifier = "descripRightSkill",
                    WrapWords = true
                };
                selectedSkillPanel.AddChild(descripRightSkill);

                var normalSkillIcon = new Icon(IconType.None, Anchor.AutoInlineNoBreak, 1, true)
                {
                    Identifier = "normalSkillIcon",
                    ToolTipText = "Normal Skill",
                    Offset = new Vector2(0, 0)
                };
                skillIconPanel.AddChild(normalSkillIcon);
                normalSkillIcon.AddChild(new Label($"", Anchor.BottomRight, offset: new Vector2(-22, -35))
                {
                    Identifier = "normalSkillLevel",
                    Size = new Vector2(10, 10),
                    Scale = 1f,
                    ClickThrough = true,
                });
                var burstSkillIcon = new Icon(IconType.None, Anchor.AutoInlineNoBreak, 1, true)
                {
                    Identifier = "burstSkillIcon",
                    ToolTipText = "Burst Skill",
                    Offset = new Vector2(0, 0)
                };
                skillIconPanel.AddChild(burstSkillIcon);
                burstSkillIcon.AddChild(new Label($"", Anchor.BottomRight, offset: new Vector2(-22, -35))
                {
                    Identifier = "burstSkillLevel",
                    Size = new Vector2(10, 10),
                    Scale = 1f,
                    ClickThrough = true,
                });
                var passiveSkillIcon = new Icon(IconType.None, Anchor.AutoInlineNoBreak, 1, true)
                {
                    Identifier = "passiveSkillIcon",
                    ToolTipText = "Passive Skill",
                    Offset = new Vector2(0, 0)
                };
                skillIconPanel.AddChild(passiveSkillIcon);
                passiveSkillIcon.AddChild(new Label($"", Anchor.BottomRight, offset: new Vector2(-22, -35))
                {
                    Identifier = "passiveSkillLevel",
                    Size = new Vector2(10, 10),
                    Scale = 1f,
                    ClickThrough = true,
                });

                normalSkillIcon.OnClick = (Entity entity) =>
                {
                    entity.Enabled = false;
                    burstSkillIcon.Enabled = true;
                    passiveSkillIcon.Enabled = true;

                    NoahSelectedSkill = "I've got the Scent!";

                    var descNormal = GameGlobals.Instance.SkillDescriptionDatas.Where
                        (s => s.Name.Equals(NoahSelectedSkill)).ToList();
                    var levelNormal = PlayerManager.Instance.Player.PlayerData.Abilities.NormalSkillLevel;

                    // Refesh skill description
                    RefreshSkillDescription("Character", descNormal, levelNormal);
                };

                burstSkillIcon.OnClick = (Entity entity) =>
                {
                    entity.Enabled = false;
                    normalSkillIcon.Enabled = true;
                    passiveSkillIcon.Enabled = true;

                    NoahSelectedSkill = "Noah Strike";

                    var descBurst = GameGlobals.Instance.SkillDescriptionDatas.Where
                        (s => s.Name.Equals(NoahSelectedSkill)).ToList();
                    var levelBurst = PlayerManager.Instance.Player.PlayerData.Abilities.BurstSkillLevel;

                    // Refesh skill description
                    RefreshSkillDescription("Character", descBurst, levelBurst);
                };

                passiveSkillIcon.OnClick = (Entity entity) =>
                {
                    entity.Enabled = false;
                    normalSkillIcon.Enabled = true;
                    burstSkillIcon.Enabled = true;

                    NoahSelectedSkill = "Survivalist";

                    var descPassive = GameGlobals.Instance.SkillDescriptionDatas.Where
                        (s => s.Name.Equals(NoahSelectedSkill)).ToList();
                    var levelPassive = PlayerManager.Instance.Player.PlayerData.Abilities.PassiveSkillLevel;

                    // Refesh skill description
                    RefreshSkillDescription("Character", descPassive, levelPassive);
                };

                upSkillButton.OnClick = (Entity entity) =>
                {
                    IsShowConfirmBox = true;
                    int skillLevel = 1;

                    switch (NoahSelectedSkill)
                    {
                        case "I've got the Scent!":
                            skillLevel = PlayerManager.Instance.Player.PlayerData.Abilities.NormalSkillLevel;
                            break;

                        case "Noah Strike":
                            skillLevel = PlayerManager.Instance.Player.PlayerData.Abilities.BurstSkillLevel;
                            break;

                        case "Survivalist":
                            skillLevel = PlayerManager.Instance.Player.PlayerData.Abilities.PassiveSkillLevel;
                            break;
                    }

                    var skillData = GameGlobals.Instance.SkillDescriptionDatas.FirstOrDefault
                        (s => s.Name.Equals(NoahSelectedSkill) && s.Level.Equals(skillLevel));

                    if (PlayerManager.Instance.Player.PlayerData.SkillPoint >= skillData.SkillPointCost
                        && InventoryManager.Instance.GoldCoin >= skillData.GoldCoinCost)
                    {
                        GeonBit.UI.Utils.MessageBox.ShowMsgBox("Up Skill Level"
                            , $"Do you want to upgrade '{NoahSelectedSkill}'?"
                            , new GeonBit.UI.Utils.MessageBox.MsgBoxOption[]
                            {
                                new("Ok", () =>
                                {
                                    // Do up level skill
                                    var isUpLevelSucc = PlayerManager.Instance.UpSkillLevel(NoahSelectedSkill);

                                    if (isUpLevelSucc)
                                    {
                                        GeonBit.UI.Utils.MessageBox.ShowMsgBox("LEVEL UP!!", $"'{NoahSelectedSkill}' was successfully up level."
                                            , onDone: () =>
                                            {                                                                                       
                                                // Refesh skill description
                                                var descSkill = GameGlobals.Instance.SkillDescriptionDatas.Where
                                                    (s => s.Name.Equals(NoahSelectedSkill)).ToList();
                                                int level = 1;

                                                if (normalSkillIcon.Enabled == false)
                                                {
                                                    level = PlayerManager.Instance.Player.PlayerData.Abilities.NormalSkillLevel;
                                                }
                                                else if (burstSkillIcon.Enabled == false)
                                                {
                                                    level = PlayerManager.Instance.Player.PlayerData.Abilities.BurstSkillLevel;
                                                }
                                                else if (passiveSkillIcon.Enabled == false)
                                                {
                                                    level = PlayerManager.Instance.Player.PlayerData.Abilities.PassiveSkillLevel;
                                                }

                                                RefreshSkillDescription("Character", descSkill, level);
                                                
                                                // Set Display Skill point
                                                var skillPoint = skillPointPanel.Children.OfType<Label>().FirstOrDefault
                                                    (l => l.Identifier.Equals("skillPoint"));
                                                skillPoint.Text = "SP: " + PlayerManager.Instance.Player.PlayerData.SkillPoint;

                                                // Set Skill Level
                                                // Normal Skill icon
                                                normalSkillIcon.Count = PlayerManager.Instance.Player.PlayerData.Abilities.NormalSkillLevel;
                                                var normalSkillLevel = normalSkillIcon.Children.OfType<Label>().FirstOrDefault
                                                            (e => e.Identifier.Equals("normalSkillLevel"));
                                                normalSkillLevel.Text = $"+{normalSkillIcon.Count}";

                                                // Burst Skill Icon
                                                burstSkillIcon.Count = PlayerManager.Instance.Player.PlayerData.Abilities.BurstSkillLevel;
                                                var burstSkillLevel = burstSkillIcon.Children.OfType<Label>().FirstOrDefault
                                                            (e => e.Identifier.Equals("burstSkillLevel"));
                                                burstSkillLevel.Text = $"+{burstSkillIcon.Count}";

                                                // Passive Skill Icon
                                                passiveSkillIcon.Count = PlayerManager.Instance.Player.PlayerData.Abilities.PassiveSkillLevel;
                                                var passiveSkillLevel = passiveSkillIcon.Children.OfType<Label>().FirstOrDefault
                                                            (e => e.Identifier.Equals("passiveSkillLevel"));
                                                passiveSkillLevel.Text = $"+{passiveSkillIcon.Count}";

                                                IsShowConfirmBox = false;
                                            });
                                    }
                                    else
                                    {
                                        int capLevel = 0;
                                        switch (NoahSelectedSkill)
                                        {
                                            case "I've got the Scent!":
                                                capLevel = PlayerManager.Instance.Player.PlayerData.Abilities.NormalSkillLevel == 3? 11 : 21;
                                                break;

                                            case "Noah Strike":
                                                capLevel = PlayerManager.Instance.Player.PlayerData.Abilities.BurstSkillLevel == 3? 11 : 21;
                                                break;

                                            case "Survivalist":
                                                capLevel = PlayerManager.Instance.Player.PlayerData.Abilities.PassiveSkillLevel == 3? 11 : 21;
                                                break;
                                        }

                                        GeonBit.UI.Utils.MessageBox.ShowMsgBox("ERROR!!", $"Character need to reach level {capLevel} to level up skills."
                                            , onDone: () => 
                                            {
                                                IsShowConfirmBox = false;                                                      
                                            });
                                    }

                                    return true;
                                }),
                                new("Cancel", () =>
                                {
                                    IsShowConfirmBox = false;
                                    return true;
                                })
                            });
                    }
                    else
                    {
                        var insuffText = string.Empty;
                        if (PlayerManager.Instance.Player.PlayerData.SkillPoint < skillData.SkillPointCost)
                        {
                            insuffText = "It seems that ya don't have enough 'Skill Point' for this huhh?, go get some level up.";
                        }
                        else if (InventoryManager.Instance.GoldCoin < skillData.GoldCoinCost)
                        {
                            insuffText = "It seems that ya don't have enough 'Gold Coin' for this huhh?, go get some gold.";
                        }

                        GeonBit.UI.Utils.MessageBox.ShowMsgBox(
                            "Insufficient Resource!",
                            insuffText,
                            onDone: () => { IsShowConfirmBox = false; } );
                    }
                };

                // Help Button
                var helpButton = new Button("", ButtonSkin.Default)
                {
                    Identifier = "helpButton",
                    Locked = false,
                    Anchor = Anchor.BottomLeft,
                    Size = new Vector2(50, 50),
                    OnClick = (btn) =>
                    {
                        IsShowConfirmBox = true;

                        GeonBit.UI.Utils.MessageBox.ShowMsgBox(
                            "Upgrading Skill",
                            "Upgrading skills uses resources such as Gold Coin and SP.\nCompanion characters will also receive skill upgrades at the same time.",
                            onDone: () => { IsShowConfirmBox = false; });
                    }
                };
                helpButton.ButtonParagraph.SetAnchorAndOffset(Anchor.AutoCenter, new Vector2(0, -35));
                helpButton.AddChild(new Icon(IconType.None, Anchor.AutoCenter)
                {
                    Texture = GetGuiTexture(GuiTextureName.help),
                    ClickThrough = true
                }, true);
                displayCharacterPanel.AddChild(helpButton);

                // Right Panel
                var rightPanel = new Panel(new Vector2(375, 600), PanelSkin.None, Anchor.TopRight)
                {
                    Identifier = "rightPanel"
                };
                characterTab.panel.AddChild(rightPanel);

                var statsPanel = new Panel(new Vector2(350, 400), PanelSkin.ListBackground, Anchor.TopCenter)
                {
                    Identifier = "statsPanel"
                };
                rightPanel.AddChild(statsPanel);

                statsPanel.AddChild(new Button("Level", ButtonSkin.Fancy, size: new Vector2(325, 30))
                {
                    Identifier = "levelHeader",
                    Locked = true,
                    Anchor = Anchor.TopCenter
                });

                // Player Level
                statsPanel.AddChild(new Label()
                {
                    Identifier = "playerLevel",
                    Scale = 1.5f,
                    Anchor = Anchor.AutoCenter
                });

                statsPanel.AddChild(new Button("Attributes", ButtonSkin.Fancy, size: new Vector2(325, 30))
                {
                    Identifier = "attributesHeader",
                    Locked = true,
                    Anchor = Anchor.AutoCenter
                });

                // ATK
                var statsATKPanel = new Panel(new Vector2(300, 15), PanelSkin.None, Anchor.AutoCenter)
                {
                    Identifier = "statsATKPanel",
                };
                statsPanel.AddChild(statsATKPanel);
                statsATKPanel.AddChild(new Label()
                {
                    Text = "ATK:",
                    Scale = 0.9f,
                    Anchor = Anchor.CenterLeft
                });
                statsATKPanel.AddChild(new Label()
                {
                    Identifier = "statsATK",
                    Scale = 0.9f,
                    Anchor = Anchor.CenterRight
                });
                
                // HP
                var statsHPPanel = new Panel(new Vector2(300, 15), PanelSkin.None, Anchor.AutoCenter)
                {
                    Identifier = "statsHPPanel",
                };
                statsPanel.AddChild(statsHPPanel);
                statsHPPanel.AddChild(new Label()
                {
                    Text = "HP:",
                    Scale = 0.9f,
                    Anchor = Anchor.CenterLeft
                });
                statsHPPanel.AddChild(new Label()
                {
                    Identifier = "statsHP",
                    Scale = 0.9f,
                    Anchor = Anchor.CenterRight
                });

                // Mana
                var statsManaPanel = new Panel(new Vector2(300, 15), PanelSkin.None, Anchor.AutoCenter)
                {
                    Identifier = "statsManaPanel",
                };
                statsPanel.AddChild(statsManaPanel);
                statsManaPanel.AddChild(new Label()
                {
                    Text = "Mana:",
                    Scale = 0.9f,
                    Anchor = Anchor.CenterLeft
                });
                statsManaPanel.AddChild(new Label()
                {
                    Identifier = "statsMana",
                    Scale = 0.9f,
                    Anchor = Anchor.CenterRight
                });

                // ManaRegen
                var statsManaRegenPanel = new Panel(new Vector2(300, 15), PanelSkin.None, Anchor.AutoCenter)
                {
                    Identifier = "statsManaRegenPanel",
                };
                statsPanel.AddChild(statsManaRegenPanel);
                statsManaRegenPanel.AddChild(new Label()
                {
                    Text = "Mana Regen:",
                    Scale = 0.9f,
                    Anchor = Anchor.CenterLeft
                });
                statsManaRegenPanel.AddChild(new Label()
                {
                    Identifier = "statsManaRegen",
                    Scale = 0.9f,
                    Anchor = Anchor.CenterRight
                });

                // DEF
                var statsDEFPanel = new Panel(new Vector2(300, 15), PanelSkin.None, Anchor.AutoCenter)
                {
                    Identifier = "statsDEFPanel",
                };
                statsPanel.AddChild(statsDEFPanel);
                statsDEFPanel.AddChild(new Label()
                {
                    Text = "DEF:",
                    Scale = 0.9f,
                    Anchor = Anchor.CenterLeft
                });
                statsDEFPanel.AddChild(new Label()
                {
                    Identifier = "statsDEF",
                    Scale = 0.9f,
                    Anchor = Anchor.CenterRight
                });

                // Crit
                var statsCritPanel = new Panel(new Vector2(300, 15), PanelSkin.None, Anchor.AutoCenter)
                {
                    Identifier = "statsCritPanel",
                };
                statsPanel.AddChild(statsCritPanel);
                statsCritPanel.AddChild(new Label()
                {
                    Text = "Crit:",
                    Scale = 0.9f,
                    Anchor = Anchor.CenterLeft
                });
                statsCritPanel.AddChild(new Label()
                {
                    Identifier = "statsCrit",
                    Scale = 0.9f,
                    Anchor = Anchor.CenterRight
                });

                // CritDMG
                var statsCritDMGPanel = new Panel(new Vector2(300, 15), PanelSkin.None, Anchor.AutoCenter)
                {
                    Identifier = "statsCritDMGPanel",
                };
                statsPanel.AddChild(statsCritDMGPanel);
                statsCritDMGPanel.AddChild(new Label()
                {
                    Text = "CritDMG:",
                    Scale = 0.9f,
                    Anchor = Anchor.CenterLeft
                });
                statsCritDMGPanel.AddChild(new Label()
                {
                    Identifier = "statsCritDMG",
                    Scale = 0.9f,
                    Anchor = Anchor.CenterRight
                });

                // Evasion
                var statsEvasionPanel = new Panel(new Vector2(300, 15), PanelSkin.None, Anchor.AutoCenter)
                {
                    Identifier = "statsEvasionPanel",
                };
                statsPanel.AddChild(statsEvasionPanel);
                statsEvasionPanel.AddChild(new Label()
                {
                    Text = "Evasion:",
                    Scale = 0.9f,
                    Anchor = Anchor.CenterLeft
                });
                statsEvasionPanel.AddChild(new Label()
                {
                    Identifier = "statsEvasion",
                    Scale = 0.9f,
                    Anchor = Anchor.CenterRight
                });

                // Speed
                var statsSpeedPanel = new Panel(new Vector2(300, 15), PanelSkin.None, Anchor.AutoCenter)
                {
                    Identifier = "statsSpeedPanel",
                };
                statsPanel.AddChild(statsSpeedPanel);
                statsSpeedPanel.AddChild(new Label()
                {
                    Text = "Movement Speed:",
                    Scale = 0.9f,
                    Anchor = Anchor.CenterLeft
                });
                statsSpeedPanel.AddChild(new Label()
                {
                    Identifier = "statsSpeed",
                    Scale = 0.9f,
                    Anchor = Anchor.CenterRight
                });
            }

            // Add tab: Companion Inspect
            {
                TabData companionTab = inspectTabs.AddTab("Companion");

                // Set true if character tab is clicked
                companionTab.button.OnClick = (Entity entity) =>
                {
                    RefreshInspectCompanionDisplay();
                    ClearSkillDescription("Companion");

                    IsCharacterTabSelected = true;
                };

                // Left Panel
                var leftPanel = new Panel(new Vector2(450, 670), PanelSkin.None, Anchor.TopLeft)
                {
                    Identifier = "leftPanel"
                };
                companionTab.panel.AddChild(leftPanel);

                // Display Character
                var displayCharacterPanel = new Panel(new Vector2(425, 400), PanelSkin.ListBackground, Anchor.TopCenter)
                {
                    Identifier = "displayCharacterPanel"
                };
                leftPanel.AddChild(displayCharacterPanel);

                displayCharacterPanel.AddChild(new Header()
                {
                    Identifier = "characterNameHeader",
                    Scale = 1.5f,
                    Anchor = Anchor.TopCenter
                });

                var prevCompanion = new Button("", ButtonSkin.Default)
                {
                    Identifier = "prevCompanion",
                    Anchor = Anchor.TopLeft,
                    Size = new Vector2(50, 50),
                    OnClick = (btn) =>
                    {
                        UpdateSelectedCompanion(false);
                    }
                };
                displayCharacterPanel.AddChild(prevCompanion);
                prevCompanion.ButtonParagraph.SetAnchorAndOffset(Anchor.AutoCenter, new Vector2(0, -35));
                prevCompanion.AddChild(new Icon(IconType.None, Anchor.AutoCenter)
                {
                    Texture = GetGuiTexture(GuiTextureName.arrow_left)
                }, true);

                var nextCompanion = new Button("", ButtonSkin.Default)
                {
                    Identifier = "nextCompanion",
                    Anchor = Anchor.TopRight,
                    Size = new Vector2(50, 50),
                    OnClick = (btn) =>
                    {
                        UpdateSelectedCompanion(true);
                    }
                };
                displayCharacterPanel.AddChild(nextCompanion);
                nextCompanion.ButtonParagraph.SetAnchorAndOffset(Anchor.AutoCenter, new Vector2(0, -35));
                nextCompanion.AddChild(new Icon(IconType.None, Anchor.AutoCenter)
                {
                    Texture = GetGuiTexture(GuiTextureName.arrow_right)
                }, true);

                //// Left Equipment Panel
                //var leftEquipmentPanel = new Panel(new Vector2(70, 210), PanelSkin.None, Anchor.CenterLeft)
                //{
                //    Identifier = "leftEquipmentPanel",
                //};
                //displayCharacterPanel.AddChild(leftEquipmentPanel);

                //// Add Slot Equipment in Left Panel
                //for (int i = 0; i < 3; i++)
                //{
                //    leftEquipmentPanel.AddChild(new Icon(IconType.None, Anchor.AutoInline, 1, true)
                //    {
                //        Identifier = "Slot_" + i,
                //        Offset = new Vector2(-16, 0)
                //    });
                //}

                //// Right Equipment Panel
                //var rightEquipmentPanel = new Panel(new Vector2(70, 210), PanelSkin.None, Anchor.CenterRight)
                //{
                //    Identifier = "rightEquipmentPanel"
                //};
                //displayCharacterPanel.AddChild(rightEquipmentPanel);

                //// Add Slot Equipment in Right Panel
                //for (int i = 3; i < 6; i++)
                //{
                //    rightEquipmentPanel.AddChild(new Icon(IconType.None, Anchor.AutoInline, 1, true)
                //    {
                //        Identifier = "Slot_" + i,
                //        Offset = new Vector2(-16, 0)
                //    });
                //}

                // Skill Icon Panel
                var skillIconPanel = new Panel(new Vector2(220, 60), PanelSkin.None, Anchor.BottomCenter)
                {
                    Identifier = "skillIconPanel"
                };
                displayCharacterPanel.AddChild(skillIconPanel);

                var normalSkillIcon = new Icon(IconType.None, Anchor.AutoInlineNoBreak, 1, true)
                {
                    Identifier = "normalSkillIcon",
                    ToolTipText = "Normal Skill",
                    Offset = new Vector2(0, 0)
                };
                skillIconPanel.AddChild(normalSkillIcon);
                normalSkillIcon.AddChild(new Label($"", Anchor.BottomRight, offset: new Vector2(-22, -35))
                {
                    Identifier = "normalSkillLevel",
                    Size = new Vector2(10, 10),
                    Scale = 1f,
                    ClickThrough = true,
                });
                var burstSkillIcon = new Icon(IconType.None, Anchor.AutoInlineNoBreak, 1, true)
                {
                    Identifier = "burstSkillIcon",
                    ToolTipText = "Burst Skill",
                    Offset = new Vector2(0, 0)
                };
                skillIconPanel.AddChild(burstSkillIcon);
                burstSkillIcon.AddChild(new Label($"", Anchor.BottomRight, offset: new Vector2(-22, -35))
                {
                    Identifier = "burstSkillLevel",
                    Size = new Vector2(10, 10),
                    Scale = 1f,
                    ClickThrough = true,
                });
                var passiveSkillIcon = new Icon(IconType.None, Anchor.AutoInlineNoBreak, 1, true)
                {
                    Identifier = "passiveSkillIcon",
                    ToolTipText = "Passive Skill",
                    Offset = new Vector2(0, 0)
                };
                skillIconPanel.AddChild(passiveSkillIcon);
                passiveSkillIcon.AddChild(new Label($"", Anchor.BottomRight, offset: new Vector2(-22, -35))
                {
                    Identifier = "passiveSkillLevel",
                    Size = new Vector2(10, 10),
                    Scale = 1f,
                    ClickThrough = true,
                });

                normalSkillIcon.OnClick = (Entity entity) =>
                {
                    entity.Enabled = false;
                    burstSkillIcon.Enabled = true;
                    passiveSkillIcon.Enabled = true;

                    // Get Companion Selected Skill
                    SetCompanionSelectedSkill("normal");

                    var descNormal = GameGlobals.Instance.SkillDescriptionDatas.Where
                        (s => s.Name.Equals(CompanionSelectedSkill)).ToList();
                    var levelNormal = PlayerManager.Instance.Player.PlayerData.Abilities.NormalSkillLevel;

                    // Refesh skill description
                    RefreshSkillDescription("Companion", descNormal, levelNormal);
                };

                burstSkillIcon.OnClick = (Entity entity) =>
                {
                    entity.Enabled = false;
                    normalSkillIcon.Enabled = true;
                    passiveSkillIcon.Enabled = true;

                    SetCompanionSelectedSkill("burst");

                    var descBurst = GameGlobals.Instance.SkillDescriptionDatas.Where
                        (s => s.Name.Equals(CompanionSelectedSkill)).ToList();
                    var levelBurst = PlayerManager.Instance.Player.PlayerData.Abilities.BurstSkillLevel;

                    // Refesh skill description
                    RefreshSkillDescription("Companion", descBurst, levelBurst);
                };

                passiveSkillIcon.OnClick = (Entity entity) =>
                {
                    entity.Enabled = false;
                    normalSkillIcon.Enabled = true;
                    burstSkillIcon.Enabled = true;

                    SetCompanionSelectedSkill("passive");

                    var descPassive = GameGlobals.Instance.SkillDescriptionDatas.Where
                        (s => s.Name.Equals(CompanionSelectedSkill)).ToList();
                    var levelPassive = PlayerManager.Instance.Player.PlayerData.Abilities.PassiveSkillLevel;

                    // Refesh skill description
                    RefreshSkillDescription("Companion", descPassive, levelPassive);
                };

                leftPanel.AddChild(new LineSpace(1));
                var selectedSkillPanel = new Panel(new Vector2(425, 210), PanelSkin.ListBackground, Anchor.AutoCenter)
                {
                    Identifier = "selectedSkillPanel",
                    Offset = new Vector2(0, 0f)
                };
                leftPanel.AddChild(selectedSkillPanel);

                var descripLeftSkill = new Paragraph(@"", Anchor.CenterLeft, Color.White, 0.9f, new Vector2(360, 250))
                {
                    Identifier = "descripLeftSkill",
                    WrapWords = true,
                    Padding = new Vector2(15, 0)
                };
                selectedSkillPanel.AddChild(descripLeftSkill);
            
                // Right Panel
                var rightPanel = new Panel(new Vector2(375, 670), PanelSkin.None, Anchor.TopRight)
                {
                    Identifier = "rightPanel"
                };
                companionTab.panel.AddChild(rightPanel);

                // Stats Panel
                var statsPanel = new Panel(new Vector2(350, 400), PanelSkin.ListBackground, Anchor.TopCenter)
                {
                    Identifier = "statsPanel"
                };
                rightPanel.AddChild(statsPanel);

                // List Consumable Item
                rightPanel.AddChild(new LineSpace(1));
                var consumableItemListPanel = new Panel(new Vector2(350, 210), PanelSkin.ListBackground, Anchor.AutoCenter)
                {
                    PanelOverflowBehavior = PanelOverflowBehavior.VerticalScroll,
                    Identifier = "consumableItemListPanel",
                    Padding = new Vector2(5, 5)
                };
                consumableItemListPanel.Scrollbar.AdjustMaxAutomatically = true;
                consumableItemListPanel.OnMouseEnter = (e) =>
                {
                    UserInterface.Active.ActiveEntity = consumableItemListPanel;
                };
                rightPanel.AddChild(consumableItemListPanel);

                statsPanel.AddChild(new Button("Level", ButtonSkin.Fancy, size: new Vector2(325, 30))
                {
                    Identifier = "levelHeader",
                    Locked = true,
                    Anchor = Anchor.TopCenter
                });

                // Companion Level
                statsPanel.AddChild(new Label()
                {
                    Identifier = "companionLevel",
                    Scale = 1.5f,
                    Anchor = Anchor.AutoCenter
                });

                statsPanel.AddChild(new Button("Attributes", ButtonSkin.Fancy, size: new Vector2(325, 30))
                {
                    Identifier = "attributesHeader",
                    Locked = true,
                    Anchor = Anchor.AutoCenter
                });

                // ATK
                var statsATKPanel = new Panel(new Vector2(300, 15), PanelSkin.None, Anchor.AutoCenter)
                {
                    Identifier = "statsATKPanel",
                };
                statsPanel.AddChild(statsATKPanel);
                statsATKPanel.AddChild(new Label()
                {
                    Text = "ATK:",
                    Scale = 0.9f,
                    Anchor = Anchor.CenterLeft
                });
                statsATKPanel.AddChild(new Label()
                {
                    Identifier = "statsATK",
                    Scale = 0.9f,
                    Anchor = Anchor.CenterRight
                });

                // HP
                var statsHPPanel = new Panel(new Vector2(300, 15), PanelSkin.None, Anchor.AutoCenter)
                {
                    Identifier = "statsHPPanel",
                };
                statsPanel.AddChild(statsHPPanel);
                statsHPPanel.AddChild(new Label()
                {
                    Text = "HP:",
                    Scale = 0.9f,
                    Anchor = Anchor.CenterLeft
                });
                statsHPPanel.AddChild(new Label()
                {
                    Identifier = "statsHP",
                    Scale = 0.9f,
                    Anchor = Anchor.CenterRight
                });

                // Mana
                var statsManaPanel = new Panel(new Vector2(300, 15), PanelSkin.None, Anchor.AutoCenter)
                {
                    Identifier = "statsManaPanel",
                };
                statsPanel.AddChild(statsManaPanel);
                statsManaPanel.AddChild(new Label()
                {
                    Text = "Mana:",
                    Scale = 0.9f,
                    Anchor = Anchor.CenterLeft
                });
                statsManaPanel.AddChild(new Label()
                {
                    Identifier = "statsMana",
                    Scale = 0.9f,
                    Anchor = Anchor.CenterRight
                });

                // ManaRegen
                var statsManaRegenPanel = new Panel(new Vector2(300, 15), PanelSkin.None, Anchor.AutoCenter)
                {
                    Identifier = "statsManaRegenPanel",
                };
                statsPanel.AddChild(statsManaRegenPanel);
                statsManaRegenPanel.AddChild(new Label()
                {
                    Text = "Mana Regen:",
                    Scale = 0.9f,
                    Anchor = Anchor.CenterLeft
                });
                statsManaRegenPanel.AddChild(new Label()
                {
                    Identifier = "statsManaRegen",
                    Scale = 0.9f,
                    Anchor = Anchor.CenterRight
                });

                // DEF
                var statsDEFPanel = new Panel(new Vector2(300, 15), PanelSkin.None, Anchor.AutoCenter)
                {
                    Identifier = "statsDEFPanel",
                };
                statsPanel.AddChild(statsDEFPanel);
                statsDEFPanel.AddChild(new Label()
                {
                    Text = "DEF:",
                    Scale = 0.9f,
                    Anchor = Anchor.CenterLeft
                });
                statsDEFPanel.AddChild(new Label()
                {
                    Identifier = "statsDEF",
                    Scale = 0.9f,
                    Anchor = Anchor.CenterRight
                });

                // Crit
                var statsCritPanel = new Panel(new Vector2(300, 15), PanelSkin.None, Anchor.AutoCenter)
                {
                    Identifier = "statsCritPanel",
                };
                statsPanel.AddChild(statsCritPanel);
                statsCritPanel.AddChild(new Label()
                {
                    Text = "Crit:",
                    Scale = 0.9f,
                    Anchor = Anchor.CenterLeft
                });
                statsCritPanel.AddChild(new Label()
                {
                    Identifier = "statsCrit",
                    Scale = 0.9f,
                    Anchor = Anchor.CenterRight
                });

                // CritDMG
                var statsCritDMGPanel = new Panel(new Vector2(300, 15), PanelSkin.None, Anchor.AutoCenter)
                {
                    Identifier = "statsCritDMGPanel",
                };
                statsPanel.AddChild(statsCritDMGPanel);
                statsCritDMGPanel.AddChild(new Label()
                {
                    Text = "CritDMG:",
                    Scale = 0.9f,
                    Anchor = Anchor.CenterLeft
                });
                statsCritDMGPanel.AddChild(new Label()
                {
                    Identifier = "statsCritDMG",
                    Scale = 0.9f,
                    Anchor = Anchor.CenterRight
                });

                // Evasion
                var statsEvasionPanel = new Panel(new Vector2(300, 15), PanelSkin.None, Anchor.AutoCenter)
                {
                    Identifier = "statsEvasionPanel",
                };
                statsPanel.AddChild(statsEvasionPanel);
                statsEvasionPanel.AddChild(new Label()
                {
                    Text = "Evasion:",
                    Scale = 0.9f,
                    Anchor = Anchor.CenterLeft
                });
                statsEvasionPanel.AddChild(new Label()
                {
                    Identifier = "statsEvasion",
                    Scale = 0.9f,
                    Anchor = Anchor.CenterRight
                });

                // Speed
                var statsSpeedPanel = new Panel(new Vector2(300, 15), PanelSkin.None, Anchor.AutoCenter)
                {
                    Identifier = "statsSpeedPanel",
                };
                statsPanel.AddChild(statsSpeedPanel);
                statsSpeedPanel.AddChild(new Label()
                {
                    Text = "Movement Speed:",
                    Scale = 0.9f,
                    Anchor = Anchor.CenterLeft
                });
                statsSpeedPanel.AddChild(new Label()
                {
                    Identifier = "statsSpeed",
                    Scale = 0.9f,
                    Anchor = Anchor.CenterRight
                });
            }

            // Add tab: Progression
            {
                TabData progressionTab = inspectTabs.AddTab("Progression");

                progressionTab.button.OnClick = (Entity entity) =>
                {
                    RefreshMedicineProgresstion();

                    IsCharacterTabSelected = false;
                };

                progressionTab.panel.AddChild(new Header("Thai Traditional Medicine")
                {
                    Anchor = Anchor.TopCenter
                });
                progressionTab.panel.AddChild(new LineSpace(1) { Anchor = Anchor.AutoCenter });

                // left Panel
                var leftPanel = new Panel(new Vector2(625, 600), PanelSkin.ListBackground, Anchor.BottomLeft)
                {
                    PanelOverflowBehavior = PanelOverflowBehavior.VerticalScroll,
                    Identifier = "leftPanel"
                };
                leftPanel.Scrollbar.AdjustMaxAutomatically = true;
                leftPanel.Scrollbar.Opacity = 0;
                leftPanel.OnMouseEnter = (e) =>
                {
                    UserInterface.Active.ActiveEntity = leftPanel;
                };
                progressionTab.panel.AddChild(leftPanel);

                leftPanel.AddChild(new Icon(IconType.None, Anchor.AutoCenter, 1, false)
                {
                    Size = new Vector2(300, 300),   // ปรับขนาดของไอคอน
                    Locked = true,
                    Identifier = "itemIcon"
                });
                leftPanel.AddChild(new LineSpace(1));

                // Thai medicine name
                leftPanel.AddChild(new Label("Name", Anchor.AutoCenter)
                {
                    Scale = 1.3f,
                    Identifier = "Name"
                });
                leftPanel.AddChild(new HorizontalLine());

                // Group
                leftPanel.AddChild(new Label("กลุ่มยา", Anchor.AutoInline) { Scale = 1.1f });
                leftPanel.AddChild(new Paragraph("Group", Anchor.AutoInline)
                {
                    Scale = 1f,
                    WrapWords = true,
                    Identifier = "Group"
                });
                leftPanel.AddChild(new LineSpace(1));

                // Characteristics
                leftPanel.AddChild(new Label("ลักษณะยา", Anchor.AutoInline) { Scale = 1.1f });
                leftPanel.AddChild(new Paragraph("Characteristics", Anchor.AutoInline)
                {
                    Scale = 1f,
                    WrapWords = true,
                    Identifier = "Characteristics"
                });
                leftPanel.AddChild(new LineSpace(1));

                // Recipe
                leftPanel.AddChild(new Label("สูตรตำรับ", Anchor.AutoInline) { Scale = 1.1f });
                leftPanel.AddChild(new Paragraph("Recipe", Anchor.AutoInline)
                {
                    Scale = 1f,
                    WrapWords = true,
                    Identifier = "Recipe"
                });
                leftPanel.AddChild(new LineSpace(1));

                // Instructions
                leftPanel.AddChild(new Label("คำแนะนำ", Anchor.AutoInline) { Scale = 1.1f });
                leftPanel.AddChild(new Paragraph("Instructions", Anchor.AutoInline)
                {
                    Scale = 1f,
                    WrapWords = true,
                    Identifier = "Instructions"
                });
                leftPanel.AddChild(new LineSpace(1));

                // HowtoUse
                leftPanel.AddChild(new Label("ขนาดและวิธีใช้", Anchor.AutoInline) { Scale = 1.1f });
                leftPanel.AddChild(new Paragraph("HowtoUse", Anchor.AutoInline)
                {
                    Scale = 1f,
                    WrapWords = true,
                    Identifier = "HowtoUse"
                });
                leftPanel.AddChild(new LineSpace(1));

                // Warning
                leftPanel.AddChild(new Label("คำเตือน", Anchor.AutoInline) { Scale = 1.1f });
                leftPanel.AddChild(new Paragraph("Warning", Anchor.AutoInline)
                {
                    Scale = 1f,
                    WrapWords = true,
                    Identifier = "Warning"
                });
                leftPanel.AddChild(new LineSpace(1));

                var listCraftableMedicine = new Panel(new Vector2(185, 600), PanelSkin.ListBackground, Anchor.BottomRight)
                {
                    PanelOverflowBehavior = PanelOverflowBehavior.VerticalScroll,
                    Identifier = "listCraftableMedicine",
                    Padding = new Vector2(18, 18)
                };
                listCraftableMedicine.Scrollbar.AdjustMaxAutomatically = true;
                progressionTab.panel.AddChild(listCraftableMedicine);
            }
        }

        public void RefreshInspectCompanionDisplay()
        {
            SelectedCompanion = PlayerManager.Instance.Companions[_selectedCompanionId];

            var inspectPanel = _mainPanels.ElementAt(InspectPanel);
            var inspectTabs = inspectPanel.Children.OfType<PanelTabs>().FirstOrDefault
                (t => t.Identifier.Equals("inspectTabs"));
            inspectTabs.SelectTab("Companion");

            var currentTabPanel = inspectTabs.ActiveTab.panel;
            var leftPanel = currentTabPanel.Children.FirstOrDefault
                (p => p.Identifier.Equals("leftPanel"));
            var displayCharacterPanel = leftPanel.Children.FirstOrDefault
                (p => p.Identifier.Equals("displayCharacterPanel"));

            // Set Main Character Name
            var characterNameHeader = displayCharacterPanel.Children.OfType<Header>().FirstOrDefault
                (h => h.Identifier.Equals("characterNameHeader"));
            characterNameHeader.Text = SelectedCompanion.Name;

            var skillIconPanel = displayCharacterPanel.Children.FirstOrDefault
                (p => p.Identifier.Equals("skillIconPanel"));

            // Normal Skill icon
            var normalSkillIcon = skillIconPanel.Children.OfType<Icon>().FirstOrDefault
                        (e => e.Identifier.Equals("normalSkillIcon"));
            normalSkillIcon.Enabled = true;
            normalSkillIcon.Count = SelectedCompanion.CompanionData.Abilities.NormalSkillLevel;
            var normalSkillLevel = normalSkillIcon.Children.OfType<Label>().FirstOrDefault
                        (e => e.Identifier.Equals("normalSkillLevel"));
            normalSkillLevel.Text = $"+{normalSkillIcon.Count}";

            // Burst Skill Icon
            var burstSkillIcon = skillIconPanel.Children.OfType<Icon>().FirstOrDefault
                        (e => e.Identifier.Equals("burstSkillIcon"));
            burstSkillIcon.Enabled = true;
            burstSkillIcon.Count = SelectedCompanion.CompanionData.Abilities.BurstSkillLevel;
            var burstSkillLevel = burstSkillIcon.Children.OfType<Label>().FirstOrDefault
                        (e => e.Identifier.Equals("burstSkillLevel"));
            burstSkillLevel.Text = $"+{burstSkillIcon.Count}";

            // Passive Skill Icon
            var passiveSkillIcon = skillIconPanel.Children.OfType<Icon>().FirstOrDefault
                        (e => e.Identifier.Equals("passiveSkillIcon"));
            passiveSkillIcon.Enabled = true;
            passiveSkillIcon.Count = SelectedCompanion.CompanionData.Abilities.PassiveSkillLevel;
            var passiveSkillLevel = passiveSkillIcon.Children.OfType<Label>().FirstOrDefault
                        (e => e.Identifier.Equals("passiveSkillLevel"));
            passiveSkillLevel.Text = $"+{passiveSkillIcon.Count}";

            // Check Companion for SkillIcon Texture
            SetCompanionSkillIconTextrue(SelectedCompanion.Name, normalSkillIcon, burstSkillIcon, passiveSkillIcon);

            var rightPanel = currentTabPanel.Children.FirstOrDefault
                (p => p.Identifier.Equals("rightPanel"));
            var statsPanel = rightPanel.Children.FirstOrDefault
                (p => p.Identifier.Equals("statsPanel"));

            // Player Level
            var companionLevel = statsPanel.Children.OfType<Label>().FirstOrDefault
                        (e => e.Identifier.Equals("companionLevel"));
            companionLevel.Text = SelectedCompanion.Level.ToString();

            // ATK
            var statsATKPanel = statsPanel.Children.FirstOrDefault(p => p.Identifier.Equals("statsATKPanel"));
            var statsATK = statsATKPanel.Children.OfType<Label>().FirstOrDefault
                        (e => e.Identifier.Equals("statsATK"));
            statsATK.Text = SelectedCompanion.ATK.ToString();

            // HP
            var statsHPPanel = statsPanel.Children.FirstOrDefault(p => p.Identifier.Equals("statsHPPanel"));
            var statsHP = statsHPPanel.Children.OfType<Label>().FirstOrDefault
                        (e => e.Identifier.Equals("statsHP"));
            statsHP.Text = SelectedCompanion.MaxHP.ToString();

            // Mana
            var statsManaPanel = statsPanel.Children.FirstOrDefault(p => p.Identifier.Equals("statsManaPanel"));
            var statsMana = statsManaPanel.Children.OfType<Label>().FirstOrDefault
                        (e => e.Identifier.Equals("statsMana"));
            statsMana.Text = ((int)SelectedCompanion.MaxMana).ToString();

            // ManaRegen
            var statsManaRegenPanel = statsPanel.Children.FirstOrDefault(p => p.Identifier.Equals("statsManaRegenPanel"));
            var statsManaRegen = statsManaRegenPanel.Children.OfType<Label>().FirstOrDefault
                        (e => e.Identifier.Equals("statsManaRegen"));
            statsManaRegen.Text = SelectedCompanion.ManaRegenRate.ToString("F2") + "/s";

            // DEF
            var statsDEFPanel = statsPanel.Children.FirstOrDefault(p => p.Identifier.Equals("statsDEFPanel"));
            var statsDEF = statsDEFPanel.Children.OfType<Label>().FirstOrDefault
                        (e => e.Identifier.Equals("statsDEF"));
            statsDEF.Text = (SelectedCompanion.DEF * 100f).ToString("F2") + "%";

            // Crit
            var statsCritPanel = statsPanel.Children.FirstOrDefault(p => p.Identifier.Equals("statsCritPanel"));
            var statsCrit = statsCritPanel.Children.OfType<Label>().FirstOrDefault
                        (e => e.Identifier.Equals("statsCrit"));
            statsCrit.Text = (SelectedCompanion.Crit * 100f).ToString("F2") + "%";

            // CritDMG
            var statsCritDMGPanel = statsPanel.Children.FirstOrDefault(p => p.Identifier.Equals("statsCritDMGPanel"));
            var statsCritDMG = statsCritDMGPanel.Children.OfType<Label>().FirstOrDefault
                        (e => e.Identifier.Equals("statsCritDMG"));
            statsCritDMG.Text = (SelectedCompanion.CritDMG * 100f).ToString("F2") + "%";

            // Evasion
            var statsEvasionPanel = statsPanel.Children.FirstOrDefault(p => p.Identifier.Equals("statsEvasionPanel"));
            var statsEvasion = statsEvasionPanel.Children.OfType<Label>().FirstOrDefault
                        (e => e.Identifier.Equals("statsEvasion"));
            statsEvasion.Text = (SelectedCompanion.Evasion * 100f).ToString("F2") + "%";

            // Speed
            var statsSpeedPanel = statsPanel.Children.FirstOrDefault(p => p.Identifier.Equals("statsSpeedPanel"));
            var statsSpeed = statsSpeedPanel.Children.OfType<Label>().FirstOrDefault
                        (e => e.Identifier.Equals("statsSpeed"));
            statsSpeed.Text = SelectedCompanion.Speed.ToString();

            // Consumable Item List
            var consumableItemListPanel = rightPanel.Children.FirstOrDefault
                (p => p.Identifier.Equals("consumableItemListPanel"));

            consumableItemListPanel.ClearChildren();

            if (SelectedCompanion.IsDead)
            {
                consumableItemListPanel.Enabled = false;
            }
            else consumableItemListPanel.Enabled = true;

            // Set item Display
            var tempIconItems = new List<Icon>();
            foreach (var item in InventoryManager.Instance.InventoryBag.Where
                (e => e.Value.GetCategory().Equals("Consumable item")))
            {
                // Item Icon
                var iconItem = new Icon(IconType.None, Anchor.AutoInline, 1, true)
                {
                    ItemId = item.Value.ItemId,
                    Count = item.Value.Count,
                    Slot = item.Value.Slot,
                    KeyIndex = item.Key,
                    Texture = GetItemTexture(item.Value.ItemId),
                };

                // Item text
                var text = iconItem.Count.ToString();

                var iconText = new Label(text, Anchor.BottomRight, offset: new Vector2(-22, -35))
                {
                    Size = new Vector2(10, 10),
                    Scale = 1f,
                    ClickThrough = true,
                };

                iconItem.AddChild(iconText);
                tempIconItems.Add(iconItem);
            }

            // Sort Item Display by Id and add to listItemPanel
            var sortedIconItems = tempIconItems.OrderBy(icon => icon.ItemId).ToList();
            foreach (var sortediconItem in sortedIconItems)
                consumableItemListPanel.AddChild(sortediconItem);

            // Set OnClick
            foreach (var iconItem in consumableItemListPanel.Children.OfType<Icon>())
            {
                iconItem.OnClick = (Entity entity) =>
                {
                    IsShowConfirmBox = true;
                    // Enable or Disable the button for each item icon
                    var item = InventoryManager.Instance.InventoryBag.FirstOrDefault
                        (i => i.Key.Equals(iconItem.KeyIndex));

                    if (IsUsableItem(iconItem.ItemId))
                    {
                        GeonBit.UI.Utils.MessageBox.ShowMsgBox("Use Item?"
                        , $"Do you wanna use '{item.Value.GetName()}' on {SelectedCompanion.Name}?"
                        , new GeonBit.UI.Utils.MessageBox.MsgBoxOption[]
                        {
                                new("Ok", () =>
                                {
                                     InventoryManager.Instance.UseItem(
                                         SelectedCompanion,
                                         item.Key,
                                         item.Value);

                                    // refresh display item after selectedItem has been use
                                    RefreshInspectCompanionDisplay();
                                    IsShowConfirmBox = false;
                                    return true;
                                }),
                                new("Cancel", () =>
                                {
                                    IsShowConfirmBox = false;
                                    return true;
                                })
                        });
                    }
                };
            }
        }

        public void RefreshMedicineProgresstion()
        {
            var inspectPanel = _mainPanels.ElementAt(InspectPanel);
            var inspectTabs = inspectPanel.Children.OfType<PanelTabs>().FirstOrDefault
                (t => t.Identifier.Equals("inspectTabs"));
            inspectTabs.SelectTab("Progression");

            var currentTabPanel = inspectTabs.ActiveTab.panel;
            var leftPanel = currentTabPanel.Children.FirstOrDefault(p => p.Identifier.Equals("leftPanel"));
            var listCraftableMedicine = currentTabPanel.Children.FirstOrDefault
                (p => p.Identifier.Equals("listCraftableMedicine"));

            // Clear craftable item display
            listCraftableMedicine.ClearChildren();

            List<CraftableItem> craftableItemList = CraftingManager.Instance.CraftableMedicineItem;

            // Set item
            foreach (var item in craftableItemList)
            {
                // Item Icon
                var iconItem = new Icon(IconType.None, Anchor.AutoInline, 1, true)
                {
                    Enabled = item.IsCraftable,
                    ItemId = item.ItemId,
                    Texture = GetItemTexture(item.ItemId),                  
                };

                listCraftableMedicine.AddChild(iconItem);

                iconItem.OnClick = (Entity entity) =>
                {
                    SetMedicineDescriptionDisplay(iconItem);
                };
            }
        }

        private void SetMedicineDescriptionDisplay(Icon icon)
        {
            var itemData = GameGlobals.Instance.MedicineDescriptionDatas.FirstOrDefault
                (i => i.ItemId.Equals(icon.ItemId));

            var inspectPanel = _mainPanels.ElementAt(InspectPanel);
            var inspectTabs = inspectPanel.Children.OfType<PanelTabs>().FirstOrDefault
                (t => t.Identifier.Equals("inspectTabs"));
            inspectTabs.SelectTab("Progression");
            var currentTabPanel = inspectTabs.ActiveTab.panel;
            var leftPanel = currentTabPanel.Children.FirstOrDefault(p => p.Identifier.Equals("leftPanel"));

            var itemIcon = leftPanel.Children.OfType<Icon>().FirstOrDefault
                (i => i.Identifier.Equals("itemIcon"));
            itemIcon.Texture = icon.Texture;

            // Thai medicine name
            var Name = leftPanel.Children.OfType<Label>().FirstOrDefault
                (i => i.Identifier.Equals("Name"));
            Name.Text = itemData.Name;

            // Group
            var Group = leftPanel.Children.OfType<Paragraph>().FirstOrDefault
                (i => i.Identifier.Equals("Group"));
            Group.Text = itemData.Group;

            // Characteristics
            var Characteristics = leftPanel.Children.OfType<Paragraph>().FirstOrDefault
                (i => i.Identifier.Equals("Characteristics"));
            Characteristics.Text = itemData.Characteristics;

            // Recipe
            var Recipe = leftPanel.Children.OfType<Paragraph>().FirstOrDefault
                (i => i.Identifier.Equals("Recipe"));
            Recipe.Text = itemData.Recipe;

            // Instructions
            var Instructions = leftPanel.Children.OfType<Paragraph>().FirstOrDefault
                (i => i.Identifier.Equals("Instructions"));
            Instructions.Text = itemData.Instructions;

            // HowtoUse
            var HowtoUse = leftPanel.Children.OfType<Paragraph>().FirstOrDefault
                (i => i.Identifier.Equals("HowtoUse"));
            HowtoUse.Text = itemData.HowtoUse;

            // Warning
            var Warning = leftPanel.Children.OfType<Paragraph>().FirstOrDefault
                (i => i.Identifier.Equals("Warning"));
            Warning.Text = itemData.Warning;
        }

        public void RefreshInspectCharacterDisplay()
        {
            CharacterType = "Player";
            CharacterSprite = new(GameGlobals.Instance.PlayerSpriteSheet);

            var inspectPanel = _mainPanels.ElementAt(InspectPanel);
            var inspectTabs = inspectPanel.Children.OfType<PanelTabs>().FirstOrDefault
                (t => t.Identifier.Equals("inspectTabs"));

            inspectTabs.SelectTab("Companion");
            if (PlayerManager.Instance.Companions.Count == 0)
            {          
                inspectTabs.ActiveTab.button.Enabled = false;
            }
            else inspectTabs.ActiveTab.button.Enabled = true;

            inspectTabs.SelectTab("Character");
            var currentTabPanel = inspectTabs.ActiveTab.panel;
            var leftPanel = currentTabPanel.Children.FirstOrDefault
                (p => p.Identifier.Equals("leftPanel"));
            var displayCharacterPanel = leftPanel.Children.FirstOrDefault
                (p => p.Identifier.Equals("displayCharacterPanel"));
            var skillPointPanel = displayCharacterPanel.Children.FirstOrDefault
                (p => p.Identifier.Equals("skillPointPanel"));
            var leftEquipmentPanel = displayCharacterPanel.Children.FirstOrDefault
                (p => p.Identifier.Equals("leftEquipmentPanel"));
            var rightEquipmentPanel = displayCharacterPanel.Children.FirstOrDefault
                (p => p.Identifier.Equals("rightEquipmentPanel"));          

            // Set Main Character Name
            var characterNameHeader = displayCharacterPanel.Children.OfType<Header>().FirstOrDefault
                (h => h.Identifier.Equals("characterNameHeader"));
            characterNameHeader.Text = PlayerManager.Instance.Player.Name;
            
            // Set Display Skill point
            var skillPoint = skillPointPanel.Children.OfType<Label>().FirstOrDefault
                (l => l.Identifier.Equals("skillPoint"));
            skillPoint.Text = "SP: " + PlayerManager.Instance.Player.PlayerData.SkillPoint;

            // Clear Equipment Slot
            leftEquipmentPanel.ClearChildren();
            rightEquipmentPanel.ClearChildren();

            // Equipment Slot
            for (int i = 0; i < 6; i++)
            {
                var itemEquipmentData = InventoryManager.Instance.InventoryBag.FirstOrDefault
                    (e => e.Value.Slot.Equals(i));

                Icon iconEquipmentSlot;

                if (itemEquipmentData.Value != null)
                {
                    iconEquipmentSlot = new Icon(IconType.None, Anchor.AutoInline, 1, true)
                    {
                        Identifier = "Slot_" + i,
                        ItemId = itemEquipmentData.Value.ItemId,
                        Slot = itemEquipmentData.Value.Slot,
                        KeyIndex = itemEquipmentData.Key, 
                        Texture = GetItemTexture(itemEquipmentData.Value.ItemId),
                        ToolTipText = "Click to unequip",
                        Locked = false,
                        Offset = new Vector2(-16, 0),
                    };

                    iconEquipmentSlot.OnClick = (Entity entity) =>
                    {
                        IsShowConfirmBox = true;

                        var item = InventoryManager.Instance.InventoryBag.FirstOrDefault
                            (i => i.Key.Equals(iconEquipmentSlot.KeyIndex));

                        GeonBit.UI.Utils.MessageBox.ShowMsgBox("Unequip?"
                            , $"Do ya wanna unequip '{item.Value.GetName()}'"
                            , new GeonBit.UI.Utils.MessageBox.MsgBoxOption[]
                            {
                                    new("Ok", () =>
                                    {
                                        // Use selected item from inventory
                                        InventoryManager.Instance.UnEquip(item.Value);

                                        RefreshInspectCharacterDisplay();

                                        IsShowConfirmBox = false;
                                        return true;
                                    }),
                                    new("Cancel", () =>
                                    {
                                        IsShowConfirmBox = false;
                                        return true;
                                    })
                            });
                    };
                }
                else
                {
                    iconEquipmentSlot = new Icon(IconType.None, Anchor.AutoInline, 1, true)
                    {
                        Identifier = "Slot_" + i,
                        Locked = true,
                        Offset = new Vector2(-16, 0)
                    };
                }

                if (i < 3)
                {
                    // Add Equipment slot in Left Panel
                    leftEquipmentPanel.AddChild(iconEquipmentSlot);
                }
                else
                {
                    // Add Equipment slot in Right Panel 
                    rightEquipmentPanel.AddChild(iconEquipmentSlot);
                }
            }         

            var skillIconPanel = displayCharacterPanel.Children.FirstOrDefault
                (p => p.Identifier.Equals("skillIconPanel"));

            // Normal Skill icon
            var normalSkillIcon = skillIconPanel.Children.OfType<Icon>().FirstOrDefault
                        (e => e.Identifier.Equals("normalSkillIcon"));
            normalSkillIcon.Enabled = true;
            normalSkillIcon.Texture = GetAbilityTexture(AbilityTextureName.Ability_Ive_got_the_Scent);
            normalSkillIcon.Count = PlayerManager.Instance.Player.PlayerData.Abilities.NormalSkillLevel;
            var normalSkillLevel = normalSkillIcon.Children.OfType<Label>().FirstOrDefault
                        (e => e.Identifier.Equals("normalSkillLevel"));
            normalSkillLevel.Text = $"+{normalSkillIcon.Count}";

            // Burst Skill Icon
            var burstSkillIcon = skillIconPanel.Children.OfType<Icon>().FirstOrDefault
                        (e => e.Identifier.Equals("burstSkillIcon"));
            burstSkillIcon.Enabled = true;
            burstSkillIcon.Texture = GetAbilityTexture(AbilityTextureName.Ability_Noah_Strike);
            burstSkillIcon.Count = PlayerManager.Instance.Player.PlayerData.Abilities.BurstSkillLevel;
            var burstSkillLevel = burstSkillIcon.Children.OfType<Label>().FirstOrDefault
                        (e => e.Identifier.Equals("burstSkillLevel"));
            burstSkillLevel.Text = $"+{burstSkillIcon.Count}";

            // Passive Skill Icon
            var passiveSkillIcon = skillIconPanel.Children.OfType<Icon>().FirstOrDefault
                        (e => e.Identifier.Equals("passiveSkillIcon"));
            passiveSkillIcon.Enabled = true;
            passiveSkillIcon.Texture = GetAbilityTexture(AbilityTextureName.Ability_Survivalist);
            passiveSkillIcon.Count = PlayerManager.Instance.Player.PlayerData.Abilities.PassiveSkillLevel;
            var passiveSkillLevel = passiveSkillIcon.Children.OfType<Label>().FirstOrDefault
                        (e => e.Identifier.Equals("passiveSkillLevel"));
            passiveSkillLevel.Text = $"+{passiveSkillIcon.Count}";

            var rightPanel = currentTabPanel.Children.FirstOrDefault(p => p.Identifier.Equals("rightPanel"));
            var statsPanel = rightPanel.Children.FirstOrDefault(p => p.Identifier.Equals("statsPanel"));

            // Player Level
            var playerLevel = statsPanel.Children.OfType<Label>().FirstOrDefault
                        (e => e.Identifier.Equals("playerLevel"));
            playerLevel.Text = PlayerManager.Instance.Player.Level.ToString();

            // ATK
            var statsATKPanel = statsPanel.Children.FirstOrDefault(p => p.Identifier.Equals("statsATKPanel"));
            var statsATK = statsATKPanel.Children.OfType<Label>().FirstOrDefault
                        (e => e.Identifier.Equals("statsATK"));
            statsATK.Text = PlayerManager.Instance.Player.ATK.ToString();

            // HP
            var statsHPPanel = statsPanel.Children.FirstOrDefault(p => p.Identifier.Equals("statsHPPanel"));
            var statsHP = statsHPPanel.Children.OfType<Label>().FirstOrDefault
                        (e => e.Identifier.Equals("statsHP"));
            statsHP.Text = PlayerManager.Instance.Player.MaxHP.ToString();

            // Mana
            var statsManaPanel = statsPanel.Children.FirstOrDefault(p => p.Identifier.Equals("statsManaPanel"));
            var statsMana = statsManaPanel.Children.OfType<Label>().FirstOrDefault
                        (e => e.Identifier.Equals("statsMana"));
            statsMana.Text = ((int)PlayerManager.Instance.Player.MaxMana).ToString();

            // ManaRegen
            var statsManaRegenPanel = statsPanel.Children.FirstOrDefault(p => p.Identifier.Equals("statsManaRegenPanel"));
            var statsManaRegen = statsManaRegenPanel.Children.OfType<Label>().FirstOrDefault
                        (e => e.Identifier.Equals("statsManaRegen"));
            statsManaRegen.Text = PlayerManager.Instance.Player.ManaRegenRate.ToString("F2") + "/s";

            // DEF
            var statsDEFPanel = statsPanel.Children.FirstOrDefault(p => p.Identifier.Equals("statsDEFPanel"));
            var statsDEF = statsDEFPanel.Children.OfType<Label>().FirstOrDefault
                        (e => e.Identifier.Equals("statsDEF"));
            statsDEF.Text = (PlayerManager.Instance.Player.DEF * 100f).ToString("F2") + "%";

            // Crit
            var statsCritPanel = statsPanel.Children.FirstOrDefault(p => p.Identifier.Equals("statsCritPanel"));
            var statsCrit = statsCritPanel.Children.OfType<Label>().FirstOrDefault
                        (e => e.Identifier.Equals("statsCrit"));
            statsCrit.Text = (PlayerManager.Instance.Player.Crit * 100f).ToString("F2") + "%";

            // CritDMG
            var statsCritDMGPanel = statsPanel.Children.FirstOrDefault(p => p.Identifier.Equals("statsCritDMGPanel"));
            var statsCritDMG = statsCritDMGPanel.Children.OfType<Label>().FirstOrDefault
                        (e => e.Identifier.Equals("statsCritDMG"));
            statsCritDMG.Text = (PlayerManager.Instance.Player.CritDMG * 100f).ToString("F2") + "%";

            // Evasion
            var statsEvasionPanel = statsPanel.Children.FirstOrDefault(p => p.Identifier.Equals("statsEvasionPanel"));
            var statsEvasion = statsEvasionPanel.Children.OfType<Label>().FirstOrDefault
                        (e => e.Identifier.Equals("statsEvasion"));
            statsEvasion.Text = (PlayerManager.Instance.Player.Evasion * 100f).ToString("F2") + "%";

            // Speed
            var statsSpeedPanel = statsPanel.Children.FirstOrDefault(p => p.Identifier.Equals("statsSpeedPanel"));
            var statsSpeed = statsSpeedPanel.Children.OfType<Label>().FirstOrDefault
                        (e => e.Identifier.Equals("statsSpeed"));
            statsSpeed.Text = PlayerManager.Instance.Player.Speed.ToString();
        }

        private void RefreshSkillDescription(string selectedTab, List<SkillDescriptionData> descSkill, int level)
        {
            var inspectPanel = _mainPanels.ElementAt(InspectPanel);
            var inspectTabs = inspectPanel.Children.OfType<PanelTabs>().FirstOrDefault
                (t => t.Identifier.Equals("inspectTabs"));
            inspectTabs.SelectTab(selectedTab);

            if (selectedTab.Equals("Character"))
            {
                var currentTabPanel = inspectTabs.ActiveTab.panel;
                var leftPanel = currentTabPanel.Children.FirstOrDefault
                    (t => t.Identifier.Equals("leftPanel"));
                var selectedSkillPanel = leftPanel.Children.FirstOrDefault
                    (t => t.Identifier.Equals("selectedSkillPanel"));
                var descripLeftSkill = selectedSkillPanel.Children.OfType<Paragraph>().FirstOrDefault
                    (t => t.Identifier.Equals("descripLeftSkill"));
                var upSkillButton = selectedSkillPanel.Children.FirstOrDefault
                    (t => t.Identifier.Equals("upSkillButton"));
                var descripRightSkill = selectedSkillPanel.Children.OfType<Paragraph>().FirstOrDefault
                    (t => t.Identifier.Equals("descripRightSkill"));

                var skillCostPanel = selectedSkillPanel.Children.FirstOrDefault
                    (t => t.Identifier.Equals("skillCostPanel"));
                skillCostPanel.Visible = true;

                // SkillPoint Cost
                var SPCost = skillCostPanel.Children.FirstOrDefault
                    (t => t.Identifier.Equals("SPCost"));
                var SPCostText = SPCost.Children.OfType<Label>().FirstOrDefault
                    (t => t.Identifier.Equals("SPCostText"));

                // GoldCoin Cost
                var GoldCost = skillCostPanel.Children.FirstOrDefault
                    (t => t.Identifier.Equals("GoldCost"));
                var GoldCostText = GoldCost.Children.OfType<Label>().FirstOrDefault
                    (t => t.Identifier.Equals("GoldCostText"));

                if (level < 10)
                {
                    descripLeftSkill.Text = $"Lv.{level} : {NoahSelectedSkill}\n"
                        + descSkill.FirstOrDefault(s => s.Level.Equals(level)).Description;

                    SPCostText.Text = descSkill.FirstOrDefault(s => s.Level.Equals(level + 1)).SkillPointCost.ToString();
                    GoldCostText.Text = descSkill.FirstOrDefault(s => s.Level.Equals(level + 1)).GoldCoinCost.ToString();
                    upSkillButton.Enabled = true;
                    upSkillButton.Visible = true;
                    
                    descripRightSkill.Text = $"Lv.{level + 1} : {NoahSelectedSkill}\n"
                        + descSkill.FirstOrDefault(s => s.Level.Equals(level + 1)).Description;
                }
                else
                {
                    descripLeftSkill.Text = $"Lv.{level} : {NoahSelectedSkill}\n"
                        + descSkill.FirstOrDefault(s => s.Level.Equals(level)).Description;

                    SPCostText.Text = "-";
                    GoldCostText.Text = "-";
                    upSkillButton.Enabled = false;
                    
                    descripRightSkill.Text = "Skill level has reached maximum.";
                }
            }
            else
            {
                // Companion
                var currentTabPanel = inspectTabs.ActiveTab.panel;
                var leftPanel = currentTabPanel.Children.FirstOrDefault
                    (t => t.Identifier.Equals("leftPanel"));
                var selectedSkillPanel = leftPanel.Children.FirstOrDefault
                    (t => t.Identifier.Equals("selectedSkillPanel"));
                var descripLeftSkill = selectedSkillPanel.Children.OfType<Paragraph>().FirstOrDefault
                    (t => t.Identifier.Equals("descripLeftSkill"));

                descripLeftSkill.Text = $"Lv.{level} : {CompanionSelectedSkill}\n"
                        + descSkill.FirstOrDefault(s => s.Level.Equals(level)).Description;
            }
        }

        public void ClearSkillDescription(string selectedTab)
        {
            IsCharacterTabSelected = false;

            var inspectPanel = _mainPanels.ElementAt(InspectPanel);
            var inspectTabs = inspectPanel.Children.OfType<PanelTabs>().FirstOrDefault(t => t.Identifier.Equals("inspectTabs"));
            inspectTabs.SelectTab(selectedTab);

            if (selectedTab.Equals("Character"))
            {
                var currentTabPanel = inspectTabs.ActiveTab.panel;
                var leftPanel = currentTabPanel.Children.FirstOrDefault
                    (t => t.Identifier.Equals("leftPanel"));
                var selectedSkillPanel = leftPanel.Children.FirstOrDefault
                    (t => t.Identifier.Equals("selectedSkillPanel"));
                var descripLeftSkill = selectedSkillPanel.Children.OfType<Paragraph>().FirstOrDefault
                    (t => t.Identifier.Equals("descripLeftSkill"));
                var upSkillButton = selectedSkillPanel.Children.FirstOrDefault
                    (t => t.Identifier.Equals("upSkillButton"));
                var descripRightSkill = selectedSkillPanel.Children.OfType<Paragraph>().FirstOrDefault
                    (t => t.Identifier.Equals("descripRightSkill"));

                var skillCostPanel = selectedSkillPanel.Children.FirstOrDefault
                    (t => t.Identifier.Equals("skillCostPanel"));
                skillCostPanel.Visible = false;

                // SkillPoint Cost
                var SPCost = skillCostPanel.Children.FirstOrDefault
                    (t => t.Identifier.Equals("SPCost"));
                var SPCostText = SPCost.Children.OfType<Label>().FirstOrDefault
                    (t => t.Identifier.Equals("SPCostText"));

                // GoldCoin Cost
                var GoldCost = skillCostPanel.Children.FirstOrDefault
                    (t => t.Identifier.Equals("GoldCost"));
                var GoldCostText = GoldCost.Children.OfType<Label>().FirstOrDefault
                    (t => t.Identifier.Equals("GoldCostText"));

                descripLeftSkill.Text = "";
                SPCostText.Text = "-";
                GoldCostText.Text = "-";
                upSkillButton.Enabled = false;
                upSkillButton.Visible = false;
                descripRightSkill.Text = "";
            }
            else
            {
                var currentTabPanel = inspectTabs.ActiveTab.panel;
                var leftPanel = currentTabPanel.Children.FirstOrDefault
                    (t => t.Identifier.Equals("leftPanel"));
                var selectedSkillPanel = leftPanel.Children.FirstOrDefault
                    (t => t.Identifier.Equals("selectedSkillPanel"));
                var descripLeftSkill = selectedSkillPanel.Children.OfType<Paragraph>().FirstOrDefault
                    (t => t.Identifier.Equals("descripLeftSkill"));

                descripLeftSkill.Text = "";
            }
        }

        /// <summary>
        /// This is Main Menu nothing more nothing less
        /// </summary>
        private void InitMainMenuUI()
        {
            var mainMenuPanel = new Panel(
                new Vector2(GameGlobals.Instance.GameScreen.X,
                GameGlobals.Instance.GameScreen.Y))
            {
                Skin = PanelSkin.None,
                Identifier = "mainMenuPanel"
            };
            _mainPanels.Add(mainMenuPanel);
            UserInterface.Active.AddEntity(mainMenuPanel);

            // Main panel
            var mainPanel = new Panel(new Vector2(520, -1), PanelSkin.None)
            {
                Identifier = "mainPanel"
            };
            mainMenuPanel.AddChild(mainPanel);

            // Option panel
            var optionPanel = new Panel(new Vector2(520, -1))
            {
                Visible = false,
                Identifier = "optionPanel"
            };
            mainMenuPanel.AddChild(optionPanel);

            // Graphics Setting panel
            var graphicsSettingPanel = new Panel(new Vector2(520, -1))
            {
                Identifier = "graphicsSettingPanel",
                Visible = false
            };
            mainMenuPanel.AddChild(graphicsSettingPanel);

            // Sound Setting panel
            var soundSettingPanel = new Panel(new Vector2(520, -1))
            {
                Identifier = "soundSettingPanel",
                Visible = false
            };
            mainMenuPanel.AddChild(soundSettingPanel);

            // Add Key Binding Panel 
            var keyBindingPanel = new Panel(new Vector2(460, 560), PanelSkin.None)
            {
                Identifier = "keyBindingPanel",
                Visible = false,
                AdjustHeightAutomatically = true
            };
            mainMenuPanel.AddChild(keyBindingPanel);

            // Load GameSave panel
            var loadGameSavePanel = new Panel(new Vector2(1200, 800))
            {
                Identifier = "loadGameSavePanel",
                Skin = PanelSkin.None,
                Visible = false
            };
            mainMenuPanel.AddChild(loadGameSavePanel);

            // Initialize UI Elements
            // Main Menu
            {              
                // add title
                var title = new Image(GetGuiTexture(GuiTextureName.game_name)
                    , anchor: Anchor.TopCenter, offset: new Vector2(0, -20))
                {
                    Identifier = "title",
                    Size = new Vector2(800, 480),
                    ShadowColor = new Color(0, 0, 0, 128),
                    ShadowOffset = Vector2.One * -6
                };
                mainPanel.AddChild(title);

                var newGameButton = new Button("New Game", ButtonSkin.Default, Anchor.AutoCenter)
                {
                    Identifier = "newGameButton"
                };
                mainPanel.AddChild(newGameButton);

                var loadButton = new Button("Load Save", ButtonSkin.Default, Anchor.AutoCenter)
                {
                    Identifier = "loadButton" 
                };
                mainPanel.AddChild(loadButton);

                var optionsButton = new Button("Options", ButtonSkin.Default, Anchor.AutoCenter)
                {
                    Identifier = "optionsButton"
                };
                mainPanel.AddChild(optionsButton);

                var quitButton = new Button("Quit", ButtonSkin.Default, Anchor.AutoCenter)
                {
                    Identifier = "quitButton"
                };
                mainPanel.AddChild(quitButton);

                // Set OnClick Buttons
                newGameButton.OnClick = (btn) =>
                {
                    // Disable input handling
                    newGameButton.Locked = true;
                    loadButton.Locked = true;
                    optionsButton.Locked = true;
                    quitButton.Locked = true;

                    ScreenManager.StartGame(true);
                };

                loadButton.OnClick = (btn) =>
                {
                    mainPanel.Visible = false;
                    loadGameSavePanel.Visible = true;

                    RefreshGameSave(MainMenu);
                    IsClickedLoadButton = true;
                };

                optionsButton.OnClick = (btn) =>
                {
                    mainPanel.Visible = false;
                    optionPanel.Visible = true;
                };

                quitButton.OnClick = (btn) =>
                {
                    GeonBit.UI.Utils.MessageBox.ShowMsgBox("Quit the Game!?", "Do you want to quit?"
                        , new GeonBit.UI.Utils.MessageBox.MsgBoxOption[]
                        {
                                new("Yes", () =>
                                {
                                    ScreenManager.Instance.Game.Exit();
                                    return true;
                                }),
                                new("No", () =>
                                {
                                    return true;
                                })
                        });
                };
            }

            // Initialize Options Menu
            InitOptionMenu(mainPanel, optionPanel, graphicsSettingPanel, soundSettingPanel, keyBindingPanel);

            // Load GameSave: Continues
            {
                // add title and text
                loadGameSavePanel.AddChild(new Header("Select Save Slot", Anchor.TopCenter));
                loadGameSavePanel.AddChild(new LineSpace(1));

                // Initialize save slots
                InitSaveSlot(loadGameSavePanel, MainMenu);

                // Buttons Panel
                loadGameSavePanel.AddChild(new LineSpace(3));
                var buttonPanel = new Panel(new Vector2(1000, 125), PanelSkin.None, Anchor.BottomCenter)
                {
                    Identifier = "buttonPanel"
                };
                loadGameSavePanel.AddChild(buttonPanel);

                // Play
                var playButton = new Button("Play Selected Save", ButtonSkin.Default, Anchor.TopCenter)
                {
                    Identifier = "playButton",
                    Enabled = false,
                    Size = new Vector2(300, 50)
                };
                buttonPanel.AddChild(playButton);

                // Rename
                var renameButton = new Button("Rename", ButtonSkin.Default, Anchor.BottomCenter)
                {
                    Identifier = "renameButton",
                    Enabled = false,
                    Size = new Vector2(145, 50),
                    Offset = new Vector2(-77.5f, -30),
                };
                buttonPanel.AddChild(renameButton);

                // Delete
                var deleteButton = new Button("Delete", ButtonSkin.Default, Anchor.BottomCenter)
                {
                    Identifier = "deleteButton",
                    Enabled = false,
                    Size = new Vector2(145, 50),
                    Offset = new Vector2(77.5f, -30)
                };
                buttonPanel.AddChild(deleteButton);

                var backButton = new Button("Back", ButtonSkin.Default, Anchor.BottomRight)
                {
                    Identifier = "backButton",
                    Size = new Vector2(145, 90),
                    Offset = new Vector2(0, -20)
                };
                buttonPanel.AddChild(backButton);

                // Set OnClick GameSave Panel
                foreach (var gameSavePanel in MainMenuSaveSlots)
                {
                    gameSavePanel.OnClick = (entity) =>
                    {
                        // Set Enable Buttons
                        playButton.Enabled = true;
                        renameButton.Enabled = true;
                        deleteButton.Enabled = true;

                        SelectedGameSavePanel = gameSavePanel;
                        UpdateSelectedGameSave(MainMenu);

                        PlaySoundEffect(Sound.Click1);
                    };
                }

                // Set OnClick Buttons on Load Save
                // Play
                playButton.OnClick = (Entity btn) =>
                {
                    GeonBit.UI.Utils.MessageBox.ShowMsgBox("Play Selected Save!", "Do you want to play the selected save?"
                        , new GeonBit.UI.Utils.MessageBox.MsgBoxOption[]
                        {
                                new("Yes!", () =>
                                {
                                    // Disable input handling
                                    playButton.Locked = true;
                                    renameButton.Locked = true;
                                    deleteButton.Locked = true;
                                    backButton.Locked = true;

                                    ScreenManager.StartGame(false);
                                    return true;
                                }),
                                new("No", () =>
                                {
                                    return true;
                                })
                        });
                };

                // Rename
                renameButton.OnClick = (Entity btn) =>
                {
                    var gameSaveData = GameGlobals.Instance.GameSave[GameGlobals.Instance.SelectedGameSaveIndex];

                    var textInput = new TextInput(false)
                    {
                        Value = gameSaveData.Name,
                        PlaceholderText = "Enter your new save name"
                    };
                    GeonBit.UI.Utils.MessageBox.ShowMsgBox("Rename", ""
                        , [
                            new("Done", () =>
                                {
                                    if (!textInput.Value.Equals("") || !textInput.Value.Equals(" "))
                                    {
                                        gameSaveData.Name = textInput.Value;
                                        JsonFileManager.SaveGame(JsonFileManager.RenameGameSave);
                                    }
                                    RefreshGameSave(MainMenu);

                                    // Reset Buttons
                                    playButton.Enabled = false;
                                    renameButton.Enabled = false;
                                    deleteButton.Enabled = false;

                                    return true;
                                })
                        ]
                        , [textInput]);
                };

                // Delete
                deleteButton.OnClick = (Entity btn) =>
                {
                    GeonBit.UI.Utils.MessageBox.ShowMsgBox("Delete Selected Save!", "Do you want to delete the selected save?"
                        , new GeonBit.UI.Utils.MessageBox.MsgBoxOption[]
                        {
                                new("Yes", () =>
                                {
                                    JsonFileManager.SaveGame(JsonFileManager.DeleteGameSave);
                                    RefreshGameSave(MainMenu);

                                    // Reset Buttons
                                    playButton.Enabled = false;
                                    renameButton.Enabled = false;
                                    deleteButton.Enabled = false;

                                    return true;
                                }),
                                new("No", () =>
                                {
                                    return true;
                                })
                        });
                };

                // Back
                backButton.OnClick = (Entity btn) =>
                {
                    loadGameSavePanel.Visible = false;
                    mainPanel.Visible = true;

                    // Reset Buttons
                    playButton.Enabled = false;
                    renameButton.Enabled = false;
                    deleteButton.Enabled = false;

                    IsClickedLoadButton = false;
                };
            }
        }

        private static void InitOptionMenu(Panel mainPanel, Panel optionPanel, Panel graphicsSettingPanel, Panel soundSettingPanel, Panel keyBindingPanel)
        {
            // Options Menu
            {
                optionPanel.AddChild(new Header("Options") { Scale = 2f });
                optionPanel.AddChild(new LineSpace(3));

                // Graphic setting
                var graphicsSettingsButton = new Button("Graphics Settings", ButtonSkin.Default)
                {
                    Identifier = "graphicsSettingsButton",
                    OnClick = (Entity btn) =>
                    {
                        optionPanel.Visible = false;
                        graphicsSettingPanel.Visible = true;

                        var radioFullScreenOn = graphicsSettingPanel.Children.OfType<RadioButton>().FirstOrDefault
                            (e => e.Identifier.Equals("radioFullScreenOn"));
                        var radioFullScreenOff = graphicsSettingPanel.Children.OfType<RadioButton>().FirstOrDefault
                            (e => e.Identifier.Equals("radioFullScreenOff"));

                        if (GameGlobals.Instance.IsFullScreen)
                        {
                            radioFullScreenOn.Checked = true;
                        }
                        else radioFullScreenOff.Checked = true;
                    }
                };
                optionPanel.AddChild(graphicsSettingsButton);

                // Sound setting
                var soundSettingsButton = new Button("Sound Settings", ButtonSkin.Default)
                {
                    Identifier = "soundSettingsButton",
                    OnClick = (Entity btn) =>
                    {
                        optionPanel.Visible = false;
                        soundSettingPanel.Visible = true;
                    }
                };
                optionPanel.AddChild(soundSettingsButton);

                // Key Binding
                var keyBindingButton = new Button("Key Binding", ButtonSkin.Default)
                {
                    Identifier = "keyBindingButton",
                    OnClick = (Entity btn) =>
                    {
                        optionPanel.Visible = false;
                        graphicsSettingPanel.Visible = false;
                        soundSettingPanel.Visible = false;
                        keyBindingPanel.Visible = true;
                    }
                };
                optionPanel.AddChild(keyBindingButton);

                optionPanel.AddChild(new LineSpace(3));
                var backButton = new Button("Back", ButtonSkin.Default)
                {
                    Identifier = "backButton",
                    OnClick = (Entity btn) =>
                    {
                        optionPanel.Visible = false;
                        mainPanel.Visible = true;
                    }
                };
                optionPanel.AddChild(backButton);
            }

            // Graphics Setting
            {
                graphicsSettingPanel.AddChild(new Header("Graphics Setting"));
                graphicsSettingPanel.AddChild(new HorizontalLine());
                graphicsSettingPanel.AddChild(new RichParagraph("Full Screen") { Scale = 1.25f });

                var radioFullScreenOn = new RadioButton("ON", offset: new Vector2(15, 0))
                {
                    Identifier = "radioFullScreenOn",
                    OnClick = (e) =>
                    {
                        GameGlobals.Instance.IsFullScreen = true;
                        ScreenManager.ToggleFullScreen();
                    }
                };
                graphicsSettingPanel.AddChild(radioFullScreenOn);

                var radioFullScreenOff = new RadioButton("OFF", offset: new Vector2(15, 0))
                {
                    Identifier = "radioFullScreenOff",
                    OnClick = (e) =>
                    {
                        GameGlobals.Instance.IsFullScreen = false;
                        ScreenManager.ToggleFullScreen();
                    }
                };
                graphicsSettingPanel.AddChild(radioFullScreenOff);

                graphicsSettingPanel.AddChild(new LineSpace(3));
                var backButton = new Button("Back", ButtonSkin.Default)
                {
                    Identifier = "backButton",
                    OnClick = (Entity btn) =>
                    {
                        graphicsSettingPanel.Visible = false;
                        optionPanel.Visible = true;

                        // Save GameConfig
                        JsonFileManager.SaveConfig();
                    }
                };
                graphicsSettingPanel.AddChild(backButton);
            }

            // Sound Settings
            {
                // sliders title
                soundSettingPanel.AddChild(new Header("Sound Settings"));
                soundSettingPanel.AddChild(new HorizontalLine());
                soundSettingPanel.AddChild(new RichParagraph("SFX Volume") { Scale = 1.25f });

                {
                    var sliderSFX = soundSettingPanel.AddChild(new Slider(0, 100, SliderSkin.Default)
                    {
                        Identifier = "sliderSFX"
                    }) as Slider;
                    sliderSFX.Value = (int)(GameGlobals.Instance.SoundEffectVolume * 100);

                    var valueLabel = new Label("Value: " + sliderSFX.Value) { Scale = 1.1f };
                    sliderSFX.OnValueChange = (Entity entity) =>
                    {
                        valueLabel.Text = "Value: " + sliderSFX.Value;
                        GameGlobals.Instance.SoundEffectVolume = sliderSFX.GetValueAsPercent();
                    };
                    soundSettingPanel.AddChild(valueLabel);
                }
                soundSettingPanel.AddChild(new LineSpace(1));

                soundSettingPanel.AddChild(new RichParagraph("Background Music Volume") { Scale = 1.25f });
                {
                    var sliderBG = soundSettingPanel.AddChild(new Slider(0, 100, SliderSkin.Default)
                    {
                        Identifier = "sliderBG"
                    }) as Slider;
                    sliderBG.Value = (int)(GameGlobals.Instance.BackgroundMusicVolume * 100);

                    var valueLabel = new Label("Value: " + sliderBG.Value) { Scale = 1.1f };
                    sliderBG.OnValueChange = (Entity entity) =>
                    {
                        valueLabel.Text = "Value: " + sliderBG.Value;
                        GameGlobals.Instance.BackgroundMusicVolume = sliderBG.GetValueAsPercent();
                    };
                    soundSettingPanel.AddChild(valueLabel);
                }
                soundSettingPanel.AddChild(new LineSpace(3));

                var backButton = new Button("Back", ButtonSkin.Default)
                {
                    Identifier = "backButton",
                    OnClick = (Entity btn) =>
                    {
                        soundSettingPanel.Visible = false;
                        optionPanel.Visible = true;

                        // Save GameConfig
                        JsonFileManager.SaveConfig();
                    }
                };
                soundSettingPanel.AddChild(backButton);
            }

            // Key Binding
            {
                keyBindingPanel.AddChild(new Image(GetGuiTexture(GuiTextureName.key_binding), new Vector2(424, 645)));

                keyBindingPanel.AddChild(new LineSpace(3));
                var backButton = new Button("Back", ButtonSkin.Default)
                {
                    Identifier = "backButton",
                    OnClick = (Entity btn) =>
                    {
                        keyBindingPanel.Visible = false;
                        optionPanel.Visible = true;
                    }
                };
                keyBindingPanel.AddChild(backButton);
            }
        }

        private void InitSaveSlot(Panel mainPanel, int fromGUI)
        {
            for (int i = 0; i < GameGlobals.Instance.MaxGameSaveSlot; i++)
            {
                var gameSavePanel = new Panel(new Vector2(1000, 120), PanelSkin.Simple, Anchor.AutoCenter)
                {
                    Identifier = $"gameSavePanel_{i}",
                    Opacity = 128,
                    OutlineColor = Color.White,
                    OutlineWidth = 0
                };
                mainPanel.AddChild(gameSavePanel);
                mainPanel.AddChild(new LineSpace(1));

                var saveName = new Label("Name", Anchor.TopLeft)
                {
                    Identifier = "saveName",
                    Scale = 1.1f,
                    ClickThrough = true
                };
                gameSavePanel.AddChild(saveName);

                var playTime = new Label("Total PlayTime", Anchor.BottomLeft)
                {
                    Identifier = "playTime",
                    Scale = 1.1f,
                    ClickThrough = true
                };
                gameSavePanel.AddChild(playTime);

                var createdTime = new Label("Created Time", Anchor.TopRight)
                {
                    Identifier = "createdTime",
                    Scale = 1.1f,
                    ClickThrough = true
                };
                gameSavePanel.AddChild(createdTime);

                var updatedTime = new Label("last Updated Time", Anchor.BottomRight)
                {
                    Identifier = "updatedTime",
                    Scale = 1.1f,
                    ClickThrough = true
                };
                gameSavePanel.AddChild(updatedTime);

                var textSaveEmpty = new Label($"Empty Slot {i + 1}", Anchor.Center)
                {
                    Identifier = "textSaveEmpty",
                    Visible = false,
                    Scale = 1.1f,
                    ClickThrough = true
                };
                gameSavePanel.AddChild(textSaveEmpty);

                if (fromGUI == MainMenu)
                {
                    MainMenuSaveSlots.Add(gameSavePanel);
                }
                else SaveMenuSaveSlots.Add(gameSavePanel);
            }
        }

        public void RefreshMainMenu()
        {
            var mainMenuPanel = _mainPanels[MainMenu];
            var mainPanel = mainMenuPanel.Children.FirstOrDefault
                (e => e.Identifier.Equals("mainPanel"));
            var loadGameSavePanel = mainMenuPanel.Children.FirstOrDefault
                (e => e.Identifier.Equals("loadGameSavePanel"));
            var optionPanel = mainMenuPanel.Children.FirstOrDefault
                (e => e.Identifier.Equals("optionPanel"));
            var graphicsSettingPanel = mainMenuPanel.Children.FirstOrDefault
                (e => e.Identifier.Equals("graphicsSettingPanel"));
            var soundSettingPanel = mainMenuPanel.Children.FirstOrDefault
                (e => e.Identifier.Equals("soundSettingPanel"));

            mainPanel.Visible = true;

            var newGameButton = mainPanel.Children.OfType<Button>().FirstOrDefault
                (e => e.Identifier.Equals("newGameButton"));
            var loadButton = mainPanel.Children.OfType<Button>().FirstOrDefault
                (e => e.Identifier.Equals("loadButton"));
            var optionsButton = mainPanel.Children.OfType<Button>().FirstOrDefault
                (e => e.Identifier.Equals("optionsButton"));
            var quitButton = mainPanel.Children.OfType<Button>().FirstOrDefault
                (e => e.Identifier.Equals("quitButton"));

            loadGameSavePanel.Visible = false;

            var buttonPanel = loadGameSavePanel.Children.FirstOrDefault
                (e => e.Identifier.Equals("buttonPanel"));
            var playButton = buttonPanel.Children.OfType<Button>().FirstOrDefault
                (e => e.Identifier.Equals("playButton"));
            var renameButton = buttonPanel.Children.OfType<Button>().FirstOrDefault
                (e => e.Identifier.Equals("renameButton"));
            var deleteButton = buttonPanel.Children.OfType<Button>().FirstOrDefault
                (e => e.Identifier.Equals("deleteButton"));
            var backButton = buttonPanel.Children.OfType<Button>().FirstOrDefault
                (e => e.Identifier.Equals("backButton"));

            optionPanel.Visible = false;
            graphicsSettingPanel.Visible = false;
            soundSettingPanel.Visible = false;

            // Reset Buttons
            // mainPanel
            newGameButton.Locked = false;
            loadButton.Locked = false;
            optionsButton.Locked = false;
            quitButton.Locked = false;

            // loadGameSavePanel
            playButton.Locked = false;
            renameButton.Locked = false;
            deleteButton.Locked = false;
            backButton.Locked = false;
            playButton.Enabled = false;         
            renameButton.Enabled = false;
            deleteButton.Enabled = false;

            IsClickedLoadButton = false;
        }

        public void RefreshGameSave(int fromGUI)
        {
            List<Panel> gameSavePanels;

            if (fromGUI == MainMenu)
            {
                gameSavePanels = MainMenuSaveSlots;
            }
            else gameSavePanels = SaveMenuSaveSlots;

            for (int i = 0; i < GameGlobals.Instance.MaxGameSaveSlot; i++)
            {
                var gameSavePanel = gameSavePanels[i];
                var saveName = gameSavePanel.Children.OfType<Label>().FirstOrDefault
                    (l => l.Identifier.Equals("saveName"));
                var playTime = gameSavePanel.Children.OfType<Label>().FirstOrDefault
                    (l => l.Identifier.Equals("playTime"));
                var createdTime = gameSavePanel.Children.OfType<Label>().FirstOrDefault
                    (l => l.Identifier.Equals("createdTime"));
                var updatedTime = gameSavePanel.Children.OfType<Label>().FirstOrDefault
                    (l => l.Identifier.Equals("updatedTime"));
                var textSaveEmpty = gameSavePanel.Children.OfType<Label>().FirstOrDefault
                    (l => l.Identifier.Equals("textSaveEmpty"));
                
                try
                {
                    var gameSaveData = GameGlobals.Instance.GameSave[i];

                    saveName.Text = gameSaveData.Name;
                    playTime.Text = $"Total PlayTime: {gameSaveData.TotalPlayTime[0]}h | {gameSaveData.TotalPlayTime[1]}m | {gameSaveData.TotalPlayTime[2]}s";
                    createdTime.Text = gameSaveData.CreatedTime.Replace("_", " ");
                    updatedTime.Text = gameSaveData.LastUpdated.Replace("_", " ");
                    textSaveEmpty.Visible = false;

                    gameSavePanel.Enabled = true;
                    gameSavePanel.Opacity = 255;
                    gameSavePanel.OutlineColor = Color.White;
                    gameSavePanel.OutlineWidth = 0;
                }
                catch (ArgumentOutOfRangeException)
                {
                    saveName.Text = "";
                    playTime.Text = "";
                    createdTime.Text = "";
                    updatedTime.Text = "";
                    textSaveEmpty.Visible = true;

                    gameSavePanel.Opacity = 128;
                    gameSavePanel.OutlineColor = Color.White;
                    gameSavePanel.OutlineWidth = 0;

                    if (fromGUI == MainMenu)
                    {
                        gameSavePanel.Enabled = false;
                    }
                    else gameSavePanel.Enabled = true;
                }
            }
        }

        private void UpdateSelectedGameSave(int fromGUI)
        {
            var numberString = SelectedGameSavePanel.Identifier.Replace("gameSavePanel_", "");

            if (int.TryParse(numberString, out int numberIndex))
                GameGlobals.Instance.SelectedGameSaveIndex = numberIndex;

            List<Panel> gameSavePanels;

            if (fromGUI == MainMenu)
            {
                gameSavePanels = MainMenuSaveSlots;
            }
            else gameSavePanels = SaveMenuSaveSlots;

            foreach (var gameSavePanel in gameSavePanels)
            {
                var textSaveEmpty = gameSavePanel.Children.OfType<Label>().FirstOrDefault
                    (l => l.Identifier.Equals("textSaveEmpty"));

                if (textSaveEmpty.Visible)
                {
                    gameSavePanel.Opacity = 128;
                }
                else gameSavePanel.Opacity = 255;

                gameSavePanel.OutlineColor = Color.White;
                gameSavePanel.OutlineWidth = 0;           
            }

            SelectedGameSavePanel.Opacity = 255;
            SelectedGameSavePanel.OutlineColor = Color.Yellow;
            SelectedGameSavePanel.OutlineWidth = 2;
        }

        /// <summary>
        /// Pause Menu, use in PlayScreen to display menu and pause the game: Resume, Options, Exit to Main and Quit.
        /// </summary>
        private void InitPauseMenuUI()
        {
            var pauseMenuPanel = new Panel()
            {
                Skin = PanelSkin.None,
                Identifier = "pauseMenuPanel"
            };
            _mainPanels.Add(pauseMenuPanel);
            UserInterface.Active.AddEntity(pauseMenuPanel);

            // Pause panel
            var pausePanel = new Panel(new Vector2(520, -1), PanelSkin.None)
            {
                Identifier = "pausePanel"
            };
            pauseMenuPanel.AddChild(pausePanel);

            // Option panel
            var optionPanel = new Panel(new Vector2(520, -1))
            {
                Visible = false,
                Identifier = "optionPanel"
            };
            pauseMenuPanel.AddChild(optionPanel);

            // Graphics Setting panel
            var graphicsSettingPanel = new Panel(new Vector2(520, -1))
            {
                Identifier = "graphicsSettingPanel",
                Visible = false
            };
            pauseMenuPanel.AddChild(graphicsSettingPanel);

            // Sound Setting panel
            var soundSettingPanel = new Panel(new Vector2(520, -1))
            {
                Identifier = "soundSettingPanel",
                Visible = false
            };
            pauseMenuPanel.AddChild(soundSettingPanel);

            // Add Key Binding Panel 
            var keyBindingPanel = new Panel(new Vector2(460, 650), PanelSkin.None)
            {
                Identifier = "keyBindingPanel",
                Visible = false,
                AdjustHeightAutomatically = true
            };
            pauseMenuPanel.AddChild(keyBindingPanel);

            // Initialize UI Elements
            // Pause Menu
            {
                pausePanel.AddChild(new Header("Paused") { Scale = 2f });
                pausePanel.AddChild(new LineSpace(3));

                var resumeButton = new Button("Resume", ButtonSkin.Default)
                {
                    Identifier = "resumeButton"
                };
                pausePanel.AddChild(resumeButton);

                var optionsButton = new Button("Options", ButtonSkin.Default)
                {
                    Identifier = "optionsButton"
                };
                pausePanel.AddChild(optionsButton);

                var exitToMainButton = new Button("Exit to Main", ButtonSkin.Default)
                {
                    Identifier = "exitToMainButton"
                };
                pausePanel.AddChild(exitToMainButton);

                var quitButton = new Button("Quit", ButtonSkin.Default)
                {
                    Identifier = "quitButton"
                };
                pausePanel.AddChild(quitButton);

                // Set OnClick Buttons
                resumeButton.OnClick = (btn) =>
                {
                    // Back to PlayScreen
                    // Toggle Pause PlayScreen
                    GameGlobals.Instance.IsGamePause = !GameGlobals.Instance.IsGamePause;
                    GameGlobals.Instance.IsOpenGUI = !GameGlobals.Instance.IsOpenGUI;

                    // Toggle the IsOpenPauseMenu flag
                    GameGlobals.Instance.IsOpenPauseMenu = false;
                    GameGlobals.Instance.IsRefreshPlayScreenUI = false;
                    CurrentUI = PlayScreen;
                };

                optionsButton.OnClick = (Entity btn) =>
                {
                    pausePanel.Visible = false;
                    optionPanel.Visible = true;
                };

                exitToMainButton.OnClick = (Entity btn) =>
                {
                    // Exit to MainMenuScreen
                    GeonBit.UI.Utils.MessageBox.ShowMsgBox("Exit to Main Menu!?", "Do you want to exit to the main menu?."
                        , new GeonBit.UI.Utils.MessageBox.MsgBoxOption[]
                        {
                                new("Yes", () =>
                                {
                                    // Disable input handling
                                    resumeButton.Locked = true;
                                    optionsButton.Locked = true;
                                    exitToMainButton.Locked = true;
                                    quitButton.Locked = true;

                                    Camera.ResetCameraPosition(true);
                                    //JsonFileManager.SaveGame(JsonFileManager.SavingPlayerData);
                                    if (GameGlobals.Instance.IsMiniGameStart)
                                        GameGlobals.Instance.EndMiniGame();

                                    ScreenManager.Instance.TransitionToScreen(ScreenManager.GameScreen.MainMenuScreen);
                                    return true;
                                }),
                                new("No", () =>
                                {
                                    return true;
                                })
                        });
                };

                quitButton.OnClick = (Entity entity) =>
                {
                    GeonBit.UI.Utils.MessageBox.ShowMsgBox("Quit the Game!?", "Do you want to quit?."
                        , new GeonBit.UI.Utils.MessageBox.MsgBoxOption[]
                        {
                                new("Yes", () =>
                                {
                                    //\nDon't worry, the game will save automatically
                                    //JsonFileManager.SaveGame(JsonFileManager.SavingPlayerData);
                                    ScreenManager.Instance.Game.Exit();
                                    return true;
                                }),
                                new("No", () =>
                                {
                                    return true;
                                })
                        });
                };
            }

            // Initialize Options Menu
            InitOptionMenu(pausePanel, optionPanel, graphicsSettingPanel, soundSettingPanel, keyBindingPanel);
        }

        public void RefreshPauseMenu()
        {
            var pauseMenuPanel = _mainPanels[PauseMenu];
            var pausePanel = pauseMenuPanel.Children.FirstOrDefault(e => e.Identifier.Equals("pausePanel"));

            var resumeButton = pausePanel.Children.OfType<Button>().FirstOrDefault
                (e => e.Identifier.Equals("resumeButton"));
            var optionsButton = pausePanel.Children.OfType<Button>().FirstOrDefault
                (e => e.Identifier.Equals("optionsButton"));
            var exitToMainButton = pausePanel.Children.OfType<Button>().FirstOrDefault
                (e => e.Identifier.Equals("exitToMainButton"));
            var quitButton = pausePanel.Children.OfType<Button>().FirstOrDefault
                (e => e.Identifier.Equals("quitButton"));

            // Reset Buttons
            resumeButton.Locked = false;
            optionsButton.Locked = false;
            exitToMainButton.Locked = false;
            quitButton.Locked = false;
        }

        private void InitSaveMenuUI()
        {
            var saveMenuPanel = new Panel(
                new Vector2(GameGlobals.Instance.GameScreen.X,
                GameGlobals.Instance.GameScreen.Y))
            {
                Skin = PanelSkin.None,
                Identifier = "saveMenuPanel"
            };
            _mainPanels.Add(saveMenuPanel);
            UserInterface.Active.AddEntity(saveMenuPanel);

            // GameSave panel
            var saveGamePanel = new Panel(new Vector2(1200, 800))
            {
                Identifier = "saveGamePanel",
                Skin = PanelSkin.None
            };
            saveMenuPanel.AddChild(saveGamePanel);

            // Save Game
            {
                // add title and text
                saveGamePanel.AddChild(new Header("Select Save Slot", Anchor.TopCenter));
                saveGamePanel.AddChild(new LineSpace(1));

                // Initialize save slots
                InitSaveSlot(saveGamePanel, SaveMenu);

                // Buttons Panel
                saveGamePanel.AddChild(new LineSpace(3));
                var buttonPanel = new Panel(new Vector2(1000, 125), PanelSkin.None, Anchor.BottomCenter)
                {
                    Identifier = "buttonPanel"
                };
                saveGamePanel.AddChild(buttonPanel);

                // Play
                var saveButton = new Button("Save on Selected Slot", ButtonSkin.Default, Anchor.TopCenter)
                {
                    Identifier = "saveButton",
                    Enabled = false,
                    Size = new Vector2(350, 90)
                };
                buttonPanel.AddChild(saveButton);

                // Back
                var backButton = new Button("Back", ButtonSkin.Default, Anchor.BottomRight)
                {
                    Identifier = "backButton",
                    Size = new Vector2(145, 90),
                    Offset = new Vector2(0, -20)
                };
                buttonPanel.AddChild(backButton);

                // Set OnClick GameSave Panel
                foreach (var gameSavePanel in SaveMenuSaveSlots)
                {
                    gameSavePanel.OnClick = (entity) =>
                    {
                        // Set Enable Buttons
                        saveButton.Enabled = true;

                        SelectedGameSavePanel = gameSavePanel;
                        UpdateSelectedGameSave(SaveMenu);

                        PlaySoundEffect(Sound.Click1);
                    };
                }

                // Set OnClick Buttons on Load Save
                // Save
                saveButton.OnClick = (Entity btn) =>
                {
                    GeonBit.UI.Utils.MessageBox.ShowMsgBox("Save on Selected Slot!", "Do you want to save the game on selected slot?"
                        , new GeonBit.UI.Utils.MessageBox.MsgBoxOption[]
                        {
                                new("Yes!", () =>
                                {
                                    JsonFileManager.SaveGame(JsonFileManager.SavingPlayerData);
                                    // Refresh
                                    RefreshGameSave(SaveMenu);
                                    saveButton.Enabled = false;
                                    return true;
                                }),
                                new("No", () =>
                                {
                                    return true;
                                })
                        });
                };

                // Back
                backButton.OnClick = (Entity btn) =>
                {
                    // Closing Save Menu and reset current gui panel
                    // Pause PlayScreen
                    GameGlobals.Instance.IsGamePause = !GameGlobals.Instance.IsGamePause;
                    GameGlobals.Instance.IsOpenGUI = !GameGlobals.Instance.IsOpenGUI;

                    // Toggle the Save Menu flag
                    GameGlobals.Instance.IsOpenSaveMenuPanel = false;
                    GameGlobals.Instance.IsRefreshPlayScreenUI = false;
                    CurrentUI = PlayScreen;

                    // Reset Buttons
                    saveButton.Enabled = false;

                    IsClickedLoadButton = false;
                };
            }
        }

        public void RefreshSaveMenu()
        {
            var saveMenuPanel = _mainPanels[SaveMenu];
            var saveGamePanel = saveMenuPanel.Children.FirstOrDefault
                (e => e.Identifier.Equals("saveGamePanel"));
            var buttonPanel = saveGamePanel.Children.FirstOrDefault
                (e => e.Identifier.Equals("buttonPanel"));

            var saveButton = buttonPanel.Children.OfType<Button>().FirstOrDefault
                (e => e.Identifier.Equals("saveButton"));
            var backButton = buttonPanel.Children.OfType<Button>().FirstOrDefault
                (e => e.Identifier.Equals("backButton"));

            // Reset Buttons
            saveButton.Enabled = false;

            RefreshGameSave(SaveMenu);

            IsClickedLoadButton = true;
        }

        private void InitTradingUI()
        {
            // Main Panel
            var tradingPanel = new Panel(new Vector2(1200, 650), PanelSkin.None, Anchor.Center)
            {
                Visible = true,
                Identifier = "tradingPanel"
            };
            _mainPanels.Add(tradingPanel);
            UserInterface.Active.AddEntity(tradingPanel);

            // create panel tabs
            var tradingTabs = new PanelTabs()
            {
                Identifier = "tradingTabs",
                BackgroundSkin = PanelSkin.Fancy,
                Anchor = Anchor.Center,
                Offset = new Vector2(0, -50)
            };
            tradingPanel.AddChild(tradingTabs);

            // Close button
            var closeButton = new Button("Close", anchor: Anchor.BottomRight
                , size: new Vector2(200, -1), offset: new Vector2(64, -120))
            {
                Identifier = "closeButton",
                Skin = ButtonSkin.Fancy,
                OnClick = (Entity entity) =>
                {
                    // Closing TradingPanel and reset current gui panel
                    // Pause PlayScreen
                    GameGlobals.Instance.IsGamePause = !GameGlobals.Instance.IsGamePause;
                    GameGlobals.Instance.IsOpenGUI = !GameGlobals.Instance.IsOpenGUI;

                    InteractingVendor.IsInteracting = !InteractingVendor.IsInteracting;

                    // Toggle the IsOpenTradingPanel flag
                    GameGlobals.Instance.IsOpenTradingPanel = false;
                    GameGlobals.Instance.IsRefreshPlayScreenUI = false;
                    CurrentUI = PlayScreen;
                }
            };
            tradingPanel.AddChild(closeButton);

            // Buy button
            var buyButton = new Button("Buy", anchor: Anchor.BottomLeft
                , size: new Vector2(200, -1), offset: new Vector2(64, -120))
            {
                Identifier = "buyButton",
                Enabled = false,
                Visible = false,
                Skin = ButtonSkin.Fancy
            };
            tradingPanel.AddChild(buyButton);

            // Sell button
            var sellButton = new Button("Sell", anchor: Anchor.BottomLeft
                , size: new Vector2(200, -1), offset: new Vector2(64, -120))
            {
                Identifier = "sellButton",
                Enabled = false,
                Visible = false,
                Skin = ButtonSkin.Fancy
            };
            tradingPanel.AddChild(sellButton);

            // selecting quantity
            var bgPanel = new Panel()
            {
                Identifier = "bgPanel",
                Size = new Vector2(
                    GameGlobals.Instance.DefaultAdapterViewport.X,
                    GameGlobals.Instance.DefaultAdapterViewport.Y),
                Visible = false
            };
            bgPanel.SetCustomSkin(GetGuiTexture(GuiTextureName.Alpha_BG));
            tradingPanel.AddChild(bgPanel);
            var selectingQuantityPanel = new Panel()
            {
                Identifier = "selectingQuantityPanel",
                Size = new Vector2(500, 300),
                Visible = false,
                AdjustHeightAutomatically = true
            };
            tradingPanel.AddChild(selectingQuantityPanel);
            selectingQuantityPanel.AddChild(new Header("Select Quantity", Anchor.AutoCenter));
            selectingQuantityPanel.AddChild(new HorizontalLine());
            selectingQuantityPanel.AddChild(new Paragraph(
                "Select the number of items you want to purchase.",
                Anchor.AutoCenter)
            {
                Identifier = "paragraphText",
                WrapWords = true
            });
            selectingQuantityPanel.AddChild(new LineSpace(1));

            var selectButtonPanel = new Panel(new Vector2(450, 60), PanelSkin.None, Anchor.AutoCenter)
            {
                Identifier = "selectButtonPanel"
            };
            selectingQuantityPanel.AddChild(selectButtonPanel);

            var minutsButton = new Button("-", ButtonSkin.Default)
            {
                Identifier = "minutsButton",
                Anchor = Anchor.TopLeft,
                Size = new Vector2(50, 50)
            };
            selectButtonPanel.AddChild(minutsButton);

            var plusButton = new Button("+", ButtonSkin.Default)
            {
                Identifier = "plusButton",
                Anchor = Anchor.TopRight,
                Size = new Vector2(50, 50)
            };
            selectButtonPanel.AddChild(plusButton);
            selectingQuantityPanel.AddChild(new LineSpace(2));

            var quantitySlider = new Slider(1, 10, SliderSkin.Default, Anchor.AutoCenter)
            {
                Identifier = "quantitySlider",
                Value = 1
            };
            var quantityLabel = new Paragraph("Quantity: " + quantitySlider.Min, Anchor.AutoCenter)
            {
                Identifier = "quantityText",
            };
            quantitySlider.OnValueChange = (e) =>
            {
                quantityLabel.Text = "Quantity: " + quantitySlider.Value;
            };
            selectingQuantityPanel.AddChild(quantityLabel);
            selectingQuantityPanel.AddChild(quantitySlider);
            selectingQuantityPanel.AddChild(new LineSpace(1));

            var Select = new Button("Select", anchor: Anchor.AutoInline)
            {
                Identifier = "Select",
                Size = new Vector2(225, -1),
                OnClick = (e) =>
                {
                    PurchaseOrSell(tradingTabs, quantitySlider.Value);
                }
            };
            selectingQuantityPanel.AddChild(Select);

            // Set OnClick selectButtons
            minutsButton.OnClick = (btn) =>
            {
                quantitySlider.Value--;
            };
            plusButton.OnClick = (btn) =>
            {
                quantitySlider.Value++;
            };

            var Cancel = new Button("Cancel", anchor: Anchor.AutoInlineNoBreak)
            {
                Identifier = "Cancel",
                Size = new Vector2(225, -1),
                OnClick = (e) =>
                {
                    buyButton.Locked = false;
                    sellButton.Locked = false;
                    closeButton.Locked = false;

                    bgPanel.Visible = false;
                    selectingQuantityPanel.Visible = false;
                }
            };
            selectingQuantityPanel.AddChild(Cancel);

            // Add tab: Buy Item
            {
                TabData buyItemTab = tradingTabs.AddTab("Buy Item");

                // Set true if character tab is clicked
                buyItemTab.button.OnClick = (Entity entity) =>
                {
                    RefreshTradingItem("Buy Item");

                    buyButton.Enabled = false;             
                    sellButton.Enabled = false;
                };

                // Left Panel
                var leftPanel = new Panel(new Vector2(500, 600), PanelSkin.Simple, Anchor.TopLeft)
                {
                    Identifier = "leftPanel"
                };
                leftPanel.SetCustomSkin(GetGuiTexture(GuiTextureName.drake_shop_window));
                buyItemTab.panel.AddChild(leftPanel);

                var descripPanel = new Panel(new Vector2(450, 225), PanelSkin.ListBackground, Anchor.AutoCenter)
                {
                    Identifier = "descripPanel",
                    PanelOverflowBehavior = PanelOverflowBehavior.VerticalScroll
                };
                descripPanel.Scrollbar.AdjustMaxAutomatically = true;
                descripPanel.OnMouseEnter = (e) =>
                {
                    UserInterface.Active.ActiveEntity = descripPanel;
                };

                descripPanel.AddChild(new Label("iconLabel", Anchor.AutoCenter)
                {
                    Scale = 1.25f,
                    ClickThrough = true,
                    Identifier = "iconLabel"
                });
                descripPanel.AddChild(new Image(GetGuiTexture(GuiTextureName.gold_coin))
                {
                    Size = new Vector2(24, 24),
                    Anchor = Anchor.AutoInline,
                    ClickThrough = true,
                });
                descripPanel.AddChild(new Paragraph("priceLabel", Anchor.AutoInlineNoBreak)
                {
                    Scale = 1.1f,
                    Offset = new Vector2(15, 0),
                    ClickThrough = true,
                    Identifier = "priceLabel"
                });
                descripPanel.AddChild(new LineSpace(1));
                descripPanel.AddChild(new Paragraph("description", Anchor.AutoInline)
                {
                    Scale = 1f,
                    WrapWords = true,
                    ClickThrough = true,
                    Identifier = "description"
                });
                descripPanel.AddChild(new LineSpace(1));

                leftPanel.AddChild(new Icon(IconType.None, Anchor.AutoCenter, 1, false)
                {
                    Size = new Vector2(300, 300),
                    Locked = true,
                    Identifier = "itemIcon"
                });
                leftPanel.AddChild(new LineSpace(1));
                leftPanel.AddChild(descripPanel);

                // Right Panel
                var rightPanel = new Panel(new Vector2(600, 600), PanelSkin.None, Anchor.TopRight)
                {
                    Identifier = "rightPanel"
                };
                buyItemTab.panel.AddChild(rightPanel);

                rightPanel.AddChild(new Header("BUY ITEM"));
                rightPanel.AddChild(new LineSpace(1));

                var listItemPanel = new Panel(new Vector2(550, 450), PanelSkin.ListBackground, Anchor.AutoCenter)
                {
                    PanelOverflowBehavior = PanelOverflowBehavior.VerticalScroll,
                    Identifier = "listItemPanel",
                    Padding = new Vector2(10, 10)
                };
                listItemPanel.Scrollbar.AdjustMaxAutomatically = true;
                listItemPanel.OnMouseEnter = (e) =>
                {
                    UserInterface.Active.ActiveEntity = listItemPanel;
                };
                rightPanel.AddChild(listItemPanel);
            }

            // Add tab: Sell Item
            {
                TabData sellItemTab = tradingTabs.AddTab("Sell Item");

                // Set true if character tab is clicked
                sellItemTab.button.OnClick = (Entity entity) =>
                {
                    RefreshTradingItem("Sell Item");

                    buyButton.Enabled = false;
                    sellButton.Enabled = false;
                };

                // Left Panel
                var leftPanel = new Panel(new Vector2(500, 600), PanelSkin.Simple, Anchor.TopLeft)
                {
                    Identifier = "leftPanel"
                };
                leftPanel.SetCustomSkin(GetGuiTexture(GuiTextureName.drake_shop_window));
                sellItemTab.panel.AddChild(leftPanel);

                var descripPanel = new Panel(new Vector2(450, 250), PanelSkin.ListBackground, Anchor.AutoCenter)
                {
                    Identifier = "descripPanel",
                    PanelOverflowBehavior = PanelOverflowBehavior.VerticalScroll
                };
                descripPanel.Scrollbar.AdjustMaxAutomatically = true;
                descripPanel.OnMouseEnter = (e) =>
                {
                    UserInterface.Active.ActiveEntity = descripPanel;
                };

                descripPanel.AddChild(new Label("iconLabel", Anchor.AutoCenter)
                {
                    Scale = 1.25f,
                    ClickThrough = true,
                    Identifier = "iconLabel"
                });
                descripPanel.AddChild(new Image(GetGuiTexture(GuiTextureName.gold_coin))
                {
                    Size = new Vector2(24, 24),
                    Anchor = Anchor.AutoInline,
                    ClickThrough = true,
                });
                descripPanel.AddChild(new Paragraph("priceLabel", Anchor.AutoInlineNoBreak)
                {
                    Scale = 1.1f,
                    Offset = new Vector2(15, 0),
                    ClickThrough = true,
                    Identifier = "priceLabel"
                });             
                descripPanel.AddChild(new LineSpace(1));
                descripPanel.AddChild(new Paragraph("description", Anchor.AutoInline)
                {
                    Scale = 1f,
                    WrapWords = true,
                    ClickThrough = true,
                    Identifier = "description"
                });
                descripPanel.AddChild(new LineSpace(1));

                leftPanel.AddChild(new Icon(IconType.None, Anchor.AutoCenter, 1, false)
                {
                    Size = new Vector2(300, 300),
                    Locked = true,
                    Identifier = "itemIcon"
                });
                leftPanel.AddChild(new LineSpace(1));
                leftPanel.AddChild(descripPanel);

                // Right Panel
                var rightPanel = new Panel(new Vector2(600, 600), PanelSkin.None, Anchor.TopRight)
                {
                    Identifier = "rightPanel"
                };
                sellItemTab.panel.AddChild(rightPanel);

                rightPanel.AddChild(new Header("SELL ITEM"));
                rightPanel.AddChild(new LineSpace(1));

                var listItemPanel = new Panel(new Vector2(550, 450), PanelSkin.ListBackground, Anchor.AutoCenter)
                {
                    PanelOverflowBehavior = PanelOverflowBehavior.VerticalScroll,
                    Identifier = "listItemPanel",
                    Padding = new Vector2(10, 10)
                };
                listItemPanel.Scrollbar.AdjustMaxAutomatically = true;
                listItemPanel.OnMouseEnter = (e) =>
                {
                    UserInterface.Active.ActiveEntity = listItemPanel;
                };
                rightPanel.AddChild(listItemPanel);
            }

            // Set on Click Button
            // Buy
            buyButton.OnClick = (e) =>
            {
                bgPanel.Visible = true;
                selectingQuantityPanel.Visible = true;

                // Set Maximum quantity that can be purchased
                var purchaseItemId = SelectedPurchaseItem.ItemId;
                var itemData = GameGlobals.Instance.ItemsDatas.FirstOrDefault
                    (e => e.ItemId.Equals(purchaseItemId));
                quantitySlider.Max = InventoryManager.Instance.GoldCoin / itemData.BuyingPrice;
                quantitySlider.Value = quantitySlider.Min;

                buyButton.Locked = true;
                closeButton.Locked = true;
            };

            // Sell
            sellButton.OnClick = (e) =>
            {
                bgPanel.Visible = true;
                selectingQuantityPanel.Visible = true;

                // Set Maximum quantity that can be sold
                var currItemInv = InventoryManager.Instance.SelectedItem;
                quantitySlider.Max = currItemInv.Value.Count;
                quantitySlider.Value = quantitySlider.Min;

                sellButton.Locked = true;
                closeButton.Locked = true;
            };
        }

        private void PurchaseOrSell(PanelTabs tradingTabs, int quantity)
        {
            if (tradingTabs.ActiveTab.name.Equals("Buy Item"))
            {
                var purchaseItemId = SelectedPurchaseItem.ItemId;
                var itemData = GameGlobals.Instance.ItemsDatas.FirstOrDefault
                    (e => e.ItemId.Equals(purchaseItemId));

                GeonBit.UI.Utils.MessageBox.ShowMsgBox("Confirm Buying Item"
                        , $"Do you want to purchase {itemData.Name} x {quantity}?"
                        , new GeonBit.UI.Utils.MessageBox.MsgBoxOption[]
                        {
                                new("Yes", () =>
                                {
                                    InventoryManager.Instance.PurchaseItem(
                                        itemData.ItemId,
                                        quantity,
                                        itemData.BuyingPrice);

                                    OnTradingItem(new TradingItemEventArgs("Buying", itemData, quantity));
                                    RefreshTradingItem("Buy Item");
                                    ClearSelectingQuantityPanel();
                                    return true;
                                }),
                                new("No", () =>
                                {
                                    return true;
                                })
                        });
            }
            else if (tradingTabs.ActiveTab.name.Equals("Sell Item"))
            {
                var currItemInv = InventoryManager.Instance.SelectedItem;
                var itemData = GameGlobals.Instance.ItemsDatas.FirstOrDefault
                    (e => e.ItemId.Equals(currItemInv.Value.ItemId));

                GeonBit.UI.Utils.MessageBox.ShowMsgBox("Confirm Selling Item"
                        , $"Do you want to sell {currItemInv.Value.GetName()} x {quantity}?"
                        , new GeonBit.UI.Utils.MessageBox.MsgBoxOption[]
                        {
                                new("Yes", () =>
                                {
                                    InventoryManager.Instance.SellItem(
                                        currItemInv.Key,
                                        currItemInv.Value,
                                        quantity);

                                    OnTradingItem(new TradingItemEventArgs("Selling", itemData, quantity));
                                    RefreshTradingItem("Sell Item");
                                    ClearSelectingQuantityPanel();
                                    return true;
                                }),
                                new("No", () =>
                                {
                                    return true;
                                })
                        });
            }
        }

        private void ClearSelectingQuantityPanel()
        {
            var tradingPanel = _mainPanels[TradingPanel];

            var closeButton = tradingPanel.Children.FirstOrDefault
                (p => p.Identifier.Equals("closeButton"));
            var buyButton = tradingPanel.Children.FirstOrDefault
                (p => p.Identifier.Equals("buyButton"));
            var sellButton = tradingPanel.Children.FirstOrDefault
                (p => p.Identifier.Equals("sellButton"));

            var bgPanel = tradingPanel.Children.FirstOrDefault
                (p => p.Identifier.Equals("bgPanel"));
            var selectingQuantityPanel = tradingPanel.Children.FirstOrDefault
                (p => p.Identifier.Equals("selectingQuantityPanel"));

            buyButton.Locked = false;
            sellButton.Locked = false;
            closeButton.Locked = false;

            bgPanel.Visible = false;
            selectingQuantityPanel.Visible = false;
        }

        public void RefreshTradingItem(string selectedTab)
        {
            if (InteractingVendor == null) return;

            var tradingPanel = _mainPanels[TradingPanel];
            var tradingTabs = tradingPanel.Children.OfType<PanelTabs>().FirstOrDefault
                (t => t.Identifier.Equals("tradingTabs"));
            tradingTabs.SelectTab(selectedTab);

            var buyButton = tradingPanel.Children.FirstOrDefault
                (p => p.Identifier.Equals("buyButton"));
            var sellButton = tradingPanel.Children.FirstOrDefault
                (p => p.Identifier.Equals("sellButton"));

            var currentTabPanel = tradingTabs.ActiveTab.panel;
            var leftPanel = currentTabPanel.Children.FirstOrDefault
                (p => p.Identifier.Equals("leftPanel"));

            var rightPanel = currentTabPanel.Children.FirstOrDefault
                (p => p.Identifier.Equals("rightPanel"));
            var listItemPanel = rightPanel.Children.FirstOrDefault
                (p => p.Identifier.Equals("listItemPanel"));

            // Clear list item display
            listItemPanel.ClearChildren();

            foreach (var itemLeftPanel in leftPanel.Children)
                itemLeftPanel.Visible = false;

            if (selectedTab.Equals("Buy Item"))
            {
                buyButton.Visible = true;
                sellButton.Visible = false;

                var tempIconItems = new List<Icon>();
                foreach (var item in InteractingVendor.TradingItems)
                {
                    // Item Icon
                    var iconItem = new Icon(IconType.None, Anchor.AutoInline, 1, true)
                    {
                        ItemId = item.ItemId,
                        Texture = GetItemTexture(item.ItemId),
                    };

                    tempIconItems.Add(iconItem);
                }

                // Sort Item Display by Id and add to listItemPanel
                var sortedIconItems = tempIconItems.OrderBy(icon => icon.ItemId).ToList();
                foreach (var sortediconItem in sortedIconItems)
                    listItemPanel.AddChild(sortediconItem);

                // Set OnClick
                foreach (var iconItem in listItemPanel.Children.OfType<Icon>())
                {
                    iconItem.OnClick = (e) =>
                    {
                        SetIconItemTradingDisplay("Buy Item", iconItem);

                        foreach (var itemLeftPanel in leftPanel.Children)
                            itemLeftPanel.Visible = true;

                        buyButton.Enabled = true;

                        // Set Selected Purchase Item
                        SelectedPurchaseItem = iconItem;
                    };
                }
            }
            else if (selectedTab.Equals("Sell Item"))
            {
                buyButton.Visible = false;
                sellButton.Visible = true;

                // Set item Display (Only item that has Slot = DefaultInventorySlot)
                var tempIconItems = new List<Icon>();
                foreach (var item in InventoryManager.Instance.InventoryBag.Where
                    (i => i.Value.Slot == GameGlobals.Instance.DefaultInventorySlot))
                {
                    // Item Icon
                    var iconItem = new Icon(IconType.None, Anchor.AutoInline, 1, true)
                    {
                        ItemId = item.Value.ItemId,
                        Count = item.Value.Count,
                        Slot = item.Value.Slot,
                        KeyIndex = item.Key,
                        Texture = GetItemTexture(item.Value.ItemId),
                    };

                    // Item text
                    var text = iconItem.Count.ToString();
                    if (item.Value.GetCategory().Equals("Equipment"))
                        text = "";

                    var iconText = new Label(text, Anchor.BottomRight, offset: new Vector2(-22, -35))
                    {
                        Size = new Vector2(10, 10),
                        Scale = 1f,
                        ClickThrough = true,
                    };

                    iconItem.AddChild(iconText);
                    tempIconItems.Add(iconItem);
                }

                // Sort Item Display by Id and add to listItemPanel
                var sortedIconItems = tempIconItems.OrderBy(icon => icon.ItemId).ToList();
                foreach (var sortediconItem in sortedIconItems)
                    listItemPanel.AddChild(sortediconItem);

                // Set OnClick
                foreach (var iconItem in listItemPanel.Children.OfType<Icon>())
                {
                    iconItem.OnClick = (e) =>
                    {
                        SetIconItemTradingDisplay("Sell Item", iconItem);

                        foreach (var itemLeftPanel in leftPanel.Children)
                            itemLeftPanel.Visible = true;

                        // Enable or Disable the button for each item icon
                        var item = InventoryManager.Instance.InventoryBag.FirstOrDefault
                            (i => i.Key.Equals(iconItem.KeyIndex));

                        InventoryManager.Instance.SelectedItem = item;

                        sellButton.Enabled = true;
                    };
                }
            }
        }

        private void SetIconItemTradingDisplay(string selectedTab, Icon icon)
        {
            var itemData = GameGlobals.Instance.ItemsDatas.FirstOrDefault
                (i => i.ItemId.Equals(icon.ItemId));

            var tradingPanel = _mainPanels[TradingPanel];
            var tradingTabs = tradingPanel.Children.OfType<PanelTabs>().FirstOrDefault
                (t => t.Identifier.Equals("tradingTabs"));
            tradingTabs.SelectTab(selectedTab);

            var currentTabPanel = tradingTabs.ActiveTab.panel;
            var leftPanel = currentTabPanel.Children.FirstOrDefault
                (p => p.Identifier.Equals("leftPanel"));
            var descripPanel = leftPanel.Children.FirstOrDefault
                (p => p.Identifier.Equals("descripPanel"));

            var itemIcon = leftPanel.Children.OfType<Icon>().FirstOrDefault
                (i => i.Identifier.Equals("itemIcon"));
            itemIcon.Texture = icon.Texture;

            var iconLabel = descripPanel.Children.OfType<Label>().FirstOrDefault
                (i => i.Identifier.Equals("iconLabel"));
            iconLabel.Text = itemData.Name;

            var priceLabel = descripPanel.Children.OfType<Paragraph>().FirstOrDefault
                (i => i.Identifier.Equals("priceLabel"));
            priceLabel.Text = selectedTab.Equals("Buy Item") ? $"Buying Price: {itemData.BuyingPrice}" : $"Selling Price: {itemData.SellingPrice}";

            var description = descripPanel.Children.OfType<Paragraph>().FirstOrDefault
                (i => i.Identifier.Equals("description"));
            description.Text = itemData.Description;
        }

        private void InitWarpPointUI()
        {
            var warpPointPanel = new Panel(
                new Vector2(GameGlobals.Instance.GameScreen.X, GameGlobals.Instance.GameScreen.X),
                PanelSkin.None,
                Anchor.Center)
            {
                Identifier = "warpPointPanel"
            };
            _mainPanels.Add(warpPointPanel);
            UserInterface.Active.AddEntity(warpPointPanel);

            warpPointPanel.AddChild(new Header("Warp Point", Anchor.TopCenter));

            var mapImage = new Image()
            {
                Identifier = "mapImage",
                Size = new Vector2(500, 500),
                Anchor = Anchor.Center
            };
            warpPointPanel.AddChild(mapImage);

            var prevWarpPoint = new Button("", ButtonSkin.Default)
            {
                Identifier = "prevWarpPoint",
                Anchor = Anchor.CenterLeft,
                Size = new Vector2(50, 50),
                Offset = new Vector2(250, 0),
                OnClick = (btn) =>
                {
                    UpdateSelectedWarpPoint(false);
                }
            };
            warpPointPanel.AddChild(prevWarpPoint);
            prevWarpPoint.ButtonParagraph.SetAnchorAndOffset(Anchor.AutoCenter, new Vector2(0, -35));
            prevWarpPoint.AddChild(new Icon(IconType.None, Anchor.AutoCenter)
            {
                Texture = GetGuiTexture(GuiTextureName.arrow_left)
            }, true);

            var nextWarpPoint = new Button("", ButtonSkin.Default)
            {
                Identifier = "nextWarpPoint",
                Anchor = Anchor.CenterRight,
                Size = new Vector2(50, 50),
                Offset = new Vector2(250, 0),
                OnClick = (btn) =>
                {
                    UpdateSelectedWarpPoint(true);
                }
            };
            warpPointPanel.AddChild(nextWarpPoint);
            nextWarpPoint.ButtonParagraph.SetAnchorAndOffset(Anchor.AutoCenter, new Vector2(0, -35));
            nextWarpPoint.AddChild(new Icon(IconType.None, Anchor.AutoCenter)
            {
                Texture = GetGuiTexture(GuiTextureName.arrow_right)
            }, true);

            // Warp Button
            var warpButton = new Button("Warp!", anchor: Anchor.BottomCenter
                , size: new Vector2(200, -1), offset: new Vector2(0, 300))
            {
                Identifier = "warpButton",
                Enabled = false,
                Skin = ButtonSkin.Fancy,
                OnClick = (e) =>
                {
                    // Closing WarpPoint and reset current gui panel
                    // Pause PlayScreen
                    GameGlobals.Instance.IsGamePause = !GameGlobals.Instance.IsGamePause;
                    GameGlobals.Instance.IsOpenGUI = !GameGlobals.Instance.IsOpenGUI;

                    // Toggle the IsOpenWarpPointPanel flag
                    GameGlobals.Instance.IsOpenWarpPointPanel = false;
                    GameGlobals.Instance.IsRefreshPlayScreenUI = false;
                    CurrentUI = PlayScreen;

                    string loadMapAction = string.Empty;
                    switch (_selectedWarpPointId)
                    {
                        case NoahHome:
                            loadMapAction = "warp_to_noah_home";
                            break;

                        case NordlingenTown:
                            loadMapAction = "warp_to_map_1";
                            break;

                        case RothenburgTown:
                            loadMapAction = "warp_to_map_2";
                            break;

                        case TallinnTown:
                            loadMapAction = "warp_to_map_2";
                            break;
                    }

                    GameGlobals.Instance.InitialCameraPos = PlayerManager.Instance.Player.Position;
                    var currLoadMapAction = ScreenManager.GetLoadMapAction(loadMapAction);

                    // if the zoneArea.Name not be found in LoadMapAction then return
                    if (currLoadMapAction == ScreenManager.EntranceZoneName.LoadSave) return;

                    ScreenManager.Instance.LoadMapByEntranceZone = currLoadMapAction;
                    ScreenManager.Instance.TransitionToScreen(ScreenManager.Instance.GetPlayScreenByLoadMapAction());

                    PlaySoundEffect(Sound.Warp);
                }
            };
            warpPointPanel.AddChild(warpButton);

            // Close button
            var closeButton = new Button("Close", anchor: Anchor.BottomRight
                , size: new Vector2(200, -1), offset: new Vector2(64, 300))
            {
                Identifier = "closeButton",
                Skin = ButtonSkin.Fancy,
                OnClick = (e) =>
                {
                    // Closing WarpPoint and reset current gui panel
                    // Pause PlayScreen
                    GameGlobals.Instance.IsGamePause = !GameGlobals.Instance.IsGamePause;
                    GameGlobals.Instance.IsOpenGUI = !GameGlobals.Instance.IsOpenGUI;

                    // Toggle the IsOpenWarpPointPanel flag
                    GameGlobals.Instance.IsOpenWarpPointPanel = false;
                    GameGlobals.Instance.IsRefreshPlayScreenUI = false;
                    CurrentUI = PlayScreen;
                }
            };
            warpPointPanel.AddChild(closeButton);
        }

        public void RefreshWarpPointUI()
        {
            var warpPointPanel = _mainPanels[WarpPointPanel];

            var mapImage = warpPointPanel.Children.OfType<Image>().FirstOrDefault
                (e => e.Identifier.Equals("mapImage"));

            var warpButton = warpPointPanel.Children.FirstOrDefault
                (e => e.Identifier.Equals("warpButton"));

            if (_selectedWarpPointId == -1)
            {
                mapImage.Texture = GetGuiTexture(GuiTextureName.Warp_NoahsHome);
                warpButton.Enabled = true;
                return;
            }

            var warpPointData = PlayerManager.Instance.Player.PlayerData.ChapterProgression.FirstOrDefault
                (c => c.ChapterId.Equals(_selectedWarpPointId + 1));

            switch (warpPointData.ChapterId)
            {
                case 1:
                    if (warpPointData.IsWarpPointUnlock)
                    {
                        mapImage.Texture = GetGuiTexture(GuiTextureName.Warp_NordlingenTown);
                    }
                    else mapImage.Texture = GetGuiTexture(GuiTextureName.Warp_NordlingenTown_lock);
                    break;

                case 2:
                    if (warpPointData.IsWarpPointUnlock)
                    {
                        mapImage.Texture = GetGuiTexture(GuiTextureName.Warp_RothenburgTown);
                    }
                    else mapImage.Texture = GetGuiTexture(GuiTextureName.Warp_RothenburgTown_lock);
                    break;

                case 3:
                    if (warpPointData.IsWarpPointUnlock)
                    {
                        mapImage.Texture = GetGuiTexture(GuiTextureName.Warp_TallinnTown);
                    }
                    else mapImage.Texture = GetGuiTexture(GuiTextureName.Warp_TallinnTown_lock);
                    break;
            }

            if (warpPointData.IsWarpPointUnlock)
            {
                warpButton.Enabled = true;
            }
            else warpButton.Enabled = false;
        }

        // Define a delegate for the event handler
        public delegate void UIEventHandler(object sender, EventArgs e);

        // Define an event based on the delegate
        public event UIEventHandler CloseDialogEventHandler;

        public event UIEventHandler TradeEventHandler;

        // Method to raise the DialogClosed event
        public virtual void OnDialogClosed(DialogClosedEventArgs e)
        {
            CloseDialogEventHandler?.Invoke(this, e);
        }

        public virtual void OnTradingItem(TradingItemEventArgs e)
        {
            TradeEventHandler?.Invoke(this, e);
        }

        public static UIManager Instance
        {
            get
            {
                instance ??= new UIManager();
                return instance;
            }
        }
    }

    public class DialogClosedEventArgs(DialogData DialogData) : EventArgs
    {
        public DialogData DialogData { get; } = DialogData;
    }

    public class TradingItemEventArgs(string action, ItemData itemData, int quantity) : EventArgs
    {
        public string Action { get; } = action;
        public ItemData ItemData { get; } = itemData;
        public int Quantity { get; } = quantity;
    }
}
