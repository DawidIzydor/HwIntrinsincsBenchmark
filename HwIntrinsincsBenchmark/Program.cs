using BenchmarkDotNet.Running;

namespace HwIntrinsincsBenchmark
{
    internal class Program
    {
        private static void Main()
        {
            BenchmarkRunner.Run<OddValuesSumBenchmark>();
        }
    }
}