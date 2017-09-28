using System.IO;

namespace Dast.Inputs
{
    public interface IDocumentReader : IDocumentInput<string>
    {
        IDocumentNode Convert(Stream stream);
    }

    public interface IDocumentReader<out TMedia> : IDocumentReader, IDocumentInput<TMedia, string>
        where TMedia : IMediaInput
    {
    }
}