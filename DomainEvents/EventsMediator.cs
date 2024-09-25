using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace DomainEvents
{
    public interface IEventsMediator
    {
        Task Publish<T>(T @event, CancellationToken cancellationToken = default) where T : IDomainEvent;
        Subscription Subscribe<T>(Action<T> action) where T : IDomainEvent;
        bool Unsubscribe(Subscription subscription);
    }

    internal class EventsMediator : IEventsMediator
    {
        private readonly IDictionary<Type, LinkedList<Subscription>> _subscriptions = new ConcurrentDictionary<Type, LinkedList<Subscription>>();
        private readonly IEventsDispatcher _dispatcher;
        private readonly DomanEventsOptions _options;

        public EventsMediator(IEventsDispatcher dispatcher, IOptions<DomanEventsOptions> options)
        {
            ArgumentNullException.ThrowIfNull(dispatcher, nameof(dispatcher));
            ArgumentNullException.ThrowIfNull(options, nameof(options));
            _dispatcher = dispatcher;
            _options = options.Value ?? new DomanEventsOptions();
        }

        public async Task Publish<T>(T @event, CancellationToken cancellationToken = default) where T : IDomainEvent
        {
            ArgumentNullException.ThrowIfNull(@event, nameof(@event));

            var eventType = @event.GetType();

            if (_subscriptions.ContainsKey(eventType))
            {
                var subscriptions = _subscriptions[eventType];

                await _dispatcher.Dispatch(@event, subscriptions, cancellationToken);
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
            var subscription = new Subscription(this, typeof(T), act);

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

            if (_subscriptions.ContainsKey(subscription.EventType))
            {
                return _subscriptions[subscription.EventType].Remove(subscription);
            }

            return false;
        }
    }
}
