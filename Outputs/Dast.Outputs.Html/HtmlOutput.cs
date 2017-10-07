using System.Collections.Generic;
using Dast.Extensibility.Outputs;
using Dast.Media.Contracts.Html;

namespace Dast.Outputs.Html
{
    public class HtmlOutput : ExtensibleDocumentMultiWriterMergerBase<FragmentedHtmlOutput, IHtmlMediaOutput, HtmlFragment>
    {
        protected override IEnumerable<HtmlFragment> MergeFragments()
        {
            using (Conditional)
            {
                Writer.WriteLine("<html>");

                using (Conditional)
                {
                    Writer.WriteLine("<head>");

                    using (Conditional)
                    {
                        Writer.Write("<title>");
                        yield return HtmlFragment.Title;
                        Writer.WriteLine("</title>");
                    }

                    using (Conditional)
                    {
                        Writer.WriteLine("<style media=\"screen\" type=\"text/css\">");
                        yield return HtmlFragment.Css;
                        Writer.WriteLine();
                        Writer.WriteLine("</style>");
                    }

                    using (Conditional)
                        yield return HtmlFragment.Head;

                    Writer.WriteLine("</head>");
                }
                
                using (Conditional)
                {
                    Writer.WriteLine("<body>");

                    using (Conditional)
                    {
                        yield return HtmlFragment.Body;
                        Writer.WriteLine();
                    }

                    using (Conditional)
                    {
                        yield return HtmlFragment.EndOfPage;
                        Writer.WriteLine();
                    }

                    Writer.WriteLine("</body>");
                }

                Writer.WriteLine("</html>");
            }
        }
    }
}