using Itemify.Core.Typing;

namespace Itemify.Core
{
    internal class ItemContext
    {
        public ItemContext(TypeManager typeManager)
        {
            TypeManager = typeManager;
        }

        public TypeManager TypeManager { get; }
    }
}