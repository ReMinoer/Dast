using System;
using System.Linq;
using Dast.Extensibility;
using Dast.Outputs.Base;

namespace Dast.Outputs.Dash
{
    public class DashOutput : ExtensibleDocumentOutputBase<Media.Contracts.Dash.IDashMediaOutput>
    {
        public override string DisplayName => "Dash";
        public override FileExtension FileExtension => FileExtensions.Text.Dash;

        private int _listLevel = -1;
        public int RecommendedLineSize { get; set; } = 100;

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

        public override string VisitInternalLink(InternalLinkNode node)
        {
            if (node.Children.Count == 1 && node.Children[0] is TextNode textNode
                && (node.AdressNode != null && node.AdressNode.Names.Any(x => x.Equals(textNode.Content, StringComparison.OrdinalIgnoreCase))
                    || node.AdressByDefault.Equals(textNode.Content, StringComparison.OrdinalIgnoreCase)))
                return $"[[{ node.AdressNode?.Names[0] ?? node.AdressByDefault ?? "" }]]";

            return $"[{ string.Join(" ", node.Children.Select(Convert)) }][{ node.AdressNode?.Names[0] ?? node.AdressByDefault ?? "" }]";
        }

        public override string VisitExternalLink(ExternalLinkNode node)
        {
            if (node.Children.Count == 1 && node.Children[0] is TextNode textNode && node.Adress.Equals(textNode.Content, StringComparison.OrdinalIgnoreCase))
                return $"[[{ node.Adress }]]";

            return $"[{ string.Join(" ", node.Children.Select(Convert)) }][{ node.Adress }]";
        }

        public override string VisitAdress(AdressNode node)
        {
            return $"@[{ string.Join("|", node.Names) }]";
        }

        protected override string VisitReference(ReferenceNode node, int index)
        {
            return $"[{ string.Join(" ", node.Children.Select(Convert)) }][{ index }]";
        }

        protected override string VisitNote(NoteNode node, int index)
        {
            return $"[{ index }] { Convert(node.Line) }";
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

        public override string VisitMedia(MediaNode node)
        {
            return $"<< .{node.Extension} >>" + Environment.NewLine + node.Content + Environment.NewLine + "<...>";
        }

        public override string VisitMediaInline(MediaInlineNode node)
        {
            return (node.Extension != null ? $"< .{node.Extension} >" : "") + "{ " + node.Content + " }";
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