using System;
using System.Collections.Generic;
using System.IO;
using Dast.Utils;

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

        public string Convert(IDocumentNode node)
        {
            IDictionary<TFragment, string> fragmentResults = DocumentMultiWriter.Convert(node, MergeFragments(TextWriter.Null));

            string result;
            using (var writer = new StringWriter())
            {
                foreach (TFragment fragment in MergeFragments(writer))
                    writer.Write(fragmentResults[fragment]);

                result = writer.ToString();
            }
            return result;
        }

        public void Convert(IDocumentNode node, Stream stream)
        {
            var multiPositionStream = new MultiPositionStream<TFragment>(stream);
            var streams = new Dictionary<TFragment, Func<Stream>>();

            using (var writer = new StreamWriter(stream))
                foreach (TFragment fragment in MergeFragments(writer))
                {
                    multiPositionStream.AddPosition(fragment, stream.Position);
                    streams[fragment] = () => multiPositionStream[fragment];
                }

            DocumentMultiWriter.Convert(node, streams);
        }

        protected abstract IEnumerable<TFragment> MergeFragments(TextWriter writer);
    }
}