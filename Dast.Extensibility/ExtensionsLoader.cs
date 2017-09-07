using System;
using System.Collections.Generic;
using System.Composition.Convention;
using System.Composition.Hosting;
using System.Linq;
using System.Reflection;

namespace Dast.Extensibility
{
    static public class ExtensionsLoader
    {
        static public void FromAssemblies(IEnumerable<Assembly> assemblies, IEnumerable<IExtensible> extensibles) => FromAssemblies(assemblies, extensibles.ToArray());

        static public void FromAssemblies(IEnumerable<Assembly> assemblies, params IExtensible[] extensibles)
        {
            var conventionBuilder = new ConventionBuilder();
            foreach (Type extensionType in extensibles.SelectMany(e => e.GetType().GetInterfacesFromDefinition(typeof(IExtensible<>)).Select(t => t.GenericTypeArguments[0])).Distinct())
                conventionBuilder.ForTypesDerivedFrom(extensionType).Export(x => x.AsContractType(extensionType));

            var nextExtensibles = new List<IExtensible>();
            IEnumerable<Assembly> enumerable = assemblies as IList<Assembly> ?? assemblies.ToList();

            ContainerConfiguration containerConfiguration = new ContainerConfiguration().WithAssemblies(enumerable, conventionBuilder);
            using (CompositionHost container = containerConfiguration.CreateContainer())
                foreach (IExtensible extensible in extensibles)
                    nextExtensibles.AddRange(extensible.Extend(container).OfType<IExtensible>());

            if (nextExtensibles.Count > 0)
                FromAssemblies(enumerable, nextExtensibles);
        }
    }
}