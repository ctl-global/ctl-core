/*
    Copyright (c) 2014, CTL Global, Inc.
    Copyright (c) 2012, iD Commerce + Logistics
    All rights reserved.

    Redistribution and use in source and binary forms, with or without modification, are permitted
    provided that the following conditions are met:

    Redistributions of source code must retain the above copyright notice, this list of conditions
    and the following disclaimer. Redistributions in binary form must reproduce the above copyright
    notice, this list of conditions and the following disclaimer in the documentation and/or other
    materials provided with the distribution.

    THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR
    IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
    FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR
    CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
    CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
    SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
    THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
    OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
    POSSIBILITY OF SUCH DAMAGE.
*/

using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Ctl.Extensions
{
    /// <summary>
    /// Provides extension methods for XLinq.
    /// </summary>
    public static class XLinqExtensions
    {
        /// <summary>
        /// Converts a SqlXml response into zero or more XNodes.
        /// </summary>
        /// <param name="xml">The SqlXml to convert.</param>
        /// <returns>Zero or more XNodes pertaining to the SqlXml.</returns>
        public static IEnumerable<XNode> ToXNodes(this SqlXml xml)
        {
            if (xml?.IsNull != false)
            {
                yield break;
            }

            var settings = new XmlReaderSettings
            {
                ConformanceLevel = ConformanceLevel.Fragment,
                IgnoreWhitespace = true
            };

            using (var stringReader = new StringReader(xml.Value))
            using (var xmlReader = XmlReader.Create(stringReader, settings))
            {
                while (xmlReader.ReadState != ReadState.EndOfFile)
                {
                    yield return XNode.ReadFrom(xmlReader);
                }
            }
        }

        /// <summary>
        /// Filters source elements, returning only those which have an attribute with a specific value.
        /// </summary>
        /// <param name="source">The elements to filter.</param>
        /// <param name="name">The name of the attribute to find.</param>
        /// <param name="value">The value to compare.</param>
        /// <returns>A collection of filtered elements.</returns>
        public static IEnumerable<XElement> HasAttribute(this IEnumerable<XElement> source, XName name, string value)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (name == null) throw new ArgumentNullException("name");

            return source.Where(x => x.HasAttribute(name, value));
        }

        /// <summary>
        /// Filters source elements, returning only those which have an attribute with a specific value.
        /// </summary>
        /// <param name="source">The elements to filter.</param>
        /// <param name="name">The name of the attribute to find.</param>
        /// <param name="value">The value to compare.</param>
        /// <param name="comparer">A comparer used to test the values.</param>
        /// <returns>A collection of filtered elements.</returns>
        public static IEnumerable<XElement> HasAttribute(this IEnumerable<XElement> source, XName name, string value, IEqualityComparer<string> comparer)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (name == null) throw new ArgumentNullException("name");
            if (comparer == null) throw new ArgumentNullException("comparer");

            return source.Where(x => x.HasAttribute(name, value, comparer));
        }

        /// <summary>
        /// Determines if an element has an attribute with a specific value.
        /// </summary>
        /// <param name="source">The element to test.</param>
        /// <param name="name">The name of the attribute to find.</param>
        /// <param name="value">The value to compare.</param>
        /// <returns>If the element has a matching attribute, true. Otherwise, false.</returns>
        public static bool HasAttribute(this XElement source, XName name, string value)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (name == null) throw new ArgumentNullException("name");

            return source.Attributes(name).Select(y => y.Value).Contains(value);
        }

        /// <summary>
        /// Determines if an element has an attribute with a specific value.
        /// </summary>
        /// <param name="source">The element to test.</param>
        /// <param name="name">The name of the attribute to find.</param>
        /// <param name="value">The value to compare.</param>
        /// <param name="comparer">A comparer used to test the values.</param>
        /// <returns>If the element has a matching attribute, true. Otherwise, false.</returns>
        public static bool HasAttribute(this XElement source, XName name, string value, IEqualityComparer<string> comparer)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (name == null) throw new ArgumentNullException("name");
            if (comparer == null) throw new ArgumentNullException("comparer");

            return source.Attributes(name).Select(y => y.Value).Contains(value, comparer);
        }

