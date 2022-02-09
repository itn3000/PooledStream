using System;
using System.Buffers;
using Xunit;

namespace PooledStream.Test
{
    public class TestPooledBufferWriter
    {
        [Fact]
        public void Write()
        {
            using var bw = new PooledBufferWriter<byte>(ArrayPool<byte>.Shared, 1024);
            var data = new byte[128];
            for (int i = 0; i < 16; i++)
            {
                data.AsSpan().Fill((byte)(i + 1));
                var sp = bw.GetSpan(data.Length);
                data.AsSpan().CopyTo(sp);
                bw.Advance(data.Length);
            }
            var resultsp = bw.ToSpanUnsafe();
            for (int i = 0; i < 16; i++)
            {
                data.AsSpan().Fill((byte)(i + 1));
                Assert.True(resultsp.Slice(i * 128, data.Length).SequenceEqual(data));
            }
        }
        [Fact]
        public void Reset()
        {
            using var bw = new PooledBufferWriter<byte>();
            var sp = bw.GetSpan(128);
            sp.Fill(1);
            bw.Advance(128);
            var rsp = bw.ToSpanUnsafe();
            Assert.Equal(128, rsp.Length);
            bw.Reset();
            rsp = bw.ToSpanUnsafe();
            Assert.Equal(0, rsp.Length);
            sp = bw.GetSpan(128);
            sp.Fill(2);
            bw.Advance(128);
            rsp = bw.ToSpanUnsafe();
            Assert.Equal(128, rsp.Length);
        }
    }
}