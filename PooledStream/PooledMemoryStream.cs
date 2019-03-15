namespace PooledStream
{
    using System;
    using System.IO;
    using System.Buffers;
    public partial class PooledMemoryStream : Stream
    {
        /// <summary>create writable memory stream with default parameters</summary>
        /// <remarks>buffer is allocated from ArrayPool.Shared</remarks>
        public PooledMemoryStream()
            : this(ArrayPool<byte>.Shared)
        {
        }
        /// <summary>create writable memory stream with specified ArrayPool</summary>
        /// <remarks>buffer is allocated from ArrayPool</remarks>
        public PooledMemoryStream(ArrayPool<byte> pool)
            : this(pool, 4096)
        {
        }
        /// <summary>create writable memory stream with ensuring buffer length</summary>
        /// <remarks>buffer is allocated from ArrayPool</remarks>
        public PooledMemoryStream(ArrayPool<byte> pool, int capacity)
        {
            m_Pool = pool;
            _currentbuffer = m_Pool.Rent(capacity);
            _Length = 0;
            _CanWrite = true;
            _Position = 0;
        }
        /// <summary>create readonly MemoryStream without buffer copy</summary>
        /// <remarks>data will be read from 'data' parameter</summary>
        public PooledMemoryStream(byte[] data)
        {
            m_Pool = null;
            _currentbuffer = data;
            _Length = data.Length;
            _CanWrite = false;
        }
        public override bool CanRead
        {
            get
            {
                return true;
            }
        }

        public override bool CanSeek => true;

        public override bool CanWrite => _CanWrite;

        public override long Length => _Length;

        public override long Position
        {
            get => _Position;
            set
            {
                _Position = (int)value;
            }
        }

        public override void Flush()
        {
        }

        void ReallocateBuffer(int minimumRequired)
        {
            var tmp = m_Pool.Rent(minimumRequired);
            #if NETSTANDARD2_1
            _currentbuffer.AsSpan().CopyTo(tmp);
            #else
            Buffer.BlockCopy(_currentbuffer, 0, tmp, 0, _currentbuffer.Length);
            #endif
            m_Pool.Return(_currentbuffer);
            _currentbuffer = tmp;
        }
        public override void SetLength(long value)
        {
            if (!_CanWrite)
            {
                throw new NotSupportedException("stream is readonly");
            }
            if (value > int.MaxValue)
            {
                throw new IndexOutOfRangeException("overflow");
            }
            if (value < 0)
            {
                throw new IndexOutOfRangeException("underflow");
            }
            _Length = (int)value;
            if (_currentbuffer.Length < _Length)
            {
                ReallocateBuffer((int)_Length);
            }
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (m_Pool != null && _currentbuffer != null)
            {
                m_Pool.Return(_currentbuffer);
                _currentbuffer = null;
            }
        }
        /// <summary>ensure the buffer size</summary>
        /// <remarks>capacity != stream buffer length</remarks>
        public void Reserve(int capacity)
        {
            if (capacity > _currentbuffer.Length)
            {
                ReallocateBuffer(capacity);
            }
        }

        /// <summary>Create newly allocated buffer and copy the stream data</summary>
        public byte[] ToArray()
        {
            var ret = new byte[_Length];
            Buffer.BlockCopy(_currentbuffer, 0, ret, 0, (int)_Length);
            return ret;
        }
        /// <summary>Create ArraySegment for current stream data without allocation buffer</summary>
        /// <remarks>After disposing stream, manupilating returned value(read or write) may cause undefined behavior</remarks>
        public ArraySegment<byte> ToUnsafeArraySegment()
        {
            return new ArraySegment<byte>(_currentbuffer, 0, (int)_Length);
        }
        public override long Seek(long offset, SeekOrigin origin)
        {
            int oldValue = _Position;
            switch ((int)origin)
            {
                case (int)SeekOrigin.Begin:
                    _Position = (int)offset;
                    break;
                case (int)SeekOrigin.End:
                    _Position = _Length - (int)offset;
                    break;
                case (int)SeekOrigin.Current:
                    _Position += (int)offset;
                    break;
                default:
                    throw new InvalidOperationException("unknown SeekOrigin");
            }
            if (_Position < 0 || _Position > _Length)
            {
                _Position = oldValue;
                throw new IndexOutOfRangeException();
            }
            return _Position;
        }
        ArrayPool<byte> m_Pool;
        byte[] _currentbuffer;
        bool _CanWrite;
        int _Length;
        int _Position;
    }
}