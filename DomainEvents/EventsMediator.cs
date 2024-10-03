using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace DomainEvents
{
    public interface IEventsMediator
    {
        Task Publish<T>(T @event, CancellationToken cancellationToken = default) where T : IDomainEvent;
        Subscription Subscribe<T>(Action<T> action) where T : IDomainEvent;
        bool Unsubscribe(Subscription subscription);
        void AddInterceptor(IDomainEventInterceptor interceptor);
    }

    sealed class EventsMediator : IEventsMediator
    {
        private readonly ConcurrentDictionary<Type, LinkedList<Subscription>> _subscriptions = new();
        private readonly ConcurrentBag<IDomainEventInterceptor> _interceptors = [];
        private readonly IEventsDispatcher _dispatcher;
        private readonly DomainEventsOptions _options;

        public EventsMediator(IEventsDispatcher dispatcher, IOptions<DomainEventsOptions> options, IEnumerable<IDomainEventInterceptor> interceptors)
        {
            ArgumentNullException.ThrowIfNull(dispatcher, nameof(dispatcher));
            ArgumentNullException.ThrowIfNull(options, nameof(options));
            ArgumentNullException.ThrowIfNull(interceptors, nameof(interceptors));

            _dispatcher = dispatcher;
            _options = options.Value ?? new DomainEventsOptions();

            foreach (var interceptor in interceptors)
            {
                AddInterceptor(interceptor);
            }
        }

        public async Task Publish<T>(T @event, CancellationToken cancellationToken = default) where T : IDomainEvent
        {
            ArgumentNullException.ThrowIfNull(@event, nameof(@event));

            var eventType = @event.GetType();

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

        public Subscription Subscribe<T>(Action<T> action) where T : IDomainEvent
        {
            ArgumentNullException.ThrowIfNull(action, nameof(action));

            Action<object> act = (evt) => action((T)evt);
            var eventType = typeof(T);
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