using System;

namespace PooledStream.Benchmark
{
    using BenchmarkDotNet.Jobs;
    using BenchmarkDotNet.Configs;
    using BenchmarkDotNet.Toolchains.CsProj;
    using BenchmarkDotNet.Attributes;
    using BenchmarkDotNet.Running;
    using BenchmarkDotNet.Diagnosers;
    using System.Buffers;
    using Microsoft.IO;
    using System.IO;
    using ObjectMemoryStream = CodeProject.ObjectPool.Specialized.MemoryStreamPool;
    [MemoryDiagnoser]
    [Config(typeof(MultiPlatformConfig))]
    public class StreamBenchmark
    {
        [Params(100, 1_000, 50_000)]
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
        [Benchmark]
        public void ObjectPoolTest()
        {
            var pool = ObjectMemoryStream.Instance;
            var data = new byte[DataSize];
            for(int i = 0;i<MaxLoop;i++)
            {
                using(var stm = pool.GetObject())
                {
                    stm.MemoryStream.Write(data, 0, data.Length);
                }
            }
        }
    }
    class MultiPlatformConfig : ManualConfig
    {
        public MultiPlatformConfig()
        {
            Add(Job.Default.WithWarmupCount(3).WithTargetCount(3)
                .With(CsProjCoreToolchain.NetCoreApp20));
            Add(Job.Default.WithWarmupCount(3).WithTargetCount(3)
                .With(CsProjCoreToolchain.NetCoreApp21));
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            var switcher = new BenchmarkSwitcher(new Type[]
            {
                typeof(StreamBenchmark),
                typeof(StreamPrallelBenchmark)
            }
            );
            switcher.Run();
        }
    }
}
