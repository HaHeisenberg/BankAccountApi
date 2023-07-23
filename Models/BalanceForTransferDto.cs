namespace BankAccountApi.Models
{
    public class BalanceForTransferDto
    {
        public double Amount { get; set; }
        public bool Receiver { get; set; }
        public bool Sender { get; set; }
    }
}
