﻿namespace DomainEvents
{
    public interface IEventsPublisher
    {
        Task Publish<T>(T @event, CancellationToken cancellationToken = default) where T : IDomainEvent;
    }

    sealed class EventsPublisher : IEventsPublisher
    {
        private readonly IEventsMediator _dispatcher;

        public EventsPublisher(IEventsMediator dispatcher)
        {
            ArgumentNullException.ThrowIfNull(dispatcher, nameof(dispatcher));

            _dispatcher = dispatcher;
        }

        public async Task Publish<T>(T @event, CancellationToken cancellationToken = default) where T : IDomainEvent
        {
            ArgumentNullException.ThrowIfNull(@event, nameof(@event));

            await _dispatcher.Publish<T>(@event, cancellationToken);
        }
    }
}