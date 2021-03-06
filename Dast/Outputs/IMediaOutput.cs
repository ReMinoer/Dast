using System.Threading.Tasks;

namespace Dast.Outputs
{
    public interface IMediaOutput : IMediaOutput<string>
    {
    }

    public interface IMediaOutput<out TOutput> : IMediaFormat
    {
        TOutput Convert(string extension, string content, bool inline);
        Task GetResourceFilesAsync(string outputDirectory);
    }
}