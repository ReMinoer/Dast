using System.Collections;
using System.Collections.Generic;
using System.Composition;
using Dast.Outputs;
using Dast.Outputs.Base;

namespace Dast.Extensibility.Outputs
{
    public abstract class ExtensibleDocumentMultiWriterBase<TMedia, TStreamKey> : DocumentMultiWriterBase<TMedia, TStreamKey>, IExtensible<TMedia>
        where TMedia : IMediaOutput
        where TStreamKey : struct
    {
        public ExtensibleFormatCatalog<TMedia> MediaCatalog { get; } = new ExtensibleFormatCatalog<TMedia>();
        protected override IEnumerable<TMedia> MediaOutputs => MediaCatalog;

        public IEnumerable<TMedia> Extend(CompositionContext context) => MediaCatalog.Extend(context);
        public void ResetToVanilla() => MediaCatalog.ResetToVanilla();
        IEnumerable IExtensible.Extend(CompositionContext context) => Extend(context);
        ICollection<TMedia> IExtensible<TMedia>.Extensions => MediaCatalog;
    }
}