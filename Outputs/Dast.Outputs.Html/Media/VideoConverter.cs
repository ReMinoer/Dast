using System.Collections.Generic;
using Dast.Outputs.Html.Media.Base;

namespace Dast.Outputs.Html.Media
{
    public class VideoConverter : HtmlMediaConverterBase
    {
        public override string DisplayName => "HTML videos";
        public override MediaType Type => MediaType.Visual;

        public override IEnumerable<FileExtension> Extensions
        {
            get
            {
                yield return FileExtensions.Video.Mp4;
            }
        }
        
        public override string Convert(string extension, string content, bool inline) => $"<figure><video src=\"{content}\" width=\"560\" height=\"315\" controls /></figure>";
    }
}