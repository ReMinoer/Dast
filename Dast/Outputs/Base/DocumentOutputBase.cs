using System.Collections.Generic;
using System.Linq;

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

        public void VisitReference(ReferenceNode node)
        {
            int index = _notes.IndexOf(node.Note);
            if (index == -1)
            {
                _notes.Add(node.Note);
                index = _notes.Count;
            }
            else
                index++;

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
        public abstract void VisitInternalLink(InternalLinkNode node);
        public abstract void VisitExternalLink(ExternalLinkNode node);
        public abstract void VisitAdress(AdressNode node);
        protected abstract void VisitReference(ReferenceNode node, int index);
        protected abstract void VisitNote(NoteNode node, int index);
        public abstract void VisitBold(BoldNode node);
        public abstract void VisitItalic(ItalicNode node);
        public abstract void VisitMark(MarkNode node);
        public abstract void VisitObsolete(ObsoleteNode node);
        public abstract void VisitEmphasis(EmphasisNode node);
        public abstract void VisitText(TextNode node);
        public abstract void VisitMedia(MediaNode node);
        public abstract void VisitMediaInline(MediaInlineNode node);
        public abstract void VisitComment(CommentNode node);
    }
}