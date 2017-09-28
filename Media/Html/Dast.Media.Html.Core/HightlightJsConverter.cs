using System;
using System.Collections.Generic;
using System.Linq;
using Dast.Media.Contracts.Html;

namespace Dast.Media.Html.Core
{
    public class HightlightJsConverter : HtmlMediaOutputBase
    {
        private const string InlineCodeClass = "dast-hljs-inline";

        public override string DisplayName => "highlight.js";
        public override MediaType Type => MediaType.Code;
        public override string MandatoryCss => $".{InlineCodeClass}" + "{display:inline !important;}";

        public override string Head => "<link rel=\"stylesheet\" href=\"http://cdnjs.cloudflare.com/ajax/libs/highlight.js/9.12.0/styles/default.min.css\">" + Environment.NewLine
                              + "<script src=\"http://cdnjs.cloudflare.com/ajax/libs/highlight.js/9.12.0/highlight.min.js\"></script>" + Environment.NewLine
                              + "<script>hljs.initHighlightingOnLoad();</script>";

        public override IEnumerable<FileExtension> FileExtensions
        {
            get
            {
                yield return FileExtension.Unknown;
                yield return Dast.FileExtensions.Text.Markdown;
                yield return Dast.FileExtensions.Text.Dash;
                yield return Dast.FileExtensions.Programming.Csharp;
            }
        }

        public override string Convert(string extension, string content, bool inline)
        {
            string keyword = GetExtentionKeyword(extension);
            string languageClass = string.IsNullOrWhiteSpace(keyword) ? "hljs no-highlight" : $"language-{keyword}";
            string languageName = GetExtentionName(extension);
            string caption = string.IsNullOrWhiteSpace(languageName) ? extension : languageName;

            if (inline)
                return $"<code class=\"{languageClass} {InlineCodeClass}\">" + content + "</code>";

            string result = "";
            if (!string.IsNullOrWhiteSpace(caption))
                result += $"<figure>{Environment.NewLine}<figcaption>{caption}</figcaption>{Environment.NewLine}";

            result += $"<pre><code class=\"{languageClass}\">{content}</code></pre>";
            
            if (!string.IsNullOrWhiteSpace(caption))
                result += $"{Environment.NewLine}</figure>";

            return result;
        }

        public string GetExtentionKeyword(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
                return null;

            if (Dast.FileExtensions.Text.Dash.Match(extension))
                return null;

            return FileExtensions.FirstOrDefault(x => x.Match(extension)).Main;
        }

        public string GetExtentionName(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
                return null;

            return FileExtensions.FirstOrDefault(x => x.Match(extension)).Name;
        }
    }
}