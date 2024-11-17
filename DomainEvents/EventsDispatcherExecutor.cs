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

    public sealed class RetriesEventDispatcherExecutor(uint Retries, TimeSpan Delay) : IEventsDispatcherExecutor
    {
        public async Task Dispatch<TEvent>(TEvent @event, Subscription subscription, CancellationToken cancellationToken = default) where TEvent : IDomainEvent
        {
            var i = 0;

            while (i < Retries)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                try
                {
                    subscription.Action(@event);
                    break;
                }
                catch
                {
                    if (++i == Retries)
                    {
                        throw;
                    }
                    await Task.Delay(Delay);
                }
            }
        }
    }
}
