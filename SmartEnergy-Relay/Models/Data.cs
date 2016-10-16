using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace SmartEnergy_Server.Models
{
    /// <summary>
    /// A set of endpoints relating to data.
    /// </summary>
    [DataContract]
    public class Data
    {
        /// <summary>
        /// The data ID.
        /// </summary>
        [DataMember]
        public int Id { get; set; }

        /// <summary>
        /// The deviceID the data has.
        /// </summary>
        [DataMember]
        [Required]
        public int DeviceId { get; set; }

        /// <summary>
        /// The date and time the data was collected.
        /// </summary>
        [DataMember]
        [Required]
        [DataType(DataType.Date)]
        public DateTime Time { get; set; }

        /// <summary>
        /// The label for the data.
        /// </summary>
        [DataMember]
        [Required]
        public string Label { get; set; }

        /// <summary>
        /// The value of the data.
        /// </summary>
        [DataMember]
        [Required]
        public decimal Value { get; set; }

        /// <summary>
        /// The device the data is coming from.
        /// </summary>
        public virtual Device Device { get; set; }
    }
}