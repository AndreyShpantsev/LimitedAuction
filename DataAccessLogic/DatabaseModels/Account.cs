namespace DataAccessLogic.DatabaseModels
{
    public class Account
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public decimal Balance { get; set; }
        public User User { get; set; }
    }
}
