namespace DomainEvents
{
    public interface IEventsSubscriber
    {
        Subscription Subscribe<T>(Action<T> action) where T : IDomainEvent;
    }

    sealed class EventsSubscriber : IEventsSubscriber
    {
        private readonly IEventsMediator _dispatcher;

        public EventsSubscriber(IEventsMediator dispatcher)
        {
            ArgumentNullException.ThrowIfNull(dispatcher, nameof(dispatcher));
            _dispatcher = dispatcher;
        }

        public Subscription Subscribe<T>(Action<T> action) where T : IDomainEvent
        {
            ArgumentNullException.ThrowIfNull(action, nameof(action));
            return _dispatcher.Register(action);
        }
    }
}