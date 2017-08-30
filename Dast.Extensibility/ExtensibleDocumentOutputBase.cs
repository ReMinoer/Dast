using System;
using System.Collections.Generic;
using System.Composition;
using Dast.Catalogs;
using Dast.Outputs;
using Dast.Outputs.Base;

namespace Dast.Extensibility
{
    public abstract class ExtensibleDocumentOutputBase<TMedia> : DocumentOutputBase<TMedia>, IExtensibleDocumentOutput<TMedia, string>
        where TMedia : IMediaOutput
    {
        private readonly ExtensibleFormatCatalog<TMedia> _mediaCatalog = new ExtensibleFormatCatalog<TMedia>();
        private readonly IReadOnlyCollection<TMedia> _readOnlyCollection;

        public override IEnumerable<TMedia> MediaOutputs => _readOnlyCollection;
        IReadOnlyCollection<IMediaOutput> IExtensibleDocumentOutput.MediaOutputs => (IReadOnlyCollection<IMediaOutput>)_readOnlyCollection;
        ICollection<TMedia> IExtensibleDocumentOutput<TMedia, string>.MediaOutputs => _mediaCatalog;

        protected ExtensibleDocumentOutputBase()
        {
            _readOnlyCollection = new ReadOnlyFormatCatalog<TMedia>(_mediaCatalog);
        }

        public IEnumerable<Type> ExtensionTypes => _mediaCatalog.ExtensionTypes;
        public void Extend(CompositionContext context) => _mediaCatalog.Extend(context);
        public void ResetToVanilla() => _mediaCatalog.ResetToVanilla();
    }
}