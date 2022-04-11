using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLogic.DatabaseModels
{
    [Owned]
    public class PriceInfo
    {
        [Required(ErrorMessage = "Укажите стартовую цену")]
        public int StartPrice { get; set; }
        public int CurrentPrice { get; set; }
        [Required(ErrorMessage = "Укажите шаг ставки")]
        public int BidStep { get; set; }
    }
}
