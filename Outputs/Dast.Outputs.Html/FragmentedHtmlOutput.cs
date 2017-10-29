using System.Collections.Generic;
using System.Linq;
using Dast.Extensibility.Outputs;
using Dast.Media.Contracts.Html;

namespace Dast.Outputs.Html
{
    public class FragmentedHtmlOutput : ExtensibleFragmentedDocumentWriterBase<IHtmlMediaOutput, HtmlFragment>
    {
        public override string DisplayName => "HTML";
        public override FileExtension FileExtension => FileExtensions.Programming.Html;

        protected override IEnumerable<HtmlFragment> DefaultKeys
        {
            get
            {
                yield return HtmlFragment.Body;
                yield return HtmlFragment.Notes;
            }
        }

        private readonly HashSet<IHtmlMediaOutput> _usedMediaConverters = new HashSet<IHtmlMediaOutput>();

        public override void VisitDocument(DocumentNode node)
        {
            CurrentStream = HtmlFragment.Title;
            LineNode title = node.MainTitles.FirstOrDefault();
            if (title != null)
                Write(title);

            CurrentStream = HtmlFragment.Body;
            JoinChildren(node, NewLine);
        }

        public override void VisitParagraph(ParagraphNode node)
        {
            Write("<p");
            if (node.Class != null)
                Write(" class=\"", ToIdentifier(node.Class), "\"");
            Write(">");
            JoinChildren(node, "<br />" + NewLine);
            Write("</p>");
        }

        public override void VisitTitle(TitleNode node)
        {
            Write("<h", node.Level.ToString(), ">");
            JoinChildren(node, "<br />" + NewLine);
            Write("</h", node.Level.ToString(), ">");
        }

        public override void VisitList(ListNode node)
        {
            string type = node.Ordered ? "ol" : "ul";

            WriteLine("<", type, ">");
            AggregateChildren(node);
            WriteLine("</", type, ">");
        }

        public override void VisitListItem(ListItemNode node)
        {
            Write("<li>");
            Write(node.Line);
            if (node.Sublist != null)
                Write(node.Sublist);
            WriteLine("</li>");
        }

        public override void VisitLine(LineNode node)
        {
            JoinChildren(node, " ");
            WriteLine();
        }

        public override void VisitInternalLink(InternalLinkNode node)
        {
            Write("<a href=\"#", ToIdentifier(node.AdressNode?.Names[0] ?? node.AdressByDefault), "\">");
            JoinChildren(node, " ");
            Write("</a>");
        }

        public override void VisitExternalLink(ExternalLinkNode node)
        {
            Write("<a href=\"", node.Adress, "\">");
            JoinChildren(node, " ");
            Write("</a>");
        }

        public override void VisitAdress(AdressNode node)
        {
            Write("<span id=\"", ToIdentifier(node.Names[0]), "\"></span>");
        }

        protected override void VisitReference(ReferenceNode node, int index)
        {
            Write("<span class=\"dast-reference\">");
            JoinChildren(node, " ");
            Write("<sup><a href=\"#dast-note-", index.ToString(), "\">", index.ToString(), "</a></sup></span>");
        }

        protected override void VisitNote(NoteNode node, int index)
        {
            CurrentStream = HtmlFragment.Notes;
            Write("<p class=\"dast-note\" id=\"dast-note-", index.ToString(), "\">", index.ToString(), ". ");
            JoinChildren(node, " ");
            Write("</p>");
            CurrentStream = HtmlFragment.Body;
        }

        public override void VisitBold(BoldNode node)
        {
            Write("<strong>");
            JoinChildren(node, " ");
            Write("</strong>");
        }

        public override void VisitItalic(ItalicNode node)
        {
            Write("<em>");
            JoinChildren(node, " ");
            Write("</em>");
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
                Write(" class=\"", ToIdentifier(node.Class), "\"");
            Write(">");
            JoinChildren(node, " ");
            Write("</span>");
        }

        public override void VisitText(TextNode textNode)
        {
            Write(textNode.Content);
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
            IEnumerable<IHtmlMediaOutput> compatibtleConverters = MediaOutputs.Where(x => x.FileExtensions.Any(e => e.Match(node.Extension)));

            IHtmlMediaOutput mediaConverter;
            if (node.Type.HasValue)
                mediaConverter = MediaOutputs.FirstOrDefault(x => x.Type == node.Type.Value);
            else
                mediaConverter = compatibtleConverters.FirstOrDefault();

            if (mediaConverter == null)
            {
                if (node.Type == null || node.Type.Value == MediaType.Code)
                {
                    if (!inline)
                    {
                        WriteLine("<figure>");
                        Write("<pre>");
                    }
                    Write("<code>", node.Content, "</code>");
                    if (!inline)
                    {
                        WriteLine("</pre>");
                        Write("</figure>");
                    }
                }

                return;
            }

            if (_usedMediaConverters.Add(mediaConverter))
            {
                if (!string.IsNullOrEmpty(mediaConverter.Head))
                {
                    CurrentStream = HtmlFragment.Head;

                    WriteLine("<!-- ", mediaConverter.DisplayName, " -->");
                    WriteLine(mediaConverter.Head);
                }

                if (!string.IsNullOrEmpty(mediaConverter.MandatoryCss) || mediaConverter.UseRecommandedCss && !string.IsNullOrEmpty(mediaConverter.RecommandedCss))
                {
                    CurrentStream = HtmlFragment.Css;

                    WriteLine("/* ", mediaConverter.DisplayName, " */");

                    if (!string.IsNullOrEmpty(mediaConverter.MandatoryCss))
                        WriteLine(mediaConverter.MandatoryCss);
                    if (mediaConverter.UseRecommandedCss && !string.IsNullOrEmpty(mediaConverter.RecommandedCss))
                        WriteLine(mediaConverter.RecommandedCss);
                }

                if (!string.IsNullOrEmpty(mediaConverter.EndOfPage))
                {
                    CurrentStream = HtmlFragment.EndOfPage;

                    WriteLine("<!-- ", mediaConverter.DisplayName, " -->");
                    WriteLine(mediaConverter.EndOfPage);
                }

                CurrentStream = HtmlFragment.Body;
            }

            Write(mediaConverter.Convert(node.Extension, node.Content, inline));
        }

        public override void VisitComment(CommentNode node)
        {
            string ws = node.Inline ? " " : NewLine;
            Write("<!--", ws, node.Content, ws, "-->");
        }

        static public string ToIdentifier(string name)
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