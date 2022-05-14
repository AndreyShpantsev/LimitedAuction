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

        public async Task<List<Application>> Read(Application model)
        {
            return await context.Applications
                .Include(app => app.User)
                .Include(app => app.AuctionLot)
                .Where(app => model == null ||
                    app.AuctionLotId == model.AuctionLotId ||
                    app.UserId == model.UserId)
                .OrderByDescending(model => model.CreatedAt)
                .ToListAsync();
        }

        public async Task Update(Application model)
        {
            if (model == null)
            {
                throw new Exception("Заявка не определена");
            }

            if (model.Id == null)
            {
                throw new Exception("Идентификатор заявки не определен");
            }

            Application dbApp = await context
                .Applications.FirstOrDefaultAsync(app => app.Id == model.Id);

            if (dbApp == null)
            {
                throw new Exception("Заявка не найдена");
            }

            if (dbApp.Status != ApplicationStatus.Submitted)
            {
                throw new Exception("Заявка находится в некорректном статусе");
            }

            dbApp.Status = model.Status;

            await context.SaveChangesAsync();
        }
    }
}
