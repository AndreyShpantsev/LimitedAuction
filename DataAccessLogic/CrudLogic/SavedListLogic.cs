using DataAccessLogic.DatabaseModels;
using DataAccessLogic.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccessLogic.CrudLogic
{
    public class SavedListLogic : ISavedLogic
    {
        private readonly ApplicationContext context;

        public SavedListLogic(ApplicationContext context)
        {
            this.context = context;
        }

        public async Task Create(User user)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.Id))
            {
                throw new Exception("Пользователь не определен");
            }

            if (!context.SavedLists.Any(savedList => 
            savedList.UserId == user.Id))
            {
                SavedList newList = new SavedList
                {
                    Id = Guid.NewGuid().ToString(),
                    User = user
                };
                await context.SavedLists.AddAsync(newList);
                await context.SaveChangesAsync();
            }
        }

        public async Task Remove(User user, AuctionLot savedLot)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.Id))
            {
                throw new Exception("Пользователь не определен");
            }

            if (savedLot == null || string.IsNullOrWhiteSpace(savedLot.Id))
            {
                throw new Exception("Лот не определен");
            }

            SavedList existingList = await context.SavedLists
                .Include(list => list.AuctionLots)
                .FirstOrDefaultAsync(list => list.UserId == user.Id);

            if (existingList == null)
            {
                throw new Exception("Список избранного не найден");
            }

            AuctionLot lotToRemove = existingList.AuctionLots
                .Find(lot => lot.Id == savedLot.Id);

            existingList.AuctionLots.Remove(lotToRemove);

            await context.SaveChangesAsync();
        }

        public async Task<SavedList> Read(User user)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.Id))
            {
                throw new Exception("Пользователь не определен");
            }

            return await context.SavedLists.Include(savedList => savedList.AuctionLots)
            .FirstOrDefaultAsync(savedList => savedList.UserId == user.Id);
        }

        public async Task Add(User user, AuctionLot savedLot)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.Id))
            {
                throw new Exception("Пользователь не определен");
            }

            if (savedLot == null || string.IsNullOrWhiteSpace(savedLot.Id))
            {
                throw new Exception("Лот не определен");
            }

            SavedList existingList = await context.SavedLists
                .Include(list => list.AuctionLots)
                .FirstOrDefaultAsync(list => list.UserId == user.Id);

            AuctionLot lotToSave = await context.AuctionLots.FindAsync(savedLot.Id);

            if (existingList == null)
            {
                throw new Exception("Список избранного не найден");
            }

            existingList.AuctionLots.Add(lotToSave);

            await context.SaveChangesAsync();
        }
    }
}
