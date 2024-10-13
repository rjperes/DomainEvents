using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Reflection.Metadata.Ecma335;

namespace DomainEvents
{
    public interface IEventsMediator
    {
        Task Publish<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IDomainEvent;
        Subscription Subscribe<TEvent>(Action<TEvent> action) where TEvent : IDomainEvent;
        bool Unsubscribe(Subscription subscription);
        void AddInterceptor(IDomainEventInterceptor interceptor);
        void AddTransformer<TSourceEvent, TTargetEvent>(Func<TSourceEvent, TTargetEvent> transformer) where TSourceEvent : IDomainEvent where TTargetEvent : IDomainEvent;
    }

    sealed class EventsMediator : IEventsMediator
    {
        private readonly ConcurrentDictionary<Type, LinkedList<Subscription>> _subscriptions = new();
        private readonly ConcurrentStack<IDomainEventInterceptor> _interceptors = [];
        private readonly ConcurrentDictionary<Type, Func<object, object>> _transformers = [];
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
            IDomainEvent concreteEvent = @event;

            _serviceProvider.GetServices<ISubscription<TEvent>>();
            _serviceProvider.GetServices<IDomainEventInterceptor<TEvent>>();
            _serviceProvider.GetServices<IDomainEventInterceptor>();

            var interceptors = _interceptors.Reverse();

            foreach (var interceptor in interceptors)
            {
                var shouldProceed = await interceptor.BeforePublish(@event, cancellationToken);
                if (!shouldProceed)
                {
                    return;
                }
            }            

            if (_transformers.TryGetValue(eventType, out var transformer))
            {
                concreteEvent = (IDomainEvent) transformer(@event);
                eventType = concreteEvent.GetType();
            }

            if (_subscriptions.TryGetValue(eventType, out var subscriptions))
            {               
                await _dispatcher.Dispatch(concreteEvent, subscriptions, cancellationToken);                
            }
            else if (_options.FailOnNoSubscribers)
            {
                throw new InvalidOperationException($"No subscribers registered for '{eventType}'.");
            }

            foreach (var interceptor in interceptors)
            {
                await interceptor.AfterPublish(@event, cancellationToken);
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
            _interceptors.Push(interceptor);
        }

        public void AddTransformer<TSourceEvent, TTargetEvent>(Func<TSourceEvent, TTargetEvent> transformer) where TSourceEvent : IDomainEvent where TTargetEvent : IDomainEvent
        {
            ArgumentNullException.ThrowIfNull(transformer, nameof(transformer));
            _transformers[typeof(TSourceEvent)] = (source) => transformer((TSourceEvent)source);
        }
    }
}