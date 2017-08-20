namespace Dast
{
    public interface IMediaInput : IMediaInput<string>
    {
    }

    public interface IMediaInput<in TInput> : IMediaFormat
    {
        string Convert(string extension, TInput content, bool inline);
    }
}