using System.Text.Json;
using System.Text.Json.Nodes;
using CLEA.EasySaveCore.L10N;
using CLEA.EasySaveCore.Models;
using CLEA.EasySaveCore.ViewModel;
using Microsoft.Extensions.Logging;
using JsonException = System.Text.Json.JsonException;

namespace CLEA.EasySaveCore.Utilities;

/// <summary>
/// Represents the configuration settings for the EasySave application.
/// It includes all the necessary settings for the application to run correctly.
/// When the application starts, it loads the configuration from a JSON file.
/// If the file does not exist or is empty, it creates a default configuration.
/// </summary>
public class EasySaveConfiguration<TJob> : IJsonSerializable where TJob : IJob
{
    private const string ConfigPath = "config.json";
    private static readonly EasySaveConfiguration<TJob> Instance = new EasySaveConfiguration<TJob>();
    private static Logger<TJob> Logger => Logger<TJob>.Get();

    /// <summary>
    /// Serialize the configuration to a JSON object.
    /// All properties have default values and are not null to avoid serialization issues.
    /// </summary>
    /// <returns></returns>
    public JsonObject JsonSerialize()
    {
        Console.WriteLine(EasySaveCore<TJob>.Get());
        
        JsonArray jobs = new JsonArray();
        foreach (TJob job in EasySaveViewModel<TJob>.Get().JobManager.GetJobs())
            if (job is IJsonSerializable jsonSerializable)
                jobs.Add(jsonSerializable.JsonSerialize());
        
        JsonObject data = new JsonObject
        {
            { "version", EasySaveCore<IJob>.Version.ToString() },
            { "language", L10N<TJob>.Get().GetLanguage().LangId },
            { "dailyLogPath", Logger.DailyLogPath },
            { "statusLogPath", Logger.StatusLogPath },
            { "jobs", jobs }
        };

        return data;
    }

    /// <summary>
    /// This method deserializes the JSON object into the configuration properties.
    /// All properties are validated to ensure they are not null and have valid values.
    /// </summary>
    /// <param name="data">The JSON object representing the configuration to deserialize.</param>
    public void JsonDeserialize(JsonObject data)
    {
        // Version
        data.TryGetPropertyValue("version", out JsonNode? version);
        if (version == null)
            throw new JsonException("Version not found in configuration file");
        if (version.ToString() != EasySaveCore<TJob>.Version.ToString())
            throw new JsonException("Version mismatch in configuration file");

        // Daily log path
        data.TryGetPropertyValue("dailyLogPath", out JsonNode? dailyLogPath);
        if (dailyLogPath != null && dailyLogPath.ToString().IndexOfAny(Path.GetInvalidPathChars()) == -1)
            Logger.DailyLogPath = dailyLogPath.ToString();
        if (!Directory.Exists(Logger.DailyLogPath))
            Directory.CreateDirectory(Logger.DailyLogPath);

        // Status log path
        data.TryGetPropertyValue("statusLogPath", out JsonNode? statusLogPath);
        if (statusLogPath != null && statusLogPath.ToString().IndexOfAny(Path.GetInvalidPathChars()) == -1)
            Logger.StatusLogPath = statusLogPath.ToString();
        if (!Directory.Exists(Logger.StatusLogPath))
            Directory.CreateDirectory(Logger.StatusLogPath);
        
        // Language
        data.TryGetPropertyValue("language", out JsonNode? lang);
        if (lang == null)
            throw new JsonException("Language property not found in configuration file");
        if (Languages.SupportedLangs.Exists(li => li.LangId == lang.ToString()))
            L10N<TJob>.Get().SetLanguage(Languages.SupportedLangs.Find(li => li.LangId == lang.ToString()) ?? Languages.EnUs);
        else
            throw new JsonException($"Language '{lang}' is not supported");
        
        // Jobs
        data.TryGetPropertyValue("jobs", out JsonNode? jobs);
        if (jobs != null)
        {
            foreach (JsonNode? job in jobs.AsArray())
                if (job is JsonObject jobObject)
                    EasySaveViewModel<IJob>.Get().JobManager.AddJob(jobObject);
        }
        else
            throw new JsonException("Jobs not found in configuration file");
    }
    
    /// <summary>
    /// Saves the current configuration to a JSON file.
    /// As <see cref="EasySaveConfiguration"/> is a singleton, it can be a static method.
    /// </summary>
    public static void SaveConfiguration()
    {
        JsonObject data = Instance.JsonSerialize();
        File.WriteAllText(ConfigPath, data.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
    }
    
    /// <summary>
    /// Loads the configuration from a JSON file.
    /// As soon as the application starts, it loads the configuration from a JSON file.
    /// As <see cref="EasySaveConfiguration"/> is a singleton, it can be a static method.
    /// </summary>
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
            // Instance.JsonDeserialize(Instance.JsonSerialize()); // Create default configuration
            SaveConfiguration();
            
            Logger.LogInternal(LogLevel.Debug, "Configuration file successfully created");
            return;
        }

        JsonNode? configurationJson = JsonNode.Parse(json);

        if (configurationJson == null)
            throw new JsonException("Failed to parse configuration file");
        
        Instance.JsonDeserialize(configurationJson.AsObject());
        Logger.LogInternal(LogLevel.Debug, "Successfully loaded configuration file");
    }
}