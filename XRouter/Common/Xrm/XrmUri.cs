﻿using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.XPath;
using System.Xml.Linq;

namespace XRouter.Common.Xrm
{
    /// <summary>
    /// Represents a URI of a XRM resource in form of an XPath.
    /// </summary>
    /// <remarks>
    /// XRM URIs can be serialized.
    /// </remarks>
    [DataContract]
    public class XrmUri
    {
        public static readonly XName GroupElementName = XName.Get("group");
        public static readonly XName ItemElementName = XName.Get("item");
        public static readonly XName NameAttributeName = XName.Get("name");
        public static readonly XName TypeAttributeName = XName.Get("type");

        /// <summary>
        /// XPath expression which selectes the resource from the XML resource
        /// storage.
        /// </summary>
        [DataMember]
        public string XPath { get; set; }

        /// <summary>
        /// Creates a new instance of XrmUri with an empty XPath expression.
        /// </summary>
        public XrmUri()
        {
            XPath = string.Empty;
        }

        /// <summary>
        /// Creates a new instance of XrmUri with a specified XPath expression.
        /// </summary>
        public XrmUri(string xpath)
        {
            XPath = xpath;
        }

        /// <summary>
        /// Creates a new instance of XrmUri with the XPath expression
        /// specified in the absolute path of an URI.
        /// </summary>
        public XrmUri(Uri uri)
        {
            XPath = uri.AbsolutePath;
        }

        public XDocument GetResource(XDocument xrmContent)
        {
            XElement xResourceContainer = xrmContent.Root.XPathSelectElement("." + XPath);
            if (xResourceContainer == null) {
                return null;
            }
            XElement xResource = xResourceContainer.Elements().FirstOrDefault();
            if (xResource == null) {
                return null;
            }
            XDocument result = new XDocument(xResource);
            return result;
        }

        /// <summary>
        /// Tests whether the XPath expression compiles well.
        /// </summary>
        /// <param name="xpath">XPath expression</param>
        /// <returns></returns>
        public static bool IsXPathValid(string xpath)
        {
            try {
                XPathExpression.Compile(xpath);
                return true;
            } catch {
                return false;
            }
        }
    }
}
