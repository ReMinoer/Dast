using System.Collections.Generic;

namespace Dast.Inputs
{
    public interface IDocumentInput : IDocumentFormat
    {
        IEnumerable<IMediaInput> MediaInputs { get; }
    }

    public interface IDocumentInput<in TInput> : IDocumentInput
    {
        IDocumentNode Convert(TInput input);
    }

    public interface IDocumentInput<TMedia, in TInput> : IDocumentInput<TInput>
        where TMedia : IMediaInput
    {
        new ICollection<TMedia> MediaInputs { get; }
    }
}