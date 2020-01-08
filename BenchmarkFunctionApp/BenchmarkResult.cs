namespace BenchmarkFunctionApp
{
    public class Average
    {
        public double Value { get; set; }
        public int NumObs { get; set; }

        public void Add(double value)
        {
            NumObs += 1;
            Value += (value - Value) / NumObs;
        }
    }

    public class BenchmarkResult
    {
        public Average CreateTime { get; set; } = new Average();
        public Average ApplyTime { get; set; } = new Average();
        public Average UndoTime { get; set; } = new Average();
        public Average PatchSize { get; set; } = new Average();
        public int ForwardErrors { get; set; }
        public int UndoErrors { get; set; }

        public void Add(DiffResult diffResult)
        {
            CreateTime.Add(diffResult.CreateTime);
            ApplyTime.Add(diffResult.ApplyTime);
            UndoTime.Add(diffResult.UndoTime);
            PatchSize.Add(diffResult.PatchSize);
            ForwardErrors += diffResult.ForwardCorrect ? 0 : 1;
            UndoErrors += diffResult.UndoCorrect ? 0 : 1;
        }
    }
}