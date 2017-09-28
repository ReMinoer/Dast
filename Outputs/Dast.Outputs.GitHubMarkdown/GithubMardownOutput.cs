using System;
using System.Linq;
using Dast.Extensibility.Outputs;
using Dast.Outputs.Base;

namespace Dast.Outputs.GitHubMarkdown
{
    public class GitHubMardownOutput : ExtensibleDocumentWriterBase<Media.Contracts.Markdown.IMarkdownMediaOutput>
    {
        public override string DisplayName => "GitHub Markdown";
        public override FileExtension FileExtension => FileExtensions.Text.Markdown;

        private int _listLevel = -1;

        public override void VisitDocument(DocumentNode node)
        {
            JoinChildren(node, NewLine);
        }

        public override void VisitParagraph(ParagraphNode node)
        {
            AggregateChildren(node);
        }

        public override void VisitTitle(TitleNode node)
        {
            Write(new string('#', node.Level) + " ");
            AggregateChildren(node);
        }

        public override void VisitList(ListNode node)
        {
            _listLevel++;
            AggregateChildren(i => new string(' ', 4 * _listLevel) + (node.Ordered ? $"{i + 1}." : "*") + " ", node);
            _listLevel--;
        }

        public override void VisitListItem(ListItemNode node)
        {
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
                && (node.AdressNode != null && node.AdressNode.Names.Any(x => x.EqualsOrdinal(textNode.Content))
                || node.AdressByDefault.EqualsOrdinal(textNode.Content)))
            {
                Write("<#", ToHtmlIdentifier(adress), ">");
            }
            else
            {
                Write("[");
                JoinChildren(node, " ");
                Write("](#", adress, ")");
            }
        }

        public override void VisitExternalLink(ExternalLinkNode node)
        {
            if (node.Children.Count == 1 && node.Children[0] is TextNode textNode && node.Adress.EqualsOrdinal(textNode.Content))
            {
                Write("<", node.Adress, ">");
            }
            else
            {
                Write("[");
                JoinChildren(node, " ");
                Write("](", node.Adress, ")");
            }
        }

        public override void VisitAdress(AdressNode node)
        {
            Write($"<span id=\"{ ToHtmlIdentifier(node.Names[0]) }\"></span>");
        }

        protected override void VisitReference(ReferenceNode node, int index)
        {
            Write("<span class=\"dast-reference\">");
            JoinChildren(node, " ");
            Write($"<sup><a href=\"#dast-note-{ index }\">{ index }</a></sup></span>");
        }

        protected override void VisitNote(NoteNode node, int index)
        {
            Write($"<p class=\"dast-note\" id=\"dast-note-{ index }\">{ index }. ");
            Write(node.Line);
            WriteLine("</p>");
        }

        public override void VisitBold(BoldNode node)
        {
            Write("**");
            JoinChildren(node, " ");
            Write("**");
        }

        public override void VisitItalic(ItalicNode node)
        {
            Write("*");
            JoinChildren(node, " ");
            Write("*");
        }

        public override void VisitMark(MarkNode node)
        {
            Write("<mark>");
            JoinChildren(node, " ");
            Write("</mark>");
        }

        public override void VisitObsolete(ObsoleteNode node)
        {
            Write("<s>");
            JoinChildren(node, " ");
            Write("</s>");
        }

        public override void VisitEmphasis(EmphasisNode node)
        {
            Write("<span");
            if (node.Class != null)
                Write(" class=\"", ToHtmlIdentifier(node.Class), "\"");
            Write(">");

            JoinChildren(node, " ");
            Write("</span>");
        }

        public override void VisitText(TextNode node)
        {
            Write(node.Content);
        }

        public override void VisitMedia(MediaNode node)
        {
            VisitMediaBase(node, false);
        }

        public override void VisitMediaInline(MediaInlineNode node)
        {
            VisitMediaBase(node, true);
        }

        private void VisitMediaBase(MediaNodeBase node, bool inline)
        {
            MediaType type;
            Media.Contracts.Markdown.IMarkdownMediaOutput mediaConverter = null;
            if (node.Type.HasValue)
                type = node.Type.Value;
            else
            {
                mediaConverter = MediaOutputs.FirstOrDefault(x => x.FileExtensions.Any(e => e.Match(node.Extension)));
                type = mediaConverter?.Type ?? MediaType.Code;
            }

            switch (type)
            {
                case MediaType.Code:
                    {
                        if (inline)
                            Write("`", node.Content, "`");
                        else
                        {
                            WriteLine("```", node.Extension);
                            WriteLine(node.Content);
                            WriteLine("```");
                        }
                        return;
                    }
                case MediaType.Visual:
                    {
                        if (mediaConverter == null)
                        {
                            mediaConverter = MediaOutputs.FirstOrDefault(x => x.FileExtensions.Any(e => e.Match(node.Extension)));
                            if (mediaConverter == null)
                                return;
                        }
                        
                        Write(mediaConverter.Convert(node.Extension, node.Content, inline));
                        WriteLine();

                        return;
                    }
                default:
                    throw new NotSupportedException();
            }
        }

        public override void VisitComment(CommentNode node)
        {
            string ws = node.Inline ? " " : NewLine;
            Write("<!--", ws, node.Content, ws, "-->");
        }

        static public string ToHtmlIdentifier(string name)
        {
            string result = "";
            bool upperNext = false;

            int startIndex = name.Length;
            for (int i = 0; i < name.Length; i++)
            {
                if (!char.IsLetter(name[i]))
                    continue;

                startIndex = i;
                break;
            }

            foreach (char c in name.Cast<char>().Skip(startIndex))
            {
                if (!char.IsLetterOrDigit(c) || char.IsWhiteSpace(c))
                    upperNext = true;
                else
                {
                    result += upperNext ? char.ToUpperInvariant(c) : c;
                    upperNext = false;
                }
            }

            return result;
        }
    }
}