using System.ComponentModel.DataAnnotations;

namespace EStoreAPI.Server.Models
{
    public class Job
    {
        [Key]
        public int JobId { get; set; }

        [Required]
        public int CustomerId { get; set; }
        public virtual Customer Customer { get; set; }

        [Required]
        public int DeviceId { get; set; }
        public virtual Device Device { get; set; }

        [Required]
        public DateTime ReceiveTime { get; set; }

        public DateTime? PickupTime { get; set; }

        public DateTime? EstimatedPickupTime { get; set; }

        public string? Note { get; set; }

        [Required]
        [MinLength(1)]
        public virtual ICollection<Problem> Problems { get; set; }

        public Decimal? EstimatedPrice { get; set; }

        public Decimal? CollectedPrice { get; set; }

        public bool IsFinished { get; set; } = false;

        public Job() {}

        public Job(Customer customer, Device device, DateTime receiveTime, DateTime pickupTime, DateTime estimatedPickupTime, string note, ICollection<Problem> problems, Decimal estimatedPrice, Decimal collectedPrice) 
        { 
            Customer = customer;
            Device = device;
            ReceiveTime = receiveTime;
            PickupTime = pickupTime;
            EstimatedPickupTime = estimatedPickupTime;
            Note = note;
            Problems = problems;
            EstimatedPrice = estimatedPrice;
            CollectedPrice = collectedPrice;
        }
    }
}
