namespace DomainEvents
{
    internal interface IEventsDispatcher
    {
        Task Dispatch<T>(T @event, IEnumerable<Subscription> subscriptions, CancellationToken cancellationToken = default) where T : IDomainEvent;
    }

    internal class SequentialEventsDispatcher : IEventsDispatcher
    {
        public Task Dispatch<T>(T @event, IEnumerable<Subscription> subscriptions, CancellationToken cancellationToken = default) where T : IDomainEvent
        {
            ArgumentNullException.ThrowIfNull(@event, nameof(@event));
            ArgumentNullException.ThrowIfNull(subscriptions, nameof(subscriptions));

            foreach (var subscription in subscriptions)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                subscription.Action(@event);
            }

            return Task.CompletedTask;
        }
    }

    internal class TaskEventsDispatcher : IEventsDispatcher
    {
        public async Task Dispatch<T>(T @event, IEnumerable<Subscription> subscriptions, CancellationToken cancellationToken = default) where T : IDomainEvent
        {
            ArgumentNullException.ThrowIfNull(@event, nameof(@event));
            ArgumentNullException.ThrowIfNull(subscriptions, nameof(subscriptions));

            var task = Task.CompletedTask;

            foreach (var subscription in subscriptions)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                task = task.ContinueWith(_ => subscription.Action(@event));
            }

            await task;
        }
    }

    internal class ParallelEventsDispatcher : IEventsDispatcher
    {
        public Task Dispatch<T>(T @event, IEnumerable<Subscription> subscriptions, CancellationToken cancellationToken = default) where T : IDomainEvent
        {
            ArgumentNullException.ThrowIfNull(@event, nameof(@event));
            ArgumentNullException.ThrowIfNull(subscriptions, nameof(subscriptions));

            foreach (var subscription in subscriptions.AsParallel())
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                subscription.Action(@event);
            }

            return Task.CompletedTask;
        }
    }

    internal class ThreadEventsDispatcher : IEventsDispatcher
    {
        public Task Dispatch<T>(T @event, IEnumerable<Subscription> subscriptions, CancellationToken cancellationToken = default) where T : IDomainEvent
        {
            ArgumentNullException.ThrowIfNull(@event, nameof(@event));
            ArgumentNullException.ThrowIfNull(subscriptions, nameof(subscriptions));

            foreach (var subscription in subscriptions)
            {
                new Thread(() => subscription.Action(@event)).Start();
            }

            return Task.CompletedTask;
        }
    }
}
