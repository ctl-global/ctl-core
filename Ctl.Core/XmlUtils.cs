using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Ctl
{
    public static class XmlUtils
    {
        /// <summary>
        /// Reads zero or more XNodes from a string.
        /// </summary>
        /// <param name="xml">The XML to convert.</param>
        /// <returns>Zero or more XNodes parsed from <paramref name="xml"/>.</returns>
        public static IEnumerable<XNode> ReadFrom(string xml)
        {
            if (string.IsNullOrEmpty(xml))
            {
                yield break;
            }

            var settings = new XmlReaderSettings
            {
                ConformanceLevel = ConformanceLevel.Fragment,
                IgnoreWhitespace = true
            };

            using (var stringReader = new StringReader(xml))
            using (var xmlReader = XmlReader.Create(stringReader, settings))
            {
                while (xmlReader.Read() && xmlReader.ReadState != ReadState.EndOfFile)
                {
                    yield return XNode.ReadFrom(xmlReader);
                }
            }
        }
    }
}
