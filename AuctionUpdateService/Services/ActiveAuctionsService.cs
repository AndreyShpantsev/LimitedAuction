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
    internal class ActiveAuctionsService : BaseScopedService
    {
        private readonly ILogger<RequestDateService> logger;
        private readonly ApplicationContext context;
        private static readonly string cronExp = "* * * * *";

        public ActiveAuctionsService(
            ILogger<RequestDateService> logger,
            ApplicationContext context) : base(logger, cronExp)
        {
            this.logger = logger;
            this.context = context;
        }

        protected override async Task UpdateAuctions(CancellationToken cancellationToken)
        {
            IDbContextTransaction tran = await context.Database.BeginTransactionAsync();
            try
            {
                DateTime currentDate = DateTime.Now;
                List<AuctionLot> lotsToUpdate = await context.AuctionLots
                    .Include(lot => lot.Bids)
                    .Where(lot => 
                    (
                        (lot.Status == LotStatus.ApplicationsView || lot.Status == LotStatus.Published) &&
                        (lot.StartDate <= currentDate)
                    ) ||
                    (lot.Status == LotStatus.Active && lot.EndDate <= currentDate)
                    ).ToListAsync();

                foreach (AuctionLot lot in lotsToUpdate)
                {
                    if (lot.Status == LotStatus.ApplicationsView || lot.Status == LotStatus.Published)
                    {
                        lot.Status = LotStatus.Active;
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
    }
}
