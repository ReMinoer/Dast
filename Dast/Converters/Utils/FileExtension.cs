using System.Linq;

namespace Dast.Converters.Utils
{
    public struct FileExtension
    {
        public readonly string Main;
        public readonly string[] Others;

        public FileExtension(string main, params string[] others)
        {
            Main = main;
            Others = others;
        }

        public bool Match(string extension)
        {
            return Main == extension || Others.Contains(extension);
        }
    }

    public class FileExtensions
    {
        public class Text
        {
            static public FileExtension Txt = new FileExtension("txt");
            static public FileExtension Markdown = new FileExtension("md", "markdown");
            static public FileExtension Dash = new FileExtension("dh", "dash");
        }
        
        public class Image
        {
            static public FileExtension Png = new FileExtension("png");
            static public FileExtension Jpeg = new FileExtension("jpg", "jpeg");
            static public FileExtension Gif = new FileExtension("gif");
            static public FileExtension Bitmap = new FileExtension("bmp", "dib", "rle");
            static public FileExtension Svg = new FileExtension("svg", "svgz");
            static public FileExtension Ico = new FileExtension("ico");
        }

        public class Video
        {
            static public FileExtension Mp4 = new FileExtension("mp4");
            static public FileExtension YouTube = new FileExtension("youtube.com");
        }

        public class Programming
        {
            static public FileExtension Html = new FileExtension("html", "htm", "xhtml", "xht");
            static public FileExtension Csharp = new FileExtension("cs");
        }
    }
}