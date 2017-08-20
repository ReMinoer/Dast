using System.Collections.Generic;

namespace Dast
{
    public interface IMediaFormat : IFormat
    {
        IEnumerable<FileExtension> Extensions { get; }
        MediaType Type { get; }
    }
}