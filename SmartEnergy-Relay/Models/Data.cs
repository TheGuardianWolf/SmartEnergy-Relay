using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace SmartEnergy_Server.Models
{
    public class Data
    {
        public int Id { get; set; }

        [Required]
        public int DeviceId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Time { get; set; }

        [Required]
        public string Label { get; set; }

        [Required]
        public decimal Value { get; set; }

        [JsonIgnore]
        public virtual Device Device { get; set; }
    }
}