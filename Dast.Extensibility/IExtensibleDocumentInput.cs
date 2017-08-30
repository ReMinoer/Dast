using System.Collections.Generic;
using Dast.Inputs;

namespace Dast.Extensibility
{
    public interface IExtensibleDocumentInput : IDocumentInput, IExtensible
    {
        new IReadOnlyCollection<IMediaInput> MediaInputs { get; }
    }

    public interface IExtensibleDocumentInput<in TInput> : IExtensibleDocumentInput, IDocumentInput<TInput>
    {
    }

    public interface IExtensibleDocumentInput<TMedia, in TInput> : IExtensibleDocumentInput<TInput>, IDocumentInput<TMedia, TInput>
        where TMedia : IMediaInput
    {
        new ICollection<TMedia> MediaInputs { get; }
    }
}