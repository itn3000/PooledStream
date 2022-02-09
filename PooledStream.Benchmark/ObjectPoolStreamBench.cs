using System;
using System.IO;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.ObjectPool;
using System.Buffers;
using System.Linq;

namespace PooledStream.Benchmark
{
    [Config(typeof(MultiPlatformConfig))]
    [MemoryDiagnoser]
    public class ObjectPoolStreamBench
    {
        [Params(10000)]
        public int DataLength;
        [Params(1000)]
        public int LoopNum;
        [Benchmark(Baseline = true)]
        public void Normal()
        {
            var data = new byte[DataLength];
            for (int i = 0; i < LoopNum; i++)
            {
                using (var mstm = new PooledMemoryStream(ArrayPool<byte>.Shared, DataLength))
                {
                    #if NETCOREAPP3_0
                    mstm.Write(data.AsSpan());
                    #else
                    mstm.Write(data, 0, data.Length);
                    #endif
                }
            }
        }
        readonly ObjectPool<PooledMemoryStream> _Pool = ObjectPool.Create<PooledMemoryStream>();
        [Benchmark]
        public void Pooled()
        {
            var data = new byte[DataLength];
            for (int i = 0; i < LoopNum; i++)
            {
                var mstm = _Pool.Get();
                {
                    // mstm.Reserve(DataLength);
                    mstm.SetLength(0);
                    // #if NETCOREAPP3_0
                    // mstm.Write(data.AsSpan());
                    // #else
                    mstm.Write(data, 0, data.Length);
                    // #endif
                    _Pool.Return(mstm);
                }
            }
        }
    }

}