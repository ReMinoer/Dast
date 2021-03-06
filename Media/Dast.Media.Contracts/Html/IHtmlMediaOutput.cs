﻿namespace Dast.Media.Contracts.Html
{
    public interface IHtmlMediaOutput : Outputs.IMediaOutput
    {
        string Head { get; }
        string EndOfPage { get; }
        string MandatoryCss { get; }
        string RecommandedCss { get; }
        bool UseRecommandedCss { get; set; }
        string Convert(string extension, string content, bool inline, out IHtmlMediaOutput[] usedMediaOutputs);
    }
}