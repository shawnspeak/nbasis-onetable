namespace NBasis.OneTable
{
    /// <summary>
    /// OneTable control 
    /// </summary>
    /// <remarks>Creation, deletion and existence methods for a table context</remarks>
    public interface ITable<TContext> where TContext : TableContext
    {
        Task CreateAsync();

        Task<bool> ExistsAsync();

        Task DeleteAsync();

        Task EnsureAsync();
    }
}
