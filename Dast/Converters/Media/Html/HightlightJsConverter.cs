using System;
using System.Collections.Generic;
using System.Linq;
using Dast.Converters.Utils;

namespace Dast.Converters.Media.Html
{
    public class HightlightJsConverter : IHtmlMediaConverter
    {
        public string DisplayName => "highlight.js";
        public MediaType DefaultType => MediaType.Code;
        public string Head => "<link rel=\"stylesheet\" href=\"http://cdnjs.cloudflare.com/ajax/libs/highlight.js/9.12.0/styles/default.min.css\">" + Environment.NewLine
                              + "<script src=\"http://cdnjs.cloudflare.com/ajax/libs/highlight.js/9.12.0/highlight.min.js\"></script>" + Environment.NewLine
                              + "<script>hljs.initHighlightingOnLoad();</script>" + Environment.NewLine
                              + "<style media=\"screen\" type=\"text/css\">.hljs-inline{display: inline;}</style>";
        public string EndOfPage => null;

        public IEnumerable<FileExtension> Extensions
        {
            get
            {
                yield return FileExtensions.Text.Markdown;
                yield return FileExtensions.Programming.Csharp;
            }
        }

        public string Convert(string extension, string content, bool inline)
        {
            string keyword = GetExtentionKeyword(extension);
            string languageClass = string.IsNullOrWhiteSpace(keyword) ? "hljs no-highlight" : $"language-{keyword}";
            string languageName = GetExtentionName(extension);
            string caption = string.IsNullOrWhiteSpace(languageName) ? extension : languageName;

            return inline
                ? $"<code class=\"hljs-inline {languageClass}\">" + content + "</code>"
                : $"<figure>{Environment.NewLine}<figcaption>{caption}</figcaption>{Environment.NewLine}<pre><code class=\"{languageClass}\">{content}</code></pre>{Environment.NewLine}</figure>";
        }

        public string GetExtentionKeyword(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
                return null;

            return Extensions.FirstOrDefault(x => x.Match(extension)).Main;
        }

        public string GetExtentionName(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
                return null;

            if (extension.Equals("dh", StringComparison.InvariantCultureIgnoreCase) || extension.Equals("dash", StringComparison.InvariantCultureIgnoreCase))
                return FileExtensions.Text.Dash.Name;

            return Extensions.FirstOrDefault(x => x.Match(extension)).Name;
        }
    }
}