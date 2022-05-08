using AuctionUpdateService.Services;
using DataAccessLogic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AuctionUpdateService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    string connectionString = hostContext.Configuration["ConnectionStrings:AuctionDB"];
                    services.AddDbContext<ApplicationContext>(options => options.UseSqlServer(connectionString));
                    services.AddHostedService<AuctionUpdateBackgroundService<RequestDateService>>();
                    services.AddHostedService<AuctionUpdateBackgroundService<ActiveAuctionsService>>();
                    services.AddHostedService<AuctionUpdateBackgroundService<ContractService>>();
                    services.AddScoped<ActiveAuctionsService>();
                    services.AddScoped<RequestDateService>();
                    services.AddScoped<ContractService>();
                });
    }
}
