using System;
using System.Collections.Generic;
using System.Linq;
using Dast.Outputs.Html.Media.Base;

namespace Dast.Outputs.Html.Media
{
    public class CsvConverter : HtmlMediaConverterBase
    {
        private const string TableClass = "dast-csv-table";

        public override string DisplayName => "CSV tables";
        public override MediaType Type => MediaType.Visual;
        public override string RecommandedCss => $".{TableClass},.{TableClass} th,.{TableClass} td" + "{border:1px solid black;border-collapse:collapse;padding:5px;}";

        public override IEnumerable<FileExtension> Extensions
        {
            get
            {
                yield return FileExtensions.Data.Csv;
            }
        }

        public override string Convert(string extension, string content, bool inline)
        {
            string[] lines = content.Split(new [] { "\r\n", "\n" }, StringSplitOptions.None);
            char delimiter = lines[0].Cast<char>().Where(x => !char.IsLetterOrDigit(x)).GroupBy(x => x).OrderByDescending(x => x.Count()).First().Key;

            string result = "<figure><table";

            if (UseRecommandedCss)
                result += $" class=\"{TableClass}\"";

            result += ">" + Environment.NewLine;
            foreach (string line in lines)
            {
                result += "<tr>" + Environment.NewLine;
                foreach (string value in line.Split(delimiter))
                    result += $"<td>{value}</td>" + Environment.NewLine;
                result += "</tr>" + Environment.NewLine;
            }
            result += "</table></figure>";

            return result;
        }
    }
}