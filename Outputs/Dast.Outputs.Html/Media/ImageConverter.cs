using System.Collections.Generic;
using System.IO;
using Dast.Outputs.Html.Media.Base;

namespace Dast.Outputs.Html.Media
{
    public class ImageConverter : HtmlMediaConverterBase
    {
        public override string DisplayName => "HTML images";
        public override MediaType Type => MediaType.Visual;

        public override IEnumerable<FileExtension> Extensions
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
        
        public override string Convert(string extension, string content, bool inline) => $"<figure><img src=\"{content}\" alt=\"{Path.GetFileNameWithoutExtension(content)}\" /></figure>";
    }
}