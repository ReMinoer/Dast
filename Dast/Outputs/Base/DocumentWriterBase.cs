using System.IO;

namespace Dast.Outputs.Base
{
    public abstract class DocumentWriterBase<TMedia> : DocumentWriterSharedBase<TMedia, string>, IDocumentWriter<TMedia>
        where TMedia : IMediaOutput
    {
        private TextWriter _mainWriter;
        protected override TextWriter MainWriter => _mainWriter;

        public override string Convert(IDocumentNode node)
        {
            string result;
            using (_mainWriter = new StringWriter())
            {
                node.Accept(this);
                result = _mainWriter.ToString();
            }
            _mainWriter = null;

            return result;
        }

        public void Convert(IDocumentNode node, Stream stream)
        {
            using (_mainWriter = new StreamWriter(stream))
                node.Accept(this);
            _mainWriter = null;
        }
    }
}