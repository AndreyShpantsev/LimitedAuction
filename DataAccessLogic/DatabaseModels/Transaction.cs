using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLogic.DatabaseModels
{
    public class Transaction
    {
        public int Id { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }
        public string DTAccountId { get; set; }
        public string CTAccountId { get; set; }
        public string Comment { get; set; }
        public DateTime TransactionDate { get; set; }
        public virtual Account DTAccount { get; set; }
        public virtual Account CTAccount { get; set; }
    }
}
