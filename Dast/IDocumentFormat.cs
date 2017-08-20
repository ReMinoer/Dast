namespace Dast
{
    public interface IDocumentFormat : IFormat
    {
        FileExtension FileExtension { get; }
    }
}