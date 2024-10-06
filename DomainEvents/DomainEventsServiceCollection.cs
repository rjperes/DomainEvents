using Microsoft.Extensions.DependencyInjection;
using System.Collections;

namespace DomainEvents
{
    public interface IDomainEventsServiceCollection : IServiceCollection
    {
        IDomainEventsServiceCollection AddInterceptor<TInterceptor>() where TInterceptor : class, IDomainEventInterceptor;
        IDomainEventsServiceCollection AddInterceptor<TEvent, TInterceptor>() where TEvent : IDomainEvent where TInterceptor : class, IDomainEventInterceptor<TEvent>;
        IDomainEventsServiceCollection AddSubscription<TEvent, TSubscription>() where TEvent : IDomainEvent where TSubscription : class, ISubscription<TEvent>;
    }

    sealed class DomainEventsServiceCollection : IDomainEventsServiceCollection
    {
        private readonly IServiceCollection _services;

        public DomainEventsServiceCollection(IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services, nameof(services));

            _services = services;
        }

        public ServiceDescriptor this[int index] { get => _services[index]; set => _services[index] = value; }

        public int Count => _services.Count;

        public bool IsReadOnly => _services.IsReadOnly;

        public void Add(ServiceDescriptor item) => _services.Add(item);

        public void Clear() => _services.Clear();

        public bool Contains(ServiceDescriptor item) => _services.Contains(item);

        public void CopyTo(ServiceDescriptor[] array, int arrayIndex) => _services.CopyTo(array, arrayIndex);

        public IEnumerator<ServiceDescriptor> GetEnumerator() => _services.GetEnumerator();

        public int IndexOf(ServiceDescriptor item) => _services.IndexOf(item);

        public void Insert(int index, ServiceDescriptor item) => _services.Insert(index, item);

        public bool Remove(ServiceDescriptor item) => _services.Remove(item);

        public void RemoveAt(int index) => _services.RemoveAt(index);

        IEnumerator IEnumerable.GetEnumerator() => _services.GetEnumerator();

        public IDomainEventsServiceCollection AddInterceptor<TInterceptor>() where TInterceptor : class, IDomainEventInterceptor
        {
            _services.AddSingleton<IDomainEventInterceptor>(sp =>
            {
                var interceptor = ActivatorUtilities.CreateInstance<TInterceptor>(sp);
                var mediator = sp.GetRequiredService<IEventsMediator>();
                mediator.AddInterceptor(interceptor);
                return interceptor;
            });
            return this;
        }

        public IDomainEventsServiceCollection AddInterceptor<TEvent, TInterceptor>() where TEvent : IDomainEvent where TInterceptor : class, IDomainEventInterceptor<TEvent>
        {
            _services.AddSingleton<IDomainEventInterceptor<TEvent>>(sp =>
            {
                var interceptor = ActivatorUtilities.CreateInstance<TInterceptor>(sp);
                var mediator = sp.GetRequiredService<IEventsMediator>();
                mediator.AddInterceptor(interceptor);
                return interceptor;

            });
            return this;
        }

        public IDomainEventsServiceCollection AddSubscription<TEvent, TSubscription>() where TEvent : IDomainEvent where TSubscription : class, ISubscription<TEvent>
        {
            _services.AddSingleton<ISubscription<TEvent>>(sp =>
            {
                var subscription = ActivatorUtilities.CreateInstance<TSubscription>(sp);
                var subscriber = sp.GetRequiredService<IEventsSubscriber>();
                var sub = subscriber.Subscribe(subscription);
                return subscription;
            });
            return this;
        }
    }
}
