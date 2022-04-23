using DataAccessLogic.DatabaseModels;
using System.Collections.Generic;

namespace WebApplicationTechSale.Models
{
    public class MoneyInfoViewModel
    {
        public string AccountId { get; set; }
        public string Balance { get; set; }
        public List<Transaction> TransactionHistory { get; set; }
    }
}
