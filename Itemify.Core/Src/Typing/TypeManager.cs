using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Itemify.Core.Exceptions;

namespace Itemify.Core.Typing
{
    public static class TypeManager
    {
        private static Hashtable _types = Hashtable.Synchronized(new Hashtable());

        public static IEnumerable<TypeDefinition> AllDefinitions => _types.Values.Cast<TypeDefinition>();

        public static void Register<T>()
            where T: struct 
        {
            Register(typeof(T));
        }

        public static void Register(Type type)
        {
            if (!type.IsEnum)
                throw new ArgumentException($"Parameter {nameof(type)} must be an enum. Actual: {type}");

            var attr = type.GetCustomAttribute(typeof(TypeDefinitionAttribute)) as TypeDefinitionAttribute;
            if (attr == null)
                throw new MissingCustomAttribute($"Type {type.Name} is missing a custom attribute of type {nameof(TypeDefinitionAttribute)}");

            if (_types[type] != null)
                throw new Exception($"Type has already been registered: {type.Name}");

            var definition = new TypeDefinition(attr, type);
            _types[type] = definition;
        }

        public static void Reset()
        {
            _types.Clear();
        }


        internal static TypeDefinition GetDefinitionByType(Type type)
        {
            var result = _types[type] as TypeDefinition;
            if (result == null)
                throw new Exception($"Type definition {type.Name} is not a registered {nameof(TypeDefinition)}");

            return result;
        }

        internal static TypeDefinition GetDefinitionByName(string name)
        {
            var result = AllDefinitions.FirstOrDefault(k => k.Name == name);
            if (result == null)
                throw new Exception($"Type with name '{name}' is not a registered {nameof(TypeDefinition)}");

            return result;
        }
    }
}
