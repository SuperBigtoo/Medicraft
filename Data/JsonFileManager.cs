﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Medicraft.Data.Models;
using Medicraft.Systems;
using Medicraft.Systems.Managers;
using Microsoft.Xna.Framework;
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

            // Get Player Position
            PlayerManager.Instance.Player.PlayerData.Position[0] = PlayerManager.Instance.Player.Position.X;
            PlayerManager.Instance.Player.PlayerData.Position[1] = PlayerManager.Instance.Player.Position.Y;

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
            if (GameGlobals.Instance.GameSave.Count != 0)
            {
                GameGlobals.Instance.GameSave[GameGlobals.Instance.GameSaveIdex] = new GameSaveData()
                {
                    SaveId = GameGlobals.Instance.GameSave.ElementAt(GameGlobals.Instance.GameSaveIdex).SaveId,
                    Name = GameGlobals.Instance.GameSave.ElementAt(GameGlobals.Instance.GameSaveIdex).Name,
                    CreatedTime = GameGlobals.Instance.GameSave.ElementAt(GameGlobals.Instance.GameSaveIdex).CreatedTime,
                    LastUpdated = dateTimeString,
                    CameraPosition = cameraPosition,
                    HUDPosition = hudPosition,
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
                    CameraPosition = cameraPosition,
                    HUDPosition = hudPosition,
                    PlayerData = PlayerManager.Instance.Player.PlayerData
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
}
