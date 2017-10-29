using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Dast.Outputs.Base
{
    public abstract class FragmentedDocumentWriterBase<TMedia, TFragment> : DocumentWriterSharedBase<TMedia, IDictionary<TFragment, string>>, IFragmentedDocumentWriter<TMedia, TFragment>
        where TMedia : IMediaOutput
        where TFragment : struct
    {
        private TextWriter _mainWriter;
        protected override TextWriter MainWriter => _mainWriter;

        private Stream _currentStream;
        private TFragment? _currentStreamKey;
        private List<TFragment> _streamKeys;
        private Dictionary<TFragment, StringWriter> _stringWriters;
        private Dictionary<TFragment, Func<Stream>> _streamProviders;
        protected abstract IEnumerable<TFragment> DefaultKeys { get; }
        IEnumerable<TMedia> IDocumentOutput<TMedia, IDictionary<TFragment, string>>.MediaOutputs => MediaOutputs;

        protected TFragment? CurrentStream
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

                if (_currentStreamKey == null || !_streamKeys.Contains(_currentStreamKey.Value))
                {
                    _currentStream = null;
                    _mainWriter = TextWriter.Null;
                    return;
                }

                if (_streamProviders != null)
                {
                    _currentStream = _streamProviders[_currentStreamKey.Value]();
                    _mainWriter = new StreamWriter(_currentStream);
                }
                else if (_stringWriters != null)
                {
                    _currentStream = null;
                    _mainWriter = _stringWriters[_currentStreamKey.Value];
                }
                else
                    throw new InvalidOperationException();
            }
        }

        public IDictionary<TFragment, string> Convert(IDocumentNode node, IEnumerable<TFragment> streamKeys)
        {
            _streamKeys = (streamKeys ?? DefaultKeys).ToList();
            _stringWriters = _streamKeys.ToDictionary(x => x, x => new StringWriter());

            node.Accept(this);
            Dictionary<TFragment, string> result = _stringWriters.ToDictionary(x => x.Key, x => x.Value.ToString());

            _currentStream = null;
            _currentStreamKey = null;
            _streamKeys = null;
            _stringWriters = null;
            _mainWriter = null;

            return result;
        }
        
        public void Convert(IDocumentNode node, IDictionary<TFragment, Func<Stream>> streamProviders)
        {
            _streamKeys = streamProviders.Keys.ToList();
            _streamProviders = streamProviders.ToDictionary(x => x.Key, x => x.Value);

            node.Accept(this);

            _currentStream = null;
            _currentStreamKey = null;
            _streamKeys = null;
            _streamProviders = null;
            _mainWriter = null;
        }

        public override IDictionary<TFragment, string> Convert(IDocumentNode node) => Convert(node, DefaultKeys);
        public IDictionary<TFragment, string> Convert(IDocumentNode node, params TFragment[] streamKeys) => Convert(node, streamKeys.AsEnumerable());
        IDictionary<TFragment, string> IDocumentOutput<IDictionary<TFragment, string>>.Convert(IDocumentNode node) => Convert(node, DefaultKeys);
    }
}