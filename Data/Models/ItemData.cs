namespace Medicraft.Data.Models
{
    public class ItemData
    {
        public int ItemId { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public bool Usable { get; set; }
        public int Effect { get; set; }
        public bool Stackable { get; set; }
    }
}
