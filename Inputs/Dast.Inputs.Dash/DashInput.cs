using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using DashCSharp;
using Dast.Catalogs;
using Dast.Extensibility;
using Dast.Outputs.Base;

namespace Dast.Inputs.Dash
{
    public class DashInput : DashParserBaseVisitor<IDocumentNode>, IExtensibleDocumentInput<Media.Contracts.Dash.IMediaInput, string>
    {
        public string DisplayName => "Dash";
        public FileExtension FileExtension => Dast.FileExtensions.Text.Dash;
        public IEnumerable<FileExtension> FileExtensions { get { yield return FileExtension; } }

        private string _paragraphMode;
        private int? _titleMode;

        private readonly Dictionary<int, List<ReferenceNode>> _indexedReferencies = new Dictionary<int, List<ReferenceNode>>();
        private readonly Queue<ReferenceNode> _referenciesQueue = new Queue<ReferenceNode>();
        private readonly Dictionary<InternalLinkNode, string> _unresolvedLinks = new Dictionary<InternalLinkNode, string>();
        private readonly Dictionary<string, AdressNode> _adresses = new Dictionary<string, AdressNode>();

        private readonly ExtensibleFormatCatalog<Media.Contracts.Dash.IMediaInput> _mediaCatalog = new ExtensibleFormatCatalog<Media.Contracts.Dash.IMediaInput>();
        private readonly IReadOnlyCollection<Media.Contracts.Dash.IMediaInput> _readOnlyCollection;

        IEnumerable<IMediaInput> IDocumentInput.MediaInputs => _readOnlyCollection;
        IReadOnlyCollection<IMediaInput> IExtensibleDocumentInput.MediaInputs => _readOnlyCollection;
        IEnumerable<Media.Contracts.Dash.IMediaInput> IDocumentInput<Media.Contracts.Dash.IMediaInput, string>.MediaInputs => _readOnlyCollection;
        public ICollection<Media.Contracts.Dash.IMediaInput> MediaInputs => _mediaCatalog;

        public IEnumerable<Type> ExtensionTypes => _mediaCatalog.ExtensionTypes;
        public void Extend(CompositionContext context) => _mediaCatalog.Extend(context);
        public void ResetToVanilla() => _mediaCatalog.ResetToVanilla();

        public DashInput()
        {
            _readOnlyCollection = new ReadOnlyFormatCatalog<Media.Contracts.Dash.IMediaInput>(_mediaCatalog);
        }

        public IDocumentNode Convert(string input)
        {
            var inputStream = new AntlrInputStream(input);

            var lexer = new DashLexer(inputStream);
            var tokens = new CommonTokenStream(lexer);

            var parser = new DashParser(tokens);
            DashParser.ParseContext context = parser.TwoStageParsing(x => x.parse());

            return new DashInput().Visit(context);
        }

        public override IDocumentNode VisitParse(DashParser.ParseContext context)
        {
            var node = new DocumentNode();

            DashParser.DocumentTitleContext[] documentTitles = context.GetRuleContexts<DashParser.DocumentTitleContext>();
            node.MainTitles.AddRange(documentTitles.Select(x => x.Accept(this)).Cast<LineNode>());

            node.Children.AddRange(context.GetRules().Skip(documentTitles.Length).Select(x => x.Accept(this)).NotNulls().DumpCollectionNodes().TransformInlineMedia().Cast<DocumentNode.IChild>());

            return node;
        }

        public override IDocumentNode VisitDocumentTitle(DashParser.DocumentTitleContext context)
        {
            return context.line().Accept(this);
        }

        public override IDocumentNode VisitHeaderMode(DashParser.HeaderModeContext context)
        {
            _paragraphMode = (context.headerModeContent()?.Accept(this) as ValueNode<string>)?.Value;
            _titleMode = null;
            return null;
        }

        public override IDocumentNode VisitHeaderModeContent(DashParser.HeaderModeContentContext context)
        {
            return new ValueNode<string>(context.GetText());
        }

        public override IDocumentNode VisitTitleHeaderMode(DashParser.TitleHeaderModeContext context)
        {
            _titleMode = context.HEADER_MODE_TITLE().GetText().Length;
            _paragraphMode = null;
            return null;
        }

