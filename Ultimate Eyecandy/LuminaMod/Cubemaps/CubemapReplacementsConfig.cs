namespace Lumina
{
    using System;
    using System.IO;
    using System.Xml.Serialization;
    using AlgernonCommons;
    using ColossalFramework;

    /// <summary>
    /// Cubemap replacement XML file.
    /// Structure from BloodyPenguin's original cubemap replacer.
    /// </summary>
    public class CubemapReplacementsConfig
    {
        /// <summary>
        /// Gets or sets the list of replacements in this file.
        /// </summary>
        [XmlArray(ElementName = "CubemapReplacements")]
        [XmlArrayItem(ElementName = "CubemapReplacement")]
        public CubemapReplacement[] Replacements { get; set; }

        /// <summary>
        /// Deserializes a cubemap file.
        /// </summary>
        /// <param name="fileName">File to deserialize.</param>
        /// <returns>Deserialized <see cref="CubemapReplacementsConfig"/></returns>
        public static CubemapReplacementsConfig Deserialize(string fileName)
        {
            // Null check.
            if (fileName.IsNullOrWhiteSpace())
            {
                Logging.Error("invalid XML filename");
                return null;
            }

            try
            {
                // Check to see if configuration file exists.
                if (File.Exists(fileName))
                {
                    // Read it.
                    using (StreamReader reader = new StreamReader(fileName))
                    {
                        XmlSerializer xmlSerializer = new XmlSerializer(typeof(CubemapReplacementsConfig));
                        if (xmlSerializer.Deserialize(reader) is CubemapReplacementsConfig xmlFile)
                        {
                            return xmlFile;
                        }   
                        else
                        {
                            Logging.Error("couldn't deserialize cubemap replacement XML file ", fileName);
                        }
                    }
                }
                else
                {
                    Logging.Message("XML file ", fileName, " not found");
                }
            }
            catch (Exception e)
            {
                Logging.LogException(e, "exception reading cubemap replacement XML file ", fileName);
            }

            // If we got here, something went wrong.
            return null;
        }
    }
}