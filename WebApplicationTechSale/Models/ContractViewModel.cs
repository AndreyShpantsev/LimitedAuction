using DataAccessLogic.DatabaseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplicationTechSale.Models
{
    public class ContractViewModel
    {
        public string ContractId { get; set; }
        public AuctionLot AuctionLot { get; set; }
        public string Participant { get; set; }
        public string Seller { get; set; }
        public decimal Price { get; set; }
    }
}
