using System.Threading.Channels;

namespace DomainEvents
{
    public interface IEventsDispatcher
    {
        Task Dispatch<TEvent>(TEvent @event, IEnumerable<Subscription> subscriptions, CancellationToken cancellationToken = default) where TEvent : IDomainEvent;
    }

    public sealed class SequentialEventsDispatcher : IEventsDispatcher
    {
        private readonly IEventsDispatcherExecutor _dispatcherExecutor;

        public SequentialEventsDispatcher(IEventsDispatcherExecutor dispatcherExecutor)
        {
            ArgumentNullException.ThrowIfNull(dispatcherExecutor, nameof(dispatcherExecutor));
            _dispatcherExecutor = dispatcherExecutor;
        }

        public async Task Dispatch<TEvent>(TEvent @event, IEnumerable<Subscription> subscriptions, CancellationToken cancellationToken = default) where TEvent : IDomainEvent
        {
            ArgumentNullException.ThrowIfNull(@event, nameof(@event));
            ArgumentNullException.ThrowIfNull(subscriptions, nameof(subscriptions));

            foreach (var subscription in subscriptions)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                await _dispatcherExecutor.Dispatch(@event, subscription, cancellationToken);
            }
        }
    }

    public sealed class TaskEventsDispatcher : IEventsDispatcher
    {
        private readonly IEventsDispatcherExecutor _dispatcherExecutor;

        public TaskEventsDispatcher(IEventsDispatcherExecutor dispatcherExecutor)
        {
            ArgumentNullException.ThrowIfNull(dispatcherExecutor, nameof(dispatcherExecutor));
            _dispatcherExecutor = dispatcherExecutor;
        }

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

                task = task.ContinueWith(async _ => await _dispatcherExecutor.Dispatch(@event, subscription, cancellationToken), cancellationToken);
            }

            await task;
        }
    }

    public sealed class ParallelEventsDispatcher : IEventsDispatcher
    {
        private readonly IEventsDispatcherExecutor _dispatcherExecutor;

        public ParallelEventsDispatcher(IEventsDispatcherExecutor dispatcherExecutor)
        {
            ArgumentNullException.ThrowIfNull(dispatcherExecutor, nameof(dispatcherExecutor));
            _dispatcherExecutor = dispatcherExecutor;
        }

        public async Task Dispatch<TEvent>(TEvent @event, IEnumerable<Subscription> subscriptions, CancellationToken cancellationToken = default) where TEvent : IDomainEvent
        {
            ArgumentNullException.ThrowIfNull(@event, nameof(@event));
            ArgumentNullException.ThrowIfNull(subscriptions, nameof(subscriptions));

            await Parallel.ForEachAsync(subscriptions.AsParallel(), cancellationToken, async (subscription, ct) =>
            {
                if (ct.IsCancellationRequested)
                {
                    return;
                }

                await _dispatcherExecutor.Dispatch(@event, subscription, ct);
            });
        }
    }

    public sealed class ThreadEventsDispatcher : IEventsDispatcher
    {
        private readonly IEventsDispatcherExecutor _dispatcherExecutor;

        public ThreadEventsDispatcher(IEventsDispatcherExecutor dispatcherExecutor)
        {
            ArgumentNullException.ThrowIfNull(dispatcherExecutor, nameof(dispatcherExecutor));
            _dispatcherExecutor = dispatcherExecutor;
        }

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

                new Thread(async (ct) =>
                {
                    if (((CancellationToken)ct!).IsCancellationRequested)
                    {
                        return;
                    }
                    await _dispatcherExecutor.Dispatch(@event, subscription, cancellationToken);
                }).Start(cancellationToken);
            }

            return Task.CompletedTask;
        }
    }

    public sealed class ThreadPoolEventsDispatcher : IEventsDispatcher
    {
        private readonly IEventsDispatcherExecutor _dispatcherExecutor;

        public ThreadPoolEventsDispatcher(IEventsDispatcherExecutor dispatcherExecutor)
        {
            ArgumentNullException.ThrowIfNull(dispatcherExecutor, nameof(dispatcherExecutor));
            _dispatcherExecutor = dispatcherExecutor;
        }

        public Task Dispatch<TEvent>(TEvent @event, IEnumerable<Subscription> subscriptions, CancellationToken cancellationToken = default) where TEvent : IDomainEvent
        {
            ArgumentNullException.ThrowIfNull(@event, nameof(@event));
            ArgumentNullException.ThrowIfNull(subscriptions, nameof(subscriptions));

            foreach (var subscription in subscriptions)
            {          
                ThreadPool.QueueUserWorkItem(async (ct) =>
                {
                    var innerCancellationToken = (CancellationToken) ct!;
                    if (innerCancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    await _dispatcherExecutor.Dispatch(@event, subscription, innerCancellationToken);
                }, cancellationToken);
            }

            return Task.CompletedTask;
        }
    }
}