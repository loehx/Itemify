using System;
using System.Collections.Generic;

namespace Itemify.Shared.Attributes
{
    /// <summary>
    /// Sets a specific value for an enum field.
    /// You can map this value using the EnumUtil.
    /// </summary>
    public class AliasAttribute : Attribute
    {
        public string Alias { get; }
        public IReadOnlyList<string> Alternatives { get; }

        public AliasAttribute(string alias)
        {
            Alias = alias;
            Alternatives = new string[0];
        }

        public AliasAttribute(string alias, params string[] alternatives)
        {
            Alias = alias;
            Alternatives = alternatives;
        }
    }
}
