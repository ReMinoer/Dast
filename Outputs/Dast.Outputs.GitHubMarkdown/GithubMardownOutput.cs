using System;
using System.Collections.Generic;
using System.Linq;
using Dast.Outputs.Base;
using Dast.Outputs.GitHubMarkdown.Media;

namespace Dast.Outputs.GitHubMarkdown
{
    public class GitHubMardownOutput : DocumentOutputBase<GitHubMardownOutput.IMediaOutput>
    {
        public interface IMediaOutput : Outputs.IMediaOutput { }

        public override string DisplayName => "GitHub Markdown";
        public override FileExtension FileExtension => FileExtensions.Text.Markdown;

        private int _listLevel = -1;

        public IEnumerable<IMediaOutput> MediaConverters { get; } = new IMediaOutput[]
        {
            new ImageConverter()
        };

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

        public override string VisitInternalLink(InternalLinkNode node)
        {
            if (node.Children.Count == 1 && node.Children[0] is TextNode textNode
                && (node.AdressNode != null && node.AdressNode.Names.Any(x => x.Equals(textNode.Content, StringComparison.OrdinalIgnoreCase))
                    || node.AdressByDefault.Equals(textNode.Content, StringComparison.OrdinalIgnoreCase)))
                return $"<#{ ToHtmlIdentifier(node.AdressNode?.Names[0] ?? node.AdressByDefault ?? "") }>";

            return "[" + string.Join(" ", node.Children.Select(Convert)) + "](#" + (node.AdressNode?.Names[0] ?? node.AdressByDefault ?? "") + ")";
        }

        public override string VisitExternalLink(ExternalLinkNode node)
        {
            if (node.Children.Count == 1 && node.Children[0] is TextNode textNode && node.Adress.Equals(textNode.Content, StringComparison.OrdinalIgnoreCase))
                return $"<{ node.Adress }>";

            return "[" + string.Join(" ", node.Children.Select(Convert)) + "](" + node.Adress + ")";
        }

        public override string VisitAdress(AdressNode node)
        {
            return $"<span id=\"{ ToHtmlIdentifier(node.Names[0]) }\"></span>";
        }

        public override string VisitReference(ReferenceNode node, int index)
        {
            return $"<span class=\"dast-reference\">{ string.Join(" ", node.Children.Select(Convert)) }<sup><a href=\"#dast-note-{ index }\">{ index }</a></sup></span>";
        }

        public override string VisitNote(NoteNode node, int index)
        {
            return $"<p class=\"dast-note\" id=\"dast-note-{ index }\">{ index }. { string.Join(" ", node.Children.Select(Convert)) }</p>{ Environment.NewLine }";
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
            return "<span" + (node.Class != null ? $" class=\"{ ToHtmlIdentifier(node.Class) }\"" : "") + ">" + string.Join(" ", node.Children.Select(Convert)) + "</span>";
        }

        public override string VisitText(TextNode textNode)
        {
            return textNode.Content;
        }

        public override string VisitMedia(MediaNode node)
        {
            return VisitMediaBase(node, false);
        }

        public override string VisitMediaInline(MediaInlineNode node)
        {
            return VisitMediaBase(node, true);
        }

        private string VisitMediaBase(MediaNodeBase node, bool inline)
        {
            MediaType type;
            IMediaOutput mediaConverter = null;
            if (node.Type.HasValue)
                type = node.Type.Value;
            else
            {
                mediaConverter = MediaConverters.FirstOrDefault(x => x.Extensions.Any(e => e.Match(node.Extension)));
                type = mediaConverter?.Type ?? MediaType.Code;
            }

            switch (type)
            {
                case MediaType.Code:
                    return inline ? "`" + node.Content + "`" : "```" + node.Extension + Environment.NewLine + node.Content + Environment.NewLine + "```";
                case MediaType.Visual:
                    if (mediaConverter == null)
                    {
                        mediaConverter = MediaConverters.FirstOrDefault(x => x.Extensions.Any(e => e.Match(node.Extension)));
                        if (mediaConverter == null)
                            return "";
                    }
                    return Environment.NewLine + mediaConverter.Convert(node.Extension, node.Content, inline) + Environment.NewLine;
                default:
                    throw new NotSupportedException();
            }
        }

        public override string VisitComment(CommentNode node)
        {
            return "<!--"
                   + (node.Inline ? " " : Environment.NewLine)
                   + node.Content
                   + (node.Inline ? " " : Environment.NewLine)
                   + "-->";
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