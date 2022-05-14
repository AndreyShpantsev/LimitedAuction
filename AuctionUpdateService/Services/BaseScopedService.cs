using AuctionUpdateService.Interfaces;
using Cronos;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AuctionUpdateService.Services
{
    internal abstract class BaseScopedService : IBaseScopedService
    {
        private readonly ILogger<BaseScopedService> logger;

        public BaseScopedService(ILogger<BaseScopedService> logger)
        {
            this.logger = logger;
        }

        public abstract Task ExecuteAsync(CancellationToken cancellationToken);

        protected async Task WaitForNextSchedule(string cronExpression)
        {
            CronExpression parsedExp = CronExpression.Parse(cronExpression, CronFormat.IncludeSeconds);
            DateTime currentUtcTime = DateTimeOffset.UtcNow.UtcDateTime;
            DateTime? occurenceTime = parsedExp.GetNextOccurrence(currentUtcTime);

            TimeSpan delay = occurenceTime.GetValueOrDefault().Subtract(currentUtcTime);

            logger.LogInformation(
                "Следующий запуск сервиса в {occurenceTime}", 
                occurenceTime?.ToLocalTime().ToString("HH:mm:ss")
            );

            await Task.Delay(delay);
        }
    }
}
