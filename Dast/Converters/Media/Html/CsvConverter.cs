using System;
using System.Collections.Generic;
using System.Linq;
using Dast.Converters.Media.Html.Base;
using Dast.Converters.Utils;

namespace Dast.Converters.Media.Html
{
    public class CsvConverter : HtmlMediaConverterBase
    {
        private const string TableClass = "dast-csv-table";

        public override string DisplayName => "CSV tables";
        public override MediaType DefaultType => MediaType.Visual;
        public override string RecommandedCss => $".{TableClass},.{TableClass} th,.{TableClass} td" + "{border:1px solid black;border-collapse:collapse;padding:5px;}";

        public override IEnumerable<FileExtension> Extensions
        {
            get
            {
                yield return FileExtensions.Data.Csv;
            }
        }

        public override string Convert(string extension, string content, bool inline, bool useRecommandedCss)
        {
            string[] lines = content.Split(new [] { "\r\n", "\n" }, StringSplitOptions.None);
            char delimiter = lines[0].Where(x => !char.IsLetterOrDigit(x)).GroupBy(x => x).OrderByDescending(x => x.Count()).First().Key;

            string result = "<figure><table";

            if (useRecommandedCss)
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