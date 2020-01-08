using Differs.Differs;
using GeoAPI.Geometries;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using NetTopologySuite.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace BenchmarkFunctionApp
{
    public static class Benchmark
    {
        public static Dictionary<string, DiffResult> RunBenchmark(DiffData payload)
        {
            var res = new Dictionary<string, DiffResult>();
            res.Add("TextDiffer", DoBenchmark(new TextGeometryDiffer(), "TextDiffer", payload));
            res.Add("JsonDiffer", DoBenchmark(new JsonDiffDiffer(), "JsonDiffer", payload));
            res.Add("BinaryDiffer", DoBenchmark(new BinaryDiffer(), "BinaryDiffer", payload));
            res.Add("GeomDiffer", DoBenchmark(new GeomDiffer(), "GeomDiffer", payload));
            return res;
        }

        [FunctionName("Function_TextDiffer")]
        public static DiffResult TextDiffer([ActivityTrigger] DiffData payload, ILogger log)
        {
            log.LogInformation($"Run TextDiffer");
            return DoBenchmark(new TextGeometryDiffer(), "TextDiffer", payload);
        }

        [FunctionName("Function_JsonDiffer")]
        public static DiffResult JsonDiffer([ActivityTrigger] DiffData payload, ILogger log)
        {
            log.LogInformation($"Run JsonDiffer");
            return DoBenchmark(new JsonDiffDiffer(), "JsonDiffer", payload);
        }

        [FunctionName("Function_BinaryDiffer")]
        public static DiffResult BinaryDiffer([ActivityTrigger] DiffData payload, ILogger log)
        {
            log.LogInformation($"Run BinaryDiffer");
            return DoBenchmark(new BinaryDiffer(), "BinaryDiffer", payload);
        }

        [FunctionName("Function_GeomDiffer")]
        public static DiffResult GeomDiffer([ActivityTrigger] DiffData payload, ILogger log)
        {
            log.LogInformation($"Run GeomDiffer");
            return DoBenchmark(new GeomDiffer(), "GeomDiffer", payload);
        }

        private static DiffResult DoBenchmark(Differs.Differs.IGeometryDiff differ, string differName, DiffData payload)
        {
            var original = ReadWkb(payload.OldGeom);
            var modified = ReadWkb(payload.NewGeom);

            var stopwatch = new Stopwatch();
            Process.GetCurrentProcess().ProcessorAffinity =
                new IntPtr(2); // Uses the second Core or Processor for the Test
            Process.GetCurrentProcess().PriorityClass =
                ProcessPriorityClass.High; // Prevents "Normal" processes // from interrupting Threads
            Thread.CurrentThread.Priority =
                ThreadPriority.Highest; // Prevents "Normal" Threads from interrupting this thread

            stopwatch.Reset();
            stopwatch.Start();
            var patch = differ.CreatePatch(original, modified);
            stopwatch.Stop();
            var createPatchTime = ((double)stopwatch.ElapsedTicks / Stopwatch.Frequency) * 1000;

            stopwatch.Reset();
            stopwatch.Start();
            var patched = differ.ApplyPatch(original, patch);
            stopwatch.Stop();
            var applyPatchTime = ((double)stopwatch.ElapsedTicks / Stopwatch.Frequency) * 1000;

            stopwatch.Reset();
            stopwatch.Start();
            var unPatched = differ.UndoPatch(modified, patch);
            stopwatch.Stop();
            var undoPatchTime = ((double)stopwatch.ElapsedTicks / Stopwatch.Frequency) * 1000;

            var forwardCorrect = patched != null && modified.EqualsExact(patched, 0.0000001);

            var undoCorrect = unPatched != null && original.EqualsExact(unPatched, 0.0000001);

            return new DiffResult()
            {
                CreateTime = createPatchTime,
                ApplyTime = applyPatchTime,
                UndoTime = undoPatchTime,
                PatchSize = patch.Length,
                ForwardCorrect = forwardCorrect,
                UndoCorrect = undoCorrect,
                Differ = differName,
                GeomId = payload.GeometryId
            };
        }


        private static IGeometry ReadWkb(string wkb)
        {
            var reader = new WKBReader();
            return reader.Read(WKBReader.HexToBytes(wkb));
        }
    }
}