#if NETSTANDARD2_0 || NET451

        /// <summary>
        /// Adds or updates a child element with a new value, respecting ordering defined by a schema.
        /// </summary>
        /// <param name="parentElement">The parent element to find the child within.</param>
        /// <param name="schemaSet">The schema set used for validation.</param>
        /// <param name="childName">The child element's local name.</param>
        /// <param name="value">The new value to assign the child element. If null, the element is removed.</param>
        public static void AddOrUpdateChild(this XElement parentElement, XmlSchemaSet schemaSet, string childName, object value)
        {
            if (parentElement == null) throw new ArgumentNullException("parentElement");
            if (schemaSet == null) throw new ArgumentNullException("schemaSet");
            if (childName == null) throw new ArgumentNullException("childName");

            XElement childElement = parentElement.Elements().SingleOrDefault(child => child.Name.LocalName == childName);

            if (childElement != null)
            {
                // Element already present. Just update and move on.

                if (value != null) childElement.SetValue(value);
                else childElement.Remove();
                return;
            }

            // Element not present.

            if (value == null)
            {
                // No value to add, so just return.
                return;
            }

            // Try to add an element with that value.

            IXmlSchemaInfo xsdInfo = parentElement.GetSchemaInfo();

            if (xsdInfo == null)
            {
                // No schema info -- just add it.

                parentElement.Add(new XElement(childName, value));
                return;
            }

            // Found a schema. Determine in what spot to add it.

            XmlSchemaElement xsdElement = xsdInfo.SchemaElement;
            XmlSchemaComplexType xsdComplex = xsdElement.SchemaType as XmlSchemaComplexType;

            if (xsdComplex == null)
            {
                throw new ArgumentException("XElement must be a complex type.", "e");
            }

            XmlSchemaGroupBase xsdGroup = xsdComplex.Particle as XmlSchemaGroupBase;

            if (xsdGroup == null)
            {
                throw new ArgumentException("XElement must have child elements.", "e");
            }

            XmlSchemaElement xsdChildElement = xsdGroup.Items.OfType<XmlSchemaElement>().SingleOrDefault(x => x.Name == childName);

            if (xsdChildElement == null)
            {
                throw new InvalidOperationException("Schema does not allow element \"" + parentElement.Name.LocalName + "\" to have a child of \"" + childName + "\".");
            }

            if (xsdGroup is XmlSchemaChoice)
            {
                throw new InvalidOperationException("Schema shows XElement content is a choice that does not match the new child. Original choice not overridden.");
            }

            childElement = new XElement(XName.Get(xsdChildElement.QualifiedName.Name, xsdChildElement.QualifiedName.Namespace), value);

            if (xsdGroup is XmlSchemaAll)
            {
                // Add is unordered. Just add it to the end.

                parentElement.Add(childElement);
            }
            else if (xsdGroup is XmlSchemaSequence)
            {
                // a sequence demands a specific order. find the element to insert after.

                var previousElement = xsdGroup.Items.OfType<XmlSchemaElement>()
                    .TakeWhile(child => child.Name != childName)
                    .Reverse()
                    .SelectMany(xsdChild => parentElement.Elements(XName.Get(xsdChild.QualifiedName.Name, xsdChild.QualifiedName.Namespace)))
                    .FirstOrDefault();

                if (previousElement != null)
                {
                    previousElement.AddAfterSelf(childElement);
                }
                else
                {
                    parentElement.AddFirst(childElement);
                }
            }
            else
            {
                throw new InvalidOperationException("Unknown complex schema grouping \"" + xsdGroup.GetType().Name + "\".");
            }
        }

#endif
    }
}
