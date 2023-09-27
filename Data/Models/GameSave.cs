
namespace Medicraft.Data.Models
{
    public class GameSave
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Created_Time { get; set; }
        public string Last_Updated { get; set; }
        public double[] Camera_Position { get; set; }
        public double[] HUD_Position { get; set; }
        public PlayerStats PlayerStats { get; set; }
    }
}
