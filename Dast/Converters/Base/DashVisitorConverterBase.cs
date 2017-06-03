using System.Collections.Generic;

namespace Dast.Converters.Base
{
    public abstract class DashVisitorConverterBase : IDocumentVisitor, IDocumentConverter
    {
        public abstract IEnumerable<string> FileExtensions { get; }

        public string Convert(IDocumentNode node)
        {
            return node?.Accept(this) ?? "";
        }

        public abstract string VisitDocument(DocumentNode node);
        public abstract string VisitParagraph(ParagraphNode node);
        public abstract string VisitTitle(TitleNode node);
        public abstract string VisitList(ListNode node);
        public abstract string VisitListItem(ListItemNode node);
        public abstract string VisitLine(LineNode node);
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