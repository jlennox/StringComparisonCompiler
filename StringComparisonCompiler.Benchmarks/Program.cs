using BenchmarkDotNet.Running;

namespace StringComparisonCompiler.Benchmarks
{
    internal sealed class Program
    {
        public static void Main(string[] args)
        {
            var summaries = new[] {
                //BenchmarkRunner.Run<Benchmarks>(),
                BenchmarkRunner.Run<LargeDictionaryBenchmarks>(),
                //BenchmarkRunner.Run<CaseInsensitiveBenchmark>()
            };
        }
    }
}
