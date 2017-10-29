using System;
using System.Collections.Generic;
using System.IO;

namespace Dast.Outputs
{
    public interface IFragmentedDocumentWriter<TFragment> : IDocumentOutput<IDictionary<TFragment, string>>
        where TFragment : struct
    {
        void Convert(IDocumentNode node, IDictionary<TFragment, Func<Stream>> streamProviders);
        IDictionary<TFragment, string> Convert(IDocumentNode node, IEnumerable<TFragment> streamKeys);
        IDictionary<TFragment, string> Convert(IDocumentNode node, params TFragment[] streamKeys);
    }

    public interface IFragmentedDocumentWriter<out TMedia, TFragment> : IFragmentedDocumentWriter<TFragment>, IDocumentOutput<TMedia, IDictionary<TFragment, string>>
        where TMedia : IMediaOutput
        where TFragment : struct
    {
    }
}