        public override IDocumentNode VisitModeClose(DashParser.ModeCloseContext context)
        {
            _paragraphMode = null;
            _titleMode = null;
            return null;
        }

        public override IDocumentNode VisitParagraph(DashParser.ParagraphContext context)
        {
            IEnumerable<IDocumentNode> content;
            IEnumerable<ParserRuleContext> rules = context.GetRules();

            DashParser.TitleHeaderContext titleHeader = context.titleHeader();
            if (titleHeader != null)
            {
                int titleLevel = ((ValueNode<int>)titleHeader.Accept(this)).Value;
                content = rules.Skip(1).Select(x => x.Accept(this)).NotNulls();
                var titleNode = new TitleNode
                {
                    Level = titleLevel
                };
                titleNode.Children.AddRange(content.Cast<TitleNode.IChild>());
                return titleNode;
            }

            DashParser.HeaderContext header = context.header();
            if (header != null)
            {
                rules = rules.Skip(1);
                _paragraphMode = null;
                _titleMode = null;
            }

            content = rules.Select(x => x.Accept(this)).NotNulls();

            IDocumentNode node;
            if (header != null)
            {
                var paragraphNode = new ParagraphNode
                {
                    Class = (header.Accept(this) as ValueNode<string>)?.Value
                };
                paragraphNode.Children.AddRange(content.Cast<ParagraphNode.IChild>());
                node = paragraphNode;
            }
            else if (_paragraphMode != null)
            {
                var paragraphNode = new ParagraphNode
                {
                    Class = _paragraphMode
                };
                paragraphNode.Children.AddRange(content.Cast<ParagraphNode.IChild>());
                node = paragraphNode;
            }
            else if (_titleMode != null)
            {
                var titleNode = new TitleNode
                {
                    Level = _titleMode.Value
                };
                titleNode.Children.AddRange(content.Cast<TitleNode.IChild>());
                node = titleNode;
            }
            else
            {
                var paragraphNode = new ParagraphNode();
                paragraphNode.Children.AddRange(content.Cast<ParagraphNode.IChild>());
                node = paragraphNode;
            }
            
            return node;
        }

        public override IDocumentNode VisitTitleHeader(DashParser.TitleHeaderContext context)
        {
            return new ValueNode<int>(context.HEADER_TITLE().GetText().Length);
        }

        public override IDocumentNode VisitHeader(DashParser.HeaderContext context)
        {
            return context.headerContent().Accept(this);
        }

        public override IDocumentNode VisitHeaderContent(DashParser.HeaderContentContext context)
        {
            return new ValueNode<string>(context?.GetText() ?? "");
        }

        public override IDocumentNode VisitList(DashParser.ListContext context)
        {
            return context.GetRule(0).Accept(this);
        }

        public override IDocumentNode VisitListBulleted(DashParser.ListBulletedContext context)
        {
            return VisitListProcess(context.line(0), context.GetRules().Skip(1));
        }

        public override IDocumentNode VisitListOrdered(DashParser.ListOrderedContext context)
        {
            return VisitListProcess(context.line(0), context.GetRules().Skip(1), ordered: true);
        }

        public override IDocumentNode VisitSublistBulleted(DashParser.SublistBulletedContext context)
        {
            return VisitListProcess(context.line(0), context.GetRules().Skip(1));
        }

        public override IDocumentNode VisitSublistOrdered(DashParser.SublistOrderedContext context)
        {
            return VisitListProcess(context.line(0), context.GetRules().Skip(1), ordered: true);
        }

        private IDocumentNode VisitListProcess(DashParser.LineContext firstLine, IEnumerable<ParserRuleContext> others, bool ordered = false)
        {
            var node = new ListNode { Ordered = ordered };
            var lastItem = new ListItemNode { Line = (LineNode)firstLine.Accept(this) };
            node.Children.Add(lastItem);

            foreach (ParserRuleContext rule in others)
            {
                if (rule is DashParser.LineContext)
                {
                    lastItem = new ListItemNode { Line = (LineNode)rule.Accept(this) };
                    node.Children.Add(lastItem);
                }
                else
                {
                    lastItem.Sublist = (ListNode)rule.Accept(this);
                }
            }

            return node;
        }

