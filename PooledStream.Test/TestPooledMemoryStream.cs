using System;
using Xunit;

namespace PooledStream.Test
{
    using System.Buffers;
    using System.Linq;
    using System.IO;
    public class TestPooledMemoryStream
    {
        [Fact]
        public void TestWriteTwice()
        {
            var data = new byte[] { 1, 2, 3, 4 };
            using (var stm = new PooledMemoryStream(ArrayPool<byte>.Shared))
            {
                Assert.True(stm.CanWrite);
                Assert.True(stm.CanRead);
                Assert.Equal(0, stm.Length);
                Assert.Equal(0, stm.Position);
                stm.Write(data, 0, data.Length);
                Assert.Equal(4, stm.Length);
                Assert.Equal(4, stm.Position);
                var ar = stm.ToArray();
                Assert.Equal(data, ar);
                stm.Write(data, 0, data.Length);
                Assert.Equal(8, stm.Length);
                Assert.Equal(8, stm.Position);
                ar = stm.ToArray();
                Assert.Equal(data.Concat(data), ar);
            }
        }
        [Fact]
        public void TestWriteMiddle()
        {
            var data = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8};
            using (var stm = new PooledMemoryStream(ArrayPool<byte>.Shared))
            {
                stm.Write(data, 0, data.Length);
                stm.Seek(4, SeekOrigin.Begin);
                stm.Write(data, 0, data.Length);
                var ar = stm.ToArray();
                Assert.Equal(data.Take(4).Concat(data), ar);
            }
        }
        [Fact]
        public void TestReadOnly()
        {
            var data = new byte[] { 1, 2, 3, 4 };
            using (var stm = new PooledMemoryStream(ArrayPool<byte>.Shared, data, 0, data.Length))
            {
                Assert.True(stm.CanRead);
                Assert.False(stm.CanWrite);
                var buf = new byte[128];
                var bytesread = stm.Read(buf, 4, buf.Length - 4);
                Assert.Equal(4, bytesread);
                Assert.Equal(4, stm.Position);
                bytesread = stm.Read(buf, 0, buf.Length);
                Assert.Equal(0, bytesread);
            }
        }
        [Fact]
        public void TestSetLength()
        {
            var data = new byte[]{1,2,3,4};
            using(var stm = new PooledMemoryStream(ArrayPool<byte>.Shared))
            {
                stm.Write(data, 0, data.Length);
                stm.SetLength(128*1024);
                Assert.Equal(128*1024, stm.Length);
                var ar = stm.ToArray();
                Assert.Equal(data, ar.Take(data.Length));
            }
        }
    }
}
