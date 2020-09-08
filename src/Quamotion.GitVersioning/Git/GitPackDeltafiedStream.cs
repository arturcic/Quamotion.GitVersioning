﻿using System;
using System.Buffers;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Reflection.Metadata.Ecma335;

namespace Quamotion.GitVersioning.Git
{
    class GitPackDeltafiedStream : Stream
    {
        private readonly long length;
        private long position;

        private readonly Stream baseStream;
        private readonly Stream deltaStream;

        private DeltaInstruction? current;
        private int offset;

        public GitPackDeltafiedStream(Stream baseStream, Stream deltaStream, long length)
        {
            this.baseStream = baseStream ?? throw new ArgumentNullException(nameof(baseStream));
            this.deltaStream = deltaStream ?? throw new ArgumentNullException(nameof(deltaStream));
            this.length = length;
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => this.length;

        public override long Position
        {
            get => this.position;
            set => throw new NotImplementedException();
        }

        public override int Read(Span<byte> buffer)
        {
            int read = 0;
            int canRead = 0;
            int didRead = 0;

            while (read < buffer.Length && this.TryGetInstruction(out DeltaInstruction instruction))
            {
                var source = instruction.InstructionType == DeltaInstructionType.Copy ? this.baseStream : this.deltaStream;

                Debug.Assert(instruction.Size > this.offset);
                canRead = Math.Min(buffer.Length - read, instruction.Size - this.offset);
                didRead = source.Read(buffer.Slice(read, canRead));

                Debug.Assert(didRead != 0);
                read += didRead;
                offset += didRead;
            }

            this.position += read;
            Debug.Assert(read <= buffer.Length);
            return read;
        }

        private bool TryGetInstruction(out DeltaInstruction instruction)
        {
            if (current != null && this.offset < current.Value.Size)
            {
                instruction = current.Value;
                return true;
            }

            current = DeltaStreamReader.Read(this.deltaStream);

            if (current == null)
            {
                instruction = default;
                return false;
            }

            instruction = current.Value;

            switch (instruction.InstructionType)
            {
                case DeltaInstructionType.Copy:
                    this.baseStream.Seek(instruction.Offset, SeekOrigin.Begin);
                    Debug.Assert(this.baseStream.Position == instruction.Offset);
                    this.offset = 0;
                    break;

                case DeltaInstructionType.Insert:
                    this.offset = 0;
                    break;

                default:
                    throw new GitException();
            }

            return true;
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return this.Read(buffer.AsSpan(offset, count));
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (origin == SeekOrigin.Begin && offset == this.position)
            {
                return this.position;
            }

            if (origin == SeekOrigin.Current && offset == 0)
            {
                return this.position;
            }

            if (origin == SeekOrigin.Begin && offset > this.position)
            {
                // We can optimise this by skipping over instructions rather than executing them
                int length = (int)(offset - this.position);

                byte[] buffer = ArrayPool<byte>.Shared.Rent(length);
                this.Read(buffer, 0, length);
                ArrayPool<byte>.Shared.Return(buffer);
                return this.position;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        protected override void Dispose(bool disposing)
        {
            this.deltaStream.Dispose();
            this.baseStream.Dispose();
        }
    }
}
