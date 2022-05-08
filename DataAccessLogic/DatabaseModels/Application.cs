using DataAccessLogic.Enums;
using System;

namespace DataAccessLogic.DatabaseModels
{
    public class Application
    {
        public string Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public ApplicationStatus Status { get; set; }
        public string UserId { get; set; }
        public string AuctionLotId { get; set; }
        public User User { get; set; }
        public AuctionLot AuctionLot { get; set; }
    }
}
