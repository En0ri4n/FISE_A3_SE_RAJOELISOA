using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using CLEA.EasySaveCore.L10N;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using JsonException = System.Text.Json.JsonException;

namespace CLEA.EasySaveCore.utilities;

public class EasySaveConfiguration : IJsonSerializable
{
    private static readonly EasySaveConfiguration Instance = new EasySaveConfiguration();

    public JsonObject Serialize()
    {
        JsonObject data = new JsonObject();
        
        data.Add("version", EasySaveCore.Version.ToString());
        data.Add("language", L10N.L10N.Get().GetLanguage().LangId);
        return data;
    }

    public void Deserialize(JsonObject data)
    {
        // Version
        data.TryGetPropertyValue("version", out JsonNode? version);
        if (version == null)
            throw new JsonException("Version not found in configuration file");
        if (version.ToString() != EasySaveCore.Version.ToString())
            throw new JsonException("Version mismatch in configuration file");
        
        // Language
        data.TryGetPropertyValue("language", out JsonNode? lang);
        if (lang == null)
            throw new JsonException("Language not found in configuration file");
        if (Languages.SupportedLangs.Exists(li => li.LangId == lang.ToString()))
            L10N.L10N.Get().SetLanguage(Languages.SupportedLangs.Find(li => li.LangId == lang.ToString()) ?? Languages.EnUs);
        else
            throw new JsonException("Language not found in configuration file");
    }
    
    public static void SaveConfiguration()
    {
        JsonObject data = Instance.Serialize();
        File.WriteAllText("config.json", data.ToJsonString());
        EasySaveCore.Logger.Log(LogLevel.Debug, "Configuration file saved");
    }
    
    public static void LoadConfiguration()
    {
        FileStream fileStream = new FileStream("config.json", FileMode.OpenOrCreate);
        StreamReader streamReader = new StreamReader(fileStream);
        
        string json = streamReader.ReadToEnd();
        
        streamReader.Close();
        fileStream.Close();
        
        if (json == null)
            throw new FileNotFoundException("Configuration file not found", "config.json");

        if (json == String.Empty)
        {
            JsonObject data = Instance.Serialize();
            File.WriteAllText("config.json", data.ToJsonString());
            EasySaveCore.Logger.Log(LogLevel.Debug, "Configuration file created");
            return;
        }

        JsonNode? jsonObject = JsonNode.Parse(json);
        
        if (jsonObject == null)
            throw new JsonException("Failed to parse configuration file");
        
        Instance.Deserialize(jsonObject.AsObject());
        EasySaveCore.Logger.Log(LogLevel.Debug, "Successfully loaded configuration file");
    }
    
    public static EasySaveConfiguration Get()
    {
        return Instance;
    }
}