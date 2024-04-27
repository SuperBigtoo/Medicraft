using Medicraft.Data.Models;
using Medicraft.Systems.Managers;
using Medicraft.Systems;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using MonoGame.Extended.Sprites;
using MonoGame.Extended;
using System.Collections.Generic;
using System.Linq;

namespace Medicraft.Entities.Mobs
{
    public class FriendlyMob : Entity
    {
        public EntityData EntityData { get; protected set; }
        public AnimatedSprite SignSprite { get; protected set; }
        public List<DialogData> DialogData { get; protected set; }

        public int MiniGameQueueIndex { get; set; } = 0;
        public MiniGameCase MiniGameCaseData { get; set; }

        public enum FriendlyMobType
        {
            Animal,
            Civilian,
            Vendor,
            QuestGiver
        }
        public FriendlyMobType MobType { get; set; }

        public bool IsInteractable { get; set; }
        public bool IsInteracting { get; set; }
        public bool IsDetected { get; set; }
        public bool IsAllwaysShowSignSprite { get; set; }

        protected string[] idleSpriteName;
        protected bool isPlayIdle = false;

        protected FriendlyMob()
        {
            IsInteractable = false;
            IsInteracting = false;
            IsDetected = false;
            IsAllwaysShowSignSprite = false;

            SignSprite = new AnimatedSprite(GameGlobals.Instance.UIBooksIconHUD)
            {
                Depth = InitDepth
            };
        }

        public void SetMobType(string mobType)
        {
            switch (mobType)
            {
                default:
                case "Civilian":
                    MobType = FriendlyMobType.Civilian;
                    break;

                case "Vendor":
                    MobType= FriendlyMobType.Vendor;
                    break;

                case "QuestGiver":
                    MobType = FriendlyMobType.QuestGiver;
                    break;

                case "Animal":
                    MobType = FriendlyMobType.Animal;
                    break;
            }
        }

        public override void Update(GameTime gameTime, float playerDepth, float topDepth, float middleDepth, float bottomDepth)
        {
            var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (!PlayerManager.Instance.IsPlayerDead)
            {
                if (!IsInteracting)
                {
                    UpdateTargetNode(deltaSeconds, EntityData);

                    // Setup PathFinding
                    SetPathFindingNode((int)EntityData.Position[0], (int)EntityData.Position[1]);
                }

                // MovementControl
                MovementControl(deltaSeconds);
            }

            // Update layer depth
            UpdateLayerDepth(playerDepth, topDepth, middleDepth, bottomDepth);

            // Update time conditions
            UpdateTimerConditions(deltaSeconds);

            // Update Sign
            // Mini Game Case
            if (Name.Equals("Patient") && MiniGameQueueIndex == -1)
            {
                // heart_1
                SignSprite.Play("heart_1");
                SignSprite.Depth = InitDepth;
                SignSprite.Update(deltaSeconds);
            }

            // Noraml Case
            if ((MobType == FriendlyMobType.Civilian || MobType == FriendlyMobType.Animal)
                && IsInteractable)
            {
                if (IsDetected)
                {
                    SignSprite.Play("interact_1");
                    SignSprite.Depth = InitDepth;
                    SignSprite.Update(deltaSeconds);
                }
            }
            else if (MobType == FriendlyMobType.QuestGiver)
            {              
                if (IsDetected)
                {
                    SignSprite.Play("interact_1");                  
                }
                else
                {
                    var questChapterId = GameGlobals.Instance.QuestDatas.FirstOrDefault
                        (e => e.QuestId.Equals(EntityData.QuestId));

                    var questStamp = PlayerManager.Instance.Player.PlayerData.ChapterProgression.FirstOrDefault
                        (e => e.ChapterId.Equals(questChapterId.ChapterId)).Quests.FirstOrDefault
                            (e => e.QuestId.Equals(questChapterId.QuestId));

                    if (!questStamp.IsQuestClear)
                    {
                        IsAllwaysShowSignSprite = true;
                    }
                    else IsAllwaysShowSignSprite = false;

                    if (questStamp.IsQuestDone)
                    {
                        SignSprite.Play("quest_2");
                    }
                    else SignSprite.Play("quest_1");
                }

                SignSprite.Depth = InitDepth;
                SignSprite.Update(deltaSeconds);
            }

            Sprite.Update(deltaSeconds);
        }

