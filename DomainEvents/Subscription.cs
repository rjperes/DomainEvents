namespace DomainEvents
{
    public class Subscription : IDisposable
    {
        private readonly IEventsMediator _dispatcher;
        private readonly Guid _id = Guid.NewGuid();

        internal Subscription(IEventsMediator dispatcher, Type eventType, Action<object> action)
        {
            ArgumentNullException.ThrowIfNull(dispatcher, nameof(dispatcher));
            ArgumentNullException.ThrowIfNull(eventType, nameof(eventType));
            ArgumentNullException.ThrowIfNull(action, nameof(action));

            _dispatcher = dispatcher;

            EventType = eventType;
            Action = action;
        }

        internal Type EventType { get; }
        internal Action<object> Action { get; }

        public void Dispose()
        {
            _dispatcher.Unregister(this);
        }

        public override string ToString() => $"{_id}@{EventType}";
    }
}
