using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace PooledStream
{
    public class PooledMemoryBufferWriter<T> : IDisposable, IBufferWriter<T> where T : struct
    {
        ArrayPool<T> _Pool;
        T[] _currentBuffer;
        int _Position;
        int _Length;
        const int DefaultSize = 1024;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Reallocate(int sizeHint)
        {
            var nar = _Pool.Rent(sizeHint);
            if (_currentBuffer != null)
            {
                Buffer.BlockCopy(_currentBuffer, 0, nar, 0, _currentBuffer.Length < nar.Length ? _currentBuffer.Length : nar.Length);
                _Pool.Return(_currentBuffer);
            }
            _currentBuffer = nar;
        }
        /// <summary>
        /// use shared instance and preallocatedSize = 1024
        /// </summary>
        public PooledMemoryBufferWriter() : this(ArrayPool<T>.Shared)
        {

        }
        /// <summary>
        /// use shared instance, use preallocateSize as reserved buffer length
        /// </summary>
        /// <param name="preallocateSize">initial reserved buffer size</param>
        public PooledMemoryBufferWriter(int preallocateSize) : this(ArrayPool<T>.Shared, preallocateSize)
        {

        }
        /// <summary>
        /// use pool for memory pool
        /// </summary>
        /// <param name="pool">memory pool</param>
        public PooledMemoryBufferWriter(ArrayPool<T> pool) : this(pool, DefaultSize)
        {
        }
        /// <summary>
        /// </summary>
        /// <param name="pool">memory pool</param>
        /// <param name="preallocateSize">initial reserved buffer size</param>
        public PooledMemoryBufferWriter(ArrayPool<T> pool, int preallocateSize)
        {
            if(pool == null)
            {
                throw new ArgumentNullException(nameof(pool));
            }
            if(preallocateSize < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(preallocateSize), "size must be greater than 0");
            }
            _Pool = pool;
            _currentBuffer = null;
            _Position = 0;
            _Length = 0;
            Reallocate(preallocateSize);
        }
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Advance(int count)
        {
            if (_Position + count > _currentBuffer.Length)
            {
                throw new ArgumentOutOfRangeException("advance too many(" + count.ToString() + ")");
            }
            _Position += count;
            if (_Length < _Position)
            {
                _Length = _Position;
            }
        }

        /// <summary>return buffer to pool and reset buffer status</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            if (_currentBuffer != null)
            {
                _Pool.Return(_currentBuffer);
                _currentBuffer = null;
                _Position = 0;
                _Length = 0;
            }
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Memory<T> GetMemory(int sizeHint = 0)
        {
            if (sizeHint < 0)
            {
                throw new ArgumentOutOfRangeException("sizeHint", "size must be greater than 0");
            }
            if (sizeHint == 0)
            {
                sizeHint = DefaultSize;
            }
            if (_Position + sizeHint > _currentBuffer.Length)
            {
                Reallocate(_Position + sizeHint);
            }
            return _currentBuffer.AsMemory(_Position, sizeHint);
        }
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> GetSpan(int sizeHint = 0)
        {
            if (sizeHint < 0)
            {
                throw new ArgumentOutOfRangeException("sizeHint", "size must be greater than 0");
            }
            if (sizeHint == 0)
            {
                sizeHint = DefaultSize;
            }
            if (_Position + sizeHint > _currentBuffer.Length)
            {
                Reallocate(_Position + sizeHint);
            }
            return _currentBuffer.AsSpan(_Position, sizeHint);
        }
        /// <summary>expose current buffer as Span</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> ToSpanUnsafe()
        {
            return _currentBuffer.AsSpan(0, _Length);
        }
        /// <summary>expose current buffer as Memory</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory<T> ToMemoryUnsafe()
        {
            return _currentBuffer.AsMemory(0, _Length);
        }
        /// <summary>reset buffer status, buffer will be reallocated</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset(int preallocateSize)
        {
            if(preallocateSize < 0)
            {
                throw new ArgumentOutOfRangeException("preallocateSize", "size must be greater than 0");
            }
            _Pool.Return(_currentBuffer);
            _currentBuffer = _Pool.Rent(preallocateSize);
            _Length = 0;
            _Position = 0;
        }
        /// <summary>reset buffer status, buffer will be reused</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            _Length = 0;
            _Position = 0;
        }
    }
}