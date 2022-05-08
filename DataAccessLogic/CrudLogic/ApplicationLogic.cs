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
    public class ApplicationLogic : ICrudLogic<Application>
    {
        private readonly ApplicationContext context;

        public ApplicationLogic(ApplicationContext context)
        {
            this.context = context;
        }

        public async Task Create(Application model)
        {
            if (model == null)
            {
                throw new Exception("Заявка не определена");
            }

            if (model.UserId == null)
            {
                throw new Exception("Пользователь не определен");
            }

            if (model.AuctionLotId == null)
            {
                throw new Exception("Лот не определен");
            }
            try
            {
                bool appExists = await context.Applications
                    .AnyAsync(app => app.AuctionLotId == model.AuctionLotId &&
                    app.UserId == model.UserId);

                if (appExists)
                {
                    throw new Exception("Заявку можно подать только один раз");
                }

                await context.Applications.AddAsync(new Application
                {
                    Id = Guid.NewGuid().ToString(),
                    CreatedAt = DateTime.Now,
                    Status = ApplicationStatus.Submitted,
                    UserId = model.UserId,
                    AuctionLotId = model.AuctionLotId
                });

                await context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw new Exception("Не удалось подать заявку");
            }
        }

        public Task Delete(Application model)
        {
            throw new NotImplementedException();
        }

        public Task<List<Application>> Read(Application model)
        {
            throw new NotImplementedException();
        }

        public Task Update(Application model)
        {
            throw new NotImplementedException();
        }
    }
}
