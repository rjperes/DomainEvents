namespace DomainEvents
{
    public static class EventsMediatorExtensions
    {
        sealed class SpecificEventDomainEventsInterceptor<TEvent> : DomainEventInterceptor where TEvent : IDomainEvent
        {
            private readonly IDomainEventInterceptor<TEvent> _interceptor;

            public SpecificEventDomainEventsInterceptor(IDomainEventInterceptor<TEvent> interceptor)
            {
                _interceptor = interceptor;
            }

            public async override Task AfterPublish(IDomainEvent @event, CancellationToken cancellationToken = default)
            {
                if (@event is TEvent)
                {
                    await _interceptor.AfterPublish((TEvent)@event, cancellationToken);
                }
            }

            public async override Task BeforePublish(IDomainEvent @event, CancellationToken cancellationToken = default)
            {
                if (@event is TEvent)
                {
                    await _interceptor.BeforePublish((TEvent)@event, cancellationToken);
                }
            }
        }

        public static IEventsMediator AddInterceptor<TEvent>(this IEventsMediator mediator, IDomainEventInterceptor<TEvent> interceptor) where TEvent : IDomainEvent
        {
            ArgumentNullException.ThrowIfNull(mediator, nameof(mediator));
            ArgumentNullException.ThrowIfNull(interceptor, nameof(interceptor));

            mediator.AddInterceptor(new SpecificEventDomainEventsInterceptor<TEvent>(interceptor));
            return mediator;
        }
    }
}