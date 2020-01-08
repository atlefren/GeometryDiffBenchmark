namespace Benchmark.Models
{
    public class DiffResult
    {
        public double CreateTime { get; set; }
        public double ApplyTime { get; set; }
        public double UndoTime { get; set; }
        public long PatchSize { get; set; }
        public bool ForwardCorrect { get; set; }
        public bool UndoCorrect { get; set; }
    }
}