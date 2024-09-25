using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Reflection;

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

            if (!services.Any(x => x.ServiceType == typeof(IEventsDispatcher)))
            {
                services.AddSingleton<IEventsDispatcher, TaskEventsDispatcher>();
            }

            if (!services.Any(x => x.ServiceType == typeof(IEventsMediator)))
            {
                services.AddSingleton<IEventsMediator, EventsMediator>();
            }

            if (!services.Any(x => x.ServiceType == typeof(IEventsSubscriber)))
            {
                services.AddSingleton<IEventsSubscriber, EventsSubscriber>();
            }

            if (!services.Any(x => x.ServiceType == typeof(IEventsPublisher)))
            {
                services.AddSingleton<IEventsPublisher, EventsPublisher>();
            }

            return services;
        }

        public static IServiceCollection AddDomainEventsFromAssembly(this IServiceCollection services, Assembly assembly, DomainEventsOptions options)
        {
            ArgumentNullException.ThrowIfNull(options, nameof(options));

            services.AddSingleton(Options.Create(options));

            return AddDomainEventsFromAssembly(services, assembly);
        }

        public static IServiceCollection AddDomainEventsFromAssembly(this IServiceCollection services, Assembly assembly, Action<DomainEventsOptions> options)
        {
            ArgumentNullException.ThrowIfNull(options, nameof(options));

            services.Configure(options);

            return AddDomainEventsFromAssembly(services, assembly);
        }

        public static IServiceCollection AddDomainEventsFromAssembly(this IServiceCollection services, Assembly assembly)
        {
            ArgumentNullException.ThrowIfNull(services, nameof(services));

            foreach (var eventsDispatcherType in assembly.GetTypes().Where(x => !x.IsAbstract && !x.IsInterface && typeof(IEventsDispatcher).IsAssignableFrom(x)))
            {
                services.AddSingleton(typeof(IEventsDispatcher), eventsDispatcherType);
            }

            foreach (var eventsMediatorType in assembly.GetTypes().Where(x => !x.IsAbstract && !x.IsInterface && typeof(IEventsMediator).IsAssignableFrom(x)))
            {
                services.AddSingleton(typeof(IEventsMediator), eventsMediatorType);
            }

            foreach (var eventsSubscriberType in assembly.GetTypes().Where(x => !x.IsAbstract && !x.IsInterface && typeof(IEventsSubscriber).IsAssignableFrom(x)))
            {
                services.AddSingleton(typeof(IEventsSubscriber), eventsSubscriberType);
            }

            foreach (var eventsPublisherType in assembly.GetTypes().Where(x => !x.IsAbstract && !x.IsInterface && typeof(IEventsPublisher).IsAssignableFrom(x)))
            {
                services.AddSingleton(typeof(IEventsPublisher), eventsPublisherType);
            }

            foreach (var domainInterceptorType in assembly.GetTypes().Where(x => !x.IsAbstract && !x.IsInterface && typeof(IDomainEventInterceptor).IsAssignableFrom(x)))
            {
                services.AddSingleton(typeof(IDomainEventInterceptor), domainInterceptorType);
            }

            foreach (var subscriptionType in assembly.GetTypes().Where(x => !x.IsAbstract && !x.IsInterface && x.GetInterfaces().Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(ISubscription<>))))
            {
                foreach (var specificSubscriptionType in subscriptionType.GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ISubscription<>)))
                {
                    var eventType = specificSubscriptionType.GetGenericArguments().First();
                    services.AddSingleton(typeof(ISubscription<>).MakeGenericType(eventType), subscriptionType);
                }
            }

            return AddDomainEvents(services);
        }
    }
}
