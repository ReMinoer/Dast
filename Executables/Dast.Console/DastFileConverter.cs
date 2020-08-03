using System.Collections;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Linq;
using Dast.Extensibility;
using Dast.Inputs;
using Dast.Outputs;

namespace Dast.Console
{
    public class DastFileConverter : IExtensible<IDocumentInput<string>>, IExtensible<IDocumentOutput<string>>, IExtensible<IDocumentReader>, IExtensible<IDocumentWriter>
    {
        private readonly DastTextConverter _textConverter = new DastTextConverter();

        public FileExtension Convert(string inputFilePath, string outputFolderPath, string outputExtension)
        {
            string inputFileName = Path.GetFileNameWithoutExtension(inputFilePath);
            string inputExtension = Path.GetExtension(inputFilePath).TrimStart('.');
            string outputFilepath = Path.Combine(outputFolderPath, Path.ChangeExtension(inputFileName, outputExtension));

            using (FileStream inputStream = File.OpenRead(inputFilePath))
            using (FileStream outputStream = File.Create(outputFilepath))
            {
                return _textConverter.Convert(inputExtension, inputStream, outputExtension, outputStream);
            }
        }

        public IEnumerable<FileExtension> Convert(string inputFilePath, string outputFolderPath, params string[] outputExtensions)
            => Convert(inputFilePath, outputFolderPath, outputExtensions.AsEnumerable());

        public IEnumerable<FileExtension> Convert(string inputFilePath, string outputFolderPath, IEnumerable<string> outputExtensions)
        {
            string inputFileName = Path.GetFileNameWithoutExtension(inputFilePath);
            string inputExtension = Path.GetExtension(inputFilePath).TrimStart('.');

            IEnumerable<FileExtension> outputFileExtensions;
            using (FileStream inputStream = File.OpenRead(inputFilePath))
            {
                Dictionary<string, Stream> outputStreams = outputExtensions
                    .ToDictionary<string, string, Stream>(x => x, x => File.Create(Path.Combine(outputFolderPath, Path.ChangeExtension(inputFileName, x))));

                outputFileExtensions = _textConverter.Convert(inputExtension, inputStream, outputStreams);

                foreach (Stream outputStream in outputStreams.Values)
                    outputStream.Dispose();
            }

            return outputFileExtensions;
        }

        static private FileStream SafeFileCreate(string path)
        {
            try
            {
                return File.Create(path);
            }
            catch (IOException)
            {
                return File.Create(path);
            }
        }

        #region IExtensible

        private IExtensible<IDocumentInput<string>> ExtensibleInputs => _textConverter;
        private IExtensible<IDocumentOutput<string>> ExtensibleOutputs => _textConverter;
        private IExtensible<IDocumentReader> ExtensibleReaders => _textConverter;
        private IExtensible<IDocumentWriter> ExtensibleWriters => _textConverter;
        ICollection<IDocumentInput<string>> IExtensible<IDocumentInput<string>>.Extensions => ExtensibleInputs.Extensions;
        ICollection<IDocumentOutput<string>> IExtensible<IDocumentOutput<string>>.Extensions => ExtensibleOutputs.Extensions;
        ICollection<IDocumentReader> IExtensible<IDocumentReader>.Extensions => ExtensibleReaders.Extensions;
        ICollection<IDocumentWriter> IExtensible<IDocumentWriter>.Extensions => ExtensibleWriters.Extensions;
        IEnumerable IExtensible.Extend(CompositionContext context) => _textConverter.Extend(context);
        IEnumerable<IDocumentInput<string>> IExtensible<IDocumentInput<string>>.Extend(CompositionContext context) => ExtensibleInputs.Extend(context);
        IEnumerable<IDocumentOutput<string>> IExtensible<IDocumentOutput<string>>.Extend(CompositionContext context) => ExtensibleOutputs.Extend(context);
        IEnumerable<IDocumentReader> IExtensible<IDocumentReader>.Extend(CompositionContext context) => ExtensibleReaders.Extend(context);
        IEnumerable<IDocumentWriter> IExtensible<IDocumentWriter>.Extend(CompositionContext context) => ExtensibleWriters.Extend(context);
        public void ResetToVanilla() => _textConverter.ResetToVanilla();

        #endregion
    }
}