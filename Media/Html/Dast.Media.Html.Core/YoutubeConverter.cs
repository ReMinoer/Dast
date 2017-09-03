using System.Collections.Generic;
using Dast.Media.Contracts.Html;

namespace Dast.Media.Html.Core
{
    public class YouTubeConverter : HtmlMediaOutputBase
    {
        public override string DisplayName => "YouTube videos";
        public override MediaType Type => MediaType.Visual;

        public override IEnumerable<FileExtension> FileExtensions
        {
            get
            {
                yield return Dast.FileExtensions.Video.YouTube;
            }
        }
        
        public override string Convert(string extension, string content, bool inline) => $"<figure><iframe width=\"560\" height=\"315\" src=\"https://www.youtube.com/embed/{content}\" frameborder=\"0\" allowfullscreen></iframe></figure>";
    }
}