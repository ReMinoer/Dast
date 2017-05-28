using System;
using System.Collections.Generic;
using System.Linq;
using Dast.Converters.Base;
using Dast.Converters.Utils;

namespace Dast.Converters
{
    public class DashConverter : DashVisitorConverterBase
    {
        private int _listLevel = -1;
        public int RecommendedLineSize { get; set; } = 100;

        public override IEnumerable<string> FileExtensions
        {
            get
            {
                yield return "dh";
                yield return "dash";
            }
        }

        public override string VisitDocument(DocumentNode node)
        {
            return string.Join(Environment.NewLine, node.Children.Select(Convert));
        }

        public override string VisitParagraph(ParagraphNode node)
        {
            string content = node.Children.Aggregate(Convert);
            if (node.Class == null)
                return content;

            string header = "< " + node.Class + " >";
            if (header.HasMultipleLine() || header.Length > RecommendedLineSize)
                header += Environment.NewLine;
            else
                header += " ";

            return header + content;
        }

        public override string VisitTitle(TitleNode node)
        {
            string content = node.Children.Aggregate(Convert);
            string header = "<" + new string('-', node.Level) + ">";
            if (header.HasMultipleLine() || header.Length > RecommendedLineSize)
                header += Environment.NewLine;
            else
                header += " ";

            return header + content;
        }

        public override string VisitList(ListNode node)
        {
            _listLevel++;
            string result = node.Children.Select((item, i) => new string(' ', 4 * _listLevel) + (node.Ordered ? (i + 1).ToString() : "") + "- " + Convert(item)).Aggregate();
            _listLevel--;
            return result;
        }

        public override string VisitListItem(ListItemNode node)
        {
            return Convert(node.Line) + Convert(node.Sublist);
        }

        public override string VisitLine(LineNode node)
        {
            return string.Join(" ", node.Children.Select(Convert)) + Environment.NewLine;
        }

        public override string VisitBold(BoldNode node)
        {
            return "*[" + string.Join(" ", node.Children.Select(Convert)) + "]";
        }

        public override string VisitItalic(ItalicNode node)
        {
            return "/[" + string.Join(" ", node.Children.Select(Convert)) + "]";
        }

        public override string VisitMark(MarkNode node)
        {
            return "=[" + string.Join(" ", node.Children.Select(Convert)) + "]";
        }

        public override string VisitObsolete(ObsoleteNode node)
        {
            return "~[" + string.Join(" ", node.Children.Select(Convert)) + "]";
        }

        public override string VisitEmphasis(EmphasisNode node)
        {
            return (node.Class != null ? $"<{node.Class}>" : "") + "[" + string.Join(" ", node.Children.Select(Convert)) + "]";
        }

        public override string VisitText(TextNode textNode)
        {
            return textNode.Content;
        }

        public override string VisitComment(CommentNode node)
        {
            return "~~"
                + (node.Inline ? " " : "~~" + Environment.NewLine)
                + node.Content
                + (node.Inline ? "" : Environment.NewLine + "~~~~");
        }
    }
}