using System.Linq;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using BenchmarkDotNet.Attributes;

namespace HwIntrinsincsBenchmark
{
    public class OddValuesSumBenchmark
    {
        private int[] _array;

        [Params(1_000, 50_000, 1_000_000)] public int N;

        [GlobalSetup]
        public void Setup()
        {
            _array = new int[N];
        }


        [Benchmark(Baseline = true)]
        public int ClassicIfSum()
        {
            var sum = 0;
            for (var i = 0; i < _array.Length; i++)
            {
                if (_array[i] % 2 != 0)
                {
                    sum += _array[i];
                }
            }

            return sum;
        }

        [Benchmark]
        public int Linq()
        {
            return _array.Where(t => t % 2 != 0).Sum();
        }

        [Benchmark]
        public int SumFromYoutube()
        {
            var counterA = 0;
            var counterB = 0;
            var counterC = 0;
            var counterD = 0;
            unsafe
            {
                fixed (int* data = &_array[0])
                {
                    var p = data;
                    for (var i = 0; i < _array.Length; i += 4)
                    {
                        counterA += (p[0] & 1) * p[0];
                        counterB += (p[1] & 1) * p[1];
                        counterC += (p[2] & 1) * p[2];
                        counterD += (p[3] & 1) * p[3];

                        p += 4;
                    }
                }
            }

            return counterA + counterB + counterC + counterD;
        }

        [Benchmark]
        public int SumUsingVectors()
        {
            var sum = 0;
            var itemsCountUsingVectors = _array.Length - _array.Length % Vector256<int>.Count;
            var template = Enumerable.Repeat(1, Vector256<int>.Count).ToArray();
            var sumVector = Vector256<int>.Zero;
            unsafe
            {
                Vector256<int> templateVector;
                fixed (int* templatePtr = template)
                {
                    templateVector = Avx.LoadVector256(templatePtr);
                }

                fixed (int* valuesPtr = _array)
                {
                    for (var i = 0; i < itemsCountUsingVectors; i += Vector256<int>.Count)
                    {
                        var valuesVector = Avx.LoadVector256(valuesPtr + i);
                        var andVector = Avx2.And(valuesVector, templateVector);
                        var multiplyVector = Avx2.MultiplyLow(andVector, valuesVector);
                        sumVector = Avx2.Add(sumVector, multiplyVector);
                    }
                }
            }

            for (var iVector = 0; iVector < Vector256<int>.Count; iVector++)
            {
                sum += sumVector.GetElement(iVector);
            }

            for (var i = itemsCountUsingVectors; i < _array.Length; i++)
            {
                sum += (_array[i] & 1) * _array[i];
            }

            return sum;
        }
    }
}