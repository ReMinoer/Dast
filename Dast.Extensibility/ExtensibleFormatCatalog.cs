using System;
using System.Collections.Generic;
using System.Composition;
using Dast.Catalogs;

namespace Dast.Extensibility
{
    public class ExtensibleFormatCatalog<TFormat> : FormatCatalog<TFormat>, IExtensible
        where TFormat : IFormat
    {
        public IEnumerable<Type> ExtensionTypes { get { yield return typeof(TFormat); } }

        public void Extend(CompositionContext context)
        {
            AddRange(context.GetExports<TFormat>());
        }

        public void ResetToVanilla()
        {
            Clear();
        }
    }
}