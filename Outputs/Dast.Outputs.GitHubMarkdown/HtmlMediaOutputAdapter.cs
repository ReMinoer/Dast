using System.Collections.Generic;
using Dast.Media.Contracts.Html;
using Dast.Media.Contracts.Markdown;

namespace Dast.Outputs.GitHubMarkdown
{
    internal class HtmlMediaOutputAdapter : IMarkdownMediaOutput
    {
        private readonly IHtmlMediaOutput _htmlMediaOutput;

        public string DisplayName => _htmlMediaOutput.DisplayName;
        public IEnumerable<FileExtension> FileExtensions => _htmlMediaOutput.FileExtensions;
        public MediaType Type => _htmlMediaOutput.Type;

        public HtmlMediaOutputAdapter(IHtmlMediaOutput htmlMediaOutput)
        {
            _htmlMediaOutput = htmlMediaOutput;
        }

        public string Convert(string extension, string content, bool inline)
        {
            return _htmlMediaOutput.Convert(extension, content, inline);
        }
    }
}