using System;
using System.Collections.Generic;
using System.IO;

namespace Dast.Outputs
{
    public interface IDocumentMultiWriter<TStreamKey> : IDocumentOutput<IDictionary<TStreamKey, string>>
        where TStreamKey : struct
    {
        void Convert(IDocumentNode node, IDictionary<TStreamKey, Func<Stream>> streamProviders);
        IDictionary<TStreamKey, string> Convert(IDocumentNode node, IEnumerable<TStreamKey> streamKeys);
        IDictionary<TStreamKey, string> Convert(IDocumentNode node, params TStreamKey[] streamKeys);
    }

    public interface IDocumentMultiWriter<out TMedia, TStreamKey> : IDocumentMultiWriter<TStreamKey>, IDocumentOutput<TMedia, IDictionary<TStreamKey, string>>
        where TMedia : IMediaOutput
        where TStreamKey : struct
    {
    }
}