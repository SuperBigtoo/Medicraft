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
            PlayerManager.Instance.Player.GetStats().Position[0] = PlayerManager.Instance.Player.Position.X;
            PlayerManager.Instance.Player.GetStats().Position[1] = PlayerManager.Instance.Player.Position.Y;

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

            if (GameGlobals.Instance.GameSave.Count != 0)
            {
                GameGlobals.Instance.GameSave[GameGlobals.Instance.GameSaveIdex] = new GameSave()
                {
                    Name = "Save_" + dateTimeString,
                    Created_Time = dateTimeString,
                    Last_Updated = dateTimeString,
                    Camera_Position = cameraPosition,
                    HUD_Position = hudPosition,
                    PlayerStats = PlayerManager.Instance.Player.GetStats(),
                };
            }
            else
            {
                GameGlobals.Instance.GameSave.Add(new GameSave()
                {
                    Name = "Save_" + dateTimeString,
                    Created_Time = dateTimeString,
                    Last_Updated = dateTimeString,
                    Camera_Position = cameraPosition,
                    HUD_Position = hudPosition,
                    PlayerStats = PlayerManager.Instance.Player.GetStats(),
                });
            }
            SaveFile(GameGlobals.Instance.GameSave, "data/stats.json");
        }

        public static List<GameSave> LoadFlie(string PATH)
        {
            var fileContents = "";
            try
            {
                fileContents = File.ReadAllText(PATH);
            } 
            catch (IOException)
            {
                {
                    var gameSave = new List<GameSave>();
                    SaveFile(gameSave, PATH);
                    fileContents = File.ReadAllText(PATH);
                }
            } 

            return JsonSerializer.Deserialize<List<GameSave>>(fileContents);
        }

        private static void SaveFile(List<GameSave> gameSave, string PATH)
        {
            string serializedText = JsonSerializer.Serialize<List<GameSave>>(gameSave);
            (new FileInfo(PATH)).Directory.Create();
            File.WriteAllText(PATH, serializedText);
        }
    }

    // Reader Entity Stats
    public class PlayerStatsReader : JsonContentTypeReader<PlayerStats> { }
    public class EntityStatsReader : JsonContentTypeReader<List<EntityStats>> { }

    // Reader Items
    public class ItemStatsReader : JsonContentTypeReader<List<ItemStats>> { }
}
