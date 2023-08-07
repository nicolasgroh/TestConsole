using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace TestConsole
{
    public static class XmlDocumentHelper
    {
        public static XmlDeclaration AppendDeclaration(this XmlDocument xmlDocument, string version, string encoding, string standalone)
        {
            var xmlDeclaration = xmlDocument.CreateXmlDeclaration(version, encoding, standalone);

            xmlDocument.AppendChild(xmlDeclaration);

            return xmlDeclaration;
        }

        public static XmlElement AppendElement(this XmlDocument xmlDocument, string name, string innerText = null)
        {
            var element = xmlDocument.CreateElement(name);

            if (innerText != null) element.InnerText = innerText;

            xmlDocument.AppendChild(element);

            return element;
        }

        public static XmlElement AppendElement(this XmlElement xmlElement, string name, string innerText = null)
        {
            if (xmlElement.OwnerDocument == null) throw new ArgumentNullException();

            var element = xmlElement.OwnerDocument.CreateElement(name);

            if (innerText != null) element.InnerText = innerText;

            xmlElement.AppendChild(element);

            return element;
        }
    }
}