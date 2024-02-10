using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Medicraft.Data.Models;
using Medicraft.Systems;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Serialization;

namespace Medicraft.Data
{
    internal class JsonFileManager
    {
        //Load & Save Game Basically
        public static void SaveGame()
        {
            // Get Time
            DateTime dateTime = DateTime.Now;
            string dateTimeString = dateTime.ToString().Replace(' ', '_');

            // Get Player Position
            PlayerManager.Instance.Player.GetPlayerData().Position[0] = PlayerManager.Instance.Player.Position.X;
            PlayerManager.Instance.Player.GetPlayerData().Position[1] = PlayerManager.Instance.Player.Position.Y;

            // Get HUD Position
            double[] hudPosition =
            {
                (double)GameGlobals.Instance.HUDPosition.X,
                (double)GameGlobals.Instance.HUDPosition.Y
            };

            // Get Camera Position
            Vector2 camPos = GameGlobals.Instance.InitialCameraPos + GameGlobals.Instance.AddingCameraPos;
            double[] cameraPosition =
            {
                (double)camPos.X,
                (double)camPos.Y
            };

            // Set Current InventoryData
            var inventoryItems = new List<InventoryItemData>();
            foreach (var item in InventoryManager.Instance.Inventory.Values)
            {
                inventoryItems.Add(item);
            }
            var inventoryData = new InventoryData() {
                GoldCoin = InventoryManager.Instance.GoldCoin,
                Inventory = inventoryItems
            };
            PlayerManager.Instance.Player.GetPlayerData().InventoryData = inventoryData;

            // Set save name
            var saveName = "Save_" + dateTimeString;
            if (GameGlobals.Instance.GameSave.Count != 0)
            {
                GameGlobals.Instance.GameSave[GameGlobals.Instance.GameSaveIdex] = new GameSaveData()
                {
                    Id = GameGlobals.Instance.GameSaveIdex,
                    Name = saveName,
                    CreatedTime = dateTimeString,
                    LastUpdated = dateTimeString,
                    CameraPosition = cameraPosition,
                    HUDPosition = hudPosition,
                    PlayerData = PlayerManager.Instance.Player.GetPlayerData(),
                };
            }
            else
            {
                GameGlobals.Instance.GameSave.Add(new GameSaveData()
                {
                    Id = 0,
                    Name = saveName,
                    CreatedTime = dateTimeString,
                    LastUpdated = dateTimeString,
                    CameraPosition = cameraPosition,
                    HUDPosition = hudPosition,
                    PlayerData = PlayerManager.Instance.Player.GetPlayerData(),
                });
            }

            SaveFile(GameGlobals.Instance.GameSave, GameGlobals.Instance.GameSavePath);
        }

        public static List<GameSaveData> LoadFlie(string PATH)
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
                    SaveFile(gameSave, PATH);
                    fileContents = File.ReadAllText(PATH);
                }
            } 

            return JsonSerializer.Deserialize<List<GameSaveData>>(fileContents);
        }

        private static void SaveFile(List<GameSaveData> gameSave, string PATH)
        {
            string serializedText = JsonSerializer.Serialize<List<GameSaveData>>(gameSave);
            (new FileInfo(PATH)).Directory.Create();
            File.WriteAllText(PATH, serializedText);
        }
    }

    // Reader Entity Data
    public class PlayerDataReader : JsonContentTypeReader<PlayerData> { }
    public class EntityDataReader : JsonContentTypeReader<List<EntityData>> { }

    // Reader Object Data
    public class ObjectDataReader : JsonContentTypeReader<List<ObjectData>> { }

    // Reader Items Data
    public class ItemDataReader : JsonContentTypeReader<List<ItemData>> { }
    
    // Reader Characters Data
    public class CharacterDataReader : JsonContentTypeReader<List<CharacterData>> { }
}
