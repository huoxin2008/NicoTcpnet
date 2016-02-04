using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Windows.Forms;

namespace Nico.Tcpnet
{
    internal static class TypeFactory
    {
       static  Dictionary<string, Type> types = new Dictionary<string, Type>();
       internal static Control Control;
       internal static void RegistType<T>()
        {
            Type type = typeof(T);
            var typeName = type.FullName;
            if (types.ContainsKey(typeName) == false)
            types.Add(typeName, type);
        }

       internal static bool Contains(string key)
       {
           return types.ContainsKey(key);
       }

       internal static Type GetType(string typeName)
       {
           return types.ContainsKey(typeName) ? types[typeName] : null;
       }
    }
}
