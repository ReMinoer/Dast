using System.Collections.Generic;
using Dast.Converters.Media.Html.Base;
using Dast.Converters.Utils;

namespace Dast.Converters.Media.Html
{
    public class VideoConverter : HtmlMediaConverterBase
    {
        public override string DisplayName => "HTML videos";
        public override MediaType DefaultType => MediaType.Visual;

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