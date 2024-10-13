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
                if (@event is TEvent specificEvent)
                {
                    await _interceptor.AfterPublish(specificEvent, cancellationToken);
                }
            }

            public async override ValueTask<bool> BeforePublish(IDomainEvent @event, CancellationToken cancellationToken = default)
            {
                if (@event is TEvent specificEvent)
                {
                    return await _interceptor.BeforePublish(specificEvent, cancellationToken);
                }

                return true;
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