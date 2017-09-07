using System.Collections.Generic;
using System.Linq;

namespace Dast.Outputs.Base
{
    public abstract class DocumentOutputBase<TMedia> : IDocumentVisitor, IDocumentOutput<TMedia, string>
        where TMedia : IMediaOutput
    {
        private readonly List<NoteNode> _notes = new List<NoteNode>();

        public abstract string DisplayName { get; }
        public abstract FileExtension FileExtension { get; }
        protected abstract IEnumerable<TMedia> MediaOutputs { get; }
        
        IEnumerable<FileExtension> IFormat.FileExtensions { get { yield return FileExtension; } }
        IEnumerable<TMedia> IDocumentOutput<TMedia, string>.MediaOutputs => MediaOutputs;
        IEnumerable<IMediaOutput> IDocumentOutput.MediaOutputs => MediaOutputs.Cast<IMediaOutput>();

        public string Convert(IDocumentNode node)
        {
            return node?.Accept(this) ?? "";
        }

        public string VisitReference(ReferenceNode node)
        {
            int index = _notes.IndexOf(node.Note);
            if (index == -1)
            {
                _notes.Add(node.Note);
                index = _notes.Count;
            }
            else
                index++;

            return VisitReference(node, index);
        }

        public string VisitNote(NoteNode node)
        {
            return VisitNote(node, _notes.IndexOf(node) + 1);
        }

        public abstract string VisitDocument(DocumentNode node);
        public abstract string VisitParagraph(ParagraphNode node);
        public abstract string VisitTitle(TitleNode node);
        public abstract string VisitList(ListNode node);
        public abstract string VisitListItem(ListItemNode node);
        public abstract string VisitLine(LineNode node);
        public abstract string VisitInternalLink(InternalLinkNode node);
        public abstract string VisitExternalLink(ExternalLinkNode node);
        public abstract string VisitAdress(AdressNode node);
        protected abstract string VisitReference(ReferenceNode node, int index);
        protected abstract string VisitNote(NoteNode node, int index);
        public abstract string VisitBold(BoldNode node);
        public abstract string VisitItalic(ItalicNode node);
        public abstract string VisitMark(MarkNode node);
        public abstract string VisitObsolete(ObsoleteNode node);
        public abstract string VisitEmphasis(EmphasisNode node);
        public abstract string VisitText(TextNode node);
        public abstract string VisitMedia(MediaNode node);
        public abstract string VisitMediaInline(MediaInlineNode node);
        public abstract string VisitComment(CommentNode node);
    }
}