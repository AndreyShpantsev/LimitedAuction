using AuctionUpdateService.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AuctionUpdateService.Services
{
    internal class AuctionUpdateBackgroundService<T> : BackgroundService where T : IBaseScopedService
    {
        private readonly ILogger<AuctionUpdateBackgroundService<T>> logger;
        private readonly IServiceProvider serviceProvider;

        public AuctionUpdateBackgroundService(
            ILogger<AuctionUpdateBackgroundService<T>> logger, 
            IServiceProvider serviceProvider
        )
        {
            this.logger = logger;
            this.serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using IServiceScope scope = serviceProvider.CreateScope();
                    T updateService = scope.ServiceProvider.GetRequiredService<T>();
                    await updateService.ExecuteAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.Message);
                }
            }
        }
    }
}
