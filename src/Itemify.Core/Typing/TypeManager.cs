using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Itemify.Core.Exceptions;
using Itemify.Shared.Utils;

namespace Itemify.Core.Typing
{
    public class TypeManager
    {
        private Hashtable _types = Hashtable.Synchronized(new Hashtable());

        public TypeManager()
        {
            Register<DefaultTypes>();
        }

        public IEnumerable<TypeDefinition> AllDefinitions => _types.Values.Cast<TypeDefinition>();

        public void Register<T>()
            where T: struct 
        {
            Register(typeof(T));
        }

        public void Register(Type type)
        {
            if (!type.GetTypeInfo().IsEnum)
                throw new ArgumentException($"Parameter {nameof(type)} must be an enum. Actual: {type}");

            var attr = type.GetTypeInfo().GetCustomAttribute(typeof(TypeDefinitionAttribute)) as TypeDefinitionAttribute;
            if (attr == null)
                throw new MissingCustomAttribute($"Type {type.Name} is missing a custom attribute of type {nameof(TypeDefinitionAttribute)}");

            if (_types[type] != null)
                throw new Exception($"Type has already been registered: {type.Name}");

            var definition = new TypeDefinition(attr, type);
            _types[type] = definition;
        }

        public void Reset()
        {
            _types.Clear();
        }


        internal TypeDefinition GetDefinitionByType(Type type)
        {
            var result = _types[type] as TypeDefinition;
            if (result == null)
                throw new Exception($"Type definition {type.Name} is not a registered {nameof(TypeDefinition)}");

            return result;
        }

        internal TypeDefinition GetDefinitionByName(string name)
        {
            var result = AllDefinitions.FirstOrDefault(k => k.Name == name);
            if (result == null)
                throw new Exception($"Type with name '{name}' is not a registered {nameof(TypeDefinition)}");

            return result;
        }

        public TypeItem GetTypeItem(Enum type)
        {
            var t = type.GetType();
            if (!t.GetTypeInfo().IsEnum)
                throw new ArgumentException($"Parameter {nameof(type)} must be an enum. Actual: {t}");

            var attr = t.GetTypeInfo().GetCustomAttribute(typeof(TypeDefinitionAttribute)) as TypeDefinitionAttribute;
            if (attr == null)
                throw new MissingCustomAttribute($"Type {t} is missing a custom attribute of type {nameof(TypeDefinitionAttribute)}");

            var typeValueAttribute = EnumUtil.GetCustomAttribute<TypeValueAttribute>(type);
            if (typeValueAttribute == null)
                throw new MissingCustomAttribute($"Parameter {nameof(type)} must have a custom attribute of type: {nameof(TypeValueAttribute)}");

            var definition = GetDefinitionByType(t);

            return definition.GetItemByValue(typeValueAttribute.Value);
        }

        public TypeItem ParseTypeItem(string source)
        {
            try
            {
                var spl = source.Split('=');
                if (spl.Length != 2)
                    throw new Exception("No seperator found (=).");

                var name = spl[0];
                var value = spl[1];
                var definition = GetDefinitionByName(name);
                var item = definition.GetItemByValue(value);

                return item;
            }
            catch (Exception err)
            {
                throw new Exception($"Unable to parse {nameof(TypeItem)} from: '{source}'", err);
            }
        }

        public TypeSet ParseTypeSet(string source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            var items = source.Split(new [] { '&' }, StringSplitOptions.RemoveEmptyEntries).Select(ParseTypeItem);
            return new TypeSet(this, items);
        }

        public TypeSet GetTypeSet(Enum enumValue)
        {
            var set = new TypeSet(this);
            set.Set(enumValue);
            return set;
        }
    }
}
