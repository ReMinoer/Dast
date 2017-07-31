namespace Dast.Converters.Media.Html
{
    public interface IHtmlMediaConverter : IMediaConverter
    {
        string Head { get; }
        string EndOfPage { get; }
        string MandatoryCss { get; }
        string RecommandedCss { get; }
        bool UseRecommandedCss { get; set; }
    }
}