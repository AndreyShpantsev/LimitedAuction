using DataAccessLogic.Enums;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLogic.DatabaseModels
{
    public class Contract
    {
        public string Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public ContractStatus Status { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }
        public string AuctionLotId { get; set; }
        public string BuyerId { get; set; }
        public string SellerId { get; set; }
        public AuctionLot AuctionLot { get; set; }
        public User Buyer { get; set; }
        public User Seller { get; set; }
    }
}
