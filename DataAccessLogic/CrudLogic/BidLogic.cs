using DataAccessLogic.DatabaseModels;
using DataAccessLogic.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccessLogic.CrudLogic
{
    public class BidLogic : ICrudLogic<Bid>
    {
        private readonly ApplicationContext context;

        public BidLogic(ApplicationContext context)
        {
            this.context = context;
        }

        public async Task Create(Bid model)
        {
            if (model.AuctionLot == null)
            {
                throw new Exception("Лот не определен");
            }

            if (model.User == null || string.IsNullOrWhiteSpace(model.User.UserName))
            {
                throw new Exception("Пользователь не определен");
            }

            if (DateTime.Now > model.AuctionLot.EndDate)
            {
                throw new Exception("Дата ставки больше даты окончания торгов");
            }
            
            model.Id = Guid.NewGuid().ToString();
            model.AuctionLot.PriceInfo.CurrentPrice += model.AuctionLot.PriceInfo.BidStep;
            model.BidTimePrice = model.AuctionLot.PriceInfo.CurrentPrice;
            model.Time = DateTime.Now;

            context.AuctionLots.Update(model.AuctionLot);
            await context.Bids.AddAsync(model);
            await context.SaveChangesAsync();
        }

        public Task Delete(Bid model)
        {
            throw new NotImplementedException();
        }

        public Task Update(Bid model)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Bid>> Read(Bid model)
        {
            return await context.Bids.Include(bid => bid.User)
            .Where(bid => model == null
            || !string.IsNullOrWhiteSpace(model.AuctionLotId) 
            && bid.AuctionLotId == model.AuctionLotId)
            .OrderByDescending(bid => bid.BidTimePrice)
            .ToListAsync();
        }
    }
}
