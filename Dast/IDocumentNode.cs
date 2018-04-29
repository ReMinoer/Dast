using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dast
{
    public interface IDocumentNode
    {
        void Accept(IDocumentVisitor visitor);
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
        public abstract void Accept(IDocumentVisitor visitor);
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
            _readOnlyChildren = new ReadOnlyCollection<TChild>(Children);
        }

        public abstract void Accept(IDocumentVisitor visitor);
    }

    public class DocumentNode : ParentNodeBase<DocumentNode.IChild>
    {
        public List<LineNode> MainTitles { get; } = new List<LineNode>();

        public interface IChild : IDocumentNode { }
        public override void Accept(IDocumentVisitor visitor) => visitor.VisitDocument(this);
    }

    public class ParagraphNode : ParentNodeBase<ParagraphNode.IChild>, DocumentNode.IChild
    {
        public string Class { get; set; }

        public interface IChild : IDocumentNode { }
        public override void Accept(IDocumentVisitor visitor) => visitor.VisitParagraph(this);
    }

    public class TitleNode : ParentNodeBase<TitleNode.IChild>, DocumentNode.IChild
    {
        public int Level { get; set; }

        public interface IChild : IDocumentNode { }
        public override void Accept(IDocumentVisitor visitor) => visitor.VisitTitle(this);
    }

    public class ListNode : ParentNodeBase<ListNode.IChild>, ParagraphNode.IChild, TitleNode.IChild, ListItemNode.IChild
    {
        public bool Ordered { get; set; }

        public interface IChild : IDocumentNode { }
        public override void Accept(IDocumentVisitor visitor) => visitor.VisitList(this);
    }

    public class ListItemNode : ListNode.IChild
    {
        public LineNode Line { get; set; }
        public ListNode Sublist { get; set; }
        public bool Important { get; set; }

        public IEnumerable<IDocumentNode> Children
        {
            get
            {
                yield return Line;
                yield return Sublist;
            }
        }

        public interface IChild : IDocumentNode { }
        public void Accept(IDocumentVisitor visitor) => visitor.VisitListItem(this);
    }

    public class LineNode : ParentNodeBase<LineNode.IChild>, ParagraphNode.IChild, TitleNode.IChild, ListItemNode.IChild
    {
        public interface IChild : IDocumentNode { }
        public override void Accept(IDocumentVisitor visitor) => visitor.VisitLine(this);
    }

    public class InternalLinkNode : ParentNodeBase<LineNode.IChild>, LineNode.IChild
    {
        public AdressNode AdressNode { get; set; }
        public string AdressByDefault { get; set; }
        public override void Accept(IDocumentVisitor visitor) => visitor.VisitInternalLink(this);
    }

    public class ExternalLinkNode : ParentNodeBase<LineNode.IChild>, LineNode.IChild
    {
        public string Adress { get; set; }
        public override void Accept(IDocumentVisitor visitor) => visitor.VisitExternalLink(this);
    }

    public class AdressNode : LeafNodeBase, LineNode.IChild
    {
        public List<string> Names { get; } = new List<string>();
        public override void Accept(IDocumentVisitor visitor) => visitor.VisitAdress(this);
    }

    public class ReferenceNode : ParentNodeBase<LineNode.IChild>, LineNode.IChild
    {
        public NoteNode Note { get; set; }
        public override void Accept(IDocumentVisitor visitor) => visitor.VisitReference(this);
    }

    public class NoteNode : DocumentNode.IChild
    {
        public LineNode Line { get; set; }
        public IEnumerable<IDocumentNode> Children { get { yield return Line; } }
        public void Accept(IDocumentVisitor visitor) => visitor.VisitNote(this);
    }

    public class BoldNode : ParentNodeBase<LineNode.IChild>, LineNode.IChild
    {
        public override void Accept(IDocumentVisitor visitor) => visitor.VisitBold(this);
    }

    public class ItalicNode : ParentNodeBase<LineNode.IChild>, LineNode.IChild
    {
        public override void Accept(IDocumentVisitor visitor) => visitor.VisitItalic(this);
    }

    public class MarkNode : ParentNodeBase<LineNode.IChild>, LineNode.IChild
    {
        public override void Accept(IDocumentVisitor visitor) => visitor.VisitMark(this);
    }

    public class ObsoleteNode : ParentNodeBase<LineNode.IChild>, LineNode.IChild
    {
        public override void Accept(IDocumentVisitor visitor) => visitor.VisitObsolete(this);
    }

    public class EmphasisNode : ParentNodeBase<LineNode.IChild>, LineNode.IChild
    {
        public string Class { get; set; }
        public override void Accept(IDocumentVisitor visitor) => visitor.VisitEmphasis(this);
    }

    public class TextNode : LeafNodeBase, LineNode.IChild
    {
        public string Content { get; set; }
        public override void Accept(IDocumentVisitor visitor) => visitor.VisitText(this);
    }

    public enum MediaType
    {
        Code,
        Visual
    }

    public abstract class MediaNodeBase : LeafNodeBase
    {
        private string _extension;

        public string Extension
        {
            get => _extension;
            set => _extension = value ?? "";
        }

        public string Content { get; set; }
        public MediaType? Type { get; set; }
    }

    public class MediaNode : MediaNodeBase, DocumentNode.IChild
    {
        public override void Accept(IDocumentVisitor visitor) => visitor.VisitMedia(this);
    }

    public class MediaInlineNode : MediaNodeBase, LineNode.IChild
    {
        public override void Accept(IDocumentVisitor visitor) => visitor.VisitMediaInline(this);
    }

    public class CommentNode : LeafNodeBase, DocumentNode.IChild, ParagraphNode.IChild, LineNode.IChild
    {
        public bool Inline { get; set; }
        public string Content { get; set; }
        public override void Accept(IDocumentVisitor visitor) => visitor.VisitComment(this);
    }
}