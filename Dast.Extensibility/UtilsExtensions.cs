using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dast.Extensibility
{
    static public class UtilsExtensions
    {
        static public IEnumerable<Type> GetInterfacesFromDefinition(this Type type, Type genericInterfaceDefinition)
        {
            return type.GetTypeInfo().ImplementedInterfaces.Where(x => x.GetTypeInfo().IsGenericType && x.GetGenericTypeDefinition() == genericInterfaceDefinition);
        }
    }
}