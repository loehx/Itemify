namespace Itemify.Core.PostgreSql
{
    public interface IDefaultEntity : IEntityBase
    {
        int Id { get; set; }
    }
}