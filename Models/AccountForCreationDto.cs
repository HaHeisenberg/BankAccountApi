using System.ComponentModel.DataAnnotations;

namespace BankAccountApi.Models
{
    public class AccountForCreationDto
    {
        public Guid UserId { get; set; }
        public double Balance { get; set; }
        public bool Active { get; set; }
        [Required(ErrorMessage = "You should provide the type of the account")]
        public string Type { get; set; }
    }
}
