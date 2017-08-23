using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;

namespace Dast.Inputs.Dash
{
    static public class AntlrExtensions
    {
        static public IEnumerable<ParserRuleContext> GetRules(this ParserRuleContext context)
        {
            return context.GetRuleContexts<ParserRuleContext>();
        }

        static public ParserRuleContext GetRule(this ParserRuleContext context, int i)
        {
            return context.GetRuleContext<ParserRuleContext>(i);
        }

        static public IEnumerable<IDocumentNode> DumpCollectionNodes(this IEnumerable<IDocumentNode> nodes)
        {
            foreach (IDocumentNode node in nodes)
            {
                if (node is DashInput.CollectionNode collectionNode)
                {
                    foreach (IDocumentNode item in collectionNode.Items)
                        yield return item;
                    continue;
                }
                yield return node;
            }
        }
        
        static public IEnumerable<IDocumentNode> TransformInlineMedia(this IEnumerable<IDocumentNode> nodes)
        {
            foreach (IDocumentNode node in nodes)
            {
                if (node is MediaInlineNode mediaInlineNode)
                {
                    yield return new MediaNode
                    {
                        Extension = mediaInlineNode.Extension,
                        Content = mediaInlineNode.Content,
                        Type = mediaInlineNode.Type
                    };
                    continue;
                }
                yield return node;
            }
        }

        static public IEnumerable<T> NotNulls<T>(this IEnumerable<T> enumerable)
        {
            return enumerable.Where(x => x != null);
        }
    }
}