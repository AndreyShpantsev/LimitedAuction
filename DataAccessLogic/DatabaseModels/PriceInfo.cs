using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

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
        [Required(ErrorMessage = "Укажите конечную цену, за которую готовы отдать товар без торгов")]
        public int FinalPrice { get; set; }
        public int? PercentBid { get; set; }
    }
}
