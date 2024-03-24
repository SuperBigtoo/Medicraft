﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Medicraft.Data.Models;
using Medicraft.Systems;
using Medicraft.Systems.Managers;
using MonoGame.Extended.Serialization;

namespace Medicraft.Data
{
    public class JsonFileManager
    {
        //Load & Save Game Basically
        public static void SaveGame()
        {
            // Get Time
            DateTime dateTime = DateTime.Now;
            string dateTimeString = dateTime.ToString().Replace(' ', '_');

            // Set current Stats          
            PlayerManager.Instance.Player.PlayerData.Level = PlayerManager.Instance.Player.Level;
            PlayerManager.Instance.Player.PlayerData.EXP = PlayerManager.Instance.Player.EXP;
            // HP Percentage
            var hpPercentage = (float)Math.Round(PlayerManager.Instance.Player.HP / PlayerManager.Instance.Player.MaxHP, 2);
            PlayerManager.Instance.Player.PlayerData.CurrentHPPercentage = hpPercentage;
            // Mana Percentage
            var manaPercentage = (float)Math.Round(PlayerManager.Instance.Player.Mana / PlayerManager.Instance.Player.MaxMana, 2);
            PlayerManager.Instance.Player.PlayerData.CurrentManaPercentage = manaPercentage;

            // Set Player Position
            PlayerManager.Instance.Player.PlayerData.CurrentMap = ScreenManager.Instance.CurrentMap;
            PlayerManager.Instance.Player.PlayerData.Position[0] = PlayerManager.Instance.Player.Position.X;
            PlayerManager.Instance.Player.PlayerData.Position[1] = PlayerManager.Instance.Player.Position.Y;

            // Set Current InventoryData
            var inventoryItems = new List<InventoryItemData>();
            foreach (var item in InventoryManager.Instance.InventoryBag.Values)
            {
                inventoryItems.Add(item);
            }
            var inventoryData = new InventoryData() {
                GoldCoin = InventoryManager.Instance.GoldCoin,
                Inventory = inventoryItems
            };
            PlayerManager.Instance.Player.PlayerData.InventoryData = inventoryData;

            // Set save name
            var saveName = "Save_" + dateTimeString;

            // Ser total playtime
            var second = (int)GameGlobals.Instance.TotalPlayTime % 60;
            var minute = (int)GameGlobals.Instance.TotalPlayTime / 60;
            var hour = (int)GameGlobals.Instance.TotalPlayTime / 3600;

            var playTime = new int[] { hour, minute, second};

            if (GameGlobals.Instance.GameSave.Count != 0)
            {
                GameGlobals.Instance.GameSave[GameGlobals.Instance.GameSaveIdex] = new GameSaveData()
                {
                    SaveId = GameGlobals.Instance.GameSave.ElementAt(GameGlobals.Instance.GameSaveIdex).SaveId,
                    Name = GameGlobals.Instance.GameSave.ElementAt(GameGlobals.Instance.GameSaveIdex).Name,
                    CreatedTime = GameGlobals.Instance.GameSave.ElementAt(GameGlobals.Instance.GameSaveIdex).CreatedTime,
                    LastUpdated = dateTimeString,
                    TotalPlayTime = playTime,
                    PlayerData = PlayerManager.Instance.Player.PlayerData
                };
            }
            else
            {
                GameGlobals.Instance.GameSave.Add(new GameSaveData()
                {
                    SaveId = 0,
                    Name = saveName,
                    CreatedTime = dateTimeString,
                    LastUpdated = dateTimeString,
                    TotalPlayTime = playTime,
                    PlayerData = PlayerManager.Instance.Player.PlayerData
                });
            }

            SaveGameFile(GameGlobals.Instance.GameSave, GameGlobals.Instance.GameSavePath);

            // Game Config
            SaveConfig();
        }

