using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using CLEA.EasySaveCore.L10N;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using JsonException = System.Text.Json.JsonException;

namespace CLEA.EasySaveCore.Utilities;

public class EasySaveConfiguration : IJsonSerializable
{
    private static readonly EasySaveConfiguration Instance = new EasySaveConfiguration();

    public JsonObject JsonSerialize()
    {
        JsonObject data = new JsonObject();
        
        data.Add("version", EasySaveCore.Version.ToString());
        data.Add("language", L10N.L10N.Get().GetLanguage().LangId);
        data.Add("dailyLogPath", "logs\\dailyLogs\\");
        data.Add("statusLogPath", "logs\\statusLogs\\");
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
        
        // Language
        data.TryGetPropertyValue("language", out JsonNode? lang);
        if (lang == null)
            throw new JsonException("Language not found in configuration file");
        if (Languages.SupportedLangs.Exists(li => li.LangId == lang.ToString()))
            L10N.L10N.Get().SetLanguage(Languages.SupportedLangs.Find(li => li.LangId == lang.ToString()) ?? Languages.EnUs);
        else
            throw new JsonException("Language not found in configuration file");

        // Daily log path
        data.TryGetPropertyValue("dailyLogPath", out JsonNode? dailyLogPath);
        if (dailyLogPath == null)
        {
            string path = "logs\\dailyLogs\\";
            Logger.Get().SetStatusLogPath(path);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
        if (Directory.Exists(dailyLogPath.ToString()))
        {
            string path = dailyLogPath.ToString() ?? "logs\\dailyLogs\\";
            Logger.Get().SetDailyLogPath(path);
        }
        else {
            string path = dailyLogPath.ToString() ?? "logs\\dailyLogs\\";
            Directory.CreateDirectory(path);
        }

        // Status log path
        data.TryGetPropertyValue("statusLogPath", out JsonNode? statusLogPath);
        if (statusLogPath == null)
        {
            string path = "logs\\statusLogs\\";
            Logger.Get().SetStatusLogPath(path);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        if (Directory.Exists(statusLogPath.ToString()))
        {
            Logger.Get().SetStatusLogPath(statusLogPath.ToString());
        }
        else
        {
            Directory.CreateDirectory(statusLogPath.ToString());
        }
    }
    
    public static void SaveConfiguration()
    {
        JsonObject data = Instance.JsonSerialize();

        JsonSerializerOptions options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        File.WriteAllText("config.json", data.ToJsonString(options));

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
            JsonObject data = Instance.JsonSerialize();
            File.WriteAllText("config.json", data.ToJsonString());
            EasySaveCore.Logger.Log(LogLevel.Debug, "Configuration file created");
            return;
        }

        JsonNode? jsonObject = JsonNode.Parse(json);
        
        if (jsonObject == null)
            throw new JsonException("Failed to parse configuration file");
        
        Instance.JsonDeserialize(jsonObject.AsObject());
        EasySaveCore.Logger.Log(LogLevel.Debug, "Successfully loaded configuration file");
    }
    
    public static EasySaveConfiguration Get()
    {
        return Instance;
    }
}