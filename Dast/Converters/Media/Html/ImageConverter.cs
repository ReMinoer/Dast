using System.Collections.Generic;
using System.IO;
using Dast.Converters.Media.Html.Base;
using Dast.Converters.Utils;

namespace Dast.Converters.Media.Html
{
    public class ImageConverter : HtmlMediaConverterBase
    {
        public override string DisplayName => "HTML images";
        public override MediaType DefaultType => MediaType.Visual;

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
        
        public override string Convert(string extension, string content, bool inline, bool useRecommandedCss) => $"<figure><img src=\"{content}\" alt=\"{Path.GetFileNameWithoutExtension(content)}\" /></figure>";
    }
}