using System.Collections;
using System.Collections.Generic;
using System.Composition;
using Dast.Outputs;
using Dast.Outputs.Base;

namespace Dast.Extensibility.Outputs
{
    public abstract class ExtensibleFragmentedDocumentWriterBase<TMedia, TFragment> : FragmentedDocumentWriterBase<TMedia, TFragment>, IExtensible<TMedia>
        where TMedia : IMediaOutput
        where TFragment : struct
    {
        public ExtensibleFormatCatalog<TMedia> MediaCatalog { get; } = new ExtensibleFormatCatalog<TMedia>();
        protected override IEnumerable<TMedia> MediaOutputs => MediaCatalog;

        public virtual IEnumerable<TMedia> Extend(CompositionContext context) => MediaCatalog.Extend(context);
        public virtual void ResetToVanilla() => MediaCatalog.ResetToVanilla();
        IEnumerable IExtensible.Extend(CompositionContext context) => Extend(context);
        ICollection<TMedia> IExtensible<TMedia>.Extensions => MediaCatalog;
    }
}