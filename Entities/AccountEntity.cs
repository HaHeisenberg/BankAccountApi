using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BankAccountApi.Entities
{
    public class AccountEntity : IConcurrencyAware
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        [Required]
        public string Number { get; set; }
        [Required]
        public Guid UserId { get; set; }
        [Required]
        public double Balance { get; set; }
        public bool Active { get; set; }
        public string Type { get; set; }
        [ConcurrencyCheck]
        public string ConcurrencyStamp { get; set; } =
            Guid.NewGuid().ToString();
    }
}
