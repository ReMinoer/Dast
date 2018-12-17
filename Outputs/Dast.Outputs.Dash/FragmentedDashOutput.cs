using System;
using System.Collections.Generic;
using System.Linq;
using Dast.Extensibility.Outputs;
using Dast.Media.Contracts.Dash;
using Dast.Outputs.Base;

namespace Dast.Outputs.Dash
{
    public class FragmentedDashOutput : ExtensibleFragmentedDocumentWriterBase<IDashMediaOutput, DashFragment>
    {
        public override string DisplayName => "Dash";
        public override FileExtension FileExtension => FileExtensions.Text.Dash;

        protected override IEnumerable<DashFragment> DefaultKeys
        {
            get
            {
                yield return DashFragment.Body;
                yield return DashFragment.Notes;
            }
        }

        private int _listLevel = -1;
        public int RecommendedLineSize { get; set; } = 100;

        public override void VisitDocument(DocumentNode node)
        {
            CurrentStream = DashFragment.Body;
            Aggregate("<> ", node.MainTitles);
            WriteLine();

            JoinChildren(node, NewLine);
        }

        public override void VisitParagraph(ParagraphNode node)
        {
            StartDump();
            AggregateChildren(node);
            string content = StopDump();

            if (!string.IsNullOrEmpty(node.Class))
            {
                Write("< ", node.Class, " >");

                if (content.HasMultipleLine() || content.Length > RecommendedLineSize)
                    WriteLine();
                else
                    Write(" ");
            }

            Write(content);
        }

        public override void VisitTitle(TitleNode node)
        {
            Write("<", new string('-', node.Level), ">");

            StartDump();
            AggregateChildren(node);
            string content = StopDump();

            if (content.HasMultipleLine() || content.Length > RecommendedLineSize)
                WriteLine();
            else
                Write(" ");

            Write(content);
        }

        public override void VisitList(ListNode node)
        {
            _listLevel++;
            AggregateChildren(i => new string(' ', 4 * _listLevel) + (node.Ordered ? (i + 1).ToString() : "") + "-", node);
            _listLevel--;
        }

        public override void VisitListItem(ListItemNode node)
        {
            if (node.Important)
                Write(">");

            Write(" ");
            Write(node.Line);
            if (node.Sublist != null)
                Write(node.Sublist);
        }

        public override void VisitLine(LineNode node)
        {
            JoinChildren(node, " ");
            WriteLine();
        }

        public override void VisitInternalLink(InternalLinkNode node)
        {
            string adress = node.AdressNode?.Names[0] ?? node.AdressByDefault ?? "";

            if (node.Children.Count == 1 && node.Children[0] is TextNode textNode
                && (node.AdressNode != null && node.AdressNode.Names.Any(x => x.Equals(textNode.Content, StringComparison.OrdinalIgnoreCase))
                || node.AdressByDefault.Equals(textNode.Content, StringComparison.OrdinalIgnoreCase)))
            {
                Write("[[", adress, "]]");
            }
            else
            {
                Write("[");
                JoinChildren(node, " ");
                Write("][", adress, "]");
            }
        }

        public override void VisitExternalLink(ExternalLinkNode node)
        {
            if (node.Children.Count == 1 && node.Children[0] is TextNode textNode && node.Adress.Equals(textNode.Content, StringComparison.OrdinalIgnoreCase))
            {
                Write("[[", node.Adress, "]]");
            }
            else
            {
                Write("[");
                JoinChildren(node, " ");
                Write("][", node.Adress, "]");
            }
        }

        public override void VisitAdress(AdressNode node)
        {
            Write("@[", string.Join("|", node.Names), "]");
        }

        protected override void VisitReference(ReferenceNode node, int index)
        {
            Write("[");
            JoinChildren(node, " ");
            Write("][", index.ToString(), "]");
        }

        protected override void VisitNote(NoteNode node, int index)
        {
            CurrentStream = DashFragment.Notes;
            Write("@[", index.ToString(), "] ");
            Write(node.Line);
            CurrentStream = DashFragment.Body;
        }

        public override void VisitBold(BoldNode node)
        {
            Write("*[");
            JoinChildren(node, " ");
            Write("]");
        }

        public override void VisitItalic(ItalicNode node)
        {
            Write("/[");
            JoinChildren(node, " ");
            Write("]");
        }

        public override void VisitMark(MarkNode node)
        {
            Write("=[");
            JoinChildren(node, " ");
            Write("]");
        }

        public override void VisitObsolete(ObsoleteNode node)
        {
            Write("~[");
            JoinChildren(node, " ");
            Write("]");
        }

        public override void VisitEmphasis(EmphasisNode node)
        {
            if (!string.IsNullOrEmpty(node.Class))
                Write("<", node.Class, ">");

            Write("[");
            JoinChildren(node, " ");
            Write("]");
        }

        public override void VisitText(TextNode textNode)
        {
            Write(textNode.Content);
        }

        public override void VisitMedia(MediaNode node)
        {
            WriteLine("<< .", node.Extension, node.Type == null ? "" : (node.Type.Value == MediaType.Visual ? "+" : "-")," >>");
            WriteLine();
            WriteLine(node.Content);
            WriteLine();
            WriteLine("<...>");
        }

        public override void VisitMediaInline(MediaInlineNode node)
        {
            if (!string.IsNullOrEmpty(node.Extension))
                Write("< .", node.Extension, ">");

            Write("{", node.Content, "}");
        }

        public override void VisitComment(CommentNode node)
        {
            if (node.Inline)
                Write("~~ ");
            else
                WriteLine("~~");

            Write(node.Content);

            if (node.Inline)
                return;

            WriteLine();
            WriteLine("~~");
        }
    }
}