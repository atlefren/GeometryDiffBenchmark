using GeoAPI.Geometries;

namespace Benchmark.Models
{
    public class DiffData
    {
        public IGeometry OldGeom { get; set; }
        public IGeometry NewGeom { get; set; }
        public string GeometryId { get; set; }
    }
}