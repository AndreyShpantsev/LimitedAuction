using System.Collections.Generic;

namespace WebApplicationTechSale.Models
{
    public class ViewAppsViewModel
    {
        public string AuctionLotId { get; set; }
        public List<ApplicationAcceptViewModel> AppsForView { get; set; }
    }
}
