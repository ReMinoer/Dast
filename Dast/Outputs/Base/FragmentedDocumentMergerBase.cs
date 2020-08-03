using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Dast.Outputs.Base
{
    public abstract class FragmentedDocumentMergerBase<TMultiWriter, TMedia, TFragment> : IDocumentWriter<TMedia>
        where TMultiWriter : IFragmentedDocumentWriter<TMedia, TFragment>, new()
        where TMedia : IMediaOutput
        where TFragment : struct
    {
        protected TMultiWriter DocumentMultiWriter { get; } = new TMultiWriter();

        public string DisplayName => DocumentMultiWriter.DisplayName;
        public IEnumerable<FileExtension> FileExtensions => DocumentMultiWriter.FileExtensions;
        public FileExtension FileExtension => DocumentMultiWriter.FileExtension;
         
        IEnumerable<IMediaOutput> IDocumentOutput.MediaOutputs => ((IDocumentOutput)DocumentMultiWriter).MediaOutputs;
        IEnumerable<TMedia> IDocumentOutput<TMedia, string>.MediaOutputs => DocumentMultiWriter.MediaOutputs;

        private bool _conditional;
        private bool _writing;
        private bool _analysing;
        private TextWriter _writer;
        private ConditionalAnalysing _currentConditionalAnalysing;
        private ConditionalWriting _currentConditionalWriting;

        protected TextWriter Writer => _writing ? (_currentConditionalWriting?.Writer ?? _writer) : TextWriter.Null;
        protected IDisposable Conditional => _writing && _conditional ? new Disposable(new ConditionalWriting(this)) : (_analysing ? new Disposable(new ConditionalAnalysing(this)) : null);

        protected abstract IEnumerable<TFragment> MergeFragments();

        public string Convert(IDocumentNode node)
        {
            string result;

            using (var stringWriter = new StringWriter())
            {
                _writer = stringWriter;
                
                if (IsUsingConditional())
                    ConvertProcess(node);
                else
                    ConvertProcessUnconditional(node);
                
                result = stringWriter.ToString();
                _writer = null;
            }

            return result;
        }

        public void Convert(IDocumentNode node, Stream stream)
        {
            using (var streamWriter = new StreamWriter(stream))
            {
                _writer = streamWriter;
                ConvertProcess(node);
                _writer = null;
            }
        }

        public Task GetResourceFilesAsync(string outputDirectory) => DocumentMultiWriter.GetResourceFilesAsync(outputDirectory);

        public bool IsUsingConditional()
        {
            _analysing = true;
            ConditionalAnalysing CurrentConditionalAnalysing() => _currentConditionalAnalysing;
            bool result = MergeFragments().Any(_ => CurrentConditionalAnalysing() != null);
            _analysing = false;

            return result;
        }

        private void ConvertProcess(IDocumentNode node)
        {
            IDictionary<TFragment, string> fragmentResults = DocumentMultiWriter.Convert(node, MergeFragments());

            _conditional = true;
            _writing = true;
            foreach (TFragment fragment in MergeFragments())
            {
                _currentConditionalWriting?.Fragments.Add(fragment);

                string fragmentResult = fragmentResults[fragment];
                if (string.IsNullOrEmpty(fragmentResult))
                    continue;

                _currentConditionalWriting?.Apply(_writer);
                _writer.Write(fragmentResult);
            }
            _writing = false;
            _conditional = false;

            if (_currentConditionalWriting != null)
                throw new InvalidOperationException();
        }

        private void ConvertProcessUnconditional(IDocumentNode node)
        {
            IDictionary<TFragment, string> fragmentResults = DocumentMultiWriter.Convert(node, MergeFragments());

            _writing = true;
            foreach (TFragment fragment in MergeFragments())
                _writer.Write(fragmentResults[fragment]);
            _writing = false;
        }

        private class ConditionalAnalysing : IDisposable
        {
            private readonly FragmentedDocumentMergerBase<TMultiWriter, TMedia, TFragment> _handler;
            private readonly ConditionalAnalysing _previous;

            public ConditionalAnalysing(FragmentedDocumentMergerBase<TMultiWriter, TMedia, TFragment> handler)
            {
                _handler = handler;
                _previous = handler._currentConditionalAnalysing;
                
                _handler._currentConditionalAnalysing = this;
            }

            public void Dispose()
            {
                _handler._currentConditionalAnalysing = _previous;
            }
        }

        private class ConditionalWriting : IDisposable
        {
            private readonly FragmentedDocumentMergerBase<TMultiWriter, TMedia, TFragment> _handler;
            private readonly ConditionalWriting _previous;

            private bool _applied;
            private bool _gotChildren;
            private StringWriter _prefixWriter = new StringWriter();

            public List<TFragment> Fragments { get; } = new List<TFragment>();
            public TextWriter Writer { get; private set; }

            public ConditionalWriting(FragmentedDocumentMergerBase<TMultiWriter, TMedia, TFragment> handler)
            {
                _handler = handler;
                _previous = handler._currentConditionalWriting;
                _handler._currentConditionalWriting = this;

                Writer = _prefixWriter;
            }

            public void Apply(TextWriter mainWriter)
            {
                _previous?.Apply(mainWriter);

                if (_applied)
                    return;

                mainWriter.Write(_prefixWriter.ToString());
                _prefixWriter.Dispose();
                _prefixWriter = null;

                Writer = mainWriter;

                _applied = true;
            }

            public void Dispose()
            {
                _prefixWriter?.Dispose();

                _handler._currentConditionalWriting = _previous;
                if (_previous != null)
                    _previous._gotChildren = true;

                if (!_gotChildren && Fragments.Count == 0)
                    throw new InvalidOperationException();
            }
        }

        private class Disposable : IDisposable
        {
            private readonly IDisposable _disposable;
            public Disposable(IDisposable disposable) => _disposable = disposable;
            public void Dispose() => _disposable.Dispose();
        }

        private class UndisposableStream : Stream
        {
            private readonly Stream _stream;

            public override bool CanRead => _stream.CanRead;
            public override bool CanSeek => _stream.CanSeek;
            public override bool CanWrite => _stream.CanWrite;
            public override long Length => _stream.Length;

            public override long Position
            {
                get => _stream.Position;
                set => _stream.Position = value;
            }

            public UndisposableStream(Stream stream)
            {
                _stream = stream;
            }

            protected override void Dispose(bool disposing)
            {
            }

            public override int Read(byte[] buffer, int offset, int count) => _stream.Read(buffer, offset, count);
            public override void Write(byte[] buffer, int offset, int count) => _stream.Write(buffer, offset, count);
            public override long Seek(long offset, SeekOrigin origin) => _stream.Seek(offset, origin);
            public override void SetLength(long value) => _stream.SetLength(value);
            public override void Flush() => _stream.Flush();
        }
    }
}