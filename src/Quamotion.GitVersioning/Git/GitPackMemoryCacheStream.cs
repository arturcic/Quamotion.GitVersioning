﻿using System;
using System.Buffers;
using System.IO;
using System.Linq;

namespace Quamotion.GitVersioning.Git
{
    class GitPackMemoryCacheStream : Stream
    {
        private readonly Stream stream;
        private readonly MemoryStream cacheStream = new MemoryStream();
        private long position = 0;

        public GitPackMemoryCacheStream(Stream stream)
        {
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
        }

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => false;

        public override long Length => stream.Length;

        public override long Position
        {
            get => this.cacheStream.Position;
            set => throw new NotSupportedException();
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override int Read(Span<byte> buffer)
        {
            if (this.cacheStream.Position + buffer.Length >= this.cacheStream.Length)
            {
                var currentPosition = this.cacheStream.Position;
                var toRead = (int)(buffer.Length - this.cacheStream.Length + this.cacheStream.Position);
                this.stream.Read(buffer.Slice(0, toRead));
                this.cacheStream.Seek(0, SeekOrigin.End);
                this.cacheStream.Write(buffer.Slice(0, toRead));
                this.cacheStream.Seek(currentPosition, SeekOrigin.Begin);
            }

            return this.cacheStream.Read(buffer);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return this.Read(buffer.AsSpan(offset, count));
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (origin != SeekOrigin.Begin)
            {
                throw new NotSupportedException();
            }

            if (offset > this.cacheStream.Length)
            {
                var toRead = (int)(offset - this.cacheStream.Length);
                byte[] buffer = ArrayPool<byte>.Shared.Rent(toRead);
                this.stream.Read(buffer.AsSpan(0, toRead));
                this.cacheStream.Seek(0, SeekOrigin.End);
                this.cacheStream.Write(buffer.AsSpan(0, toRead));
                ArrayPool<byte>.Shared.Return(buffer);
                return this.cacheStream.Position;
            }
            else
            {
                return this.cacheStream.Seek(offset, origin);
            }
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }
}
