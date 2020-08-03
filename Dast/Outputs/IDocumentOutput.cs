using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dast.Outputs
{
    public interface IDocumentOutput : IDocumentFormat
    {
        IEnumerable<IMediaOutput> MediaOutputs { get; }
        Task GetResourceFilesAsync(string outputDirectory);
    }

    public interface IDocumentOutput<out TOutput> : IDocumentOutput
    {
        TOutput Convert(IDocumentNode node);
    }

    public interface IDocumentOutput<out TMedia, out TOutput> : IDocumentOutput<TOutput>
        where TMedia : IMediaOutput
    {
        new IEnumerable<TMedia> MediaOutputs { get; }
    }
}