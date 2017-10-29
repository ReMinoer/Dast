using System.Collections;
using System.Collections.Generic;
using System.Composition;
using Dast.Outputs;
using Dast.Outputs.Base;

namespace Dast.Extensibility.Outputs
{
    public abstract class ExtensibleFragmentedDocumentMergerBase<TMultiWriter, TMedia, TFragment> : FragmentedDocumentMergerBase<TMultiWriter, TMedia, TFragment>, IExtensible<TMedia>
        where TMultiWriter : IFragmentedDocumentWriter<TMedia, TFragment>, IExtensible<TMedia>, new()
        where TMedia : IMediaOutput
        where TFragment : struct
    {
        public ICollection<TMedia> Extensions => ((IExtensible<TMedia>)DocumentMultiWriter).Extensions;
        IEnumerable IExtensible.Extend(CompositionContext context) => ((IExtensible)DocumentMultiWriter).Extend(context);
        public IEnumerable<TMedia> Extend(CompositionContext context) => DocumentMultiWriter.Extend(context);
        public void ResetToVanilla() => DocumentMultiWriter.ResetToVanilla();
    }
}