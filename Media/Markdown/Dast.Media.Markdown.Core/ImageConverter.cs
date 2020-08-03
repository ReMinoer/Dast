using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Dast.Media.Contracts.Markdown;

namespace Dast.Media.Markdown.Core
{
    public class ImageConverter : IMarkdownMediaOutput
    {
        public string DisplayName => "Markdown images";
        public MediaType Type => MediaType.Visual;

        public IEnumerable<FileExtension> FileExtensions
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

        public string Convert(string extension, string content, bool inline) => $"![{Path.GetFileNameWithoutExtension(content)}]({content})";
        public async Task GetResourceFilesAsync(string outputDirectory) {}
    }
}