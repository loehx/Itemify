using System;

namespace Itemify.Core.PostgreSql
{
    public interface IGloballyUniqueEntity : IEntityBase
    {
        Guid Guid { get; set; }
    }
}