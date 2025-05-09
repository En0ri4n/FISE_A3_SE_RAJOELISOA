using System.Text.Json.Nodes;
using System.Xml;
using CLEA.EasySaveCore.Utilities;

namespace CLEA.EasySaveCore.Models;

public abstract class JobTask : IJsonSerializable, IXmlSerializable
{
    private readonly List<Property<dynamic>> _properties = new List<Property<dynamic>>();
    
    public List<Property<dynamic>> GetProperties()
    {
        return _properties;
    }

    protected JobTask(string name)
    {
        _properties.Add(new Property<dynamic>("name", name));
    }

    public string GetName()
    {
        Property<dynamic>? property = _properties.Find(tp => tp.Name == "name");

        if (property == null)
            throw new Exception("Name property not found");
        
        return property.Value;
    }

    public bool UpdadeProperty(string name, dynamic value)
    {
        Property<dynamic>? property = _properties.Find(tp => tp.Name == name);

        if (property == null)
            return false;
        
        property.Value = value;
        return true;
    }
    
    public abstract void ExecuteTask();

    public abstract JsonObject JsonSerialize();

    public abstract void JsonDeserialize(JsonObject data);

    public abstract XmlElement XmlSerialize();
    public abstract void XmlDeserialize(XmlElement data);
}