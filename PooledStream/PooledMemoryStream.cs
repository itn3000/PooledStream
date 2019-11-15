namespace PooledStream
{
    using System;
    using System.IO;
    using System.Buffers;
    using System.Runtime.CompilerServices;
    public sealed partial class PooledMemoryStream : Stream
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ReallocateBuffer(int minimumRequired)
        {
            var tmp = m_Pool.Rent(minimumRequired);
            if (_currentbuffer != null)
            {
                // #if NETSTANDARD2_1
                //                 _currentbuffer.AsSpan().CopyTo(tmp);
                // #else
                //                 Buffer.BlockCopy(_currentbuffer, 0, tmp, 0, _currentbuffer.Length);
                // #endif
                Buffer.BlockCopy(_currentbuffer, 0, tmp, 0, _currentbuffer.Length < tmp.Length ? _currentbuffer.Length : tmp.Length);
                m_Pool.Return(_currentbuffer);
            }
            _currentbuffer = tmp;
        }
        /// <summary>set stream length</summary>
        /// <remarks>if length is larger than current buffer length, re-allocating buffer</remarks>
        /// <exception cref="System.InvalidOperationException">if stream is readonly</exception>
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
            if (_currentbuffer == null || _currentbuffer.Length < _Length)
            {
                ReallocateBuffer((int)_Length);
            }
            if (_Position >= _Length)
            {
                if (_Length == 0)
                {
                    _Position = 0;
                }
                else
                {
                    _Position = _Length - 1;
                }
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
            _Length = 0;
            _Position = 0;
        }
        /// <summary>ensure the buffer size</summary>
        /// <remarks>capacity != stream buffer length</remarks>
        /// <exception cref="System.InvalidOperationException">if stream is readonly</exception>
        public void Reserve(int capacity)
        {
            if (!_CanWrite)
            {
                throw new InvalidOperationException("stream is readonly");
            }
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
        public override int Read(byte[] buffer, int offset, int count)
        {
            int readlen = count > (int)(_Length - _Position) ? (int)(_Length - _Position) : count;
            if (readlen > 0)
            {
                Buffer.BlockCopy(_currentbuffer
                    , (int)_Position
                    , buffer, offset
                    , readlen)
                    ;
                _Position += readlen;
                return readlen;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>write data to stream</summary>
        /// <remarks>if stream data length is over int.MaxValue, this method throws IndexOutOfRangeException</remarks>
        /// <exception cref="System.InvalidOperationException">if stream is readonly</exception>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (!_CanWrite)
            {
                throw new InvalidOperationException("stream is readonly");
            }
            int endOffset = _Position + count;
            if (_currentbuffer == null || endOffset > _currentbuffer.Length)
            {
                ReallocateBuffer((int)(endOffset) * 2);
            }
            Buffer.BlockCopy(buffer, offset,
                _currentbuffer, (int)_Position, count);
            if (endOffset > _Length)
            {
                _Length = endOffset;
            }
            _Position = endOffset;
        }
        /// <summary>shrink internal buffer by re-allocating memory</summary>
        /// <remarks>if internal buffer is shorter than minimumRequired, nothing to do</remarks>
        /// <exception cref="System.InvalidOperationException">if stream is readonly</exception>
        public void Shrink(int minimumRequired)
        {
            if (!_CanWrite)
            {
                throw new InvalidOperationException("stream is readonly");
            }
            if (_currentbuffer == null)
            {
                return;
            }
            if(_currentbuffer.Length > minimumRequired)
            {
                ReallocateBuffer(minimumRequired);
            }
            if (minimumRequired <= _Length)
            {
                _Length = minimumRequired;
            }
        }
        /// <summary>get internal data as Span</summary>
        /// <remarks>you must not use returned value outside of stream's lifetime</remarks>
        public ReadOnlySpan<byte> ToSpanUnsafe()
        {
            if (_currentbuffer == null || _Length <= 0)
            {
                return Span<byte>.Empty;
            }
            return _currentbuffer.AsSpan(0, _Length);
        }
        /// <summary>get internal data as Memory</summary>
        /// <remarks>you must not use returned value outside of stream's lifetime</remarks>
        public ReadOnlyMemory<byte> ToMemoryUnsafe()
        {
            if (_currentbuffer == null || _Length <= 0)
            {
                return Memory<byte>.Empty;
            }
            return _currentbuffer.AsMemory(0, _Length);
        }
    }
}