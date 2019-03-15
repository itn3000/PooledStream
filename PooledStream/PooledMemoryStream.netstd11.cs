namespace PooledStream
{
    using System;
    using System.IO;
    using System.Buffers;
    public partial class PooledMemoryStream : Stream
    {
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
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (!_CanWrite)
            {
                throw new InvalidOperationException("stream is readonly");
            }
            int endOffset = _Position + count;
            if (endOffset > _currentbuffer.Length)
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

    }
}