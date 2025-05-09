using System.Text.Json.Nodes;

namespace CLEA.EasySaveCore.utilities;

public interface IJsonSerializable
{
    public JsonObject Serialize();
    public void Deserialize(JsonObject data);
}