using System.Xml;

namespace CLEA.EasySaveCore.Utilities
{
    public interface IXmlSerializable
    {
        public XmlElement XmlSerialize(XmlDocument parent);
        public void XmlDeserialize(XmlElement data);
    }
}