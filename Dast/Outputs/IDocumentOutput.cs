using System.Collections.Generic;

namespace Dast.Outputs
{
    public interface IDocumentOutput : IDocumentFormat
    {
        IEnumerable<IMediaOutput> MediaOutputs { get; }
    }

    public interface IDocumentOutput<out TOutput> : IDocumentOutput
    {
        TOutput Convert(IDocumentNode node);
    }

    public interface IDocumentOutput<TMedia, out TOutput> : IDocumentOutput<TOutput>
        where TMedia : IMediaOutput
    {
        new ICollection<TMedia> MediaOutputs { get; }
    }
}