using System.Collections.Generic;
using Dast.Converters.Utils;

namespace Dast.Converters
{
    public interface IMediaConverter
    {
        string DisplayName { get; }
        IEnumerable<FileExtension> Extensions { get; }
        MediaType DefaultType { get; }
        string Convert(string extension, string content, bool inline);
    }
}