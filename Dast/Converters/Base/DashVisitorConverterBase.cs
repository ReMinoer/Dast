using System.Collections.Generic;
using Dast.Converters.Utils;

namespace Dast.Converters.Base
{
    public abstract class DashVisitorConverterBase : IDocumentVisitor, IDocumentConverter
    {
        private readonly List<NoteNode> _notes = new List<NoteNode>();
        public abstract FileExtension FileExtension { get; }

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
        public abstract string VisitReference(ReferenceNode node, int index);
        public abstract string VisitNote(NoteNode node, int index);
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