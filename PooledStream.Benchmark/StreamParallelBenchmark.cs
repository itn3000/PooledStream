namespace PooledStream.Benchmark
{
    using BenchmarkDotNet.Attributes;
    using BenchmarkDotNet.Running;
    using BenchmarkDotNet.Diagnosers;
    using System.Buffers;
    using Microsoft.IO;
    using System.IO;
    using System.Threading.Tasks;
    using System.Threading;
    using System.Linq;
    using ObjectMemoryStream = CodeProject.ObjectPool.Specialized.MemoryStreamPool;
    [MemoryDiagnoser]
    [Config(typeof(MultiPlatformConfig))]
    public class StreamPrallelBenchmark
    {
        [Params(5, 10)]
        public int ParallelNum{get;set;}
        [Params(1000)]
        public int DataSize{get;set;}
        [Params(10000)]
        public int MaxLoop{get;set;}
        [Benchmark(Baseline=true)]
        public void NormalStreamParallel()
        {
            var data = new byte[DataSize];
            Task.WhenAll(Enumerable.Range(0, ParallelNum)
                .Select(async (idx) => {
                    for(int i = 0;i<MaxLoop/ParallelNum;i++)
                    {
                        using(var stm = new MemoryStream(DataSize))
                        {
                            await stm.WriteAsync(data, 0, data.Length).ConfigureAwait(false);
                        }
                    }
                    return idx;
                })).Wait();
        }
        [Benchmark]
        public void PooledStreamParallel()
        {
            var data = new byte[DataSize];
            Task.WhenAll(Enumerable.Range(0, ParallelNum)
                .Select(async (idx) => {
                    for(int i = 0;i<MaxLoop/ParallelNum;i++)
                    {
                        using(var stm = new PooledMemoryStream(ArrayPool<byte>.Shared, data.Length))
                        {
                            await stm.WriteAsync(data, 0, data.Length).ConfigureAwait(false);
                        }
                    }
                    return idx;
                })).Wait();
        }
        [Benchmark]
        public void ObjectPoolParallel()
        {
            var data = new byte[DataSize];
            var pool = ObjectMemoryStream.Instance;
            Task.WhenAll(Enumerable.Range(0, ParallelNum)
                .Select(async (idx) => {
                    for(int i = 0;i<MaxLoop;i++)
                    {
                        using(var stm = pool.GetObject())
                        {
                            await stm.MemoryStream.WriteAsync(data, 0, data.Length).ConfigureAwait(false);
                        }
                    }
                    return idx;
                })).Wait();
        }
        [Benchmark]
        public void RecyclableStreamParallel()
        {
            var data = new byte[DataSize];
            var manager = new RecyclableMemoryStreamManager();
            Task.WhenAll(Enumerable.Range(0, ParallelNum)
                .Select(async (idx) => {
                    for(int i = 0;i<MaxLoop;i++)
                    {
                        using(var stm = manager.GetStream())
                        {
                            await stm.WriteAsync(data, 0, data.Length).ConfigureAwait(false);
                        }
                    }
                })).Wait();
        }
    }
}