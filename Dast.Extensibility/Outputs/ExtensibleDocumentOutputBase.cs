using System.Collections;
using System.Collections.Generic;
using System.Composition;
using Dast.Outputs;
using Dast.Outputs.Base;

namespace Dast.Extensibility.Outputs
{
    public abstract class ExtensibleDocumentOutputBase<TMedia, TOutput> : DocumentOutputBase<TMedia, TOutput>, IExtensible<TMedia>
        where TMedia : IMediaOutput
    {
        public ExtensibleFormatCatalog<TMedia> MediaCatalog { get; } = new ExtensibleFormatCatalog<TMedia>();
        protected override IEnumerable<TMedia> MediaOutputs => MediaCatalog;
        
        public IEnumerable<TMedia> Extend(CompositionContext context) => MediaCatalog.Extend(context);
        public void ResetToVanilla() => MediaCatalog.ResetToVanilla();
        IEnumerable IExtensible.Extend(CompositionContext context) => Extend(context);
        ICollection<TMedia> IExtensible<TMedia>.Extensions => MediaCatalog;
    }
}