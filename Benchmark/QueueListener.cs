using Benchmark.Models;
using Benchmark.services;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Benchmark
{
    public class QueueListener
    {
        private readonly IGeometryFetcher _geometryFetcher;
        private readonly IResultSaver _resultSaver;

        public QueueListener(IGeometryFetcher geometryFetcher, IResultSaver resultSaver)
        {
            _geometryFetcher = geometryFetcher;
            _resultSaver = resultSaver;
        }

        [FunctionName("QueueListener")]
        public void Run([QueueTrigger("diffqueue", Connection = "queueConnectionString")]
            string myQueueItem, ILogger log)
        {
            var data = JsonConvert.DeserializeObject<QueueMessage>(myQueueItem);

            var table = data.GeomTable;

            var v1 = _geometryFetcher.GetGeom(table, data.Id, data.InitialVersion);
            var v2 = _geometryFetcher.GetGeom(table, data.Id, data.LastVersion);
            var res = Differ.RunBenchmark(
                new DiffData() { GeometryId = $"{table}_{data.Id}", OldGeom = v1, NewGeom = v2 });
            //_resultSaver.SaveResult(data.ResultTable, res, data.Id);
        }
    }
}