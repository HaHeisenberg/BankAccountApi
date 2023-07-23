namespace BankAccountApi.Entities
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public bool Active { get; set; }
    }
}
