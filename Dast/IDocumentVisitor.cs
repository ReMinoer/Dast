namespace Dast
{
    public interface IDocumentVisitor
    {
        string VisitDocument(DocumentNode node);
        string VisitParagraph(ParagraphNode node);
        string VisitTitle(TitleNode node);
        string VisitList(ListNode node);
        string VisitListItem(ListItemNode node);
        string VisitLine(LineNode node);
        string VisitLink(LinkNode node);
        string VisitAdress(AdressNode node);
        string VisitBold(BoldNode node);
        string VisitItalic(ItalicNode node);
        string VisitMark(MarkNode node);
        string VisitObsolete(ObsoleteNode node);
        string VisitEmphasis(EmphasisNode node);
        string VisitText(TextNode node);
        string VisitMedia(MediaNode node);
        string VisitMediaInline(MediaInlineNode node);
        string VisitComment(CommentNode node);
    }
}