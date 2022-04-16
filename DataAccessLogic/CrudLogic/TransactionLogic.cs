using DataAccessLogic.DatabaseModels;
using DataAccessLogic.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLogic.CrudLogic
{
    public class TransactionLogic : ICrudLogic<Transaction>
    {
        private readonly ApplicationContext context;

        public TransactionLogic(ApplicationContext context)
        {
            this.context = context;
        }
        public async Task Create(Transaction model)
        {
            if (model == null || model.CTId == null 
                || model.DTId == null || model.Amount == 0.0m)
            {
                throw new Exception("Не все параметры транзакции определены.");
            }

            try
            {
                await context.Transactions.AddAsync(model);
                Account accountDT = await context.Accounts.FirstOrDefaultAsync(acc =>
                acc.Id == model.DTId);
                Account accountCT = await context.Accounts.FirstOrDefaultAsync(acc =>
                acc.Id == model.CTId);

                if (accountDT == null || accountCT == null)
                {
                    throw new Exception();
                }

                accountCT.Balance -= model.Amount;
                accountDT.Balance += model.Amount;
                
                await context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw new Exception("Не удалось провести операцию по счету");
            }
        }

        public Task Delete(Transaction model)
        {
            throw new NotImplementedException();
        }

        public Task<List<Transaction>> Read(Transaction model)
        {
            throw new NotImplementedException();
        }

        public Task Update(Transaction model)
        {
            throw new NotImplementedException();
        }
    }
}
