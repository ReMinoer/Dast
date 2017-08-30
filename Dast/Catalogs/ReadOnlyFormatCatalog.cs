using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Dast.Catalogs
{
    public class ReadOnlyFormatCatalog<TMedia> : IReadOnlyCollection<TMedia>
        where TMedia : IFormat
    {
        private readonly FormatCatalog<TMedia> _formatCatalog;
        public int Count => _formatCatalog.Count;

        public ReadOnlyFormatCatalog(FormatCatalog<TMedia> formatCatalog)
        {
            _formatCatalog = formatCatalog;
        }

        public IEnumerator<TMedia> GetEnumerator() => (_formatCatalog ?? Enumerable.Empty<TMedia>()).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}