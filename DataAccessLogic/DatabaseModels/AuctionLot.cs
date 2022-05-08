using DataAccessLogic.Enums;
using System;
using System.Collections.Generic;

namespace DataAccessLogic.DatabaseModels
{
    public class AuctionLot
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string PhotoSrc { get; set; }
        public LotStatus? Status { get; set; }
        public TypeOfAuction? TypeOfAuction { get; set; }
        public DateTime? AppStartDate { get; set; }
        public DateTime? AppEndDate { get; set;  }

        public PriceInfo PriceInfo { get; set; }

        public Note Note { get; set; }

        public string UserId { get; set; }
        public User User { get; set; }

        public List<SavedList> SavedLists { get; set; }

        public List<Bid> Bids { get; set; }
        public List<Application> Applications { get; set; }
    }
}
