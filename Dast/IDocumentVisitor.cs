namespace Dast
{
    public interface IDocumentVisitor
    {
        void VisitDocument(DocumentNode node);
        void VisitParagraph(ParagraphNode node);
        void VisitTitle(TitleNode node);
        void VisitList(ListNode node);
        void VisitListItem(ListItemNode node);
        void VisitLine(LineNode node);
        void VisitLink(LinkNode node);
        void VisitAddress(AddressNode node);
        void VisitReference(ReferenceNode node);
        void VisitNote(NoteNode node);
        void VisitBold(BoldNode node);
        void VisitItalic(ItalicNode node);
        void VisitQuote(QuoteNode node);
        void VisitObsolete(ObsoleteNode node);
        void VisitEmphasis(EmphasisNode node);
        void VisitText(TextNode node);
        void VisitMedia(MediaNode node);
        void VisitMediaInline(MediaInlineNode node);
        void VisitComment(CommentNode node);
    }
}