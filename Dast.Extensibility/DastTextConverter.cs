using System.Collections;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Linq;
using Dast.Inputs;
using Dast.Outputs;

namespace Dast.Extensibility
{
    public class DastTextConverter : DastConverter<string, string>, IExtensible<IDocumentReader>, IExtensible<IDocumentWriter>
    {
        public ExtensibleFormatCatalog<IDocumentReader> ReaderCatalog { get; } = new ExtensibleFormatCatalog<IDocumentReader>();
        public ExtensibleFormatCatalog<IDocumentWriter> WriterCatalog { get; } = new ExtensibleFormatCatalog<IDocumentWriter>();
        ICollection<IDocumentReader> IExtensible<IDocumentReader>.Extensions => ReaderCatalog;
        ICollection<IDocumentWriter> IExtensible<IDocumentWriter>.Extensions => WriterCatalog;

        public void Convert(FileExtension inputExtension, Stream inputStream, FileExtension outputExtension, Stream outputStream)
        {
            IDocumentReader reader = ReaderCatalog.FirstOrDefault(x => x.FileExtension == inputExtension);
            if (reader == null)
                return;

            IDocumentWriter writer = WriterCatalog.FirstOrDefault(x => x.FileExtension == outputExtension);
            writer?.Convert(reader.Convert(inputStream), outputStream);
        }

        public void Convert(FileExtension inputExtension, Stream inputStream, Dictionary<FileExtension, Stream> outputStreams)
        {
            IDocumentReader reader = ReaderCatalog.FirstOrDefault(x => x.FileExtension == inputExtension);
            if (reader == null)
                return;

            foreach ((IDocumentWriter writer, Stream stream) item in outputStreams.Select(e => (WriterCatalog.FirstOrDefault(x => x.FileExtension == e.Key), e.Value)))
                item.writer?.Convert(reader.Convert(inputStream), item.stream);
        }

        public FileExtension Convert(string inputExtension, Stream inputStream, string outputExtension, Stream outputStream)
        {
            IDocumentReader reader = ReaderCatalog.BestMatch(inputExtension);
            if (reader == null)
                return default(FileExtension);

            IDocumentWriter writer = WriterCatalog.BestMatch(outputExtension);
            if (writer == null)
                return default(FileExtension);

            writer.Convert(reader.Convert(inputStream), outputStream);
            return writer.FileExtension;
        }

        public IEnumerable<FileExtension> Convert(string inputExtension, Stream inputStream, Dictionary<string, Stream> outputStreams)
        {
            IDocumentReader reader = ReaderCatalog.BestMatch(inputExtension);
            if (reader == null)
                return Enumerable.Empty<FileExtension>();

            (IDocumentWriter writer, Stream stream)[] writers = outputStreams.Select(e => (WriterCatalog.BestMatch(e.Key), e.Value)).ToArray();

            IDocumentNode document = reader.Convert(inputStream);
            foreach ((IDocumentWriter writer, Stream stream) item in writers)
                item.writer?.Convert(document, item.stream);

            return writers.Select(x => x.writer?.FileExtension ?? FileExtension.Unknown);
        }

        public override IEnumerable Extend(CompositionContext context)
        {
            foreach (object extension in base.Extend(context))
                yield return extension;

            foreach (IDocumentReader documentReaderExtension in ReaderCatalog.Extend(context))
                yield return documentReaderExtension;
            foreach (IDocumentWriter documentWriterExtension in WriterCatalog.Extend(context))
                yield return documentWriterExtension;
        }

        IEnumerable<IDocumentReader> IExtensible<IDocumentReader>.Extend(CompositionContext context) => ReaderCatalog.Extend(context);
        IEnumerable<IDocumentWriter> IExtensible<IDocumentWriter>.Extend(CompositionContext context) => WriterCatalog.Extend(context);

        public override void ResetToVanilla()
        {
            base.ResetToVanilla();
            ReaderCatalog.ResetToVanilla();
            WriterCatalog.ResetToVanilla();
        }
    }
}