        protected override void MovementControl(float deltaSeconds)
        {
            var walkSpeed = deltaSeconds * Speed;
            prevPos = Position;

            // Check Object Collsion
            CheckCollision();

            // Play Sprite: Idle 
            if (!IsMoving && !isPlayIdle)
            {
                isPlayIdle = true;
                int randomIndex = new Random().Next(idleSpriteName.Length);
                var idle = idleSpriteName[randomIndex];
                CurrentAnimation = SpriteCycle + idle;
                Sprite.Play(CurrentAnimation);
            }

            // Check movement according to PathFinding
            if (ScreenManager.Instance.IsScreenLoaded && !IsInteracting)
            {
                if (pathFinding != null || pathFinding.GetPath().Count != 0)
                {
                    if (currentNodeIndex < pathFinding.GetPath().Count - stoppingNodeIndex)
                    {
                        // Calculate direction to the next node
                        var direction = new Vector2(
                            pathFinding.GetPath()[currentNodeIndex + 1].Col - pathFinding.GetPath()[currentNodeIndex].Col,
                            pathFinding.GetPath()[currentNodeIndex + 1].Row - pathFinding.GetPath()[currentNodeIndex].Row);
                        direction.Normalize();

                        // Move the character towards the next node
                        Position += direction * walkSpeed;
                        IsMoving = true;
                        isPlayIdle = false;

                        // Check Animation
                        if (direction.Y < 0)
                        {
                            CurrentAnimation = SpriteCycle + "_walking_up";     // Up
                        }
                        if (direction.Y > 0)
                        {
                            CurrentAnimation = SpriteCycle + "_walking_down";     // Down
                        }
                        if (direction.X < 0)
                        {
                            CurrentAnimation = SpriteCycle + "_walking_left";     // Left
                        }
                        if (direction.X > 0)
                        {
                            CurrentAnimation = SpriteCycle + "_walking_right";     // Right
                        }

                        Sprite.Play(CurrentAnimation);

                        // Check if the character has passed the next node
                        var tileSize = GameGlobals.Instance.TILE_SIZE;
                        var nextNodePosition = new Vector2(
                            (pathFinding.GetPath()[currentNodeIndex + 1].Col * tileSize) + tileSize / 2,
                            (pathFinding.GetPath()[currentNodeIndex + 1].Row * tileSize) + tileSize / 2);

                        var boundingCenter = new Vector2(
                            BoundingDetectCollisions.Center.X,
                            BoundingDetectCollisions.Center.Y);

                        if ((boundingCenter - nextNodePosition).Length() < tileSize / 2)
                        {
                            currentNodeIndex++;
                        }

                        //if (GetType().Name.Equals("Civilian"))
                        //{
                        //    System.Diagnostics.Debug.WriteLine($"currentNodeIndex : {currentNodeIndex} | {pathFinding.GetPath().Count}");
                        //    System.Diagnostics.Debug.WriteLine($"Position : {boundingCenter}");
                        //    System.Diagnostics.Debug.WriteLine($"nextNodePosition : {nextNodePosition}");
                        //    System.Diagnostics.Debug.WriteLine($"direction : {direction}");
                        //    System.Diagnostics.Debug.WriteLine($"Length : {(boundingCenter - nextNodePosition).Length()}");
                        //}
                    }
                    else IsMoving = false;
                }
            }
            else IsMoving = false;
        }

        public void Interact()
        {
            if (Name.Equals("Patient"))
            {
                var isMedicineFound = false;
                var medicineItem = PlayerManager.Instance.SelectedHotbarItem;

                if (medicineItem.Value != null)
                {
                    foreach (var targetId in MiniGameCaseData.TargetId)
                    {
                        if (medicineItem.Value.ItemId.Equals(targetId))
                        {
                            GameGlobals.Instance.DequeuePatient(this);
                            InventoryManager.Instance.RemoveItem(medicineItem.Key, medicineItem.Value, 1);
                            UIManager.Instance.RefreshHotbar();
                            isMedicineFound = true;
                            break;
                        }
                    }
                }          

                if (isMedicineFound)
                {
                    return;
                }
                else UIManager.Instance.CreateDialog(this);
            }

            UIManager.Instance.CreateDialog(this);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Sprite, Transform);

            var shadowTexture = GameGlobals.GetShadowTexture(GameGlobals.ShadowTextureName.shadow_1);

            DrawShadow(spriteBatch, shadowTexture);

            // Test Draw BoundingRec for Collision
            if (GameGlobals.Instance.IsDebugMode)
            {
                var pixelTexture = new Texture2D(ScreenManager.Instance.GraphicsDevice, 1, 1);
                pixelTexture.SetData(new Color[] { Color.White });
                spriteBatch.Draw(pixelTexture, (Rectangle)BoundingDetectCollisions, Color.Red);
            }
        }

        public override void DrawShadow(SpriteBatch spriteBatch, Texture2D shadowTexture)
        {
            var position = new Vector2(Position.X - shadowTexture.Width / 2f
                    , BoundingDetectCollisions.Bottom - Sprite.TextureRegion.Height / 10);

            spriteBatch.Draw(shadowTexture, position, null, Color.White
                , 0f, Vector2.Zero, 1f, SpriteEffects.None, Sprite.Depth + 0.0000025f);
        }

        public virtual void DrawDetectedSign(SpriteBatch spriteBatch)
        {
            if (IsDetected || IsAllwaysShowSignSprite || (Name.Equals("Patient") && MiniGameQueueIndex == -1))
            {
                var position = new Vector2(
                    Position.X,
                    Position.Y - ((Sprite.TextureRegion.Height / 2) + SignSprite.TextureRegion.Height));

                var transform = new Transform2
                {
                    Scale = Vector2.One,
                    Rotation = 0f,
                    Position = position
                };

                spriteBatch.Draw(SignSprite, transform);
            }
        }
    }
}
