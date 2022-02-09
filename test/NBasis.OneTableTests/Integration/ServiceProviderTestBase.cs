using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace NBasis.OneTableTests.Integration
{
    public abstract class ServiceProviderTestBase : IDisposable
    {
        private IServiceProvider _provider;

        protected IServiceProvider Container { get { return _provider; } }


        protected void Given(Action<IServiceCollection> given = null)
        {
            var services = new ServiceCollection();

            // invoke given if needed
            given?.Invoke(services);

            _provider = services.BuildServiceProvider();
        }

        private Func<IServiceProvider, Task> _when;

        protected void When(Func<IServiceProvider, Task> when)
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
                await _when(_provider);

                await then(null);
            }
            catch (Exception ex)
            {
                await then(ex);
            }
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
