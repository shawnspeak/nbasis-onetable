using Amazon.DynamoDBv2;
using Microsoft.Extensions.DependencyInjection;
using NBasis.OneTable;
using System;
using System.Threading.Tasks;
using Xunit;

namespace NBasis.OneTableTests.Integration
{
    [Collection("DynamoDbDocker")]
    public abstract class OneTableTestBase<TContext> : IDisposable
        where TContext : TableContext
    {
        readonly DynamoDbDockerFixture _fixture;
        readonly protected string TableName = Guid.NewGuid().ToString();

        public OneTableTestBase(DynamoDbDockerFixture fixture)
        {
            _fixture = fixture;
        }

        private IServiceProvider _provider;

        protected IServiceProvider Container { get { return _provider; } }


        protected async Task Given(Func<IItemStore<TContext>, IServiceProvider, Task> given = null)
        {
            var services = new ServiceCollection();

            services.AddSingleton<IAmazonDynamoDB>(_fixture.DynamoDbClient);
            services.AddOneTable<TContext>(TableName);

            _provider = services.BuildServiceProvider();

            // created the table
            var oneTable = _provider.GetRequiredService<ITable<TContext>>();
            await oneTable.Create();


            if (given != null)
            {
                var store = _provider.GetRequiredService<IItemStore<TContext>>();

                // invoke given
                await given(store, _provider);
            }
        }

        private Func<IItemStore<TContext>, IServiceProvider, Task> _when;

        protected void When(Func<IItemStore<TContext>, IServiceProvider, Task> when)
        {
            if (_provider == null)
                throw new Exception("Given has not been called");
            _when = when ?? throw new ArgumentNullException(nameof(when));
            if (_when == null)
                throw new Exception("No when was returned");
        }

        protected async Task Then(Func<Exception, Task> then)
        {
            if (_when == null)
                throw new Exception("When has not been set");
            if (then == null)
                throw new ArgumentNullException(nameof(then));

            try
            {
                var store = _provider.GetRequiredService<IItemStore<TContext>>();
                await _when(store, _provider);
            }
            catch (Exception ex)
            {
                await then(ex);
                return;
            }

            await then(null);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    _provider = null;
                }

                disposed = true;
            }
        }
    }
}
