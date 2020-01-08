namespace BenchmarkFunctionApp
{
    public class DiffResult
    {
        public string GeomId { get; set; }
        public string Differ { get; set; }
        public double CreateTime { get; set; }
        public double ApplyTime { get; set; }
        public double UndoTime { get; set; }
        public long PatchSize { get; set; }
        public bool ForwardCorrect { get; set; }
        public bool UndoCorrect { get; set; }
    }
}