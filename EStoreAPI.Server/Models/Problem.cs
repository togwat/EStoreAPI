using System.ComponentModel.DataAnnotations;

namespace EStoreAPI.Server.Models
{
    public class Problem
    {
        [Key]
        public int ProblemId { get; set; }

        [Required]
        public string ProblemName { get; set; }

        [Required]
        public int DeviceId { get; set; }
        public virtual Device Device { get; set; }

        [Required]
        public decimal Price { get; set; }
    }
}
