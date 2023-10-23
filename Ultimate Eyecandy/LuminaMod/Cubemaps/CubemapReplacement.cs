namespace Lumina
{
    using System.Xml.Serialization;

    /// <summary>
    /// Cubemap replacement XML record.
    /// Structure from BloodyPenguin's original cubemap replacer.
    /// </summary>
    public class CubemapReplacement
    {
        /// <summary>
        /// Gets or sets the cubemap texture size.
        /// </summary>
        [XmlAttribute("size")]
        public int Size { get; set; } = 1024;

        /// <summary>
        /// Gets or sets a value indicating whether this cubemap uses the split format.
        /// </summary>
        [XmlAttribute("is_split_format")]
        public bool SplitFormat { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether this cubemap is an outer-space cubemap.
        /// </summary>
        [XmlAttribute("is_outer_space")]
        public bool IsOuterSpace { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether this cubemap is a nighttime cubemap.
        /// </summary>
        [XmlAttribute("is_night")]
        public bool IsNight { get; set; } = false;

        /// <summary>
        /// Gets or sets the cubemap reference code.
        /// </summary>
        [XmlAttribute("code")]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the cubemap's description.
        /// </summary>
        [XmlAttribute("description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the cubemap's filename prefix.
        /// </summary>
        [XmlAttribute("file_prefix")]
        public string FilePrefix { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the time period for this cubemap.
        /// </summary>
        [XmlAttribute("time_period")]
        public string TimePeriod { get; set; }

        /// <summary>
        /// Gets or sets the weaher type for this cubemap.
        /// </summary>
        [XmlAttribute("weather")]
        public string WeatherType { get; set; }

        /// <summary>
        /// Gets or sets this cubemap's file directory.
        /// </summary>
        [XmlIgnore]
        public string Directory { get; set; }
    }
}