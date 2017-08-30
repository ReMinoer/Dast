using System;
using System.Collections.Generic;
using System.Composition;

namespace Dast.Extensibility
{
    public interface IExtensible
    {
        IEnumerable<Type> ExtensionTypes { get; }
        void Extend(CompositionContext context);
        void ResetToVanilla();
    }
}