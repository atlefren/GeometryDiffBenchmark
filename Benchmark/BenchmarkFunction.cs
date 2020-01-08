using Benchmark.Models;
using Benchmark.services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace Benchmark
{
    public class BenchmarkFunction
    {
        private readonly IGeometryFetcher _geometryFetcher;

        public BenchmarkFunction(IGeometryFetcher geometryFetcher)
        {
            _geometryFetcher = geometryFetcher;
        }

        [FunctionName("DiffBenchmark")]
        public IActionResult DiffBenchmark(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]
            HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            if (!long.TryParse(req.Query["id"], out var id))
            {
                return new BadRequestObjectResult("Invalid id");
            }

            if (!long.TryParse(req.Query["first"], out var initialVersion))
            {
                return new BadRequestObjectResult("Invalid initialVersion");
            }

            if (!long.TryParse(req.Query["last"], out var lastVersion))
            {
                return new BadRequestObjectResult("Invalid lastVersion");
            }


            var table = req.Query["table"];

            var v1 = _geometryFetcher.GetGeom(table, id, initialVersion);
            var v2 = _geometryFetcher.GetGeom(table, id, lastVersion);
            var res = Differ.RunBenchmark(new DiffData() { GeometryId = $"{table}_{id}", OldGeom = v1, NewGeom = v2 });


            return new OkObjectResult(res);
        }
    }
}