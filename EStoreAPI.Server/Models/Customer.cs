using System.ComponentModel.DataAnnotations;

namespace EStoreAPI.Server.Models
{
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }

        [Required]
        public string CustomerName { get; set; }

        [Required]
        [MinLength(1)]
        public string[] PhoneNumbers { get; set; }

        public string? Email { get; set; }

        public string? Address { get; set; }

        public virtual ICollection<Job> Jobs { get; set; }

        public Customer() { }
        
        public Customer(string name, string[] phoneNumbers, string email, string address)
        {
            CustomerName = name;
            PhoneNumbers = phoneNumbers;
            Email = email;
            Address = address;
        }
    }

}
