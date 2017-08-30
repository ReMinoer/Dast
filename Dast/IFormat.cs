using System.Collections.Generic;

namespace Dast
{
    public interface IFormat
    {
        string DisplayName { get; }
        IEnumerable<FileExtension> FileExtensions { get; }
    }
}