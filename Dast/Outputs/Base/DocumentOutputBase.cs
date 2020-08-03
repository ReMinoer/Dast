using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dast.Outputs.Base
{
    public abstract class DocumentOutputBase<TMedia, TOutput> : IDocumentVisitor, IDocumentOutput<TMedia, TOutput>
        where TMedia : IMediaOutput
    {
        private readonly List<NoteNode> _notes = new List<NoteNode>();

        public abstract string DisplayName { get; }
        public abstract FileExtension FileExtension { get; }
        protected abstract IEnumerable<TMedia> MediaOutputs { get; }
        
        IEnumerable<FileExtension> IFormat.FileExtensions { get { yield return FileExtension; } }
        IEnumerable<TMedia> IDocumentOutput<TMedia, TOutput>.MediaOutputs => MediaOutputs;
        IEnumerable<IMediaOutput> IDocumentOutput.MediaOutputs => MediaOutputs.Cast<IMediaOutput>();

        public abstract TOutput Convert(IDocumentNode node);

        private readonly Dictionary<Type, TMedia> _usedMediaConverters = new Dictionary<Type, TMedia>();
        public IEnumerable<TMedia> UsedMediaConverters => _usedMediaConverters.Values;

        protected virtual bool RegisterUsedMediaConverter(TMedia usedMediaConverter)
        {
            var converterType = usedMediaConverter.GetType();
            if (_usedMediaConverters.ContainsKey(converterType))
                return false;

            _usedMediaConverters.Add(converterType, usedMediaConverter);
            return true;
        }

        public virtual async Task GetResourceFilesAsync(string outputDirectory)
        {
            await Task.WhenAll(_usedMediaConverters.Values.Select(x => x.GetResourceFilesAsync(outputDirectory)));
        }

        public void VisitReference(ReferenceNode node)
        {
            int index = -1;
            if (!node.IsInlined)
            {
                index = _notes.IndexOf(node.NoteNode);
                if (index == -1)
                {
                    _notes.Add(node.NoteNode);
                    index = _notes.Count;
                }
                else
                    index++;
            }

            VisitReference(node, index);
        }

        public void VisitNote(NoteNode node)
        {
            VisitNote(node, _notes.IndexOf(node) + 1);
        }

        public abstract void VisitDocument(DocumentNode node);
        public abstract void VisitParagraph(ParagraphNode node);
        public abstract void VisitTitle(TitleNode node);
        public abstract void VisitList(ListNode node);
        public abstract void VisitListItem(ListItemNode node);
        public abstract void VisitLine(LineNode node);
        public abstract void VisitLink(LinkNode node);
        public abstract void VisitAddress(AddressNode node);
        protected abstract void VisitReference(ReferenceNode node, int index);
        protected abstract void VisitNote(NoteNode node, int index);
        public abstract void VisitBold(BoldNode node);
        public abstract void VisitItalic(ItalicNode node);
        public abstract void VisitQuote(QuoteNode node);
        public abstract void VisitObsolete(ObsoleteNode node);
        public abstract void VisitEmphasis(EmphasisNode node);
        public abstract void VisitText(TextNode node);
        public abstract void VisitMedia(MediaNode node);
        public abstract void VisitMediaInline(MediaInlineNode node);
        public abstract void VisitComment(CommentNode node);
    }
}