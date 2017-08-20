using System.Collections.Generic;
using Dast.Outputs.Html.Media.Base;

namespace Dast.Outputs.Html.Media
{
    public class YouTubeConverter : HtmlMediaConverterBase
    {
        public override string DisplayName => "YouTube videos";
        public override MediaType Type => MediaType.Visual;

        public override IEnumerable<FileExtension> Extensions
        {
            get
            {
                yield return FileExtensions.Video.YouTube;
            }
        }
        
        public override string Convert(string extension, string content, bool inline) => $"<figure><iframe width=\"560\" height=\"315\" src=\"https://www.youtube.com/embed/{content}\" frameborder=\"0\" allowfullscreen></iframe></figure>";
    }
}