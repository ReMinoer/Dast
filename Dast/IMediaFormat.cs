using System.Collections.Generic;

namespace Dast
{
    public interface IMediaFormat : IFormat
    {
        MediaType Type { get; }
    }
}