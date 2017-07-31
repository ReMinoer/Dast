using System.Collections.Generic;
using Dast.Converters.Utils;

namespace Dast.Converters.Media.Html.Base
{
    public abstract class HtmlMediaConverterBase : IHtmlMediaConverter
    {
        public abstract string DisplayName { get; }
        public abstract IEnumerable<FileExtension> Extensions { get; }
        public abstract MediaType DefaultType { get; }
        public bool UseRecommandedCss { get; set; } = true;

        public virtual string Head => null;
        public virtual string EndOfPage => null;
        public virtual string MandatoryCss => null;
        public virtual string RecommandedCss => null;
        
        public abstract string Convert(string extension, string content, bool inline);
    }
}