using System.Collections.Generic;

namespace Dast.Media.Contracts.Html
{
    public abstract class HtmlMediaOutputBase : IHtmlMediaOutput
    {
        public abstract string DisplayName { get; }
        public abstract IEnumerable<FileExtension> FileExtensions { get; }
        public abstract MediaType Type { get; }
        public bool UseRecommandedCss { get; set; } = true;

        public virtual string Head => null;
        public virtual string EndOfPage => null;
        public virtual string MandatoryCss => null;
        public virtual string RecommandedCss => null;

        public abstract string Convert(string extension, string content, bool inline);
    }
}