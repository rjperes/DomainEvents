using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace DomainEvents
{
    public interface IEventsMediator
    {
        Task Publish<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IDomainEvent;
        Subscription Subscribe<TEvent>(Action<TEvent> action) where TEvent : IDomainEvent;
        bool Unsubscribe(Subscription subscription);
        void AddInterceptor(IDomainEventInterceptor interceptor);
    }

    sealed class EventsMediator : IEventsMediator
    {
        private readonly ConcurrentDictionary<Type, LinkedList<Subscription>> _subscriptions = new();
        private readonly ConcurrentBag<IDomainEventInterceptor> _interceptors = [];
        private readonly IEventsDispatcher _dispatcher;
        private readonly DomainEventsOptions _options;
        private readonly IServiceProvider _serviceProvider;

        public EventsMediator(IServiceProvider serviceProvider, IEventsDispatcher dispatcher, IOptions<DomainEventsOptions> options)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider, nameof(serviceProvider));
            ArgumentNullException.ThrowIfNull(dispatcher, nameof(dispatcher));
            ArgumentNullException.ThrowIfNull(options, nameof(options));

            _serviceProvider = serviceProvider;
            _dispatcher = dispatcher;
            _options = options.Value ?? new DomainEventsOptions();
        }

        public async Task Publish<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IDomainEvent
        {
            ArgumentNullException.ThrowIfNull(@event, nameof(@event));

            var eventType = @event.GetType();

            var genericSubscriptions = _serviceProvider.GetServices<ISubscription<TEvent>>().ToList();
            var genericInterceptors = _serviceProvider.GetServices<IDomainEventInterceptor<TEvent>>().ToList();
            var interceptors = _serviceProvider.GetServices<IDomainEventInterceptor>().ToList();

            if (_subscriptions.TryGetValue(eventType, out var subscriptions))
            {
                foreach (var interceptor in _interceptors)
                {
                    await interceptor.BeforePublish(@event, cancellationToken);
                }

                await _dispatcher.Dispatch(@event, subscriptions, cancellationToken);

                foreach (var interceptor in _interceptors)
                {
                    await interceptor.AfterPublish(@event, cancellationToken);
                }
            }
            else if (_options.FailOnNoSubscribers)
            {
                throw new InvalidOperationException($"No subscribers registered for '{eventType}'.");
            }
        }

        public Subscription Subscribe<TEvent>(Action<TEvent> action) where TEvent : IDomainEvent
        {
            ArgumentNullException.ThrowIfNull(action, nameof(action));

            Action<object> act = (evt) => action((TEvent)evt);
            var eventType = typeof(TEvent);
            var subscription = new Subscription(this, eventType, act);

            if (!_subscriptions.TryGetValue(eventType, out var list))
            {
                list = _subscriptions[eventType] = new LinkedList<Subscription>();
            }

            list.AddLast(subscription);

            return subscription;
        }

        public bool Unsubscribe(Subscription subscription)
        {
            ArgumentNullException.ThrowIfNull(subscription, nameof(subscription));

            if (_subscriptions.TryGetValue(subscription.EventType, out var list))
            {
                return list.Remove(subscription);
            }

            return false;
        }

        public void AddInterceptor(IDomainEventInterceptor interceptor)
        {
            ArgumentNullException.ThrowIfNull(interceptor, nameof(interceptor));

            _interceptors.Add(interceptor);
        }
    }
}