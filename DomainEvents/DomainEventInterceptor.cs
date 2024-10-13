namespace DomainEvents
{
    public interface IDomainEventInterceptor
    {
        ValueTask<bool> BeforePublish(IDomainEvent @event, CancellationToken cancellationToken = default);
        Task AfterPublish(IDomainEvent @event, CancellationToken cancellationToken = default);
    }

    public abstract class DomainEventInterceptor : IDomainEventInterceptor
    {
        public virtual Task AfterPublish(IDomainEvent @event, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public virtual ValueTask<bool> BeforePublish(IDomainEvent @event, CancellationToken cancellationToken = default) => ValueTask.FromResult(true);
    }

    public interface IDomainEventInterceptor<TEvent> where TEvent : IDomainEvent
    {
        ValueTask<bool> BeforePublish(TEvent @event, CancellationToken cancellationToken = default);
        Task AfterPublish(TEvent @event, CancellationToken cancellationToken = default);
    }

    public abstract class DomainEventInterceptor<TEvent> : IDomainEventInterceptor<TEvent> where TEvent : IDomainEvent
    {
        public virtual Task AfterPublish(TEvent @event, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public virtual ValueTask<bool> BeforePublish(TEvent @event, CancellationToken cancellationToken = default) => ValueTask.FromResult(true);
    }
}
