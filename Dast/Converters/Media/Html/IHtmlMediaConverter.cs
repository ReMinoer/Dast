namespace Dast.Converters.Media.Html
{
    public interface IHtmlMediaConverter : IMediaConverter
    {
        string Head { get; }
        string EndOfPage { get; }
        string MandatoryCss { get; }
        string RecommandedCss { get; }
        string Convert(string extension, string content, bool inline, bool useRecommandedCss);
    }
}