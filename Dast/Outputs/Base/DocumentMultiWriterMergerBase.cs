using System;
using System.Collections.Generic;
using System.IO;

namespace Dast.Outputs.Base
{
    public abstract class DocumentMultiWriterMergerBase<TMultiWriter, TMedia, TFragment> : IDocumentWriter<TMedia>
        where TMultiWriter : IDocumentMultiWriter<TMedia, TFragment>, new()
        where TMedia : IMediaOutput
        where TFragment : struct
    {
        protected TMultiWriter DocumentMultiWriter { get; } = new TMultiWriter();

        public string DisplayName => DocumentMultiWriter.DisplayName;
        public IEnumerable<FileExtension> FileExtensions => DocumentMultiWriter.FileExtensions;
        public FileExtension FileExtension => DocumentMultiWriter.FileExtension;
         
        IEnumerable<IMediaOutput> IDocumentOutput.MediaOutputs => ((IDocumentOutput)DocumentMultiWriter).MediaOutputs;
        IEnumerable<TMedia> IDocumentOutput<TMedia, string>.MediaOutputs => DocumentMultiWriter.MediaOutputs;
        
        private bool _writing;
        private TextWriter _writer;
        private ConditionalWriting _currentConditional;

        protected TextWriter Writer => _writing ? (_currentConditional?.Writer ?? _writer) : TextWriter.Null;
        protected IDisposable Conditional => _writing ? new Disposable(new ConditionalWriting(this)) : null;

        protected abstract IEnumerable<TFragment> MergeFragments();

        public string Convert(IDocumentNode node)
        {
            string result;
            using (var stringWriter = new StringWriter())
            {
                _writer = stringWriter;
                ConvertProcess(node);
                result = stringWriter.ToString();
            }
            return result;
        }

        public void Convert(IDocumentNode node, Stream stream)
        {
            using (var streamWriter = new StreamWriter(stream))
            {
                _writer = streamWriter;
                ConvertProcess(node);
            }
        }

        private void ConvertProcess(IDocumentNode node)
        {
            IDictionary<TFragment, string> fragmentResults = DocumentMultiWriter.Convert(node, MergeFragments());
            
            _writing = true;
            foreach (TFragment fragment in MergeFragments())
            {
                _currentConditional.Fragments.Add(fragment);

                string fragmentResult = fragmentResults[fragment];
                if (string.IsNullOrEmpty(fragmentResult))
                    continue;

                _currentConditional.Apply(_writer);
                _writer.Write(fragmentResult);
            }
            _writing = false;

            if (_currentConditional != null)
                throw new InvalidOperationException();

            _writer = null;
        }

        private class ConditionalWriting : IDisposable
        {
            private readonly DocumentMultiWriterMergerBase<TMultiWriter, TMedia, TFragment> _handler;
            private readonly ConditionalWriting _previous;

            private bool _applied;
            private bool _gotChildren;
            private StringWriter _prefixWriter = new StringWriter();

            public List<TFragment> Fragments { get; } = new List<TFragment>();
            public TextWriter Writer { get; private set; }

            public ConditionalWriting(DocumentMultiWriterMergerBase<TMultiWriter, TMedia, TFragment> handler)
            {
                _handler = handler;
                _previous = handler._currentConditional;
                _handler._currentConditional = this;

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

                _handler._currentConditional = _previous;
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
    }
}