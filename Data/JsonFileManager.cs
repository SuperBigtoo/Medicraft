using System;
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
        public const int SavingPlayerData = 0;
        public const int RenameGameSave = 1;
        public const int DeleteGameSave = 2;

        //Load & Save Game Basically
        public static void SaveGame(int SavingCase)
        {
            switch (SavingCase)
            {
                case SavingPlayerData:
                    // Set current Stats          
                    PlayerManager.Instance.Player.PlayerData.Level = PlayerManager.Instance.Player.Level;
                    PlayerManager.Instance.Player.PlayerData.EXP = PlayerManager.Instance.Player.EXP;
                    // HP Percentage
                    var hpPercentage = PlayerManager.Instance.Player.HP / PlayerManager.Instance.Player.MaxHP;
                    PlayerManager.Instance.Player.PlayerData.CurrentHPPercentage = Math.Round(hpPercentage, 2);
                    // Mana Percentage
                    var manaPercentage = PlayerManager.Instance.Player.Mana / PlayerManager.Instance.Player.MaxMana;
                    PlayerManager.Instance.Player.PlayerData.CurrentManaPercentage = Math.Round(manaPercentage, 2);

                    // Set Player Position
                    PlayerManager.Instance.Player.PlayerData.CurrentMap = ScreenManager.Instance.CurrentMap;
                    PlayerManager.Instance.Player.PlayerData.Position[0] = Math.Round(PlayerManager.Instance.Player.Position.X, 2);
                    PlayerManager.Instance.Player.PlayerData.Position[1] = Math.Round(PlayerManager.Instance.Player.Position.Y, 2);

                    // Set Current InventoryData
                    var inventoryItems = new List<InventoryItemData>();
                    foreach (var item in InventoryManager.Instance.InventoryBag.Values)
                    {
                        inventoryItems.Add(item);
                    }
                    var inventoryData = new InventoryData()
                    {
                        GoldCoin = InventoryManager.Instance.GoldCoin,
                        Inventory = inventoryItems
                    };
                    PlayerManager.Instance.Player.PlayerData.InventoryData = inventoryData;

                    // Get Time
                    DateTime dateTime = DateTime.Now;
                    string dateTimeString = dateTime.ToString().Replace(' ', '_');

                    // Set save name
                    var saveName = "Save_" + dateTimeString;

                    // Ser total playtime
                    var second = (int)GameGlobals.Instance.TotalPlayTime % 60;
                    var minute = (int)GameGlobals.Instance.TotalPlayTime / 60;
                    var hour = (int)GameGlobals.Instance.TotalPlayTime / 3600;

                    var playTime = new int[] { hour, minute, second };      

                    try
                    {
                        var gameSaveData = GameGlobals.Instance.GameSave[GameGlobals.Instance.SelectedGameSaveIndex];

                        GameGlobals.Instance.GameSave[GameGlobals.Instance.SelectedGameSaveIndex] = new GameSaveData()
                        {
                            SaveId = gameSaveData.SaveId,
                            Name = gameSaveData.Name,
                            CreatedTime = gameSaveData.CreatedTime,
                            LastUpdated = dateTimeString,
                            TotalPlayTime = playTime,
                            PlayerData = PlayerManager.Instance.Player.PlayerData
                        };
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        GameGlobals.Instance.GameSave.Add(new GameSaveData()
                        {
                            SaveId = GameGlobals.Instance.GameSave.Count,
                            Name = saveName,
                            CreatedTime = dateTimeString,
                            LastUpdated = dateTimeString,
                            TotalPlayTime = playTime,
                            PlayerData = PlayerManager.Instance.Player.PlayerData
                        });
                    }

                    break;

                case RenameGameSave:
                    GameGlobals.Instance.GameSave[GameGlobals.Instance.SelectedGameSaveIndex] = new GameSaveData()
                    {
                        SaveId = GameGlobals.Instance.GameSave.ElementAt(GameGlobals.Instance.SelectedGameSaveIndex).SaveId,
                        Name = GameGlobals.Instance.GameSave.ElementAt(GameGlobals.Instance.SelectedGameSaveIndex).Name,
                        CreatedTime = GameGlobals.Instance.GameSave.ElementAt(GameGlobals.Instance.SelectedGameSaveIndex).CreatedTime,
                        LastUpdated = GameGlobals.Instance.GameSave.ElementAt(GameGlobals.Instance.SelectedGameSaveIndex).LastUpdated,
                        TotalPlayTime = GameGlobals.Instance.GameSave.ElementAt(GameGlobals.Instance.SelectedGameSaveIndex).TotalPlayTime,
                        PlayerData = GameGlobals.Instance.GameSave.ElementAt(GameGlobals.Instance.SelectedGameSaveIndex).PlayerData
                    };

                    break;

                case DeleteGameSave:
                    GameGlobals.Instance.GameSave.Remove(GameGlobals.Instance.GameSave[GameGlobals.Instance.SelectedGameSaveIndex]);
                    break;
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
