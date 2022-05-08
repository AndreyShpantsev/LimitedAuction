using AuctionUpdateService.Interfaces;
using DataAccessLogic;
using DataAccessLogic.DatabaseModels;
using DataAccessLogic.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AuctionUpdateService.Services
{
    internal class RequestDateService : BaseScopedService, IAuctionUpdater
    {
        private readonly ILogger<RequestDateService> logger;
        private readonly ApplicationContext context;
        private static readonly string cronExp = "* * * * *";

        public RequestDateService(
            ILogger<RequestDateService> logger, 
            ApplicationContext context) : base(logger, cronExp)
        {
            this.logger = logger;
            this.context = context;
        }

        public async Task UpdateAuctions(CancellationToken cancellationToken)
        {
            IDbContextTransaction tran = await context.Database.BeginTransactionAsync();
            try
            {
                DateTime currentDate = DateTime.Now;
                List<AuctionLot> lotsToUpdate = await context.AuctionLots
                    .Where(lot => lot.TypeOfAuction == TypeOfAuction.Closed &&
                    lot.AppStartDate != null && lot.AppEndDate != null && 
                    (
                        (lot.Status == LotStatus.Applications && lot.AppEndDate <= currentDate) || 
                        (lot.Status == LotStatus.Published && lot.AppStartDate <= currentDate)
                    )
                    ).ToListAsync();

                foreach (AuctionLot lot in lotsToUpdate)
                {
                    if (lot.Status == LotStatus.Applications)
                    {
                        lot.Status = LotStatus.ApplicationsView;
                    }
                    if (lot.Status == LotStatus.Published)
                    {
                        lot.Status = LotStatus.Applications;
                    }
                }
                await context.SaveChangesAsync();
                await tran.CommitAsync();
            }
            catch (Exception ex)
            {
                await tran.RollbackAsync();
                logger.LogError("Error: {0}", ex.Message);
            }
        }

        public override async Task ExecuteAsync(CancellationToken cancellationToken)
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
    }
}
