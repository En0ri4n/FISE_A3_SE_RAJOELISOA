using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using CLEA.EasySaveCore;
using CLEA.EasySaveCore.Jobs.Backup;
using CLEA.EasySaveCore.L10N;
using CLEA.EasySaveCore.Utilities;
using CLEA.EasySaveCore.External;
using EasySaveCore.Jobs.Backup.ViewModels;
using EasySaveCore.Models;
using Microsoft.Extensions.Logging;

namespace EasySaveCore.Jobs.Backup.Configurations
{
    public class BackupJobConfiguration : EasySaveConfigurationBase
    {
        private const string ConfigPath = "config.json";
        private static readonly BackupJobConfiguration Instance = new BackupJobConfiguration();

        private static Logger Logger => Logger.Get();

        private ObservableCollection<string> _extensionsToEncrypt;
        public ObservableCollection<string> ExtensionsToEncrypt => _extensionsToEncrypt;

        private ObservableCollection<string> _processesToBlacklist;
        public ObservableCollection<string> ProcessesToBlacklist => _processesToBlacklist;

        private string _encryptionKey = ExternalEncryptor.ProcessEncryptionKey("SuperProtectedKey-CLEA-@.NET-2025");
        public string EncryptionKey { 
            get => _encryptionKey;
            set
            {
                _encryptionKey = value;
                SaveConfiguration();
            } 
        }

        private BackupJobConfiguration()
        {
            _extensionsToEncrypt = new ObservableCollection<string>();
            _processesToBlacklist = new ObservableCollection<string>();

            _extensionsToEncrypt.CollectionChanged += (sender, args) => SaveConfiguration();
            _processesToBlacklist.CollectionChanged += (sender, args) => SaveConfiguration();
        }

        /// <summary>
        /// Serialize the configuration to a JSON object.
        /// All properties have default values and are not null to avoid serialization issues.
        /// </summary>
        /// <returns></returns>
        public override JsonObject JsonSerialize()
        {
            JsonArray jobs = new JsonArray();
            foreach (BackupJob job in BackupJobViewModel.Get().JobManager.GetJobs())
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
                { "version", EasySaveCore<BackupJob, BackupJobManager, BackupJobConfiguration>.Version.ToString() },
                { "language", L10N<BackupJob>.Get().GetLanguage().LangId },
                { "dailyLogPath", Logger.DailyLogPath },
                { "statusLogPath", Logger.StatusLogPath },
                { "dailyLogFormat", Logger.DailyLogFormat.ToString() },
                { "encryptionKey", _encryptionKey },
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
        public override void JsonDeserialize(JsonObject data)
        {
            // Version
            data.TryGetPropertyValue("version", out JsonNode? version);
            if (version == null)
                throw new JsonException("Version not found in configuration file");
            if (version.ToString() != EasySaveCore<BackupJob, BackupJobManager, BackupJobConfiguration>.Version.ToString())
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

            // Encryption Key
            data.TryGetPropertyValue("encryptionKey", out JsonNode? encryptionKey);
            if (encryptionKey != null)
                _encryptionKey = encryptionKey.ToString();

            // Encrypted file extensions
            ObservableCollection<string> extensionsToEncryptList = new ObservableCollection<string>();
            data.TryGetPropertyValue("extensionsToEncrypt", out JsonNode? extensionsToEncrypt);
            if (extensionsToEncrypt != null)
            {
                foreach (JsonNode? format in extensionsToEncrypt.AsArray())
                    if (format is JsonValue formatValue)
                        extensionsToEncryptList.Add(formatValue.ToString());

                extensionsToEncryptList.CollectionChanged += (sender, args) => SaveConfiguration();
                _extensionsToEncrypt = extensionsToEncryptList;
            }
            else
                throw new JsonException("Encrypted file extensions not found in configuration file");

            // Processes to blacklist
            ObservableCollection<string> processesToBlacklistList = new ObservableCollection<string>();
            data.TryGetPropertyValue("processesToBlacklist", out JsonNode? processesToBlacklist);
            if (processesToBlacklist != null)
            {
                foreach (JsonNode? process in processesToBlacklist.AsArray())
                    if (process is JsonValue processValue)
                        processesToBlacklistList.Add(processValue.ToString());

                processesToBlacklistList.CollectionChanged += (sender, args) => SaveConfiguration();
                _processesToBlacklist = processesToBlacklistList;
            }
            else
                throw new JsonException("Processes to Blacklist not found in configuration file");

            // Jobs
            data.TryGetPropertyValue("jobs", out JsonNode? jobs);
            if (jobs != null)
            {
                foreach (JsonNode? job in jobs.AsArray())
                    if (job is JsonObject jobObject)
                        BackupJobViewModel.Get().JobManager.AddJob(jobObject);
            }
            else
                throw new JsonException("Jobs not found in configuration file");

            // Language
            data.TryGetPropertyValue("language", out JsonNode? lang);
            if (lang == null)
                throw new JsonException("Language property not found in configuration file");
            if (Languages.SupportedLangs.Exists(li => li.LangId == lang.ToString()))
                L10N<BackupJob>.Get().SetLanguage(Languages.SupportedLangs.Find(li => li.LangId == lang.ToString()) ??
                                             Languages.EnUs);
            else
                throw new JsonException($"Language '{lang}' is not supported");
        }

        public override void SaveConfiguration()
        {
            JsonObject data = Instance.JsonSerialize();
            File.WriteAllText(ConfigPath, data.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
        }

        public override void LoadConfiguration()
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

        public static BackupJobConfiguration Get()
        {
            return Instance;
        }
    }
}