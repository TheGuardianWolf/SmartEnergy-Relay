using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace SmartEnergy_Server.Models
{
    /// <summary>
    /// A set of endpoints relating to user data.
    /// </summary>
    [DataContract]
    public class User
    {
        /// <summary>
        /// The user ID.
        /// </summary>
        [DataMember]
        public int Id { get; set; }

        /// <summary>
        /// The username for the user.
        /// </summary>
        [DataMember]
        [Required]
        [RegularExpression(@"^(?!.*[._-]{2})[a-z][a-z0-9._-]*[a-z0-9]$")]
        public string Username { get; set; }

        /// <summary>
        /// The devices the user has.
        /// </summary>
        public ICollection<Device> Device { get; set; }

        /// <summary>
        /// The data the user has.
        /// </summary>
        public ICollection<Data> Data { get; set; }
    }
}