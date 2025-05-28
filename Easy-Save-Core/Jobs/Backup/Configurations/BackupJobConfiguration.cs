using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using CLEA.EasySaveCore.External;
using CLEA.EasySaveCore.Translations;
using CLEA.EasySaveCore.Utilities;
using Microsoft.Extensions.Logging;

namespace EasySaveCore.Jobs.Backup.Configurations
{
    public class BackupJobConfiguration : EasySaveConfigurationBase
    {
        private const string ConfigPath = "config.json";

        private string _encryptionKey = ExternalEncryptor.ProcessEncryptionKey("SuperProtectedKey-CLEA-@.NET-2025");

        public BackupJobConfiguration()
        {
            ExtensionsToEncrypt = new ObservableCollection<string>();
            ProcessesToBlacklist = new ObservableCollection<string>();

            ExtensionsToEncrypt.CollectionChanged += (sender, args) => SaveConfiguration();
            ProcessesToBlacklist.CollectionChanged += (sender, args) => SaveConfiguration();
        }

        private static Logger Logger => Logger.Get();
        public ObservableCollection<string> ExtensionsToEncrypt { get; private set; }

        public ObservableCollection<string> ProcessesToBlacklist { get; private set; }

        public string EncryptionKey
        {
            get => _encryptionKey;
            set
            {
                _encryptionKey = value;
                SaveConfiguration();
            }
        }

        /// <summary>
        ///     Serialize the configuration to a JSON object.
        ///     All properties have default values and are not null to avoid serialization issues.
        /// </summary>
        /// <returns></returns>
        public override JsonObject JsonSerialize()
        {
            var jobs = new JsonArray();
            foreach (var job in CLEA.EasySaveCore.Core.EasySaveCore.Get().JobManager.GetJobs())
                if (job is IJsonSerializable jsonSerializable)
                    jobs.Add(jsonSerializable.JsonSerialize());

            var extensionsToEncrypt = new JsonArray();
            foreach (var extension in ExtensionsToEncrypt)
                extensionsToEncrypt.Add(extension);

            var processesToBlacklist = new JsonArray();
            foreach (var process in ProcessesToBlacklist)
                processesToBlacklist.Add(process);

            var data = new JsonObject
            {
                { "version", CLEA.EasySaveCore.Core.EasySaveCore.Version.ToString() },
                { "language", L10N.Get().GetLanguage().LangId },
                { "dailyLogPath", Logger.DailyLogPath },
                { "statusLogPath", Logger.StatusLogPath },
                { "dailyLogFormat", Logger.DailyLogFormat.ToString() },
                { "encryptionKey", _encryptionKey },
                { "extensionsToEncrypt", extensionsToEncrypt },
                { "processesToBlacklist", processesToBlacklist },
                { "jobs", jobs }
            };

            return data;
        }

        /// <summary>
        ///     This method deserializes the JSON object into the configuration properties.
        ///     All properties are validated to ensure they are not null and have valid values.
        /// </summary>
        /// <param name="data">The JSON object representing the configuration to deserialize.</param>
        public override void JsonDeserialize(JsonObject data)
        {
            // Version
            data.TryGetPropertyValue("version", out var version);
            if (version == null)
                throw new JsonException("Version not found in configuration file");
            if (version.ToString() !=
                CLEA.EasySaveCore.Core.EasySaveCore.Version.ToString())
                throw new JsonException("Version mismatch in configuration file");

            // Daily log path
            data.TryGetPropertyValue("dailyLogPath", out var dailyLogPath);
            if (dailyLogPath != null && dailyLogPath.ToString().IndexOfAny(Path.GetInvalidPathChars()) == -1)
                Logger.DailyLogPath = dailyLogPath.ToString();
            if (!Directory.Exists(Logger.DailyLogPath))
                Directory.CreateDirectory(Logger.DailyLogPath);

            // Status log path
            data.TryGetPropertyValue("statusLogPath", out var statusLogPath);
            if (statusLogPath != null && statusLogPath.ToString().IndexOfAny(Path.GetInvalidPathChars()) == -1)
                Logger.StatusLogPath = statusLogPath.ToString();
            if (!Directory.Exists(Logger.StatusLogPath))
                Directory.CreateDirectory(Logger.StatusLogPath);

            // Daily log Format
            data.TryGetPropertyValue("dailyLogFormat", out var dailyLogFormat);
            if (dailyLogFormat != null)
                Logger.DailyLogFormat = (Format)Enum.Parse(typeof(Format), dailyLogFormat.ToString());
            if (!Directory.Exists(Logger.DailyLogPath))
                Directory.CreateDirectory(Logger.DailyLogPath);

            // Encryption Key
            data.TryGetPropertyValue("encryptionKey", out var encryptionKey);
            if (encryptionKey != null)
                _encryptionKey = encryptionKey.ToString();

            // Encrypted file extensions
            var extensionsToEncryptList = new ObservableCollection<string>();
            data.TryGetPropertyValue("extensionsToEncrypt", out var extensionsToEncrypt);
            if (extensionsToEncrypt != null)
            {
                foreach (var format in extensionsToEncrypt.AsArray())
                    if (format is JsonValue formatValue)
                        extensionsToEncryptList.Add(formatValue.ToString());

                extensionsToEncryptList.CollectionChanged += (sender, args) => SaveConfiguration();
                ExtensionsToEncrypt = extensionsToEncryptList;
            }
            else
            {
                throw new JsonException("Encrypted file extensions not found in configuration file");
            }

            // Processes to blacklist
            var processesToBlacklistList = new ObservableCollection<string>();
            data.TryGetPropertyValue("processesToBlacklist", out var processesToBlacklist);
            if (processesToBlacklist != null)
            {
                foreach (var process in processesToBlacklist.AsArray())
                    if (process is JsonValue processValue)
                        processesToBlacklistList.Add(processValue.ToString());

                processesToBlacklistList.CollectionChanged += (sender, args) => SaveConfiguration();
                ProcessesToBlacklist = processesToBlacklistList;
            }
            else
            {
                throw new JsonException("Processes to Blacklist not found in configuration file");
            }

            // Jobs
            data.TryGetPropertyValue("jobs", out var jobs);
            if (jobs != null)
            {
                foreach (var job in jobs.AsArray())
                    if (job is JsonObject jobObject)
                        CLEA.EasySaveCore.Core.EasySaveCore.Get().JobManager.AddJob(jobObject);
            }
            else
            {
                throw new JsonException("Jobs not found in configuration file");
            }

            // Language
            data.TryGetPropertyValue("language", out var lang);
            if (lang == null)
                throw new JsonException("Language property not found in configuration file");
            if (Languages.SupportedLangs.Exists(li => li.LangId == lang.ToString()))
                L10N.Get().SetLanguage(Languages.SupportedLangs.Find(li => li.LangId == lang.ToString()) ??
                                       Languages.EnUs);
            else
                throw new JsonException($"Language '{lang}' is not supported");
        }

        public override void SaveConfiguration()
        {
            JsonObject data = JsonSerialize();
            File.WriteAllText(ConfigPath, data.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
        }

        public override void LoadConfiguration()
        {
            var fileStream = new FileStream(ConfigPath, FileMode.OpenOrCreate);
            var streamReader = new StreamReader(fileStream);

            var json = streamReader.ReadToEnd();

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

            var configurationJson = JsonNode.Parse(json);

            if (configurationJson == null)
                throw new JsonException("Failed to parse configuration file");

            JsonDeserialize(configurationJson.AsObject());
            Logger.LogInternal(LogLevel.Debug, "Successfully loaded configuration file");
        }
    }
}