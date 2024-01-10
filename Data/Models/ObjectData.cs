namespace Medicraft.Data.Models
{
    public class ObjectData
    {
        public int Id { get; set; }
        public int ReferId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double[] Position { get; set; }
    }
}
