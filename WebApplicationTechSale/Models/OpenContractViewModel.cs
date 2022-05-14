using DataAccessLogic.Enums;

namespace WebApplicationTechSale.Models
{
    public class OpenContractViewModel
    {
        public string ContractId { get; set; }
        public string AuctionName { get; set; }
        public decimal ContractAmount { get; set; }
        public ContractStatus ContractStatus { get; set; }
        public string SellerName { get; set; }
        public string BuyerName { get; set; }
        public string DeliveryInfo { get; set; }
    }
}
