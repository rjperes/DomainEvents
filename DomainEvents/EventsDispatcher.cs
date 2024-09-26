namespace DomainEvents
{
    internal interface IEventsDispatcher
    {
        Task Dispatch<T>(T @event, IEnumerable<Subscription> subscriptions, CancellationToken cancellationToken = default) where T : IDomainEvent;
    }

    sealed class SequentialEventsDispatcher : IEventsDispatcher
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

    sealed class TaskEventsDispatcher : IEventsDispatcher
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

                task = task.ContinueWith(_ => subscription.Action(@event), cancellationToken);
            }

            await task;
        }
    }

    sealed class ParallelEventsDispatcher : IEventsDispatcher
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

    sealed class ThreadEventsDispatcher : IEventsDispatcher
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

                new Thread(() => subscription.Action(@event)).Start();
            }

            return Task.CompletedTask;
        }
    }
}
