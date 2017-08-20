using System.Collections.Generic;

namespace Dast.Outputs.Html.Media.Base
{
    public abstract class HtmlMediaConverterBase : HtmlOutput.IMediaOutput
    {
        public abstract string DisplayName { get; }
        public abstract IEnumerable<FileExtension> Extensions { get; }
        public abstract MediaType Type { get; }
        public bool UseRecommandedCss { get; set; } = true;

        public virtual string Head => null;
        public virtual string EndOfPage => null;
        public virtual string MandatoryCss => null;
        public virtual string RecommandedCss => null;
        
        public abstract string Convert(string extension, string content, bool inline);
    }
}