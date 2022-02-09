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
        public PooledMemoryBufferWriter() : this(ArrayPool<T>.Shared)
        {

        }
        public PooledMemoryBufferWriter(int preallocateSize) : this(ArrayPool<T>.Shared, preallocateSize)
        {

        }
        public PooledMemoryBufferWriter(ArrayPool<T> pool) : this(pool, DefaultSize)
        {
        }
        public PooledMemoryBufferWriter(ArrayPool<T> pool, int preallocateSize)
        {
            _Pool = pool;
            _currentBuffer = null;
            _Position = 0;
            _Length = 0;
            Reallocate(preallocateSize);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Advance(int count)
        {
            if (_Position + count > _currentBuffer.Length)
            {
                throw new IndexOutOfRangeException("advance too many(" + count.ToString() + ")");
            }
            _Position += count;
            if (_Length < _Position)
            {
                _Length = _Position;
            }
        }

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Memory<T> GetMemory(int sizeHint = 0)
        {
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> GetSpan(int sizeHint = 0)
        {
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> ToSpanUnsafe()
        {
            return _currentBuffer.AsSpan(0, _Length);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory<T> ToMemoryUnsafe()
        {
            return _currentBuffer.AsMemory(0, _Length);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset(int preallocateSize = DefaultSize)
        {
            _Pool.Return(_currentBuffer);
            _currentBuffer = _Pool.Rent(preallocateSize);
            _Length = 0;
            _Position = 0;
        }
    }
}