using System;

namespace DataAccessLogic.DatabaseModels
{
    public class Bid
    {
        public string Id { get; set; }
        public DateTime Time { get; set; }
        public int BidTimePrice { get; set; }

        public string UserId { get; set; }
        public User User { get; set; }

        public string AuctionLotId { get; set; }
        public AuctionLot AuctionLot { get; set; }
    }
}
