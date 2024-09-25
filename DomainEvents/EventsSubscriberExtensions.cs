namespace DomainEvents
{
    public static class EventsSubscriberExtensions
    {
        public static Subscription Subscribe<T>(this IEventsSubscriber subscriber, ISubscription<T> subscription) where T : IDomainEvent
        {
            ArgumentNullException.ThrowIfNull(subscriber, nameof(subscriber));
            ArgumentNullException.ThrowIfNull(subscription, nameof(subscription));
            return subscriber.Subscribe<T>(async (evt) => await subscription.OnEvent(evt));
        }
    }
}