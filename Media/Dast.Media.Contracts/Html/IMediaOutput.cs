namespace Dast.Media.Contracts.Html
{
    public interface IMediaOutput : Outputs.IMediaOutput
    {
        string Head { get; }
        string EndOfPage { get; }
        string MandatoryCss { get; }
        string RecommandedCss { get; }
        bool UseRecommandedCss { get; set; }
    }
}