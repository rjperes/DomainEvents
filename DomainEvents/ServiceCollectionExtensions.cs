using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DomainEvents
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDomainEvents(this IServiceCollection services, DomainEventsOptions options)
        {
            ArgumentNullException.ThrowIfNull(options, nameof(options));

            services.AddSingleton(Options.Create(options));

            return AddDomainEvents(services);
        }

        public static IServiceCollection AddDomainEvents(this IServiceCollection services, Action<DomainEventsOptions> options)
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
