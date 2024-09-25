﻿namespace DomainEvents
{
    public interface IDomainEventInterceptor
    {
        Task BeforePublish(IDomainEvent @event, CancellationToken cancellationToken = default);
        Task AfterPublish(IDomainEvent @event, CancellationToken cancellationToken = default);
    }

    public abstract class DomainEventInterceptor : IDomainEventInterceptor
    {
        public virtual Task AfterPublish(IDomainEvent @event, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public virtual Task BeforePublish(IDomainEvent @event, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
