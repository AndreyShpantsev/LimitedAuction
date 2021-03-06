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
    internal class ActiveAuctionsService : BaseScopedService, IAuctionUpdater
    {
        private readonly ILogger<ActiveAuctionsService> logger;
        private readonly ApplicationContext context;
        private static readonly string cronExp = "20 * * * * *";

        public ActiveAuctionsService(
            ILogger<ActiveAuctionsService> logger,
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
                    .Include(lot => lot.Bids)
                    .Where(lot => 
                    (
                        (
                            lot.Status == LotStatus.ApplicationsView ||
                            lot.Status == LotStatus.Published
                        ) &&
                        (lot.StartDate <= currentDate)
                    ) ||
                    (lot.Status == LotStatus.Active && lot.EndDate <= currentDate)
                    ).ToListAsync();

                foreach (AuctionLot lot in lotsToUpdate)
                {
                    if (lot.Status == LotStatus.ApplicationsView && lot.StartDate <= currentDate)
                    {
                        lot.Status = LotStatus.NotHeld;
                        continue;
                    }
                    if (lot.Status == LotStatus.Published)
                    {
                        lot.Status = LotStatus.Active;
                        continue;
                    }
                    if (lot.Status == LotStatus.Active)
                    {
                        if (lot.Bids.Count > 0)
                        {
                            lot.Status = LotStatus.Contract;
                        }
                        else
                        {
                            lot.Status = LotStatus.NotHeld;
                        }
                        continue;
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
