using System;
using System.Collections.Generic;
using System.Linq;
using Dast.Converters.Base;
using Dast.Converters.Media.Html;
using Dast.Converters.Utils;

namespace Dast.Converters
{
    public class HtmlConverter : DashVisitorConverterBase
    {
        private readonly HashSet<IHtmlMediaConverter> _usedMediaConverters = new HashSet<IHtmlMediaConverter>();
        public bool UseRecommandedCss { get; set; } = true;

        public override FileExtension FileExtension => FileExtensions.Programming.Html;
        public IHtmlMediaConverter DefaultConverter { get; set; } = new HightlightJsConverter();

        public IEnumerable<IHtmlMediaConverter> MediaConverters { get; } = new IHtmlMediaConverter[]
        {
            new ImageConverter(),
            new VideoConverter(),
            new YouTubeConverter() 
        };

        public override string VisitDocument(DocumentNode node)
        {
            IEnumerable<string> convertion = node.Children.Select(Convert).ToArray();
            
            string result = $"<html>{Environment.NewLine}<head>{Environment.NewLine}<title>{node.MainTitles[0].Accept(this)}</title>";

            string head = string.Join(Environment.NewLine, _usedMediaConverters.Where(x => !string.IsNullOrEmpty(x.Head)).Select(x => $"<!-- {x.DisplayName} -->" + Environment.NewLine + x.Head));
            if (!string.IsNullOrWhiteSpace(head))
                result += Environment.NewLine + head;

            string css = "";
            foreach (IHtmlMediaConverter converter in _usedMediaConverters)
            {
                string converterCss = "";
                if (!string.IsNullOrEmpty(converter.MandatoryCss))
                {
                    converterCss += converter.MandatoryCss;
                }
                if (UseRecommandedCss && !string.IsNullOrEmpty(converter.RecommandedCss))
                {
                    if (!string.IsNullOrEmpty(converterCss))
                        converterCss += Environment.NewLine;
                    converterCss += converter.RecommandedCss;
                }

                if (string.IsNullOrEmpty(converterCss))
                    continue;

                if (!string.IsNullOrEmpty(css))
                    css += Environment.NewLine;
                css += $"/* {converter.DisplayName} */" + Environment.NewLine + converterCss;
            }

            if (!string.IsNullOrWhiteSpace(css))
                result += Environment.NewLine + $"<style media=\"screen\" type=\"text/css\">{Environment.NewLine}{css}{Environment.NewLine}</style>";

            result += Environment.NewLine + "</head>" + Environment.NewLine + "<body>" + Environment.NewLine
                + string.Join(Environment.NewLine, convertion)
                + Environment.NewLine + Environment.NewLine;

            string endOfPage = string.Join(Environment.NewLine, _usedMediaConverters.Where(x => !string.IsNullOrEmpty(x.EndOfPage)).Select(x => $"<!-- {x.DisplayName} -->" + Environment.NewLine + x.EndOfPage));
            if (!string.IsNullOrWhiteSpace(endOfPage))
                result += Environment.NewLine + endOfPage;

            result += "</body>" + Environment.NewLine + "</html>";

            return result;
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

        public override string VisitLink(LinkNode node)
        {
            if (node.IsLocal)
                return $"<a href=\"#{ node.Adress }\" >{ string.Join(" ", node.Children.Select(Convert)) }</a>";
            return $"<a href=\"{ node.Adress }\" >{ string.Join(" ", node.Children.Select(Convert)) }</a>";
        }

        public override string VisitAdress(AdressNode node)
        {
            return $"<span id=\"{string.Join(" ", node.Names)}\"></span>";
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
            IEnumerable<IHtmlMediaConverter> compatibtleConverters = MediaConverters.Where(x => x.Extensions.Any(e => e.Match(node.Extension)));

            IHtmlMediaConverter mediaConverter;
            if (node.Type.HasValue)
            {
                mediaConverter = MediaConverters.FirstOrDefault(x => x.DefaultType == node.Type.Value);
                if (mediaConverter == null && DefaultConverter.DefaultType == node.Type.Value)
                    mediaConverter = DefaultConverter;
            }
            else
                mediaConverter = compatibtleConverters.FirstOrDefault() ?? DefaultConverter;

            if (mediaConverter == null)
                return "";

            _usedMediaConverters.Add(mediaConverter);
            return mediaConverter.Convert(node.Extension, node.Content, inline, UseRecommandedCss);
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