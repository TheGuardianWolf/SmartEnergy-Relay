using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartEnergy_Server.Models
{
    public class Data
    {
        public int Id { get; set; }

        public int DeviceId { get; set; }

        public DateTime Time { get; set; }

        public int Power { get; set; }

        [JsonIgnore]
        public virtual Device Device { get; set; }
    }
}