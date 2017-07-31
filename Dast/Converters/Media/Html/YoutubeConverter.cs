using System.Collections.Generic;
using Dast.Converters.Media.Html.Base;
using Dast.Converters.Utils;

namespace Dast.Converters.Media.Html
{
    public class YouTubeConverter : HtmlMediaConverterBase
    {
        public override string DisplayName => "YouTube videos";
        public override MediaType DefaultType => MediaType.Visual;

        public override IEnumerable<FileExtension> Extensions
        {
            get
            {
                yield return FileExtensions.Video.YouTube;
            }
        }
        
        public override string Convert(string extension, string content, bool inline, bool useRecommandedCss) => $"<figure><iframe width=\"560\" height=\"315\" src=\"https://www.youtube.com/embed/{content}\" frameborder=\"0\" allowfullscreen></iframe></figure>";
    }
}