        public static void SaveConfig()
        {
            GameGlobals.Instance.GameConfig.IsFullScreen = GameGlobals.Instance.IsFullScreen;
            GameGlobals.Instance.GameConfig.SFXVolume = Math.Round(GameGlobals.Instance.SoundEffectVolume, 2);
            GameGlobals.Instance.GameConfig.BGMusicVolume = Math.Round(GameGlobals.Instance.BackgroundMusicVolume, 2);

            SaveConfigFile(GameGlobals.Instance.GameConfig, GameGlobals.Instance.GameConfigPath);
        }

        public static List<GameSaveData> LoadGameSave(string PATH)
        {
            string fileContents;
            try
            {
                fileContents = File.ReadAllText(PATH);
            } 
            catch (IOException)
            {
                {
                    var gameSave = new List<GameSaveData>();
                    SaveGameFile(gameSave, PATH);
                    fileContents = File.ReadAllText(PATH);
                }
            } 

            return JsonSerializer.Deserialize<List<GameSaveData>>(fileContents);
        }

        public static GameConfigData LoadGameConfig(string PATH)
        {
            string fileContents;
            try
            {
                fileContents = File.ReadAllText(PATH);
            }
            catch (IOException)
            {
                {
                    var gameSave = new GameConfigData()
                    {
                        IsFullScreen = true,
                        SFXVolume = Math.Round(GameGlobals.Instance.SoundEffectVolume, 2),
                        BGMusicVolume = Math.Round(GameGlobals.Instance.BackgroundMusicVolume, 2)
                    };
                    SaveConfigFile(gameSave, PATH);
                    fileContents = File.ReadAllText(PATH);
                }
            }

            return JsonSerializer.Deserialize<GameConfigData>(fileContents);
        }

        private static void SaveGameFile(List<GameSaveData> gameSave, string PATH)
        {
            string serializedText = JsonSerializer.Serialize(gameSave);
            (new FileInfo(PATH)).Directory.Create();
            File.WriteAllText(PATH, serializedText);
        }

        private static void SaveConfigFile(GameConfigData gameConfig, string PATH)
        {
            string serializedText = JsonSerializer.Serialize(gameConfig);
            (new FileInfo(PATH)).Directory.Create();
            File.WriteAllText(PATH, serializedText);
        }
    }

    // Reader Entity Data such as playerdata.json and entites_demo.json
    public class PlayerDataReader : JsonContentTypeReader<PlayerData> { }
    public class EntityDataReader : JsonContentTypeReader<List<EntityData>> { }

    // Reader Object Data such as object_demo.json
    public class ObjectDataReader : JsonContentTypeReader<List<ObjectData>> { }

    // Reader Items Data: items.json
    public class ItemDataReader : JsonContentTypeReader<List<ItemData>> { }
    
    // Reader Characters Data: characters.json
    public class CharacterDataReader : JsonContentTypeReader<List<CharacterData>> { }

    // Reader Crafting Recipes Data: craftingrecipes.json
    public class CraftingRecipeDataReader : JsonContentTypeReader<List<CraftingRecipeData>> { }

    // Reader Map Position Datas: map_positions.json
    public class MapLocationPointDataReader : JsonContentTypeReader<List<MapLocationPointData>> { }

    // Reader Experience Capacity Datas: exp_capacity.json
    public class ExperienceCapacityDataReader : JsonContentTypeReader<List<ExperienceCapacityData>> { }

    // Reader Chapter Item Drop Datas: chapter_item.json
    public class ChapterItemDataReader : JsonContentTypeReader<List<ChapterItemData>> { }

    // Reader Item Effect Datas: item_effects.json
    public class ItemEffectDataReader : JsonContentTypeReader<List<ItemEffectData>> { }

    // Reader Equipment Item Stats Datas: equipment_stats.json
    public class EquipmentStatsDataReader : JsonContentTypeReader<List<EquipmentStatsData>> { }

    // Reader Character Skill Description Datas: character_skills.json
    public class SkillDescriptionDataReader : JsonContentTypeReader<List<SkillDescriptionData>> { }

    //Reader Medicine Description Datas: medicine_description.json
    public class MedicineDescriptionDataReader : JsonContentTypeReader<List<MedicineDescriptionData>> { }
}
