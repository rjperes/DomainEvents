namespace DomainEvents
{
    public interface IEventsSubscriber
    {
        Subscription Subscribe<TEvent>(Action<TEvent> action) where TEvent : IDomainEvent;
    }

    sealed class EventsSubscriber : IEventsSubscriber
    {
        private readonly IEventsMediator _mediator;

        public EventsSubscriber(IEventsMediator mediator)
        {
            ArgumentNullException.ThrowIfNull(mediator, nameof(mediator));
            _mediator = mediator;
        }

        public Subscription Subscribe<TEvent>(Action<TEvent> action) where TEvent : IDomainEvent
        {
            ArgumentNullException.ThrowIfNull(action, nameof(action));
            return _mediator.Subscribe(action);
        }
    }
}