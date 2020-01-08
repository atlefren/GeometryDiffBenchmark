using Microsoft.Azure.WebJobs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BenchmarkFunctionApp
{
    public class Orchestrator
    {
        private IGeometryReader _geometryReader;

        public Orchestrator(IGeometryReader reader)
        {
            _geometryReader = reader;
        }

        [FunctionName("OrchestratorFunction")]
        public async Task<Dictionary<string, BenchmarkResult>> RunOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context)
        {
            var benchmarkResults = new Dictionary<string, BenchmarkResult>
            {
                {"TextDiffer", new BenchmarkResult()},
                //{"JsonDiffer", new BenchmarkResult()},
                {"BinaryDiffer", new BenchmarkResult()},
                {"GeomDiffer", new BenchmarkResult()}
            };

            var differs = new Dictionary<string, string>
            {
                {"TextDiffer", "Function_TextDiffer"},
                //{"JsonDiffer", "Function_JsonDiffer"},
                {"BinaryDiffer", "Function_BinaryDiffer"},
                {"GeomDiffer", "Function_GeomDiffer"}
            };


            //var reader = new GeometryReader("osm_test.point_versions", "osm_test.nodes");

            while (true)
            {
                var data = _geometryReader.GetNext();
                if (data == null)
                {
                    break;
                }

                foreach (var (name, function) in differs)
                {
                    var result = await context.CallActivityAsync<DiffResult>(function, data);
                    benchmarkResults[name].Add(result);
                }
            }

            return benchmarkResults;
        }
    }
}