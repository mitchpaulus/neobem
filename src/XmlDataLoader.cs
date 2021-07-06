using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace src
{
    public class XmlDataLoader
    {
        public IdfPlusObjectExpression Load(string xml)
        {
            XmlDocument document = new();
            document.LoadXml(xml);

            // The assumption is that the XML data has a single root element.
            var firstElement = document.ChildElements().First();
            var structure = ParseXmlNode(firstElement);
            return structure;
        }

        private IdfPlusObjectExpression ParseXmlNode(XmlElement node)
        {
            IdfPlusObjectExpression structure = new IdfPlusObjectExpression();
            var attributes = node.AttributeList();

            foreach (var attribute in attributes)
            {
                Expression parsedExpression = Expression.Parse(attribute.Value);
                structure.Members[attribute.Name] = parsedExpression;
            }

            var groupedChildren = node.ChildElements().GroupBy(elem => elem.Name);

            foreach (var elemGroup in groupedChildren)
            {
                if (elemGroup.Count() == 1)
                {
                    structure.Members[elemGroup.Key] = ParseXmlNode(elemGroup.First());
                }
                else
                {
                    var expressions = elemGroup.Select(ParseXmlNode).Cast<Expression>().ToList();
                    structure.Members[elemGroup.Key] = new ListExpression(expressions);
                }
            }

            structure.Members["value"] = Expression.Parse(node.ElementValue());

            return structure;
        }
    }
    public static class XmlExtensions
    {
        public static List<XmlAttribute> AttributeList(this XmlNode node)
        {
            if (node.Attributes == null) return new List<XmlAttribute>();
            List<XmlAttribute> attributeList = new();
            for (int i = 0; i < node.Attributes.Count; i++) attributeList.Add(node.Attributes[i]);
            return attributeList;
        }

        public static string ElementValue(this XmlNode node)
        {
            // A element can have several XmlText nodes making up the 'value'.
            // For example, <element> text <br></br> more text</element>
            // has 2 XmlText nodes. I decided that one reasonable way to handle this
            // would be to trim all individual portions and then separate with a space.
            // I expect that this situation would occur more in document XML vs the XML that
            // is mostly data for buildings.
            List<string> nodes = new();
            for (int i = 0; i < node.ChildNodes.Count; i++)
            {
                XmlNode childNode = node.ChildNodes[i];
                if (childNode is XmlText text) nodes.Add(text.Value);
            }

            return string.Join(" ", nodes.Select(s => s.Trim()));
        }

        // The enumerator for ChildNodes is not typed, so need helpful extension methods like this
        public static List<XmlElement> ChildElements(this XmlNode node)
        {
            List<XmlElement> nodes = new();
            for (int i = 0; i < node.ChildNodes.Count; i++)
            {
                XmlNode childNode = node.ChildNodes[i];
                if (childNode is XmlElement element) nodes.Add(element);
            }
            return nodes;
        }
    }
}