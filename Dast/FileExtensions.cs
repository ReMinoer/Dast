namespace Dast
{
    static public class FileExtensions
    {
        static public class Text
        {
            static public FileExtension Dash => new FileExtension("Dash", "dh", "dash");
            static public FileExtension Markdown => new FileExtension("Markdown", "md", "markdown");
        }

        static public class Image
        {
            static public FileExtension Png => new FileExtension("PNG", "png");
            static public FileExtension Jpeg => new FileExtension("JPEG", "jpg", "jpeg");
            static public FileExtension Gif => new FileExtension("GIF", "gif");
            static public FileExtension Bitmap => new FileExtension("Bitmap", "bmp", "dib", "rle");
            static public FileExtension Svg => new FileExtension("SVG", "svg", "svgz");
            static public FileExtension Ico => new FileExtension("ICO", "ico");
        }

        static public class Video
        {
            static public FileExtension Mp4 => new FileExtension("MPEG-4", "mp4");
            static public FileExtension YouTube => new FileExtension("Youtube", "youtube.com");
        }

        static public class Programming
        {
            static public FileExtension Html => new FileExtension("HTML", "html", "htm", "xhtml", "xht");
            static public FileExtension Csharp => new FileExtension("C#", "cs");
        }

        static public class Math
        {
            static public FileExtension R => new FileExtension("R", "r");
        }

        static public class Data
        {
            static public FileExtension Csv => new FileExtension("CSV", "csv");
        }
    }
}