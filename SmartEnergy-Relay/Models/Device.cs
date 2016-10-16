using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartEnergy_Server.Models
{
    /// <summary>
    /// A set of endpoints relating to device data.
    /// </summary>
    public class Device
    {
        /// <summary>
        /// The device ID.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The userID for the device.
        /// </summary>
        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// This is a permanent ID associated with the device hardware.
        /// </summary>
        [Required]
        public string HardwareId { get; set; }

        /// <summary>
        /// The name of the device.
        /// </summary>
        [Required]
        public string Alias { get; set; }

        /// <summary>
        /// The user associated with the device.
        /// </summary>
        public virtual User User { get; set; }

        /// <summary>
        /// The data the device has.
        /// </summary>
        public ICollection<Data> Data { get; set; }

    }
}