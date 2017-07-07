using System;

namespace PooledStream.Benchmark
{
    using BenchmarkDotNet.Attributes;
    using BenchmarkDotNet.Running;
    using BenchmarkDotNet.Diagnosers;
    using System.Buffers;
    using Microsoft.IO;
    using System.IO;
    [MemoryDiagnoser]
    public class StreamBenchmark
    {
        [Params(100, 1_000, 100_000)]
        public int DataSize { get; set; }
        [Params(10000)]
        public int MaxLoop { get; set; }
        [Benchmark(Baseline = true)]
        public void NormalStreamTest()
        {
            var data = new byte[DataSize];
            for(int i = 0;i<MaxLoop;i++)
            {
                using(var stm = new MemoryStream())
                {
                    stm.Write(data, 0, data.Length);
                }
            }
        }
        [Benchmark]
        public void PooledStreamBench()
        {
            var data = new byte[DataSize];
            for (int i = 0; i < MaxLoop; i++)
            {
                using (var stm = new PooledMemoryStream(ArrayPool<byte>.Shared, DataSize))
                {
                    stm.Write(data, 0, data.Length);
                }
            }
        }
        [Benchmark]
        public void RecyclableStreamTest()
        {
            var data = new byte[DataSize];
            var manager = new RecyclableMemoryStreamManager();
            for(int i = 0;i<MaxLoop;i++)
            {
                using(var stm = manager.GetStream("mytag", DataSize))
                {
                    stm.Write(data, 0, data.Length);
                }
            }
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            var reporter = BenchmarkRunner.Run<StreamBenchmark>();
        }
    }
}
