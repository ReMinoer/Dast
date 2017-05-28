using System.Collections.Generic;

namespace Dast
{
    public interface IDocumentConverter
    {
        IEnumerable<string> FileExtensions { get; }
        string Convert(IDocumentNode node);
    }
}