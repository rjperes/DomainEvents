namespace DomainEvents
{
    public interface ISubscription<T> where T : IDomainEvent
    {
        Task OnEvent(T @event);
    }
}