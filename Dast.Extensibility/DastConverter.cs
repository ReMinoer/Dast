using System.Collections;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Dast.Inputs;
using Dast.Outputs;

namespace Dast.Extensibility
{
    public class DastConverter<TInput, TOutput> : IExtensible<IDocumentInput<TInput>>, IExtensible<IDocumentOutput<TOutput>>
    {
        public ExtensibleFormatCatalog<IDocumentInput<TInput>> InputCatalog { get; } = new ExtensibleFormatCatalog<IDocumentInput<TInput>>();
        public ExtensibleFormatCatalog<IDocumentOutput<TOutput>> OutputCatalog { get; } = new ExtensibleFormatCatalog<IDocumentOutput<TOutput>>();
        ICollection<IDocumentInput<TInput>> IExtensible<IDocumentInput<TInput>>.Extensions => InputCatalog;
        ICollection<IDocumentOutput<TOutput>> IExtensible<IDocumentOutput<TOutput>>.Extensions => OutputCatalog;
        
        public TOutput Convert(FileExtension inputExtension, TInput content, FileExtension outputExtension)
        {
            IDocumentInput<TInput> input = InputCatalog.FirstOrDefault(x => x.FileExtension == inputExtension);
            if (input == null)
                return default(TOutput);

            IDocumentOutput<TOutput> output = OutputCatalog.FirstOrDefault(x => x.FileExtension == outputExtension);
            return output != null ? output.Convert(input.Convert(content)) : default(TOutput);
        }

        public IEnumerable<TOutput> Convert(FileExtension inputExtension, TInput content, IEnumerable<FileExtension> outputExtensions)
        {
            IDocumentInput<TInput> input = InputCatalog.FirstOrDefault(x => x.FileExtension == inputExtension);
            if (input == null)
                return Enumerable.Empty<TOutput>();

            IDocumentNode document = input.Convert(content);
            return outputExtensions.Select(e => OutputCatalog.FirstOrDefault(x => x.FileExtension == e))
                                   .Select(o => o != null ? o.Convert(document) : default(TOutput));
        }

        public (FileExtension extension, TOutput result) Convert(string inputExtension, TInput content, string outputExtension)
        {
            IDocumentInput<TInput> input = InputCatalog.BestMatch(inputExtension);
            if (input == null)
                return default((FileExtension, TOutput));

            IDocumentOutput<TOutput> output = OutputCatalog.BestMatch(outputExtension);
            return output != null ? (output.FileExtension, output.Convert(input.Convert(content))) : default((FileExtension, TOutput));
        }

        public IEnumerable<(FileExtension extension, TOutput result)> Convert(string inputExtension, TInput content, IEnumerable<string> outputExtensions)
        {
            IDocumentInput<TInput> input = InputCatalog.BestMatch(inputExtension);
            if (input == null)
                return Enumerable.Empty<(FileExtension, TOutput)>();

            IDocumentNode document = input.Convert(content);
            return outputExtensions.Select(e => OutputCatalog.BestMatch(e))
                .Select(o => (o?.FileExtension ?? FileExtension.Unknown, o != null ? o.Convert(document) : default(TOutput)));
        }

        public IEnumerable<TOutput> Convert(FileExtension inputExtension, TInput content, params FileExtension[] outputExtensions)
            => Convert(inputExtension, content, outputExtensions.AsEnumerable());
        public IEnumerable<(FileExtension extension, TOutput result)> Convert(string inputExtension, TInput content, params string[] outputExtensions)
            => Convert(inputExtension, content, outputExtensions.AsEnumerable());

        public virtual IEnumerable Extend(CompositionContext context)
        {
            foreach (IDocumentInput<TInput> documentInputExtension in InputCatalog.Extend(context))
                yield return documentInputExtension;
            foreach (IDocumentOutput<TOutput> documentOutputExtension in OutputCatalog.Extend(context))
                yield return documentOutputExtension;
        }

        IEnumerable<IDocumentInput<TInput>> IExtensible<IDocumentInput<TInput>>.Extend(CompositionContext context) => InputCatalog.Extend(context);
        IEnumerable<IDocumentOutput<TOutput>> IExtensible<IDocumentOutput<TOutput>>.Extend(CompositionContext context) => OutputCatalog.Extend(context);

        public virtual void ResetToVanilla()
        {
            InputCatalog.Clear();
            OutputCatalog.Clear();
        }
    }
}