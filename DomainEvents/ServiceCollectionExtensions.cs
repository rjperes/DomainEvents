using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace DomainEvents
{
    public static class ServiceCollectionExtensions
    {
        //private static readonly Dictionary<Type, Type> _defaultServiceImplementations = new()
        //{
        //    [typeof(IEventsDispatcher)] = typeof(ThreadEventsDispatcher),
        //    [typeof(IEventsMediator)] = typeof(EventsMediator),
        //    [typeof(IEventsSubscriber)] = typeof(EventsSubscriber),
        //    [typeof(IEventsPublisher)] = typeof(EventsPublisher)
        //};

        public static IDomainEventsServiceCollection AddDomainEvents(this IServiceCollection services, DomainEventsOptions options)
        {
            ArgumentNullException.ThrowIfNull(options, nameof(options));

            services.AddSingleton(Options.Create(options));

            return AddDomainEvents(services);
        }

        public static IDomainEventsServiceCollection AddDomainEvents(this IServiceCollection services, Action<DomainEventsOptions> options)
        {
            ArgumentNullException.ThrowIfNull(options, nameof(options));

            services.Configure(options);

            return AddDomainEvents(services);
        }

        public static IDomainEventsServiceCollection AddDomainEvents(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services, nameof(services));

            if (!services.Any(x => x.ServiceType == typeof(IEventsDispatcher)))
            {
                services.AddSingleton<IEventsDispatcher, TaskEventsDispatcher>();
            }

            if (!services.Any(x => x.ServiceType == typeof(IEventsPublisher)))
            {
                services.AddSingleton<IEventsPublisher, EventsPublisher>();
            }

            if (!services.Any(x => x.ServiceType == typeof(IEventsMediator)))
            {
                services.AddSingleton<IEventsMediator, EventsMediator>();
            }

            if (!services.Any(x => x.ServiceType == typeof(IEventsSubscriber)))
            {
                services.AddSingleton<IEventsSubscriber, EventsSubscriber>();
            }

            if (!services.Any(x => x.ServiceType == typeof(IEventsDispatcherExecutor)))
            {
                services.AddSingleton<IEventsDispatcherExecutor, SimpleEventsDispatcherExecutor>();
            }

            return new DomainEventsServiceCollection(services);
        }

        public static IDomainEventsServiceCollection AddDomainEventsFromAssembly(this IServiceCollection services, Assembly assembly, DomainEventsOptions options)
        {
            ArgumentNullException.ThrowIfNull(options, nameof(options));

            services.AddSingleton(Options.Create(options));

            return AddDomainEventsFromAssembly(services, assembly);
        }

        public static IDomainEventsServiceCollection AddDomainEventsFromAssembly(this IServiceCollection services, Assembly assembly, Action<DomainEventsOptions> options)
        {
            ArgumentNullException.ThrowIfNull(options, nameof(options));

            services.Configure(options);

            return AddDomainEventsFromAssembly(services, assembly);
        }

        private static readonly MethodInfo _subscribe = typeof(EventsSubscriberExtensions).GetMethod(nameof(EventsSubscriberExtensions.Subscribe))!;
        private static readonly MethodInfo _addInterceptor = typeof(EventsMediatorExtensions).GetMethod(nameof(EventsMediatorExtensions.AddInterceptor))!;

        public static IDomainEventsServiceCollection AddDomainEventsFromAssembly(this IServiceCollection services, Assembly assembly)
        {
            ArgumentNullException.ThrowIfNull(services, nameof(services));
            ArgumentNullException.ThrowIfNull(assembly, nameof(assembly));

            foreach (var domainInterceptorType in assembly.GetTypes().Where(x => !x.IsAbstract && !x.IsInterface && !x.IsGenericTypeDefinition && typeof(IDomainEventInterceptor).IsAssignableFrom(x)))
            {
                services.AddSingleton<IDomainEventInterceptor>(sp =>
                {
                    var interceptor = ActivatorUtilities.CreateInstance(sp, domainInterceptorType) as IDomainEventInterceptor;
                    var mediator = sp.GetRequiredService<IEventsMediator>();
                    mediator.AddInterceptor(interceptor!);
                    return interceptor!;
                });
            }

            foreach (var subscriptionType in assembly.GetTypes().Where(x => !x.IsAbstract && !x.IsInterface && !x.IsGenericTypeDefinition && x.GetInterfaces().Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(ISubscription<>))))
            {
                foreach (var subscriptionInterfaceType in subscriptionType.GetInterfaces().Where(x => x.GetGenericTypeDefinition() == typeof(ISubscription<>)))
                {
                    var eventType = subscriptionInterfaceType.GetGenericArguments().First();

                    services.AddSingleton(typeof(ISubscription<>).MakeGenericType(eventType), sp =>
                    {
                        var genericSubscription = ActivatorUtilities.CreateInstance(sp, subscriptionType);
                        var subscriber = sp.GetRequiredService<IEventsSubscriber>();
                        _subscribe.MakeGenericMethod(eventType).Invoke(null, [subscriber, genericSubscription]);
                        return genericSubscription;
                    });
                }
            }

            foreach (var genericDomainInterceptorType in assembly.GetTypes().Where(x => !x.IsAbstract && !x.IsInterface && !x.IsGenericTypeDefinition && x.GetInterfaces().Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IDomainEventInterceptor<>))))
            {
                foreach (var interceptionInterfaceType in genericDomainInterceptorType.GetInterfaces().Where(x => x.GetGenericTypeDefinition() == typeof(IDomainEventInterceptor<>)))
                {
                    var eventType = interceptionInterfaceType.GetGenericArguments().First();

                    services.AddSingleton(typeof(IDomainEventInterceptor<>).MakeGenericType(eventType), sp =>
                    {
                        var genericInterceptor = ActivatorUtilities.CreateInstance(sp, genericDomainInterceptorType);
                        var mediator = sp.GetRequiredService<IEventsMediator>();
                        _addInterceptor.MakeGenericMethod(eventType).Invoke(null, [mediator, genericInterceptor]);
                        return genericInterceptor;
                    });
                }
            }

            return AddDomainEvents(services);
        }
    }
}
