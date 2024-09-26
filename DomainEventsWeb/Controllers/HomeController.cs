using DomainEvents;
using DomainEventsWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace DomainEventsWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IEventsSubscriber _subscriber;
        private readonly IEventsPublisher _publisher;
        private readonly Subscription _subscription;

        public HomeController(ILogger<HomeController> logger, IEventsSubscriber subscriber, IEventsPublisher publisher)
        {
            _logger = logger;
            _subscriber = subscriber;
            _publisher = publisher;
            _subscription = _subscriber.Subscribe<TestEvent>(evt => OnEvent(evt));
        }

        private void OnEvent(TestEvent evt)
        {
            
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Publish(CancellationToken cancellationToken)
        {
            await _publisher.Publish(new TestEvent(), cancellationToken);

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Unsubscribe()
        {
            _subscription.Dispose();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
