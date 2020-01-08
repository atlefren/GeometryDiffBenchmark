using Benchmark.Models;
using Differs.Differs;
using GeoAPI.Geometries;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Benchmark.services
{
    internal class Result<TResult>
    {
        public double Time { get; set; }

        //   public Stats Stats { get; set; }
        public TResult Data { get; set; }
    }

    public class Stats
    {
        public double Mean { get; set; }
        public double StDev { get; set; }
    }

    public static class Differ
    {
        public static Dictionary<string, DiffResult> RunBenchmark(DiffData payload)
        {
            var res = new Dictionary<string, DiffResult>();
            res.Add("TextDiffer", DoBenchmark(new TextGeometryDiffer(), payload));
            res.Add("JsonDiffer", DoBenchmark(new JsonDiffDiffer(), payload));
            res.Add("BinaryDiffer", DoBenchmark(new BinaryDiffer(), payload));
            res.Add("GeomDiffer", DoBenchmark(new GeomDiffer(), payload));
            return res;
        }

        private static DiffResult DoBenchmark(IGeometryDiff differ, DiffData payload)
        {
            var patch = RunWithTimer(CreatePatch, differ, payload, null);

            var forward = RunWithTimer(ApplyPatch, differ, payload, patch.Data);

            var reverse = RunWithTimer(UndoPatch, differ, payload, patch.Data);


            return new DiffResult()
            {
                CreateTime = patch.Time,
                ApplyTime = forward.Time,
                UndoTime = reverse.Time,
                PatchSize = patch.Data.Length,
                ForwardCorrect = Equals(forward.Data, payload.NewGeom),
                UndoCorrect = Equals(reverse.Data, payload.OldGeom),
            };
        }

        private static bool Equals(IGeometry a, IGeometry b) =>
            a != null && b.EqualsExact(a, 0.0000001);

        private static byte[] CreatePatch(IGeometryDiff differ, DiffData payload, byte[] patch) =>
            differ.CreatePatch(payload.OldGeom, payload.NewGeom);

        private static IGeometry ApplyPatch(IGeometryDiff differ, DiffData payload, byte[] patch) =>
            differ.ApplyPatch(payload.OldGeom, patch);


        private static IGeometry UndoPatch(IGeometryDiff differ, DiffData payload, byte[] patch) =>
            differ.UndoPatch(payload.NewGeom, patch);

        private static double Mean(IReadOnlyCollection<double> values) => values.Sum() / values.Count;

        private static double StDev(IReadOnlyCollection<double> values, double mean) =>
            Math.Sqrt(values.Select(v => Math.Pow(v - mean, 2)).Sum() / values.Count);

        private static Result<TResult> RunWithTimer<TResult>(Func<IGeometryDiff,
            DiffData, byte[], TResult> method, IGeometryDiff differ, DiffData payload, byte[] patch)
        {
            var stopwatch = new Stopwatch();
            /*Process.GetCurrentProcess().ProcessorAffinity =
                new IntPtr(2); // Uses the second Core or Processor for the Test
            Process.GetCurrentProcess().PriorityClass =
                ProcessPriorityClass.High; // Prevents "Normal" processes // from interrupting Threads
            Thread.CurrentThread.Priority =
                ThreadPriority.Highest; // Prevents "Normal" Threads from interrupting this thread
                */

            double time = 0;
            var data = default(TResult);
            for (var i = 0; i < 2; i++)
            {
                stopwatch.Reset();
                stopwatch.Start();
                data = method(differ, payload, patch);
                stopwatch.Stop();

                if (i > 0)
                {
                    time = ((double) stopwatch.ElapsedTicks / Stopwatch.Frequency) * 1000;
                }
            }

            //var mean = Mean(res);
            // var stDev = StDev(res, mean);
            //return new Result<TResult>() {Stats = new Stats() {Mean = mean, StDev = stDev}, Data = data};
            return new Result<TResult>() {Time = time, Data = data};
        }
    }
}