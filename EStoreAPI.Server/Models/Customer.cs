using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace EStoreAPI.Server.Models
{
    [Index(nameof(PrimaryContact), IsUnique = true)]
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }

        public string? CustomerName { get; set; }

        [Required]
        public string PrimaryContact { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Email { get; set; }

        public string? Address { get; set; }

        public virtual ICollection<Job> Jobs { get; set; }
    }
}
