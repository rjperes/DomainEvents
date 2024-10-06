namespace DomainEvents
{
    public static class EventsSubscriberExtensions
    {
        public static Subscription Subscribe<TEvent>(this IEventsSubscriber subscriber, ISubscription<TEvent> subscription) where TEvent : IDomainEvent
        {
            ArgumentNullException.ThrowIfNull(subscriber, nameof(subscriber));
            ArgumentNullException.ThrowIfNull(subscription, nameof(subscription));
            Subscription sub = null;
            sub = subscriber.Subscribe<TEvent>(async (evt) => await subscription.OnEvent(evt, sub));
            return sub;
        }
    }
}