namespace PooledStream
{
#if NETSTANDARD2_1
    using System;
    using System.IO;
    using System.Buffers;
    public partial class PooledMemoryStream : Stream
    {
        public override int Read(Span<byte> buffer)
        {
            int readlen = buffer.Length > (int)(_Length - _Position) ? (int)(_Length - _Position) : buffer.Length;
            if(readlen > 0)
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
        public override int Read(byte[] buffer, int offset, int count)
        {
            return Read(buffer.AsSpan(offset, count));
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long oldValue = _Position;
            switch ((int)origin)
            {
                case (int)SeekOrigin.Begin:
                    _Position = offset;
                    break;
                case (int)SeekOrigin.End:
                    _Position = _Length - offset;
                    break;
                case (int)SeekOrigin.Current:
                    _Position += offset;
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
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            if (!_CanWrite)
            {
                throw new InvalidOperationException("stream is readonly");
            }
            long endOffset = _Position + buffer.Length;
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
        /// <summary>write data to stream</summary>
        /// <remarks>if stream data length is over int.MaxValue, this method throws IndexOutOfRangeException</remarks>
        public override void Write(byte[] buffer, int offset, int count)
        {
            Write(buffer.AsSpan(offset, count));
        }

    }
#endif
}