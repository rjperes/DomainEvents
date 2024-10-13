using DomainEvents;

namespace DomainEventsWeb
{
    public class TestInterceptor : DomainEventInterceptor
    {
        public TestInterceptor(IServiceProvider serviceProvider)
        {
            
        }

        public override Task AfterPublish(IDomainEvent @event, CancellationToken cancellationToken = default)
        {
            return base.AfterPublish(@event, cancellationToken);
        }

        public override ValueTask<bool> BeforePublish(IDomainEvent @event, CancellationToken cancellationToken = default)
        {
            return base.BeforePublish(@event, cancellationToken);
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddDomainEventsFromAssembly(typeof(Program).Assembly);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            app.MapDefaultControllerRoute();

            app.Run();
        }
    }
}
