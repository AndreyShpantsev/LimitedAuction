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
        private static readonly string cronExp = "10 * * * * *";

        public RequestDateService(
            ILogger<RequestDateService> logger, 
            ApplicationContext context) : base(logger)
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
                        (
                            lot.Status == LotStatus.Published &&
                            (
                                lot.AppStartDate <= currentDate ||
                                lot.AppEndDate <= currentDate
                            ) &&
                            !context.Applications.Any(app => app.AuctionLotId == lot.Id)
                        )
                    )
                    ).ToListAsync();

                foreach (AuctionLot lot in lotsToUpdate)
                {
                    if (lot.Status == LotStatus.Applications)
                    {
                        int appsCount = await GetAppsCount(lot.Id);

                        if (appsCount > 1)
                        {
                            lot.Status = LotStatus.ApplicationsView;
                        }
                        
                        if (appsCount == 1)
                        {
                            await RejectSingleApp(lot.Id);
                            lot.Status = LotStatus.NotHeld;
                        }

                        if (appsCount == 0)
                        {
                            lot.Status = LotStatus.NotHeld;
                        }
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

        public async Task<int> GetAppsCount(string auctionLotId)
        {
            return await context.Applications
                .CountAsync(app => app.AuctionLotId == auctionLotId);
        }

        public async Task RejectSingleApp(string auctionLotId)
        {
            Application singleApp = await context.Applications
                .FirstOrDefaultAsync(app => 
                    app.AuctionLotId == auctionLotId &&
                    app.Status == ApplicationStatus.Submitted
                );

            if (singleApp != null)
            {
                singleApp.Status = ApplicationStatus.Rejected;
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
