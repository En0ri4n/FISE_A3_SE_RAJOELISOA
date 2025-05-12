using System.Xml;
using CLEA.EasySaveCore.Models;

namespace CLEA.EasySaveCore.Utilities;

public interface IXmlSerializable
{
    public XmlElement XmlSerialize();
    public void XmlDeserialize(XmlElement data);
}