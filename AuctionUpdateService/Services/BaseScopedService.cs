using AuctionUpdateService.Interfaces;
using Cronos;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
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

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            try
            {
                await WaitForNextSchedule(cronExp);
                await UpdateAuctions(cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
            }
        }

        private async Task WaitForNextSchedule(string cronExpression)
        {
            var parsedExp = CronExpression.Parse(cronExpression);
            var currentUtcTime = DateTimeOffset.UtcNow.UtcDateTime;
            var occurenceTime = parsedExp.GetNextOccurrence(currentUtcTime);

            var delay = occurenceTime.GetValueOrDefault().Subtract(currentUtcTime);
            logger.LogInformation("The run is delayed for {delay}. Current time: {time}", delay, DateTimeOffset.Now);

            await Task.Delay(delay);
        }

        protected abstract Task UpdateAuctions(CancellationToken cancellationToken);
    }
}
