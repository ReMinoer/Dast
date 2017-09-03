using System.Collections.Generic;
using Dast.Media.Contracts.Html;

namespace Dast.Media.Html.Core
{
    public class VideoConverter : HtmlMediaOutputBase
    {
        public override string DisplayName => "HTML videos";
        public override MediaType Type => MediaType.Visual;

        public override IEnumerable<FileExtension> FileExtensions
        {
            get
            {
                yield return Dast.FileExtensions.Video.Mp4;
            }
        }
        
        public override string Convert(string extension, string content, bool inline) => $"<figure><video src=\"{content}\" width=\"560\" height=\"315\" controls /></figure>";
    }
}