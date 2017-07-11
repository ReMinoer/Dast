using Dast.Converters.Utils;

namespace Dast
{
    public interface IDocumentConverter
    {
        FileExtension FileExtension { get; }
        string Convert(IDocumentNode node);
    }
}