﻿using System.ComponentModel.DataAnnotations;

namespace EStoreAPI.Server.Models
{
    public class Problem
    {
        [Key]
        public int ProblemId { get; set; }

        [Required]
        public string ProblemName { get; set; }

        [Required]
        public Device Device { get; set; }

        [Required]
        public decimal Price { get; set; }

        public Problem() {}

        public Problem(string name, Device device, decimal price)
        {
            ProblemName = name;
            Device = device;
            Price = price;
        }
    }
}
