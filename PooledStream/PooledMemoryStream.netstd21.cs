namespace PooledStream
{
    using System;
    using System.IO;
    using System.Buffers;
    using System.Runtime.CompilerServices;
    public partial class PooledMemoryStream : Stream
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int ReadInternal(Span<byte> buffer)
        {
            int readlen = buffer.Length > (int)(_Length - _Position) ? (int)(_Length - _Position) : buffer.Length;
            if (readlen > 0)
            {
                _currentbuffer.AsSpan((int)_Position, readlen).CopyTo(buffer);
                _Position += readlen;
                return readlen;
            }
            else
            {
                return 0;
            }
        }
        public override int Read(Span<byte> buffer)
        {
            return ReadInternal(buffer);
        }
        public override int Read(byte[] buffer, int offset, int count)
        {
            return ReadInternal(buffer.AsSpan(offset, count));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void WriteInternal(in ReadOnlySpan<byte> buffer)
        {
            if (!_CanWrite)
            {
                throw new InvalidOperationException("stream is readonly");
            }
            int endOffset = _Position + buffer.Length;
            if (endOffset > _currentbuffer.Length)
            {
                ReallocateBuffer((int)(endOffset) * 2);
            }
            buffer.CopyTo(_currentbuffer.AsSpan((int)_Position));
            if (endOffset > _Length)
            {
                _Length = endOffset;
            }
            _Position = endOffset;
        }
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            WriteInternal(buffer);
        }
        /// <summary>write data to stream</summary>
        /// <remarks>if stream data length is over int.MaxValue, this method throws IndexOutOfRangeException</remarks>
        public override void Write(byte[] buffer, int offset, int count)
        {
            WriteInternal(buffer.AsSpan(offset, count));
        }

    }
}