using System;

namespace Itemify.Core.PostgreSql
{
    public interface IPostgreSqlEntity
    {
        Guid Id { get; }
        int Type { get; }
    }
}