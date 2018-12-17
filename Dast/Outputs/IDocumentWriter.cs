using System.IO;

namespace Dast.Outputs
{
    public interface IDocumentWriter : IDocumentOutput<string>
    {
        void Convert(IDocumentNode node, Stream stream);
    }

    public interface IDocumentWriter<out TMedia> : IDocumentWriter, IDocumentOutput<TMedia, string>
        where TMedia : IMediaOutput
    {
    }
}