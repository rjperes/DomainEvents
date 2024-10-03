using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace DomainEvents
{
    public static class ServiceCollectionExtensions
    {
        private static readonly MethodInfo _subscribe = typeof(EventsSubscriberExtensions).GetMethod(nameof(EventsSubscriberExtensions.Subscribe))!;

        //private static readonly Dictionary<Type, Type> _defaultServiceImplementations = new()
        //{
        //    [typeof(IEventsDispatcher)] = typeof(ThreadEventsDispatcher),
        //    [typeof(IEventsMediator)] = typeof(EventsMediator),
        //    [typeof(IEventsSubscriber)] = typeof(EventsSubscriber),
        //    [typeof(IEventsPublisher)] = typeof(EventsPublisher)
        //};

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
                services.AddSingleton<IEventsDispatcher, ThreadEventsDispatcher>();
            }

            if (!services.Any(x => x.ServiceType == typeof(IEventsMediator)))
            {
                services.AddSingleton<IEventsMediator, EventsMediator>();
            }

            if (!services.Any(x => x.ServiceType == typeof(IEventsSubscriber)))
            {
                var subscriptionTypes = services.Where(x => x.ImplementationType != null && !x.ImplementationType.IsAbstract && !x.ImplementationType.IsInterface && x.ServiceType.IsGenericType && x.ServiceType.GetGenericTypeDefinition() == typeof(ISubscription<>)).Select(x => x.ServiceType);
                var subscriptions = new Dictionary<Type, Type>();

                foreach (var subscriptionType in subscriptionTypes)
                {                    
                    var eventType = subscriptionType.GetGenericArguments().First();
                    subscriptions[subscriptionType] = eventType;
                }

                services.AddSingleton(typeof(IEventsSubscriber), sp => RegisterEventSubscriber(sp, typeof(EventsSubscriber), subscriptions));
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
            ArgumentNullException.ThrowIfNull(assembly, nameof(assembly));

            //foreach (var eventsDispatcherType in assembly.GetTypes().Where(x => !x.IsAbstract && !x.IsInterface && typeof(IEventsDispatcher).IsAssignableFrom(x)))
            //{
            //    services.AddSingleton(typeof(IEventsDispatcher), eventsDispatcherType);
            //}

            //foreach (var eventsMediatorType in assembly.GetTypes().Where(x => !x.IsAbstract && !x.IsInterface && typeof(IEventsMediator).IsAssignableFrom(x)))
            //{
            //    services.AddSingleton(typeof(IEventsMediator), eventsMediatorType);
            //}

            //foreach (var eventsPublisherType in assembly.GetTypes().Where(x => !x.IsAbstract && !x.IsInterface && typeof(IEventsPublisher).IsAssignableFrom(x)))
            //{
            //    services.AddSingleton(typeof(IEventsPublisher), eventsPublisherType);
            //}

            foreach (var domainInterceptorType in assembly.GetTypes().Where(x => !x.IsAbstract && !x.IsInterface && !x.IsGenericTypeDefinition && typeof(IDomainEventInterceptor).IsAssignableFrom(x)))
            {
                services.AddSingleton(typeof(IDomainEventInterceptor), domainInterceptorType);
            }

            //var subscriptions = new Dictionary<Type, Type>();

            foreach (var subscriptionType in assembly.GetTypes().Where(x => !x.IsAbstract && !x.IsInterface && !x.IsGenericTypeDefinition && x.GetInterfaces().Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(ISubscription<>))))
            {
                foreach (var specificSubscriptionType in subscriptionType.GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ISubscription<>)))
                {
                    var eventType = specificSubscriptionType.GetGenericArguments().First();
                    //subscriptions[subscriptionType] = eventType;
                    services.AddSingleton(typeof(ISubscription<>).MakeGenericType(eventType), subscriptionType);
                }
            }

            //foreach (var eventsSubscriberType in assembly.GetTypes().Where(x => !x.IsAbstract && !x.IsInterface && typeof(IEventsSubscriber).IsAssignableFrom(x)))
            //{
            //    services.AddSingleton(typeof(IEventsSubscriber), sp => RegisterEventSubscriber(sp, eventsSubscriberType, subscriptions));
            //}

            return AddDomainEvents(services);
        }

        private static IEventsSubscriber RegisterEventSubscriber(IServiceProvider serviceProvider, Type eventsSubscriberType, IDictionary<Type, Type> subscriptionEventTypes)
        {
            var eventsSubscriber = ActivatorUtilities.CreateInstance(serviceProvider, eventsSubscriberType) as IEventsSubscriber;

            foreach (var entry in subscriptionEventTypes)
            {
                var subscriber = serviceProvider.GetService(entry.Key);
                _subscribe.MakeGenericMethod(entry.Value).Invoke(null, [eventsSubscriber, subscriber]);
            }

            return eventsSubscriber!;
        }
    }
}
