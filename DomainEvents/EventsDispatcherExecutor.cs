namespace DomainEvents
{
    public interface IEventsDispatcherExecutor
    {
        Task Dispatch<TEvent>(TEvent @event, Subscription subscription, CancellationToken cancellation = default) where TEvent : IDomainEvent;
    }

    public sealed class SimpleEventsDispatcherExecutor : IEventsDispatcherExecutor
    {
        public Task Dispatch<TEvent>(TEvent @event, Subscription subscription, CancellationToken cancellation = default) where TEvent : IDomainEvent
        {
            subscription.Action(@event);
            return Task.CompletedTask;
        }
    }

    public sealed class RetriesEventDispatcherExecutor : IEventsDispatcherExecutor
    {
        private readonly IEventsDispatcherExecutor _eventsDispatcher;
        private readonly uint _retries;
        private readonly TimeSpan _delay;

        public RetriesEventDispatcherExecutor(uint retries, TimeSpan delay, IEventsDispatcherExecutor? eventsDispatcher)
        {
            _retries = retries;
            _delay = delay;
            _eventsDispatcher = eventsDispatcher ?? new SimpleEventsDispatcherExecutor();
        }

        public async Task Dispatch<TEvent>(TEvent @event, Subscription subscription, CancellationToken cancellationToken = default) where TEvent : IDomainEvent
        {
            var i = 0;

            while (i < _retries)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                try
                {
                    await _eventsDispatcher.Dispatch(@event, subscription, cancellationToken);
                    break;
                }
                catch
                {
                    if (++i == _retries)
                    {
                        throw;
                    }
                    await Task.Delay(_delay);
                }
            }
        }
    }
}
