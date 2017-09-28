using System.Collections.Generic;
using System.IO;
using Dast.Extensibility.Outputs;
using Dast.Media.Contracts.Html;

namespace Dast.Outputs.Html
{
    public class HtmlOutput : ExtensibleDocumentMultiWriterMergerBase<FragmentedHtmlOutput, IHtmlMediaOutput, HtmlFragment>
    {
        protected override IEnumerable<HtmlFragment> MergeFragments(TextWriter writer)
        {
            writer.WriteLine("<html>");
            writer.WriteLine("<head>");

            writer.Write("<title>");
            yield return HtmlFragment.Title;
            writer.WriteLine("</title>");

            writer.WriteLine("<style media=\"screen\" type=\"text/css\">");
            yield return HtmlFragment.Css;
            writer.WriteLine();
            writer.WriteLine("</style>");

            yield return HtmlFragment.Head;

            writer.WriteLine("</head>");
            writer.WriteLine("<body>");

            yield return HtmlFragment.Body;
            writer.WriteLine();
            yield return HtmlFragment.EndOfPage;
            writer.WriteLine();

            writer.WriteLine("</body>");
            writer.WriteLine("</html>");
        }
    }
}