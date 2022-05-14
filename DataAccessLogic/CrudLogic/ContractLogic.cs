using DataAccessLogic.DatabaseModels;
using DataAccessLogic.Enums;
using DataAccessLogic.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace DataAccessLogic.CrudLogic
{
    public class ContractLogic : ICrudLogic<Contract>
    {
        private readonly ApplicationContext context;

        public ContractLogic(ApplicationContext context)
        {
            this.context = context;
        }

        public Task Create(Contract model)
        {
            throw new NotImplementedException();
        }

        public Task Delete(Contract model)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Contract>> Read(Contract model)
        {
            return await context.Contracts
                .Include(cntr => cntr.Seller)
                .Include(cntr => cntr.Buyer)
                .Include(cntr => cntr.AuctionLot)
                .Where(cntr => 
                    model == null ||
                    cntr.Id == model.Id ||
                    cntr.SellerId == model.SellerId ||
                    cntr.BuyerId == model.BuyerId
                )
                .ToListAsync();
        }

        public async Task Update(Contract model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Id))
            {
                throw new Exception("Контракт не определен");
            }

            Contract contractToUpdate = await context.Contracts
                .FirstOrDefaultAsync(cntr => cntr.Id == model.Id);

            if (contractToUpdate == null)
            {
                throw new Exception("Контракт не найден");
            }

            contractToUpdate.Status = model.Status;
            contractToUpdate.DeliveryInfo = model.DeliveryInfo;

            await context.SaveChangesAsync();
        }
    }
}
