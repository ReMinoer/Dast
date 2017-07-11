using System.Collections.Generic;
using Dast.Converters.Utils;

namespace Dast.Converters.Media.Html
{
    public class VideoConverter : IHtmlMediaConverter
    {
        public string DisplayName => "HTML videos";
        public MediaType DefaultType => MediaType.Visual;
        public string Head => null;
        public string EndOfPage => null;

        public IEnumerable<FileExtension> Extensions
        {
            get
            {
                yield return FileExtensions.Video.Mp4;
            }
        }
        
        public string Convert(string extension, string content, bool inline) => $"<div><video src=\"{content}\" width=\"560\" height=\"315\" controls /></div>";
    }
}