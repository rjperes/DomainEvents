namespace DomainEvents
{
    public interface IEventsPublisher
    {
        Task Publish<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IDomainEvent;
    }

    sealed class EventsPublisher : IEventsPublisher
    {
        private readonly IEventsMediator _dispatcher;

        public EventsPublisher(IEventsMediator dispatcher)
        {
            ArgumentNullException.ThrowIfNull(dispatcher, nameof(dispatcher));

            _dispatcher = dispatcher;
        }

        public async Task Publish<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IDomainEvent
        {
            ArgumentNullException.ThrowIfNull(@event, nameof(@event));

            await _dispatcher.Publish(@event, cancellationToken);
        }
    }
}