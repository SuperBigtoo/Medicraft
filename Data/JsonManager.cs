using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Medicraft.Data.Models;
using MonoGame.Extended.Serialization;

namespace Medicraft.Data
{
    internal class JsonManager
    {
        //Load & Save Game Basically
        public static List<PlayerStats> Load(string PATH)
        {
            var fileContents = File.ReadAllText(PATH);
            return JsonSerializer.Deserialize<List<PlayerStats>>(fileContents);
        }

        public static void Save(List<PlayerStats> playerStats, string PATH)
        {
            string serializedText = JsonSerializer.Serialize<List<PlayerStats>>(playerStats);
            (new FileInfo(PATH)).Directory.Create();
            File.WriteAllText(PATH, serializedText);
        }
    }

    //Reader Entity Stats
    public class EntityStatsReader : JsonContentTypeReader<EntityStats> { }
}
