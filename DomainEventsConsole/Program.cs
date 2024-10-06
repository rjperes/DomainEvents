using DomainEvents;
using Microsoft.Extensions.DependencyInjection;

namespace DomainEventsConsole
{
    public class DummyEvent : IDomainEvent
    {
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public int Id { get; set; }
    }

    public class XptoEvent : IDomainEvent
    {
    }

    public class XptoInterceptor : DomainEventInterceptor<XptoEvent>
    {
        public override Task AfterPublish(XptoEvent @event, CancellationToken cancellationToken = default)
        {
            return base.AfterPublish(@event, cancellationToken);
        }

        public override Task BeforePublish(XptoEvent @event, CancellationToken cancellationToken = default)
        {
            return base.BeforePublish(@event, cancellationToken);
        }
    }

    public class DummyInterceptor : DomainEventInterceptor
    {
        public override Task AfterPublish(IDomainEvent @event, CancellationToken cancellationToken = default)
        {
            return base.AfterPublish(@event, cancellationToken);
        }

        public override Task BeforePublish(IDomainEvent @event, CancellationToken cancellationToken = default)
        {
            return base.BeforePublish(@event, cancellationToken);
        }
    }

    public class DummyGenericInterceptor : DomainEventInterceptor<DummyEvent>
    {
        public override Task AfterPublish(DummyEvent @event, CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"AfterPublish {@event}");
            return base.AfterPublish(@event, cancellationToken);
        }

        public override Task BeforePublish(DummyEvent @event, CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"BeforePublish {@event}");
            return base.BeforePublish(@event, cancellationToken);
        }
    }

    public class DummySubscription : ISubscription<DummyEvent>
    {
        public Task OnEvent(DummyEvent @event, Subscription subscription)
        {
            return Task.CompletedTask;
        }
    }

    internal class Program
    {
        static async Task Main()
        {
            var services = new ServiceCollection();
            //services.AddDomainEvents(options =>
            //{
            //    options.FailOnNoSubscribers = true;
            //});
            //services.AddDomainEvents()
            //    .AddInterceptor<DummyInterceptor>()
            //    .AddInterceptor<DummyEvent, DummyGenericInterceptor>()
            //    .AddSubscription<DummyEvent, DummySubscription>();
            services.AddDomainEventsFromAssembly(typeof(Program).Assembly);
            services.AddOptions();

            //services.AddSingleton<IDomainEventInterceptor, DummyInterceptor>();

            var serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });

            var subscriber = serviceProvider.GetRequiredService<IEventsSubscriber>();
            var publisher = serviceProvider.GetRequiredService<IEventsPublisher>();
            
            //var mediator = serviceProvider.GetRequiredService<IEventsMediator>();
            //mediator.AddInterceptor(new DummyInterceptor());

            //await publisher.Publish(new XptoEvent());

            var @event = new ManualResetEvent(false);

            //var publisherThread = new Thread(async () =>
            //{
            //    @event.WaitOne();

            //    for (var i = 0; i < 10; i++)
            //    {
            //        Console.WriteLine($"Publish: {i}");
            //        await publisher.Publish(new DummyEvent { Id = i });
            //    }
            //});
            //publisherThread.Start();

            //var subscriberThread = new Thread(() =>
            //{
            //    @event.WaitOne();
            //    var count = 0;

            //    using var subscription = subscriber.Subscribe<DummyEvent>(@event =>
            //    {
            //        Console.WriteLine($"Subscribe: {@event.Id}");
            //        count++;
            //    });

            //    while (count < 10)
            //    {
            //        Thread.Sleep(100);
            //    }

            //    Console.WriteLine($"{count} consumed");
            //});
            //subscriberThread.Start();

            @event.Set();

            await publisher.Publish(new DummyEvent());
            await publisher.Publish(new XptoEvent());

            //publisherThread.Join();
            //subscriberThread.Join();

            Console.ReadLine();
        }
    }
}
