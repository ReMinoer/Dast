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
                              + "<script>hljs.initHighlightingOnLoad();</script>";
        public string EndOfPage => null;

        public IEnumerable<FileExtension> Extensions
        {
            get
            {
                yield return FileExtensions.Text.Txt;
                yield return FileExtensions.Text.Markdown;
                yield return FileExtensions.Programming.Csharp;
            }
        }

        public string Convert(string extension, string content, bool inline)
        {
            string keyword = GetExtentionKeyword(extension);
            string codeClass = string.IsNullOrEmpty(keyword) ? "class=\"nohighlight\"" : $"class=\"{keyword}\"";
            return inline ? $"<code {codeClass}>" + content + "</code>" : $"<pre><code {codeClass}>" + content + "</code></pre>";
        }

        public string GetExtentionKeyword(string extension)
        {
            return Extensions.FirstOrDefault(x => x.Match(extension)).Main;
        }
    }
}