namespace Medicraft.Data.Models
{
    public class EntityData
    {
        public int Id { get; set; }
        public int CharId { get; set; }
        public int Level { get; set; }
        public string Name { get; set; }
        public int PathFindingType { get; set; }
        public int NodeCycleTime { get; set; }
        public int AggroTime { get; set; }
        public double[] Position { get; set; }
    }
}
