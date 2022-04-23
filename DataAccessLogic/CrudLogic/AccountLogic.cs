using DataAccessLogic.DatabaseModels;
using DataAccessLogic.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccessLogic.CrudLogic
{
    public class AccountLogic : ICrudLogic<Account>
    {
        private readonly ApplicationContext context;

        public AccountLogic(ApplicationContext context)
        {
            this.context = context;
        }

        public async Task Create(Account model)
        {
            if (model == null || model.UserId == null)
            {
                throw new Exception("Пользователь не определен");
            }

            try
            {
                Account newAccount = new Account()
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = model.UserId,
                    Balance = 0.0m
                };

                await context.Accounts.AddAsync(newAccount);
                await context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw new Exception("Ошибка при создании счета");
            }
        }

        public Task Delete(Account model)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Account>> Read(Account model)
        {
            List<Account> userAccountData = await context
                .Accounts
                .Where(accInfo => accInfo.UserId == model.UserId)
                .ToListAsync();

            return userAccountData;
        }

        public Task Update(Account model)
        {
            throw new NotImplementedException();
        }
    }
}
