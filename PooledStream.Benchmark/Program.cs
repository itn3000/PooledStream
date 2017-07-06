using System;

namespace PooledStream.Benchmark
{
    using BenchmarkDotNet.Attributes;
    using BenchmarkDotNet.Running;
    using System.Buffers;
    using Microsoft.IO;
    public class StreamBenchmark
    {
        [Params(1_000, 100_000)]
        public int DataSize { get; set; }
        [Params(10000)]
        public int MaxLoop { get; set; }
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
