using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Dast.Extensibility;
using Dast.Extensibility.Outputs;
using Dast.Media.Contracts.Html;
using Dast.Media.Contracts.Markdown;
using Dast.Outputs.Base;

namespace Dast.Outputs.GitHubMarkdown
{
    public class FragmentedGitHubMarkdownOutput : ExtensibleFragmentedDocumentWriterBase<IMarkdownMediaOutput, GithubMarkdownFragment>, IExtensionAdapter<IMarkdownMediaOutput, IHtmlMediaOutput>
    {
        private int _listLevel = -1;

        public override string DisplayName => "GitHub Markdown";
        public override FileExtension FileExtension => FileExtensions.Text.Markdown;

        protected override IEnumerable<GithubMarkdownFragment> DefaultKeys
        {
            get
            {
                yield return GithubMarkdownFragment.Body;
                yield return GithubMarkdownFragment.Notes;
            }
        }

        private readonly ExtensibleFormatCatalog<IHtmlMediaOutput> _htmlMediaOutputs = new ExtensibleFormatCatalog<IHtmlMediaOutput>();
        
        ICollection<IHtmlMediaOutput> IExtensible<IHtmlMediaOutput>.Extensions => _htmlMediaOutputs;
        public IMarkdownMediaOutput Adapt(IHtmlMediaOutput adaptee) => new HtmlMediaOutputAdapter(adaptee);

        public override IEnumerable<IMarkdownMediaOutput> Extend(CompositionContext context)
        {
            foreach (IMarkdownMediaOutput markdownMediaOutput in base.Extend(context))
                yield return markdownMediaOutput;

            foreach (IHtmlMediaOutput htmlMediaOutput in _htmlMediaOutputs.Extend(context))
            {
                if (htmlMediaOutput is IExtensible extensible)
                    extensible.Extend(context);

                IMarkdownMediaOutput adapted = Adapt(htmlMediaOutput);
                MediaCatalog.Add(adapted);
                yield return adapted;
            }
        }

        IEnumerable<IHtmlMediaOutput> IExtensible<IHtmlMediaOutput>.Extend(CompositionContext context)
        {
            IHtmlMediaOutput[] htmlMediaOutputs = _htmlMediaOutputs.Extend(context).ToArray();
            MediaCatalog.AddRange(htmlMediaOutputs.Select(Adapt));
            return htmlMediaOutputs;
        }

        public override void ResetToVanilla()
        {
            _htmlMediaOutputs.ResetToVanilla();
            base.ResetToVanilla();
        }

        public override void VisitDocument(DocumentNode node)
        {
            CurrentStream = GithubMarkdownFragment.Body;
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
            if (node.Important)
            {
                Write("**");
                Write(node.Line);
                Write("**");
            }
            else
                Write(node.Line);

            if (node.Sublist != null)
                Write(node.Sublist);
        }

        public override void VisitLine(LineNode node)
        {
            JoinChildren(node, " ");
            WriteLine();
        }

        public override void VisitLink(LinkNode node)
        {
            string address = ToHtmlIdentifier(node.AddressNode?.Id) ?? node.Address ?? "";
            
            if (node.Children.Count == 1 && node.Children[0] is TextNode textNode && address.EqualsOrdinal(textNode.Content))
            {
                Write("<");
                if (node.IsInternal)
                    Write("#");
                Write(address, ">");
            }
            else
            {
                Write("[");
                JoinChildren(node, " ");
                Write("]");
                Write("(");
                if (node.IsInternal)
                    Write("#");
                Write(address, ")");
            }
        }

        public override void VisitAddress(AddressNode node)
        {
            Write($"<span id=\"{ ToHtmlIdentifier(node.Id) }\"></span>");
        }

        protected override void VisitReference(ReferenceNode node, int index)
        {
            if (node.IsInlined)
            {
                Write("<abbr class=\"dast-reference\" title=\"", node.Note, "\">");
                JoinChildren(node, " ");
                Write("</abbr>");
            }
            else
            {
                Write("<span class=\"dast-reference\">");
                JoinChildren(node, " ");
                Write("<sup><a href=\"#dast-note-", index.ToString(), "\">", index.ToString(), "</a></sup></span>");
            }
        }

        protected override void VisitNote(NoteNode node, int index)
        {
            CurrentStream = GithubMarkdownFragment.Notes;
            Write("<p class=\"dast-note\" id=\"dast-note-", index.ToString(), "}\">", index.ToString(), ". ");
            JoinChildren(node, " ");
            WriteLine("</p>");
            CurrentStream = GithubMarkdownFragment.Body;
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

        public override void VisitQuote(QuoteNode node)
        {
            Write("<q>");
            JoinChildren(node, " ");
            Write("</q>");
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
            IMarkdownMediaOutput mediaConverter = null;
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
                            mediaConverter = MediaOutputs.FirstOrDefault(x => x.Type == MediaType.Visual && x.FileExtensions.Any(e => e.Match(node.Extension)));
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
            if (name == null)
                return null;

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