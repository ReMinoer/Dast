using System.Collections.Generic;
using Dast.Converters.Utils;

namespace Dast.Converters.Media.Html
{
    public class YouTubeConverter : IHtmlMediaConverter
    {
        public string DisplayName => "YouTube videos";
        public MediaType DefaultType => MediaType.Visual;
        public string Head => null;
        public string EndOfPage => null;

        public IEnumerable<FileExtension> Extensions
        {
            get
            {
                yield return FileExtensions.Video.YouTube;
            }
        }
        
        public string Convert(string extension, string content, bool inline) => $"<figure><iframe width=\"560\" height=\"315\" src=\"https://www.youtube.com/embed/{content}\" frameborder=\"0\" allowfullscreen></iframe></figure>";
    }
}