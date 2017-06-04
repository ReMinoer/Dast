using System.Collections.Generic;
using System.Linq;

namespace Dast
{
    public interface IDocumentNode
    {
        string Accept(IDocumentVisitor visitor);
        IEnumerable<IDocumentNode> Children { get; }
    }

    public interface IDocumentNode<out TChild> : IDocumentNode
        where TChild : IDocumentNode
    {
        new IEnumerable<TChild> Children { get; }
    }

    public abstract class LeafNodeBase : IDocumentNode
    {
        public IEnumerable<IDocumentNode> Children { get { yield break; } }
        public abstract string Accept(IDocumentVisitor visitor);
    }

    public abstract class ParentNodeBase<TChild> : IDocumentNode<TChild>
        where TChild : IDocumentNode
    {
        private readonly IEnumerable<TChild> _readOnlyChildren;
        public List<TChild> Children { get; } = new List<TChild>();
        IEnumerable<TChild> IDocumentNode<TChild>.Children => _readOnlyChildren;
        IEnumerable<IDocumentNode> IDocumentNode.Children => _readOnlyChildren.Cast<IDocumentNode>();

        protected ParentNodeBase()
        {
            _readOnlyChildren = Children.AsReadOnly();
        }

        public abstract string Accept(IDocumentVisitor visitor);
    }

    public class DocumentNode : ParentNodeBase<DocumentNode.IChild>
    {
        public List<LineNode> MainTitles { get; } = new List<LineNode>();

        public interface IChild : IDocumentNode { }
        public override string Accept(IDocumentVisitor visitor) => visitor.VisitDocument(this);
    }

    public class ParagraphNode : ParentNodeBase<ParagraphNode.IChild>, DocumentNode.IChild
    {
        public string Class { get; set; }

        public interface IChild : IDocumentNode { }
        public override string Accept(IDocumentVisitor visitor) => visitor.VisitParagraph(this);
    }

    public class TitleNode : ParentNodeBase<TitleNode.IChild>, DocumentNode.IChild
    {
        public int Level { get; set; }

        public interface IChild : IDocumentNode { }
        public override string Accept(IDocumentVisitor visitor) => visitor.VisitTitle(this);
    }

    public class ListNode : ParentNodeBase<ListNode.IChild>, ParagraphNode.IChild, TitleNode.IChild, ListItemNode.IChild
    {
        public bool Ordered { get; set; }

        public interface IChild : IDocumentNode { }
        public override string Accept(IDocumentVisitor visitor) => visitor.VisitList(this);
    }

    public class ListItemNode : ListNode.IChild
    {
        public LineNode Line { get; set; }
        public ListNode Sublist { get; set; }

        public IEnumerable<IDocumentNode> Children
        {
            get
            {
                yield return Line;
                yield return Sublist;
            }
        }

        public interface IChild : IDocumentNode { }
        public string Accept(IDocumentVisitor visitor) => visitor.VisitListItem(this);
    }

    public class LineNode : ParentNodeBase<LineNode.IChild>, ParagraphNode.IChild, TitleNode.IChild, ListItemNode.IChild
    {
        public interface IChild : IDocumentNode { }
        public override string Accept(IDocumentVisitor visitor) => visitor.VisitLine(this);
    }

    public class BoldNode : ParentNodeBase<LineNode.IChild>, LineNode.IChild
    {
        public override string Accept(IDocumentVisitor visitor) => visitor.VisitBold(this);
    }

    public class ItalicNode : ParentNodeBase<LineNode.IChild>, LineNode.IChild
    {
        public override string Accept(IDocumentVisitor visitor) => visitor.VisitItalic(this);
    }

    public class MarkNode : ParentNodeBase<LineNode.IChild>, LineNode.IChild
    {
        public override string Accept(IDocumentVisitor visitor) => visitor.VisitMark(this);
    }

    public class ObsoleteNode : ParentNodeBase<LineNode.IChild>, LineNode.IChild
    {
        public override string Accept(IDocumentVisitor visitor) => visitor.VisitObsolete(this);
    }

    public class EmphasisNode : ParentNodeBase<LineNode.IChild>, LineNode.IChild
    {
        public string Class { get; set; }
        public override string Accept(IDocumentVisitor visitor) => visitor.VisitEmphasis(this);
    }

    public class TextNode : LeafNodeBase, LineNode.IChild
    {
        public string Content { get; set; }
        public override string Accept(IDocumentVisitor visitor) => visitor.VisitText(this);
    }

    public enum MediaType
    {
        Code,
        Visual
    }

    public abstract class MediaNodeBase : LeafNodeBase
    {
        public string Extension { get; set; }
        public string Content { get; set; }
        public MediaType? Type { get; set; }
    }

    public class MediaNode : MediaNodeBase, DocumentNode.IChild
    {
        public override string Accept(IDocumentVisitor visitor) => visitor.VisitMedia(this);
    }

    public class MediaInlineNode : MediaNodeBase, LineNode.IChild
    {
        public override string Accept(IDocumentVisitor visitor) => visitor.VisitMediaInline(this);
    }

    public class CommentNode : LeafNodeBase, DocumentNode.IChild, ParagraphNode.IChild, LineNode.IChild
    {
        public bool Inline { get; set; }
        public string Content { get; set; }
        public override string Accept(IDocumentVisitor visitor) => visitor.VisitComment(this);
    }
}