using System;
using System.Collections.Generic;
using System.Linq;
using Dast.Converters.Base;
using Dast.Converters.Utils;

namespace Dast.Converters
{
    public class HtmlConverter : DashVisitorConverterBase
    {
        public override IEnumerable<string> FileExtensions
        {
            get
            {
                yield return "html";
                yield return "htm";
                yield return "xhtml";
                yield return "xht";
            }
        }

        public override string VisitDocument(DocumentNode node)
        {
            return string.Join(Environment.NewLine, node.Children.Select(Convert));
        }

        public override string VisitParagraph(ParagraphNode node)
        {
            return "<p" + (node.Class != null ? $" class=\"{ToIdentifier(node.Class)}\"" : "") + ">" + string.Join("<br />" + Environment.NewLine, node.Children.Select(Convert)) + "</p>";
        }

        public override string VisitTitle(TitleNode node)
        {
            return $"<h{node.Level}>" + string.Join("<br />", node.Children.Select(Convert)) + $"</h{node.Level}>";
        }

        public override string VisitList(ListNode node)
        {
            string type = node.Ordered ? "ol" : "ul";
            return "<" + type + ">" + Environment.NewLine + node.Children.Aggregate(Convert) + "</" + type + ">" + Environment.NewLine;
        }

        public override string VisitListItem(ListItemNode node)
        {
            return "<li>" + Convert(node.Line) + (node.Sublist != null ? Convert(node.Sublist) : "") + "</li>" + Environment.NewLine;
        }

        public override string VisitLine(LineNode node)
        {
            return string.Join(" ", node.Children.Select(Convert));
        }

        public override string VisitBold(BoldNode node)
        {
            return "<strong>" + string.Join(" ", node.Children.Select(Convert)) + "</strong>";
        }

        public override string VisitItalic(ItalicNode node)
        {
            return "<em>" + string.Join(" ", node.Children.Select(Convert)) + "</em>";
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
            return "<span" + (node.Class != null ? $" class=\"{ToIdentifier(node.Class)}\"" : "") + ">" + string.Join(" ", node.Children.Select(Convert)) + "</span>";
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

        static public string ToIdentifier(string name)
        {
            string result = "";
            bool upperNext = false;
            foreach (char c in name)
            {
                switch (c)
                {
                    case ' ':
                    case '\t':
                    {
                        upperNext = true;
                    }
                    break;
                    default:
                    {
                        result += upperNext ? char.ToUpperInvariant(c) : c;
                        upperNext = false;
                    }
                    break;
                }
            }

            return result;
        }
    }
}