namespace BankAccountApi.Entities
{
    public interface IConcurrencyAware
    {
        public String ConcurrencyStamp { get; set; }
    }
}
