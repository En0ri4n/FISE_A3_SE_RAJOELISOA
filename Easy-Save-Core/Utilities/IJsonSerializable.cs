using System.Text.Json.Nodes;
using CLEA.EasySaveCore.Models;

namespace CLEA.EasySaveCore.Utilities
{
    public interface IJsonSerializable
    {
        public JsonObject JsonSerialize();
        public void JsonDeserialize(JsonObject data);
    }
}