namespace Dast.Converters.Media.Html
{
    public interface IHtmlMediaConverter : IMediaConverter
    {
        string Head { get; }
        string EndOfPage { get; }
    }
}