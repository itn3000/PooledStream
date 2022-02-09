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
    // [DisassemblyDiagnoser(printIL: true, printSource: true, printAsm: true)]
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
                    stm.Write(data, 0, DataSize);
                }
            }
            // ArrayPool<byte>.Shared.Return(data);
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
            AddJob(Job.Default.WithWarmupCount(3).WithIterationCount(3)
                .WithToolchain(CsProjCoreToolchain.NetCoreApp60));
            AddJob(Job.Default.WithWarmupCount(3).WithIterationCount(3)
                .WithToolchain(CsProjCoreToolchain.NetCoreApp31));
            this.Options |= ConfigOptions.DisableOptimizationsValidator;
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            var switcher = new BenchmarkSwitcher(typeof(Program).Assembly);
            switcher.Run();
        }
    }
}
