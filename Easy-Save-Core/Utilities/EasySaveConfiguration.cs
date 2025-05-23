using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Nodes;
using CLEA.EasySaveCore.L10N;
using CLEA.EasySaveCore.Models;
using CLEA.EasySaveCore.ViewModel;
using Microsoft.Extensions.Logging;
using JsonException = System.Text.Json.JsonException;

namespace CLEA.EasySaveCore.Utilities
{
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
        public EasySaveConfiguration()
        {
            ExtensionsToEncrypt.CollectionChanged += (sender, args) =>
            {
                SaveConfiguration();
            };

            ProcessesToBlacklist.CollectionChanged += (sender, args) =>
            {
                SaveConfiguration();
            };
        }

        private static Logger Logger => Logger.Get();
        public static EasySaveConfiguration<TJob> Get()
        {
            return Instance;
        }

        private ObservableCollection<string> _extensionsToEncrypt = new ObservableCollection<string>();

        public ObservableCollection<string> ExtensionsToEncrypt
        {
            get => _extensionsToEncrypt;
            set
            {
                _extensionsToEncrypt = value;
                SaveConfiguration();
            }
        }

        private ObservableCollection<string> _processesToBlacklist = new ObservableCollection<string>();

        public ObservableCollection<string> ProcessesToBlacklist
        {
            get => _processesToBlacklist;
            set
            {
                _processesToBlacklist = value;
                SaveConfiguration();
            }
        }

        private List<string> _extensionsToEncrypt = new List<string>();
        public List<string> ExtensionsToEncrypt
        {
            get => _extensionsToEncrypt;
            set
            {
                _extensionsToEncrypt = value;
                SaveConfiguration();
            }
        }

        private ObservableCollection<string> _priorityProcesses = new ObservableCollection<string>();
        public ObservableCollection<string> PriorityProcesses
        {
            get => _priorityProcesses;
            set
            {
                _priorityProcesses = value;
                SaveConfiguration();
            }
        }

        public static EasySaveConfiguration<TJob> Get()
        {
            return Instance;
        }
        
        public static bool IsEncryptorLoaded()
        {
            return Type.GetType("CLEA.Encryptor.Encryptor") != null;
        }

        /// <summary>
        /// Serialize the configuration to a JSON object.
        /// All properties have default values and are not null to avoid serialization issues.
        /// </summary>
        /// <returns></returns>
        public JsonObject JsonSerialize()
        {
            JsonArray jobs = new JsonArray();
            foreach (TJob job in EasySaveViewModel<TJob>.Get().JobManager.GetJobs())
                if (job is IJsonSerializable jsonSerializable)
                    jobs.Add(jsonSerializable.JsonSerialize());

            JsonArray extensionsToEncrypt = new JsonArray();
            foreach (string extension in ExtensionsToEncrypt)
                extensionsToEncrypt.Add(extension);

            JsonArray processesToBlacklist = new JsonArray();
            foreach (string process in ProcessesToBlacklist)
                processesToBlacklist.Add(process);

            JsonObject data = new JsonObject
            {
                { "version", EasySaveCore<TJob>.Version.ToString() },
                { "language", L10N<TJob>.Get().GetLanguage().LangId },
                { "dailyLogPath", Logger.DailyLogPath },
                { "statusLogPath", Logger.StatusLogPath },
                { "dailyLogFormat", Logger.DailyLogFormat.ToString() },
                { "extensionsToEncrypt",  extensionsToEncrypt},
                { "processesToBlacklist",  processesToBlacklist},
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

            // Daily log Format
            data.TryGetPropertyValue("dailyLogFormat", out JsonNode? dailyLogFormat);
            if (dailyLogFormat != null)
                Logger.DailyLogFormat = (Format)Enum.Parse(typeof(Format), dailyLogFormat.ToString());
            if (!Directory.Exists(Logger.DailyLogPath))
                Directory.CreateDirectory(Logger.DailyLogPath);

            // Encrypted file extensions
            data.TryGetPropertyValue("extensionsToEncrypt", out JsonNode? extensionsToEncrypt);
            if (extensionsToEncrypt != null)
            {
                _extensionsToEncrypt.Clear();
                foreach (JsonNode? format in extensionsToEncrypt.AsArray())
                {
                    if (format is JsonValue formatValue)
                        _extensionsToEncrypt.Add(formatValue.ToString());
                }
            }
            else
                throw new JsonException("Encrypted file extensions not found in configuration file");

            // Processes to blacklist
            data.TryGetPropertyValue("processesToBlacklist", out JsonNode? processesToBlacklist);
            if (processesToBlacklist != null)
            {
                _processesToBlacklist.Clear();
                foreach (JsonNode? process in processesToBlacklist.AsArray())
                {
                    if (process is JsonValue processValue)
                        _processesToBlacklist.Add(processValue.ToString());
                }
            }
            else
                throw new JsonException("Processes to Blacklist not found in configuration file");

            // Jobs
            data.TryGetPropertyValue("jobs", out JsonNode? jobs);
            if (jobs != null)
            {
                foreach (JsonNode? job in jobs.AsArray())
                    if (job is JsonObject jobObject)
                        EasySaveViewModel<TJob>.Get().JobManager.AddJob(jobObject);
            }
            else
                throw new JsonException("Jobs not found in configuration file");
        
        // Encrypted file extensions
        data.TryGetPropertyValue("extensionsToEncrypt", out JsonNode? extensionsToEncrypt);
        if (extensionsToEncrypt != null)
        {
            _extensionsToEncrypt.Clear();
            foreach(JsonNode? format in extensionsToEncrypt.AsArray())
            {
                if (format is JsonValue formatValue)
                    _extensionsToEncrypt.Add(formatValue.ToString());
            }
        }
        else
            throw new JsonException("Encrypted file extensions not found in configuration file");
        
        // Priority processes
        data.TryGetPropertyValue("priorityProcesses", out JsonNode? priorityProcesses);
        if (priorityProcesses != null)
        {
            _priorityProcesses.Clear();
            foreach(JsonNode? process in priorityProcesses.AsArray())
            {
                if (process is JsonValue processValue)
                    _priorityProcesses.Add(processValue.ToString());
            }
        }
        else
            throw new JsonException("Priority processes not found in configuration file");

            // Language
            // ALWAYS AT THE END, BECAUSE IT CALLS SAVE CONFIGURATION
            data.TryGetPropertyValue("language", out JsonNode? lang);
            if (lang == null)
                throw new JsonException("Language property not found in configuration file");
            if (Languages.SupportedLangs.Exists(li => li.LangId == lang.ToString()))
                L10N<TJob>.Get().SetLanguage(Languages.SupportedLangs.Find(li => li.LangId == lang.ToString()) ??
                                             Languages.EnUs);
            else
                throw new JsonException($"Language '{lang}' is not supported");
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
}