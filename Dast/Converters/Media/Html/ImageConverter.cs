using System.Collections.Generic;
using System.IO;
using Dast.Converters.Utils;

namespace Dast.Converters.Media.Html
{
    public class ImageConverter : IHtmlMediaConverter
    {
        public string DisplayName => "HTML images";
        public MediaType DefaultType => MediaType.Visual;
        public string Head => null;
        public string EndOfPage => null;

        public IEnumerable<FileExtension> Extensions
        {
            get
            {
                yield return FileExtensions.Image.Png;
                yield return FileExtensions.Image.Jpeg;
                yield return FileExtensions.Image.Gif;
                yield return FileExtensions.Image.Bitmap;
                yield return FileExtensions.Image.Svg;
                yield return FileExtensions.Image.Ico;
            }
        }
        
        public string Convert(string extension, string content, bool inline) => $"<figure><img src=\"{content}\" alt=\"{Path.GetFileNameWithoutExtension(content)}\" /></figure>";
    }
}