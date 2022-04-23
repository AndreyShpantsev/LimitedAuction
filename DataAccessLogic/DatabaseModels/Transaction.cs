using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLogic.DatabaseModels
{
    public class Transaction
    {
        public int Id { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }
        public string DTId { get; set; }
        public string CTId { get; set; }
        public string Comment { get; set; }
        public Account DTAccount { get; set; }
        public Account CTAccount { get; set; }

    }
}
