using System.Text.Json.Nodes;

namespace CLEA.EasySaveCore.Utilities
{
    public interface IJsonSerializable
    {
        public JsonObject JsonSerialize();
        public void JsonDeserialize(JsonObject data);
    }
}