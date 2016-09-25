using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartEnergy_Server.Models
{
    public class Device
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public string HardwareId { get; set; }
        public string Alias { get; set; }

        [JsonIgnore]
        public virtual User User { get; set; }

        [JsonIgnore]
        public virtual ICollection<Data> Data { get; set; }

    }
}