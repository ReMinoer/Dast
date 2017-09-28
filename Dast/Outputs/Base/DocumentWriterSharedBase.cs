using System;
using System.Collections.Generic;
using System.IO;

namespace Dast.Outputs.Base
{
    public abstract class DocumentWriterSharedBase<TMedia, TOutput> : DocumentOutputBase<TMedia, TOutput>
        where TMedia : IMediaOutput
    {
        private readonly Stack<StringWriter> _dumpWriters = new Stack<StringWriter>();
        protected abstract TextWriter MainWriter { get; }
        private TextWriter Writer => _dumpWriters.Count > 0 ? _dumpWriters.Peek() : MainWriter;
        protected string NewLine => Writer.NewLine;

        protected void StartDump()
        {
            var dumpWriter = new StringWriter();
            _dumpWriters.Push(dumpWriter);
        }

        protected string StopDump()
        {
            StringWriter dumpWriter = _dumpWriters.Pop();
            string result = dumpWriter.ToString();
            dumpWriter.Dispose();
            return result;
        }

        protected void Write(string value)
        {
            Writer.Write(value);
        }

        protected void Write(IEnumerable<string> values)
        {
            foreach (string value in values)
                Writer.Write(value);
        }

        protected void Write(params string[] values)
        {
            foreach (string value in values)
                Writer.Write(value);
        }

        protected void Write(IDocumentNode node)
        {
            node.Accept(this);
        }

        protected void WriteLine()
        {
            Writer.WriteLine();
        }

        protected void WriteLine(string value)
        {
            Writer.WriteLine(value);
        }

        protected void WriteLine(IEnumerable<string> values)
        {
            foreach (string value in values)
                Writer.Write(value);
            Writer.WriteLine();
        }

        protected void WriteLine(params string[] values)
        {
            foreach (string value in values)
                Writer.Write(value);
            Writer.WriteLine();
        }

        protected void Join(IEnumerable<string> values, string betweenEach)
        {
            using (IEnumerator<string> enumerator = values.GetEnumerator())
            {
                if (!enumerator.MoveNext())
                    return;

                Writer.Write(enumerator.Current);

                while (enumerator.MoveNext())
                {
                    Writer.Write(betweenEach);
                    Writer.Write(enumerator.Current);
                }
            }
        }

        protected void Join(IEnumerable<IDocumentNode> nodes, string betweenEach)
        {
            using (IEnumerator<IDocumentNode> enumerator = nodes.GetEnumerator())
            {
                if (!enumerator.MoveNext())
                    return;

                enumerator.Current?.Accept(this);

                while (enumerator.MoveNext())
                {
                    Writer.Write(betweenEach);
                    enumerator.Current?.Accept(this);
                }
            }
        }

        protected void JoinChildren(IDocumentNode node, string betweenEach)
        {
            using (IEnumerator<IDocumentNode> enumerator = node.Children.GetEnumerator())
            {
                if (!enumerator.MoveNext())
                    return;

                enumerator.Current?.Accept(this);

                while (enumerator.MoveNext())
                {
                    Writer.Write(betweenEach);
                    enumerator.Current?.Accept(this);
                }
            }
        }

        protected void Aggregate(IEnumerable<string> values)
        {
            foreach (string value in values)
                Writer.Write(value);
        }

        protected void Aggregate(IEnumerable<IDocumentNode> nodes)
        {
            foreach (IDocumentNode node in nodes)
                node?.Accept(this);
        }

        protected void Aggregate(string beforeEach, IEnumerable<string> values)
        {
            foreach (string value in values)
            {
                Writer.Write(beforeEach);
                Writer.Write(value);
            }
        }

        protected void Aggregate(string beforeEach, IEnumerable<IDocumentNode> nodes)
        {
            foreach (IDocumentNode node in nodes)
            {
                Writer.Write(beforeEach);
                node?.Accept(this);
            }
        }

        protected void Aggregate(IDocumentNode node, string afterEach)
        {
            foreach (IDocumentNode child in node.Children)
            {
                child?.Accept(this);
                Writer.Write(afterEach);
            }
        }

        protected void Aggregate(IEnumerable<IDocumentNode> nodes, string afterEach)
        {
            foreach (IDocumentNode node in nodes)
            {
                node?.Accept(this);
                Writer.Write(afterEach);
            }
        }

        protected void Aggregate(string beforeEach, IEnumerable<string> values, string afterEach)
        {
            foreach (string value in values)
            {
                Writer.Write(beforeEach);
                Writer.Write(value);
                Writer.Write(afterEach);
            }
        }

        protected void Aggregate(string beforeEach, IEnumerable<IDocumentNode> nodes, string afterEach)
        {
            foreach (IDocumentNode node in nodes)
            {
                Writer.Write(beforeEach);
                node?.Accept(this);
                Writer.Write(afterEach);
            }
        }

        protected void Aggregate(Func<IDocumentNode, string> beforeEach, IEnumerable<IDocumentNode> nodes)
        {
            foreach (IDocumentNode node in nodes)
            {
                Writer.Write(beforeEach(node));
                node?.Accept(this);
            }
        }

        protected void Aggregate(IEnumerable<IDocumentNode> nodes, Func<IDocumentNode, string> afterEach)
        {
            foreach (IDocumentNode node in nodes)
            {
                node?.Accept(this);
                Writer.Write(afterEach(node));
            }
        }

        protected void Aggregate(Func<IDocumentNode, string> beforeEach, IEnumerable<IDocumentNode> nodes, Func<IDocumentNode, string> afterEach)
        {
            foreach (IDocumentNode node in nodes)
            {
                Writer.Write(beforeEach(node));
                node?.Accept(this);
                Writer.Write(afterEach(node));
            }
        }

        protected void Aggregate(Func<int, string> beforeEach, IEnumerable<IDocumentNode> nodes)
        {
            int i = 0;
            using (IEnumerator<IDocumentNode> enumerator = nodes.GetEnumerator())
                while (enumerator.MoveNext())
                {
                    Writer.Write(beforeEach(i));
                    enumerator.Current?.Accept(this);
                    i++;
                }
        }

        protected void Aggregate(IEnumerable<IDocumentNode> nodes, Func<int, string> afterEach)
        {
            int i = 0;
            using (IEnumerator<IDocumentNode> enumerator = nodes.GetEnumerator())
                while (enumerator.MoveNext())
                {
                    enumerator.Current?.Accept(this);
                    Writer.Write(afterEach(i));
                    i++;
                }
        }

        protected void Aggregate(Func<int, string> beforeEach, IEnumerable<IDocumentNode> nodes, Func<int, string> afterEach)
        {
            int i = 0;
            using (IEnumerator<IDocumentNode> enumerator = nodes.GetEnumerator())
                while (enumerator.MoveNext())
                {
                    Writer.Write(beforeEach(i));
                    enumerator.Current?.Accept(this);
                    Writer.Write(afterEach(i));
                    i++;
                }
        }

        protected void Aggregate(Func<IDocumentNode, int, string> beforeEach, IEnumerable<IDocumentNode> nodes)
        {
            int i = 0;
            using (IEnumerator<IDocumentNode> enumerator = nodes.GetEnumerator())
                while (enumerator.MoveNext())
                {
                    IDocumentNode node = enumerator.Current;

                    Writer.Write(beforeEach(node, i));
                    node?.Accept(this);
                    i++;
                }
        }

        protected void Aggregate(IEnumerable<IDocumentNode> nodes, Func<IDocumentNode, int, string> afterEach)
        {
            int i = 0;
            using (IEnumerator<IDocumentNode> enumerator = nodes.GetEnumerator())
                while (enumerator.MoveNext())
                {
                    IDocumentNode node = enumerator.Current;

                    node?.Accept(this);
                    Writer.Write(afterEach(node, i));
                    i++;
                }
        }

        protected void Aggregate(Func<IDocumentNode, int, string> beforeEach, IEnumerable<IDocumentNode> nodes, Func<IDocumentNode, int, string> afterEach)
        {
            int i = 0;
            using (IEnumerator<IDocumentNode> enumerator = nodes.GetEnumerator())
                while (enumerator.MoveNext())
                {
                    IDocumentNode node = enumerator.Current;

                    Writer.Write(beforeEach(node, i));
                    node?.Accept(this);
                    Writer.Write(afterEach(node, i));
                    i++;
                }
        }

        protected void AggregateChildren(IDocumentNode node) => Aggregate(node.Children);
        protected void AggregateChildren(string beforeEach, IDocumentNode node) => Aggregate(beforeEach, node.Children);
        protected void AggregateChildren(IDocumentNode node, string afterEach) => Aggregate(node.Children, afterEach);
        protected void AggregateChildren(string beforeEach, IDocumentNode node, string afterEach) => Aggregate(beforeEach, node.Children, afterEach);
        protected void AggregateChildren(Func<IDocumentNode, string> beforeEach, IDocumentNode node) => Aggregate(beforeEach, node.Children);
        protected void AggregateChildren(IDocumentNode node, Func<IDocumentNode, string> afterEach) => Aggregate(node.Children, afterEach);
        protected void AggregateChildren(Func<IDocumentNode, string> beforeEach, IDocumentNode node, Func<IDocumentNode, string> afterEach) => Aggregate(beforeEach, node.Children, afterEach);
        protected void AggregateChildren(Func<int, string> beforeEach, IDocumentNode node) => Aggregate(beforeEach, node.Children);
        protected void AggregateChildren(IDocumentNode node, Func<int, string> afterEach) => Aggregate(node.Children, afterEach);
        protected void AggregateChildren(Func<int, string> beforeEach, IDocumentNode node, Func<int, string> afterEach) => Aggregate(beforeEach, node.Children, afterEach);
        protected void AggregateChildren(Func<IDocumentNode, int, string> beforeEach, IDocumentNode node) => Aggregate(beforeEach, node.Children);
        protected void AggregateChildren(IDocumentNode node, Func<IDocumentNode, int, string> afterEach) => Aggregate(node.Children, afterEach);
        protected void AggregateChildren(Func<IDocumentNode, int, string> beforeEach, IDocumentNode node, Func<IDocumentNode, int, string> afterEach) => Aggregate(beforeEach, node.Children, afterEach);
    }
}