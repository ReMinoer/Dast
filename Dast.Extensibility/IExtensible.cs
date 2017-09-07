using System.Collections;
using System.Collections.Generic;
using System.Composition;

namespace Dast.Extensibility
{
    public interface IExtensible
    {
        IEnumerable Extend(CompositionContext context);
        void ResetToVanilla();
    }

    public interface IExtensible<T> : IExtensible
    {
        ICollection<T> Extensions { get; }
        new IEnumerable<T> Extend(CompositionContext context);
    }
}