namespace Benchmark.Models
{
    public class QueueMessage
    {
        public long Id { get; set; }
        public long InitialVersion { get; set; }
        public long LastVersion { get; set; }
        public string GeomTable { get; set; }
        public string ResultTable { get; set; }
    }
}