using DataAccessLogic.DatabaseModels;
using System.ComponentModel.DataAnnotations;

namespace WebApplicationTechSale.Models
{
    public class LotModerationModel
    {
        public AuctionLot AuctionLot { get; set; }
        public bool Expanded { get; set; }

        [Required(ErrorMessage = "Укажите причину отказа")]
        [MinLength(15, ErrorMessage = "Не менее 15 символов")]
        [MaxLength(250, ErrorMessage = "Не более 250 символов")]
        public string RejectNote { get; set; }
    }
}
