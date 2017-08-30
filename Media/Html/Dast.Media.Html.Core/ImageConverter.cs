using System.Collections.Generic;
using System.IO;

namespace Dast.Media.Html.Core
{
    public class ImageConverter : Contracts.Html.MediaOutputBase
    {
        public override string DisplayName => "HTML images";
        public override MediaType Type => MediaType.Visual;

        public override IEnumerable<FileExtension> FileExtensions
        {
            get
            {
                yield return Dast.FileExtensions.Image.Png;
                yield return Dast.FileExtensions.Image.Jpeg;
                yield return Dast.FileExtensions.Image.Gif;
                yield return Dast.FileExtensions.Image.Bitmap;
                yield return Dast.FileExtensions.Image.Svg;
                yield return Dast.FileExtensions.Image.Ico;
            }
        }
        
        public override string Convert(string extension, string content, bool inline) => $"<figure><img src=\"{content}\" alt=\"{Path.GetFileNameWithoutExtension(content)}\" /></figure>";
    }
}