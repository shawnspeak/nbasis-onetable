namespace NBasis.OneTable
{
    public interface ITable<TContext> where TContext : TableContext
    {
        Task CreateAsync();

        Task<bool> ExistsAsync();

        Task DeleteAsync();

        Task EnsureAsync();
    }
}
