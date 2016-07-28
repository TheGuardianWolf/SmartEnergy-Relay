using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartEnergy_Server.Models
{
    public class Device
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string HardwareId { get; set; }
        public string Alias { get; set; }

        [JsonIgnore]
        public virtual User User { get; set; }

        [JsonIgnore]
        public virtual ICollection<Data> Data { get; set; }

    }
}