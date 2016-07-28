using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartEnergy_Server.Models
{
    public class User
    {
        public int Id { get; set; }

        public string Username { get; set; }

        [JsonIgnore]
        public virtual ICollection<Device> Device { get; set; }

        [JsonIgnore]
        public virtual ICollection<Data> Data { get; set; }
    }
}