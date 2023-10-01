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
            PlayerManager.Instance.player.GetPlayerStats().Position[0] = (double)PlayerManager.Instance.player.Position.X;
            PlayerManager.Instance.player.GetPlayerStats().Position[1] = (double)PlayerManager.Instance.player.Position.Y;

            // Get HUD Position
            double[] hudPosition =
            {
                (double)Singleton.Instance.addingHudPos.X,
                (double)Singleton.Instance.addingHudPos.Y
            };

            // Get Camera Position
            Vector2 camPos = Singleton.Instance.cameraPosition + Singleton.Instance.addingCameraPos;
            double[] cameraPosition =
            {
                (double)camPos.X,
                (double)camPos.Y
            };

            if (Singleton.Instance.gameSave.Count != 0)
            {
                Singleton.Instance.gameSave[Singleton.Instance.gameSaveIdex] = new GameSave()
                {
                    Name = "Save_" + dateTimeString,
                    Created_Time = dateTimeString,
                    Last_Updated = dateTimeString,
                    Camera_Position = cameraPosition,
                    HUD_Position = hudPosition,
                    PlayerStats = PlayerManager.Instance.player.GetPlayerStats(),
                };
            }
            else
            {
                Singleton.Instance.gameSave.Add(new GameSave()
                {
                    Name = "Save_" + dateTimeString,
                    Created_Time = dateTimeString,
                    Last_Updated = dateTimeString,
                    Camera_Position = cameraPosition,
                    HUD_Position = hudPosition,
                    PlayerStats = PlayerManager.Instance.player.GetPlayerStats(),
                });
            }
            SaveFile(Singleton.Instance.gameSave, "data/stats.json");
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

    //Reader Entity Stats
    public class PlayerStatsReader : JsonContentTypeReader<PlayerStats> { }
    public class EntityStatsReader : JsonContentTypeReader<List<EntityStats>> { }
}
