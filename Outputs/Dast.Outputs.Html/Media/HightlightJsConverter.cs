using System;
using System.Collections.Generic;
using System.Linq;
using Dast.Outputs.Html.Media.Base;

namespace Dast.Outputs.Html.Media
{
    public class HightlightJsConverter : HtmlMediaConverterBase
    {
        private const string InlineCodeClass = "dast-hljs-inline";

        public override string DisplayName => "highlight.js";
        public override MediaType Type => MediaType.Code;
        public override string MandatoryCss => $".{InlineCodeClass}" + "{display:inline;}";

        public override string Head => "<link rel=\"stylesheet\" href=\"http://cdnjs.cloudflare.com/ajax/libs/highlight.js/9.12.0/styles/default.min.css\">" + Environment.NewLine
                              + "<script src=\"http://cdnjs.cloudflare.com/ajax/libs/highlight.js/9.12.0/highlight.min.js\"></script>" + Environment.NewLine
                              + "<script>hljs.initHighlightingOnLoad();</script>";

        public override IEnumerable<FileExtension> Extensions
        {
            get
            {
                yield return FileExtensions.Text.Markdown;
                yield return FileExtensions.Programming.Csharp;
            }
        }

        public override string Convert(string extension, string content, bool inline)
        {
            string keyword = GetExtentionKeyword(extension);
            string languageClass = string.IsNullOrWhiteSpace(keyword) ? "hljs no-highlight" : $"language-{keyword}";
            string languageName = GetExtentionName(extension);
            string caption = string.IsNullOrWhiteSpace(languageName) ? extension : languageName;

            return inline
                ? $"<code class=\"{InlineCodeClass} {languageClass}\">" + content + "</code>"
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

            if (extension.Equals("dh", StringComparison.OrdinalIgnoreCase) || extension.Equals("dash", StringComparison.OrdinalIgnoreCase))
                return FileExtensions.Text.Dash.Name;

            return Extensions.FirstOrDefault(x => x.Match(extension)).Name;
        }
    }
}