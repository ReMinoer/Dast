using System.Collections.Generic;

namespace Dast
{
    public interface IDocumentInput : IDocumentFormat
    {
        IEnumerable<IMediaInput> MediaOutputs { get; }
    }

    public interface IDocumentInput<in TInput> : IDocumentInput
    {
        IDocumentNode Convert(TInput node);
    }

    public interface IDocumentInput<TMedia, in TInput> : IDocumentInput<TInput>
        where TMedia : IMediaInput
    {
        new ICollection<TMedia> MediaOutputs { get; }
    }
}