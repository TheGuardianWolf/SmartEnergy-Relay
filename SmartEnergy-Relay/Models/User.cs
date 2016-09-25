using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SmartEnergy_Server.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [RegularExpression(@"^(?!.*[._-]{2})[a-z][a-z0-9._-]*[a-z0-9]$")]
        public string Username { get; set; }

        [JsonIgnore]
        public virtual ICollection<Device> Device { get; set; }

        [JsonIgnore]
        public virtual ICollection<Data> Data { get; set; }
    }
}