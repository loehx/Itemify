

namespace Itemify.Shared.Interfaces
{
#if NET_CORE
    public interface ICloneable
    {
        object Clone();
    }
#endif
}
