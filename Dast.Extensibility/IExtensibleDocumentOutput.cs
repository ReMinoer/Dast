using System.Collections.Generic;
using Dast.Outputs;

namespace Dast.Extensibility
{
    public interface IExtensibleDocumentOutput : IDocumentOutput, IExtensible
    {
        new IReadOnlyCollection<IMediaOutput> MediaOutputs { get; }
    }

    public interface IExtensibleDocumentOutput<out TOutput> : IExtensibleDocumentOutput, IDocumentOutput<TOutput>
    {
    }

    public interface IExtensibleDocumentOutput<TMedia, out TOutput> : IExtensibleDocumentOutput<TOutput>, IDocumentOutput<TMedia, TOutput>
        where TMedia : IMediaOutput
    {
        new ICollection<TMedia> MediaOutputs { get; }
    }
}