using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace EStoreAPI.Server.Models
{
    // an email permitted to sign in via Google OAuth
    // whitelist only for now, must manually add via sql
    [Index(nameof(Email), IsUnique = true)]
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        public required string Email { get; set; }
    }
}