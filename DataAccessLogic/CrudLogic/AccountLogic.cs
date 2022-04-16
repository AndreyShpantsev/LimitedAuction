using DataAccessLogic.DatabaseModels;
using DataAccessLogic.Interfaces;
using System;
using System.Collections.Generic;
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

        public Task<List<Account>> Read(Account model)
        {
            throw new NotImplementedException();
        }

        public Task Update(Account model)
        {
            throw new NotImplementedException();
        }
    }
}
