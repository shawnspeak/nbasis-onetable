using Microsoft.Extensions.DependencyInjection;

namespace NBasis.OneTable
{
    public static class ConfigurationExtensions
    {
        public static IServiceCollection AddOneTable<TContext>(this IServiceCollection services, string tableName)
             where TContext : TableContext
        {
            // see if context is already registered
            if (services.Any(s => s.ServiceType == typeof(TContext)))
                throw new TableContextAlreadyAddedException();

            // register context, table and items
            services.AddSingleton<TContext>(sp =>
            {
                // create context
                TContext ctx = Activator.CreateInstance<TContext>();

                // set table name
                ctx.SetTableName(tableName);

                // build/validate configuration
                ctx.ValidateConfiguration();

                return ctx;
            });

            services.AddSingleton<ITable<TContext>, DynamoDbTable<TContext>>();
            services.AddSingleton<IItemLookup<TContext>, DynamoDbItemLookup<TContext>>();
            services.AddSingleton<IItemStore<TContext>, DynamoDbItemStore<TContext>>();
            return services;
        }       
    }
}
