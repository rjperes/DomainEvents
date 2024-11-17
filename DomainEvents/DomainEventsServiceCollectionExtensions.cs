using Microsoft.Extensions.DependencyInjection;

namespace DomainEvents
{
    public static class DomainEventsServiceCollectionExtensions
    {
        public static IDomainEventsServiceCollection WithRetries(this IDomainEventsServiceCollection services, uint retries, TimeSpan delay)
        {
            services.AddSingleton<IEventsDispatcherExecutor>(sp => new RetriesEventDispatcherExecutor(retries, delay));
            return services;
        }
    }
}
