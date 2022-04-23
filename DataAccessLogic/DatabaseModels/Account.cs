using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLogic.DatabaseModels
{
    public class Account
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Balance { get; set; }
        public User User { get; set; }
    }
}
