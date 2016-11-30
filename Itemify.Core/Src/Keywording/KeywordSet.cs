using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Itemify.Core.Keywording
{
    public class KeywordSet : ICloneable
    {
        private List<string> keywords;

        public IEnumerable<string> Keywords => keywords.OrderBy(k => k);

        public KeywordSet()
        {
            keywords = new List<string>();
        }

        internal KeywordSet(IEnumerable<string> keywords)
        {
            this.keywords = new List<string>(keywords);
            Debug.Assert(keywords.Distinct().Count() == this.keywords.Count);
        }

        public static KeywordSet From(params string[] keywords)
        {
            var set = new KeywordSet();

            foreach (var keyword in keywords)
                set.Set(keyword);
            
            return set;
        }

        private void Set(string keyword)
        {
            if (keyword == null) throw new ArgumentNullException(nameof(keyword));

            if (!keywords.Contains(keyword))
            {
                keywords.Add(keyword);
            }
        }

        private void Unset(string keyword)
        {
            if (keyword == null) throw new ArgumentNullException(nameof(keyword));

            keywords.Remove(keyword);
        }

        public override string ToString()
        {
            return $"[{keywords.Count} keywords] <{nameof(KeywordSet)}>";
        }

        public object Clone()
        {
            return new KeywordSet(keywords);
        }
    }
}
