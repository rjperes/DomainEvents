using DomainEvents;
using Microsoft.Extensions.DependencyInjection;

namespace DomainEventsConsole
{
    public class SampleEvent : IDomainEvent
    {
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public int Id { get; set; }
    }

    public class XptoEvent : IDomainEvent
    {
    }

    internal class Program
    {
        static async Task Main()
        {
            var services = new ServiceCollection();
            services.AddDomainEvents(options =>
            {
                options.FailOnNoSubscribers = true;
            });
            services.AddOptions();

            var serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });

            var subscriber = serviceProvider.GetRequiredService<IEventsSubscriber>();
            var publisher = serviceProvider.GetRequiredService<IEventsPublisher>();

            //await publisher.Publish(new XptoEvent());

            var @event = new ManualResetEvent(false);

            new Thread(async () =>
            {
                @event.WaitOne();

                for (var i = 0; i < 10; i++)
                {
                    Console.WriteLine($"Publish: {i}");
                    await publisher.Publish(new SampleEvent { Id = i });
                }
            }).Start();

            new Thread(() =>
            {
                @event.WaitOne();
                var count = 0;

                var subscription = subscriber.Subscribe<SampleEvent>(@event =>
                {
                    Console.WriteLine($"Subscribe: {@event.Id}");
                    count++;
                });

                while (count < 10)
                {
                    Thread.Sleep(100);
                }

                Console.WriteLine($"{count} consumed");

                subscription.Dispose();
            }).Start();

            @event.Set();

            Console.ReadLine();
        }
    }
}
