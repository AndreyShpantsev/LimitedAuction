using DataAccessLogic.DatabaseModels;
using System.Collections.Generic;

namespace WebApplicationTechSale.Models
{
    public class AuctionLotsViewModel
    {
        public IEnumerable<AuctionLot> AuctionLots { get; set; }
        public PageViewModel PageViewModel { get; set; }
    }
}
