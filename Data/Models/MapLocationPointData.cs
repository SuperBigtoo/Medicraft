using System.Collections.Generic;

namespace Medicraft.Data.Models
{
    public class MapLocationPointData
    {
        public string Name { get; set; }
        public List<PositionData> Positions { get; set; }
    }

    public class PositionData
    {
        public string Name { get; set; }
        public double[] Value { get; set; }
    }
}
