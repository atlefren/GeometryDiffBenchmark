namespace BenchmarkFunctionApp
{
    public class Counterr
    {
        private int _count;

        public Counterr()
        {
            _count = 0;
        }

        public int? GetNext()
        {
            if (_count < 100)
            {
                return _count++;
            }

            return null;
        }
    }
}