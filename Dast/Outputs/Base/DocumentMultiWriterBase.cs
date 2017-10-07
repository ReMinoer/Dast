using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Dast.Outputs.Base
{
    public abstract class DocumentMultiWriterBase<TMedia, TStreamKey> : DocumentWriterSharedBase<TMedia, IDictionary<TStreamKey, string>>, IDocumentMultiWriter<TMedia, TStreamKey>
        where TMedia : IMediaOutput
        where TStreamKey : struct
    {
        private TextWriter _mainWriter;
        protected override TextWriter MainWriter => _mainWriter;

        private Stream _currentStream;
        private TStreamKey _currentStreamKey;
        private List<TStreamKey> _streamKeys;
        private Dictionary<TStreamKey, StringWriter> _stringWriters;
        private Dictionary<TStreamKey, Func<Stream>> _streamProviders;
        protected abstract IEnumerable<TStreamKey> DefaultKeys { get; }
        IEnumerable<TMedia> IDocumentOutput<TMedia, IDictionary<TStreamKey, string>>.MediaOutputs => MediaOutputs;

        protected TStreamKey CurrentStream
        {
            get => _currentStreamKey;
            set
            {
                if (_currentStreamKey.Equals(value))
                    return;

                _currentStreamKey = value;

                if (_currentStream != null)
                {
                    MainWriter.Dispose();
                    _currentStream.Dispose();
                }

                if (!_streamKeys.Contains(value))
                {
                    _currentStream = null;
                    _mainWriter = TextWriter.Null;
                    return;
                }

                if (_streamProviders != null)
                {
                    _currentStream = _streamProviders[value]();
                    _mainWriter = new StreamWriter(_currentStream);
                }
                else if (_stringWriters != null)
                {
                    _currentStream = null;
                    _mainWriter = _stringWriters[value];
                }
                else
                    throw new InvalidOperationException();
            }
        }

        public IDictionary<TStreamKey, string> Convert(IDocumentNode node, IEnumerable<TStreamKey> streamKeys)
        {
            _streamKeys = (streamKeys ?? DefaultKeys).ToList();
            _stringWriters = _streamKeys.ToDictionary(x => x, x => new StringWriter());

            node.Accept(this);
            Dictionary<TStreamKey, string> result = _stringWriters.ToDictionary(x => x.Key, x => x.Value.ToString());

            _streamKeys = null;
            _stringWriters = null;
            _mainWriter = null;

            return result;
        }
        
        public void Convert(IDocumentNode node, IDictionary<TStreamKey, Func<Stream>> streamProviders)
        {
            _streamKeys = streamProviders.Keys.ToList();
            _streamProviders = streamProviders.ToDictionary(x => x.Key, x => x.Value);

            node.Accept(this);

            _streamKeys = null;
            _streamProviders = null;
            _mainWriter = null;
        }

        public override IDictionary<TStreamKey, string> Convert(IDocumentNode node) => Convert(node, DefaultKeys);
        public IDictionary<TStreamKey, string> Convert(IDocumentNode node, params TStreamKey[] streamKeys) => Convert(node, streamKeys.AsEnumerable());
        IDictionary<TStreamKey, string> IDocumentOutput<IDictionary<TStreamKey, string>>.Convert(IDocumentNode node) => Convert(node, DefaultKeys);
    }
}