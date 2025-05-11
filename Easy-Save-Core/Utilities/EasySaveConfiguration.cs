using System.Text.Json;
using System.Text.Json.Nodes;
using CLEA.EasySaveCore.L10N;
using Microsoft.Extensions.Logging;
using JsonException = System.Text.Json.JsonException;

namespace CLEA.EasySaveCore.Utilities;

public class EasySaveConfiguration : IJsonSerializable
{
    private const string ConfigPath = "config.json";
    private static readonly EasySaveConfiguration Instance = new EasySaveConfiguration();

    public JsonObject JsonSerialize()
    {
        JsonObject data = new JsonObject
        {
            { "version", EasySaveCore.Version.ToString() },
            { "language", L10N.L10N.Get().GetLanguage().LangId },
            { "dailyLogPath", Logger.Get().DailyLogPath },
            { "statusLogPath", Logger.Get().StatusLogPath }
        };

        return data;
    }

    public void JsonDeserialize(JsonObject data)
    {
        // Version
        data.TryGetPropertyValue("version", out JsonNode? version);
        if (version == null)
            throw new JsonException("Version not found in configuration file");
        if (version.ToString() != EasySaveCore.Version.ToString())
            throw new JsonException("Version mismatch in configuration file");

        // Daily log path
        data.TryGetPropertyValue("dailyLogPath", out JsonNode? dailyLogPath);
        if (dailyLogPath != null && dailyLogPath.ToString().IndexOfAny(Path.GetInvalidPathChars()) == -1)
            Logger.Get().DailyLogPath = dailyLogPath.ToString();
        if (!Directory.Exists(Logger.Get().DailyLogPath))
            Directory.CreateDirectory(Logger.Get().DailyLogPath);

        // Status log path
        data.TryGetPropertyValue("statusLogPath", out JsonNode? statusLogPath);
        if (statusLogPath != null && statusLogPath.ToString().IndexOfAny(Path.GetInvalidPathChars()) == -1)
            Logger.Get().StatusLogPath = statusLogPath.ToString();
        if (!Directory.Exists(Logger.Get().StatusLogPath))
            Directory.CreateDirectory(Logger.Get().StatusLogPath);
        
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
        JsonObject data = Instance.JsonSerialize();
        File.WriteAllText(ConfigPath, data.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
    }
    
    public static void LoadConfiguration()
    {
        FileStream fileStream = new FileStream(ConfigPath, FileMode.OpenOrCreate);
        StreamReader streamReader = new StreamReader(fileStream);

        string json = streamReader.ReadToEnd();
        
        streamReader.Close();
        fileStream.Close();
        
        if (json == null)
            throw new FileNotFoundException("Configuration file not found", ConfigPath);

        if (json.Length == 0)
        {
            Instance.JsonDeserialize(Instance.JsonSerialize()); // Create default configuration
            SaveConfiguration();
            
            Logger.Log(LogLevel.Debug, "Configuration file successfully created");
            return;
        }

        JsonNode? configurationJson = JsonNode.Parse(json);

        if (configurationJson == null)
            throw new JsonException("Failed to parse configuration file");
        
        Instance.JsonDeserialize(configurationJson.AsObject());
        Logger.Log(LogLevel.Debug, "Successfully loaded configuration file");
    }
}