using DataAccessLogic.DatabaseModels;
using DataAccessLogic.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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
            if (model == null || model.CTAccountId == null 
            || model.DTAccountId == null || model.Amount == 0.0m)
            {
                throw new Exception("Не все параметры транзакции определены.");
            }

            try
            {
                model.TransactionDate = DateTime.Now;
                Account accountDT = await context.Accounts.FirstOrDefaultAsync(acc =>
                acc.Id == model.DTAccountId);
                Account accountCT = await context.Accounts.FirstOrDefaultAsync(acc =>
                acc.Id == model.CTAccountId);
                decimal am = model.Amount * -1;

                if (accountDT == null || accountCT == null || accountCT.Balance < am)
                {
                    throw new Exception();
                }

                if (model.CTAccountId == model.DTAccountId)
                {
                    accountDT.Balance += model.Amount;
                }
                else
                {
                    accountCT.Balance -= model.Amount;
                    accountDT.Balance += model.Amount;
                }
                
                await context.Transactions.AddAsync(model);
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

        public async Task<List<Transaction>> Read(Transaction model)
        {
            if (string.IsNullOrWhiteSpace(model.CTAccountId) || string.IsNullOrWhiteSpace(model.DTAccountId))
            {
                throw new Exception("Номер счета не определен");
            }

            List<Transaction> transactionHistory = await context.Transactions
                .Where(tran => tran.CTAccountId == model.CTAccountId || tran.DTAccountId == model.DTAccountId)
                .OrderByDescending(tran => tran.TransactionDate)
                .ToListAsync();

            return transactionHistory;
        }

        public Task Update(Transaction model)
        {
            throw new NotImplementedException();
        }
    }
}
