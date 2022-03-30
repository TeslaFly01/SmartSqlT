using System.Xml;

namespace TSqlFormatter.Interfaces
{
    interface IParseTree
    {
        XmlDocument ToXmlDoc();
    }
}
