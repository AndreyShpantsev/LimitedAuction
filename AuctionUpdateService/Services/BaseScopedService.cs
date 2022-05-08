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
        private readonly string cronExp;

        public BaseScopedService(ILogger<BaseScopedService> logger, string cronExp)
        {
            this.logger = logger;
            this.cronExp = cronExp;
        }

        public abstract Task ExecuteAsync(CancellationToken cancellationToken);

        protected async Task WaitForNextSchedule(string cronExpression)
        {
            var parsedExp = CronExpression.Parse(cronExpression);
            var currentUtcTime = DateTimeOffset.UtcNow.UtcDateTime;
            var occurenceTime = parsedExp.GetNextOccurrence(currentUtcTime);

            var delay = occurenceTime.GetValueOrDefault().Subtract(currentUtcTime);
            logger.LogInformation("The run is delayed for {delay}. Current time: {time}", delay, DateTimeOffset.Now);

            await Task.Delay(delay);
        }
    }
}
