namespace DomainEvents
{
    public interface IEventsDispatcher
    {
        Task Dispatch<TEvent>(TEvent @event, IEnumerable<Subscription> subscriptions, CancellationToken cancellationToken = default) where TEvent : IDomainEvent;
    }

    public sealed class RetriesEventsDispatcher : IEventsDispatcher
    {
        private readonly IEventsDispatcher _dispatcher;
        private readonly TimeSpan _delay;
        private readonly uint _retries;

        public RetriesEventsDispatcher(IEventsDispatcher dispatcher, TimeSpan delay, uint retries)
        {
            ArgumentNullException.ThrowIfNull(dispatcher, nameof(dispatcher));
            ArgumentOutOfRangeException.ThrowIfEqual<uint>(retries, 0, nameof(retries));

            _dispatcher = dispatcher;
            _delay = delay;
            _retries = retries;
        }

        public async Task Dispatch<TEvent>(TEvent @event, IEnumerable<Subscription> subscriptions, CancellationToken cancellationToken = default) where TEvent : IDomainEvent
        {
            var i = 0;

            while (i < _retries)
            {
                try
                {
                    await _dispatcher.Dispatch(@event, subscriptions, cancellationToken);
                    break;
                }
                catch
                {
                    if (++i == _retries)
                    {
                        throw;
                    }
                    Thread.Sleep(_delay);
                }
            }
        }
    }

    public sealed class SequentialEventsDispatcher : IEventsDispatcher
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

    public sealed class TaskEventsDispatcher : IEventsDispatcher
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

    public sealed class ParallelEventsDispatcher : IEventsDispatcher
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

    public sealed class ThreadEventsDispatcher : IEventsDispatcher
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

                new Thread((ct) =>
                {
                    if (((CancellationToken)ct!).IsCancellationRequested)
                    {
                        return;
                    }
                    subscription.Action(@event);
                }).Start(cancellationToken);
            }

            return Task.CompletedTask;
        }
    }

    public sealed class ThreadPoolEventsDispatcher : IEventsDispatcher
    {
        public Task Dispatch<TEvent>(TEvent @event, IEnumerable<Subscription> subscriptions, CancellationToken cancellationToken = default) where TEvent : IDomainEvent
        {
            ArgumentNullException.ThrowIfNull(@event, nameof(@event));
            ArgumentNullException.ThrowIfNull(subscriptions, nameof(subscriptions));

            foreach (var subscription in subscriptions)
            {          
                ThreadPool.QueueUserWorkItem((ct) =>
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    subscription.Action(@event);
                }, cancellationToken);
            }

            return Task.CompletedTask;
        }
    }
}
