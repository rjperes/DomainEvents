namespace DomainEvents
{
    internal interface IEventsDispatcher
    {
        Task Dispatch<TEvent>(TEvent @event, IEnumerable<Subscription> subscriptions, CancellationToken cancellationToken = default) where TEvent : IDomainEvent;
    }

    sealed class SequentialEventsDispatcher : IEventsDispatcher
    {
        public Task Dispatch<TEvent>(TEvent @event, IEnumerable<Subscription> subscriptions, CancellationToken cancellationToken = default) where TEvent : IDomainEvent
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
        public async Task Dispatch<TEvent>(TEvent @event, IEnumerable<Subscription> subscriptions, CancellationToken cancellationToken = default) where TEvent : IDomainEvent
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
        public async Task Dispatch<TEvent>(TEvent @event, IEnumerable<Subscription> subscriptions, CancellationToken cancellationToken = default) where TEvent : IDomainEvent
        {
            ArgumentNullException.ThrowIfNull(@event, nameof(@event));
            ArgumentNullException.ThrowIfNull(subscriptions, nameof(subscriptions));

            await Parallel.ForEachAsync(subscriptions.AsParallel(), cancellationToken, (subscription, ct) =>
            {
                if (ct.IsCancellationRequested)
                {
                    return ValueTask.CompletedTask;
                }

                subscription.Action(@event);

                return ValueTask.CompletedTask;
            });
        }
    }

    sealed class ThreadEventsDispatcher : IEventsDispatcher
    {
        public Task Dispatch<TEvent>(TEvent @event, IEnumerable<Subscription> subscriptions, CancellationToken cancellationToken = default) where TEvent : IDomainEvent
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
