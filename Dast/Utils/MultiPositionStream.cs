using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Dast.Utils
{
    public class MultiPositionStream<TKey>
        where TKey : struct
    {
        private readonly Stream _stream;
        private readonly Dictionary<TKey, long> _positions = new Dictionary<TKey, long>();
        private TKey _currentKey;
        private long _currentTextLength;

        public Stream this[TKey key]
        {
            get
            {
                long previousPosition = _positions[_currentKey];

                foreach (TKey nextKeys in _positions.Where(x => x.Value >= previousPosition).Select(x => x.Key))
                    _positions[nextKeys] += _currentTextLength;

                _currentKey = key;
                _currentTextLength = 0;

                _stream.Seek(_positions[key], SeekOrigin.Begin);
                return new Wrapper(this, _stream);
            }
        }

        public MultiPositionStream(Stream stream)
        {
            _stream = stream;
        }

        public void AddPosition(TKey key, long position)
        {
            _positions.Add(key, position);
        }

        private class Wrapper : Stream
        {
            private readonly MultiPositionStream<TKey> _handler;
            private readonly Stream _stream;

            public override bool CanRead => false;
            public override bool CanSeek => false;
            public override bool CanWrite => true;
            public override long Length => _stream.Length;
            public override bool CanTimeout => _stream.CanTimeout;
            public override int ReadTimeout => _stream.ReadTimeout;
            public override int WriteTimeout => _stream.WriteTimeout;

            public override long Position
            {
                get => throw new InvalidOperationException();
                set => throw new InvalidOperationException();
            }

            public Wrapper(MultiPositionStream<TKey> handler, Stream stream)
            {
                _handler = handler;
                _stream = stream;
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                long previousPosition = _stream.Position;
                _stream.Write(buffer, offset, count);
                _handler._currentTextLength += _stream.Position - previousPosition;
            }

            public override void WriteByte(byte value)
            {
                long previousPosition = _stream.Position;
                _stream.WriteByte(value);
                _handler._currentTextLength += _stream.Position - previousPosition;
            }

            public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                long previousPosition = _stream.Position;
                await _stream.WriteAsync(buffer, offset, count, cancellationToken);
                _handler._currentTextLength += _stream.Position - previousPosition;
            }

            public override void Flush() => _stream.Flush();
            public override Task FlushAsync(CancellationToken cancellationToken) => _stream.FlushAsync(cancellationToken);

            public override int Read(byte[] buffer, int offset, int count) => throw new InvalidOperationException();
            public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => throw new InvalidOperationException();
            public override int ReadByte() => throw new InvalidOperationException();
            public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken) => throw new InvalidOperationException();
            public override long Seek(long offset, SeekOrigin origin) => throw new InvalidOperationException();
            public override void SetLength(long value) => throw new InvalidOperationException();

            protected override void Dispose(bool disposing)
            {
            }
        }
    }
}