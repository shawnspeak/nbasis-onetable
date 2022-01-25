using Amazon.DynamoDBv2;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBasis.OneTable
{
    public static class ConfigurationExtensions
    {
        public static IServiceCollection AddOneTable(this IServiceCollection serivces, string tableName, Action<OneTableConfiguration> options)
        {
            var config = OneTableConfiguration.Default();
            config.Tables[tableName] = tableName;

            options?.Invoke(config);

            AddOneTable(serivces, config);

            return serivces;
        }

        public static IServiceCollection AddOneTable(this IServiceCollection serivces, Action<OneTableConfiguration> options)
        {
            var config = OneTableConfiguration.Default();

            options?.Invoke(config);

            AddOneTable(serivces, config);

            return serivces;
        }

        private static void AddOneTable(IServiceCollection serivces, OneTableConfiguration config)
        {
            // validate configuration
            config.Validate();

            // register services
            serivces.AddSingleton<IItemLookup>(sp =>
            {
                return new DynamoDbItemLookup(
                    sp.GetRequiredService<IAmazonDynamoDB>(),
                    new TableNameResolver(config)
                );
            });
        }
    }
}
