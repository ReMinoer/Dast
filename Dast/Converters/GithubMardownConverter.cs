using System;
using System.Collections.Generic;
using System.Linq;
using Dast.Converters.Base;
using Dast.Converters.Utils;

namespace Dast.Converters
{
    public class GithubMardownConverter : DashVisitorConverterBase
    {
        private int _listLevel = -1;

        public override IEnumerable<string> FileExtensions
        {
            get
            {
                yield return "md";
                yield return "markdown";
            }
        }

        public override string VisitDocument(DocumentNode node)
        {
            return string.Join(Environment.NewLine, node.Children.Select(Convert));
        }

        public override string VisitParagraph(ParagraphNode node)
        {
            return node.Children.Aggregate(Convert);
        }

        public override string VisitTitle(TitleNode node)
        {
            return node.Children.Aggregate(x => new string('#', node.Level) + " " + Convert(x));
        }

        public override string VisitList(ListNode node)
        {
            _listLevel++;
            string result = node.Children.Select((item, i) => new string(' ', 4 * _listLevel) + (node.Ordered ? (i + 1).ToString() + "." : "*") + " " + Convert(item)).Aggregate();
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
            return "**" + string.Join(" ", node.Children.Select(Convert)) + "**";
        }

        public override string VisitItalic(ItalicNode node)
        {
            return "*" + string.Join(" ", node.Children.Select(Convert)) + "*";
        }

        public override string VisitMark(MarkNode node)
        {
            return "<mark>" + string.Join(" ", node.Children.Select(Convert)) + "</mark>";
        }

        public override string VisitObsolete(ObsoleteNode node)
        {
            return "<s>" + string.Join(" ", node.Children.Select(Convert)) + "</s>";
        }

        public override string VisitEmphasis(EmphasisNode node)
        {
            return "<span" + (node.Class != null ? $" class=\"{HtmlConverter.ToIdentifier(node.Class)}\"" : "") + ">" + string.Join(" ", node.Children.Select(Convert)) + "</span>";
        }

        public override string VisitText(TextNode textNode)
        {
            return textNode.Content;
        }

        public override string VisitComment(CommentNode node)
        {
            return "<!--"
                   + (node.Inline ? " " : Environment.NewLine)
                   + node.Content
                   + (node.Inline ? " " : Environment.NewLine)
                   + "-->";
        }
    }
}