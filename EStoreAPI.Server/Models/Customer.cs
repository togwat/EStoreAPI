using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace EStoreAPI.Server.Models
{
    [Index(nameof(PhoneNumber), IsUnique = true)]
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }

        public string? CustomerName { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        public string? PhoneNumberSecondary { get; set; }

        public string? Email { get; set; }

        public string? Address { get; set; }

        public virtual ICollection<Job> Jobs { get; set; }
    }
}
