namespace Dast
{
    public interface IMediaOutput : IMediaOutput<string>
    {
    }

    public interface IMediaOutput<out TOutput> : IMediaFormat
    {
        TOutput Convert(string extension, string content, bool inline);
    }
}