using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Dast.Media.Contracts.Html;

namespace Dast.Media.Html.Core
{
    public class AbcjsConverter : HtmlMediaOutputBase
    {
        public override string DisplayName => "abcjs";
        public override MediaType Type => MediaType.Visual;
        public override string Head => "<script src=\"abcjs_basic-min.js\"></script>";

        public override IEnumerable<FileExtension> FileExtensions
        {
            get
            {
                yield return Dast.FileExtensions.Music.Abc;
            }
        }

        public override async Task GetResourceFilesAsync(string outputDirectory)
        {
            using (WebClient webClient = new WebClient())
            using (Stream stream = await webClient.OpenReadTaskAsync("https://raw.githubusercontent.com/paulrosen/abcjs/master/bin/abcjs_basic_5.10.3-min.js"))
            {
                string path = Path.Combine(outputDirectory, "abcjs_basic-min.js");

                using (var file = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                {
                    await stream.CopyToAsync(file);
                }
            }
        }

        public override string Convert(string extension, string content, bool inline)
        {
            string id = $"abcjs-{Guid.NewGuid()}";
            string javascript = $"window.ABCJS.renderAbc(\"{id}\", `{content}`);";

            return $"<div id=\"{id}\"></div><script>{javascript}</script>";
        }
    }
}