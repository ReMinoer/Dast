using System.Collections.Generic;
using System.Threading.Tasks;

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

        public virtual async Task GetResourceFilesAsync(string outputDirectory) {}
        public abstract string Convert(string extension, string content, bool inline);

        public virtual string Convert(string extension, string content, bool inline, out IHtmlMediaOutput[] usedMediaOutputs)
        {
            usedMediaOutputs = null;
            return Convert(extension, content, inline);
        }
    }
}