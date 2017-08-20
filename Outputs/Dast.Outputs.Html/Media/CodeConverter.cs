using System;
using System.Collections.Generic;
using System.Linq;
using Dast.Outputs.Html.Media.Base;

namespace Dast.Outputs.Html.Media
{
    public class CodeConverter : HtmlMediaConverterBase
    {
        public override string DisplayName => "HTML code";
        public override MediaType Type => MediaType.Code;

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
            string languageName = GetExtentionName(extension);
            string caption = string.IsNullOrWhiteSpace(languageName) ? extension : languageName;

            return inline
                ? $"<code>{ content }</code>"
                : $"<figure>{Environment.NewLine}<figcaption>{caption}</figcaption>{Environment.NewLine}<pre><code>{content}</code></pre>{Environment.NewLine}</figure>";
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