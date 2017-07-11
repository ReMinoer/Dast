using System.Collections.Generic;
using System.IO;
using Dast.Converters.Utils;

namespace Dast.Converters.Media.Markdown
{
    public class ImageConverter : IMediaConverter
    {
        public string DisplayName => "Markdown images";
        public MediaType DefaultType => MediaType.Visual;

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

        public string Convert(string extension, string content, bool inline) => $"![{Path.GetFileNameWithoutExtension(content)}]({content})";
    }
}