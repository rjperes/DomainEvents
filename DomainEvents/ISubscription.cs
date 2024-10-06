namespace DomainEvents
{
    public interface ISubscription<TEvent> where TEvent : IDomainEvent
    {
        Task OnEvent(TEvent @event, Subscription subscription);
    }
}