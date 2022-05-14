using System;
using System.ComponentModel.DataAnnotations;

namespace WebApplicationTechSale.Models
{
    public class SendAppViewModel
    {
        public string AuctionLotId { get; set; }
        public string AuctionName { get; set; }
        public string AuctionDescription { get; set; }
        public string Seller { get; set; }
        public decimal StartPrice { get; set; }
        public DateTime AppStartDate { get; set; }
        public DateTime AppEndDate { get; set; }
        public DateTime AuctionStartDate { get; set; }
        public DateTime AuctionEndDate { get; set; }
        [Display(Name = "Принять условия участия в закрытом аукционе")]
        public bool RulesIsAccepted { get; set; }
    }
}
