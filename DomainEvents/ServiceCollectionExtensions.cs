using Microsoft.Extensions.DependencyInjection;

namespace DomainEvents
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDomainEvents(this IServiceCollection services, Action<DomanEventsOptions> options)
        {
            ArgumentNullException.ThrowIfNull(options, nameof(options));

            services.Configure(options);

            return AddDomainEvents(services);
        }

        public static IServiceCollection AddDomainEvents(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services, nameof(services));

            services.AddSingleton<IEventsDispatcher, TaskEventsDispatcher>();
            services.AddSingleton<IEventsMediator, EventsMediator>();
            services.AddSingleton<IEventsSubscriber, EventsSubscriber>();
            services.AddSingleton<IEventsPublisher, EventsPublisher>();

            return services;
        }
    }
}
