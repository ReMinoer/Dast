using System.Collections;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Dast.Catalogs;

namespace Dast.Extensibility
{
    public class ExtensibleFormatCatalog<TFormat> : FormatCatalog<TFormat>, IExtensible<TFormat>
        where TFormat : IFormat
    {
        ICollection<TFormat> IExtensible<TFormat>.Extensions => this;

        public IEnumerable<TFormat> Extend(CompositionContext context)
        {
            TFormat[] extensions = context.GetExports<TFormat>().ToArray();
            AddRange(extensions);
            return extensions;
        }

        IEnumerable IExtensible.Extend(CompositionContext context) => Extend(context);
        public void ResetToVanilla() => Clear();
    }
}