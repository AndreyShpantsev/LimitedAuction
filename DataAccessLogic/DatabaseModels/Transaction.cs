using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLogic.DatabaseModels
{
    public class Transaction
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string DTId { get; set; }
        public string CTId { get; set; }
        public string Comment { get; set; }
        public Account DTAccount { get; set; }
        public Account CTAccount { get; set; }

    }
}