        public override IDocumentNode VisitLine(DashParser.LineContext context)
        {
            var node = new LineNode();
            node.Children.AddRange(context.GetRules().Select(x => x.Accept(this)).DumpCollectionNodes().Cast<LineNode.IChild>());
            return node;
        }

        public override IDocumentNode VisitLink(DashParser.LinkContext context)
        {
            ParentNodeBase<LineNode.IChild> node = ResolveLink(((ValueNode<string>)context.linkContent().Accept(this)).Value);
            node.Children.AddRange(context.linkLine().Accept(this).Children.DumpCollectionNodes().Cast<LineNode.IChild>());
            return node;
        }

        public override IDocumentNode VisitLinkLine(DashParser.LinkLineContext context)
        {
            var node = new LineNode();
            node.Children.AddRange(context.GetRules().Select(x => x.Accept(this)).DumpCollectionNodes().Cast<LineNode.IChild>());
            return node;
        }

        public override IDocumentNode VisitLinkContent(DashParser.LinkContentContext context)
        {
            return new ValueNode<string>(context.GetText());
        }

        public override IDocumentNode VisitDirectLink(DashParser.DirectLinkContext context)
        {
            var textNode = (TextNode)context.directLinkContent().Accept(this);
            ParentNodeBase<LineNode.IChild> node = ResolveLink(textNode.Content);
            node.Children.Add(textNode);
            return node;
        }

        public override IDocumentNode VisitDirectLinkContent(DashParser.DirectLinkContentContext context)
        {
            return new TextNode { Content = context.GetText() };
        }

        private ParentNodeBase<LineNode.IChild> ResolveLink(string adress)
        {
            if (adress.ContainsAny('.', '/', '\\'))
                return new ExternalLinkNode { Adress = adress };

            var internalLinkNode = new InternalLinkNode
            {
                AdressNode = _adresses.FirstOrDefault(x => adress.Equals(x.Key, StringComparison.OrdinalIgnoreCase)).Value,
                AdressByDefault = adress
            };
            if (internalLinkNode.AdressNode == null)
                _unresolvedLinks.Add(internalLinkNode, adress);

            return internalLinkNode;
        }

        public override IDocumentNode VisitAddress(DashParser.AddressContext context)
        {
            var node = new AdressNode();
            node.Names.AddRange(context.addressContent().Select(x => x.Accept(this)).Cast<ValueNode<string>>().Select(x => x.Value));

            foreach (string name in node.Names)
            {
                _adresses[name] = node;

                InternalLinkNode[] links = _unresolvedLinks.Where(x => name.Equals(x.Value, StringComparison.OrdinalIgnoreCase)).Select(x => x.Key).ToArray();
                foreach (InternalLinkNode link in links)
                {
                    link.AdressNode = node;
                    _unresolvedLinks.Remove(link);
                }
            }

            return node;
        }

        public override IDocumentNode VisitAddressContent(DashParser.AddressContentContext context)
        {
            return new ValueNode<string>(context.GetText());
        }

        public override IDocumentNode VisitReference(DashParser.ReferenceContext context)
        {
            var node = new ReferenceNode();
            node.Children.AddRange(context.linkLine().Accept(this).Children.DumpCollectionNodes().Cast<LineNode.IChild>());

            int? index = ((ValueNode<int?>)context.referenceNumber().Accept(this)).Value;
            if (index == null)
                _referenciesQueue.Enqueue(node);
            else
            {
                if (!_indexedReferencies.TryGetValue(index.Value, out List<ReferenceNode> list))
                    _indexedReferencies[index.Value] = list = new List<ReferenceNode>();
                list.Add(node);
            }

            return node;
        }

        public override IDocumentNode VisitReferenceNumber(DashParser.ReferenceNumberContext context)
        {
            return new ValueNode<int?>(int.TryParse(context.GetText(), out int number) ? (int?)number : null);
        }

