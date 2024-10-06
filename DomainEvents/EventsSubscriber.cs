namespace DomainEvents
{
    public interface IEventsSubscriber
    {
        Subscription Subscribe<TEvent>(Action<TEvent> action) where TEvent : IDomainEvent;
    }

    sealed class EventsSubscriber : IEventsSubscriber
    {
        private readonly IEventsMediator _dispatcher;

        public EventsSubscriber(IEventsMediator dispatcher)
        {
            ArgumentNullException.ThrowIfNull(dispatcher, nameof(dispatcher));
            _dispatcher = dispatcher;
        }

        public Subscription Subscribe<TEvent>(Action<TEvent> action) where TEvent : IDomainEvent
        {
            ArgumentNullException.ThrowIfNull(action, nameof(action));
            return _dispatcher.Subscribe(action);
        }
    }
}