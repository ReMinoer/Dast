using System;
using System.Collections.Generic;
using System.Linq;
using Dast.Converters.Base;
using Dast.Converters.Media.Markdown;
using Dast.Converters.Utils;

namespace Dast.Converters
{
    public class GithubMardownConverter : DashVisitorConverterBase
    {
        private int _listLevel = -1;
        public override FileExtension FileExtension => FileExtensions.Text.Markdown;

        public IEnumerable<IMediaConverter> MediaConverters { get; } = new IMediaConverter[]
        {
            new ImageConverter(),
            new Media.Html.VideoConverter(),
            new Media.Html.YouTubeConverter()
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
                && (node.AdressNode != null && node.AdressNode.Names.Any(x => x.Equals(textNode.Content, StringComparison.InvariantCultureIgnoreCase))
                    || node.AdressByDefault.Equals(textNode.Content, StringComparison.InvariantCultureIgnoreCase)))
                return $"<#{ HtmlConverter.ToIdentifier(node.AdressNode?.Names[0] ?? node.AdressByDefault ?? "") }>";

            return "[" + string.Join(" ", node.Children.Select(Convert)) + "](#" + (node.AdressNode?.Names[0] ?? node.AdressByDefault ?? "") + ")";
        }

        public override string VisitExternalLink(ExternalLinkNode node)
        {
            if (node.Children.Count == 1 && node.Children[0] is TextNode textNode && node.Adress.Equals(textNode.Content, StringComparison.InvariantCultureIgnoreCase))
                return $"<{ node.Adress }>";

            return "[" + string.Join(" ", node.Children.Select(Convert)) + "](" + node.Adress + ")";
        }

        public override string VisitAdress(AdressNode node)
        {
            return $"<span id=\"{ HtmlConverter.ToIdentifier(node.Names[0]) }\"></span>";
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
            return "<span" + (node.Class != null ? $" class=\"{HtmlConverter.ToIdentifier(node.Class)}\"" : "") + ">" + string.Join(" ", node.Children.Select(Convert)) + "</span>";
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
            IMediaConverter mediaConverter = null;
            if (node.Type.HasValue)
                type = node.Type.Value;
            else
            {
                mediaConverter = MediaConverters.FirstOrDefault(x => x.Extensions.Any(e => e.Match(node.Extension)));
                type = mediaConverter?.DefaultType ?? MediaType.Code;
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
    }
}