        public override IDocumentNode VisitNote(DashParser.NoteContext context)
        {
            var node = new NoteNode { Line = (LineNode)context.line().Accept(this) };

            int? index = ((ValueNode<int?>)context.noteNumber().Accept(this)).Value;
            if (index == null)
                _referenciesQueue.Dequeue().Note = node;
            else
                foreach (ReferenceNode reference in _indexedReferencies[index.Value])
                    reference.Note = node;

            return node;
        }

        public override IDocumentNode VisitNoteNumber(DashParser.NoteNumberContext context)
        {
            return new ValueNode<int?>(int.TryParse(context.GetText(), out int number) ? (int?)number : null);
        }

        public override IDocumentNode VisitRedirection(DashParser.RedirectionContext context)
        {
            var node = new NoteNode { Line = new LineNode { Children = { (LineNode.IChild)context.directLinkContent().Accept(this) } } };

            int? index = ((ValueNode<int?>)context.noteNumber().Accept(this)).Value;
            if (index == null)
                _referenciesQueue.Dequeue().Note = node;
            else
                foreach (ReferenceNode reference in _indexedReferencies[index.Value])
                    reference.Note = node;

            return node;
        }

        public override IDocumentNode VisitEmphasisLine(DashParser.EmphasisLineContext context)
        {
            var node = new LineNode();
            node.Children.AddRange(context.GetRules().Select(x => x.Accept(this)).DumpCollectionNodes().Cast<LineNode.IChild>());
            return node;
        }

        public override IDocumentNode VisitBold(DashParser.BoldContext context)
        {
            var node = new BoldNode();
            node.Children.AddRange(context.emphasisLine().Accept(this).Children.DumpCollectionNodes().Cast<LineNode.IChild>());
            return node;
        }

        public override IDocumentNode VisitItalic(DashParser.ItalicContext context)
        {
            var node = new ItalicNode();
            node.Children.AddRange(context.emphasisLine().Accept(this).Children.DumpCollectionNodes().Cast<LineNode.IChild>());
            return node;
        }

        public override IDocumentNode VisitMark(DashParser.MarkContext context)
        {
            var node = new MarkNode();
            node.Children.AddRange(context.emphasisLine().Accept(this).Children.DumpCollectionNodes().Cast<LineNode.IChild>());
            return node;
        }

        public override IDocumentNode VisitObsolete(DashParser.ObsoleteContext context)
        {
            var node = new ObsoleteNode();
            node.Children.AddRange(context.emphasisLine().Accept(this).Children.DumpCollectionNodes().Cast<LineNode.IChild>());
            return node;
        }

        public override IDocumentNode VisitEmphasis(DashParser.EmphasisContext context)
        {
            var node = new EmphasisNode { Class = ((ValueNode<string>)context.headerContent().Accept(this)).Value };
            node.Children.AddRange(context.emphasisLine().Accept(this).Children.DumpCollectionNodes().Cast<LineNode.IChild>());
            return node;
        }

        public override IDocumentNode VisitText(DashParser.TextContext context)
        {
            return new TextNode { Content = context.GetText() };
        }

        public override IDocumentNode VisitOthers(DashParser.OthersContext context)
        {
            return new TextNode { Content = context.GetText() };
        }

        public override IDocumentNode VisitEmphasisOthers(DashParser.EmphasisOthersContext context)
        {
            return new TextNode { Content = context.GetText() };
        }

        public override IDocumentNode VisitLinkOthers(DashParser.LinkOthersContext context)
        {
            return new TextNode { Content = context.GetText() };
        }

        public override IDocumentNode VisitExtensionMode(DashParser.ExtensionModeContext context)
        {
            string extension = ((ValueNode<string>)context.extensionModeExtension()?.Accept(this))?.Value;
            string content = ((ValueNode<string>)context.extensionModeContent().Accept(this)).Value;
            return HandleMedia<MediaNode>(context, extension, content);
        }

        public override IDocumentNode VisitExtensionModeExtension(DashParser.ExtensionModeExtensionContext context)
        {
            return new ValueNode<string>(context.GetText());
        }

