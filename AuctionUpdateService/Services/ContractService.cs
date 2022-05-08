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
    internal class ContractService : BaseScopedService, IContractCreator
    {
        private readonly ILogger<ContractService> logger;
        private readonly ApplicationContext context;
        private static readonly string cronExp = "* * * * *";

        public ContractService(
            ILogger<ContractService> logger,
            ApplicationContext context) : base(logger, cronExp)
        {
            this.logger = logger;
            this.context = context;
        }

        public async Task CreateContracts(CancellationToken cancellationToken)
        {
            IDbContextTransaction tran = await context.Database.BeginTransactionAsync();
            try
            {
                List<AuctionLot> completedLots = await context.AuctionLots
                    .Where(lot => lot.Status == LotStatus.Contract)
                    .Join(
                        context.Contracts,
                        lot => lot.Id,
                        cntr => cntr.AuctionLotId.DefaultIfEmpty(),
                        (lot, cntr) => new
                        {
                            HasContract = cntr.AuctionLotId != null,
                            AuctionLot = lot
                        })
                    .Where(res => res.HasContract)
                    .Select(res => res.AuctionLot)
                    .ToListAsync();

                foreach (AuctionLot lot in completedLots)
                {
                    await CreateContract(lot);
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

        private async Task CreateContract(AuctionLot lot)
        {
            Bid winnerBid = await context.Bids
                .OrderByDescending(bid => bid.Time)
                .FirstOrDefaultAsync(bid => bid.AuctionLotId == lot.Id);

            if (winnerBid == null)
            {
                throw new Exception("Ставка победителя не определена");
            }

            await context.Contracts.AddAsync(new Contract
            {
                Id = Guid.NewGuid().ToString(),
                Amount = lot.PriceInfo.CurrentPrice,
                AuctionLotId = lot.Id,
                CreatedAt = DateTime.Now,
                Status = ContractStatus.PatricipantSigning,
                BuyerId = winnerBid.UserId,
                SellerId = lot.UserId
            });
        }

        public override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            try
            {
                await WaitForNextSchedule(cronExp);
                await CreateContracts(cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
            }
        }
    }
}