        public override IDocumentNode VisitExtensionModeContent(DashParser.ExtensionModeContentContext context)
        {
            return new ValueNode<string>(context.GetText());
        }

        public override IDocumentNode VisitDashExtensionMode(DashParser.DashExtensionModeContext context)
        {
            string content = ((ValueNode<string>)context.dashExtensionModeContent().Accept(this)).Value;
            return HandleMedia<MediaNode>(context, Dast.FileExtensions.Text.Dash.Main, content);
        }

        public override IDocumentNode VisitDashExtensionModeContent(DashParser.DashExtensionModeContentContext context)
        {
            return new ValueNode<string>(context.GetText());
        }

        public override IDocumentNode VisitMedia(DashParser.MediaContext context)
        {
            string extension = ((ValueNode<string>)context.mediaExtension()?.Accept(this))?.Value;
            string content = ((ValueNode<string>)context.mediaContent().Accept(this)).Value;
            return HandleMedia<MediaInlineNode>(context, extension, content);
        }

        public override IDocumentNode VisitMediaExtension(DashParser.MediaExtensionContext context)
        {
            return new ValueNode<string>(context.GetText());
        }

        public override IDocumentNode VisitMediaContent(DashParser.MediaContentContext context)
        {
            return new ValueNode<string>(context.GetText());
        }

        static private IDocumentNode HandleMedia<TMediaNode>(ParserRuleContext context, string extension, string content)
            where TMediaNode : MediaNodeBase, new()
        {
            MediaType? type = null;
            TMediaNode mediaNode = null;
            CollectionNode collectionNode = null;
            foreach (ITerminalNode terminalNode in context.children.OfType<ITerminalNode>())
            {
                switch (terminalNode.Symbol.Type)
                {
                    case DashParser.EXTENSION_MODE_PLUS:
                        type = MediaType.Visual;
                        break;
                    case DashParser.EXTENSION_MODE_MINUS:
                        type = MediaType.Code;
                        break;
                }

                if (type == null)
                    break;

                if (mediaNode != null && collectionNode == null)
                {
                    collectionNode = new CollectionNode();
                    collectionNode.Items.Add(mediaNode);
                }

                mediaNode = new TMediaNode
                {
                    Extension = extension,
                    Content = content,
                    Type = type
                };

                collectionNode?.Items.Add(mediaNode);
                type = null;
            }

            if (collectionNode != null)
                return collectionNode;

            if (mediaNode != null)
                return mediaNode;

            return new TMediaNode
            {
                Extension = extension,
                Content = content
            };
        }

        public override IDocumentNode VisitCommentBlock(DashParser.CommentBlockContext context)
        {
            return new CommentNode
            {
                Content = ((ValueNode<string>)context.commentBlockContent().Accept(this)).Value,
                Inline = false
            };
        }

        public override IDocumentNode VisitCommentBlockContent(DashParser.CommentBlockContentContext context)
        {
            return new ValueNode<string>(context.GetText());
        }

        public override IDocumentNode VisitCommentInline(DashParser.CommentInlineContext context)
        {
            return new CommentNode
            {
                Content = ((ValueNode<string>)context.commentInlineContent().Accept(this)).Value,
                Inline = true
            };
        }

        public override IDocumentNode VisitCommentInlineContent(DashParser.CommentInlineContentContext context)
        {
            return new ValueNode<string>(context.GetText());
        }

        public override IDocumentNode VisitChildren(IRuleNode node)
        {
            return null;
        }

        public override IDocumentNode VisitTerminal(ITerminalNode node)
        {
            return null;
        }

        public class ValueNode<T> : IDocumentNode
        {
            public T Value { get; }
            public IEnumerable<IDocumentNode> Children { get { yield break; } }

            public ValueNode(T value)
            {
                Value = value;
            }

            public string Accept(IDocumentVisitor visitor) => throw new InvalidOperationException();
        }

        public class CollectionNode : IDocumentNode
        {
            public List<IDocumentNode> Items { get; } = new List<IDocumentNode>();
            public IEnumerable<IDocumentNode> Children { get { yield break; } }
            
            public string Accept(IDocumentVisitor visitor) => throw new InvalidOperationException();
        }
    }
}