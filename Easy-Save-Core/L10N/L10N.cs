using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CLEA.EasySaveCore.Utilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CLEA.EasySaveCore.Translations
{
    public class L10N
    {
        private static readonly L10N Instance = new L10N();

        private LangIdentifier _currentLang;
        private Dictionary<string, string> _translations = new Dictionary<string, string>();

        private L10N()
        {
            _currentLang = Languages.EnUs;
            LoadTranslations();
        }

        public static event EventHandler? LanguageChanged;

        public void SetLanguage(LangIdentifier lang)
        {
            if (!Languages.SupportedLangs.Contains(lang))
                throw new ArgumentException("Translation lang not found");

            _currentLang = lang;
            LoadTranslations();
            LanguageChanged?.Invoke(null, EventArgs.Empty);
            Logger.Log(LogLevel.Information, $"Language changed to [{_currentLang.Name}]");
        }

        public LangIdentifier GetLanguage()
        {
            return _currentLang;
        }

        private void LoadTranslations()
        {
            _translations.Clear();

            // Construct the resource name based on the current language
            string resourceName = $"EasySaveCore.Assets.Lang.{_currentLang.LangId}.json";

            // Get the current assembly
            Assembly assembly = Assembly.GetExecutingAssembly();

            // Try to load the resource
            using (Stream? stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    throw new FileNotFoundException("Translation file not found", resourceName);

                // Read the JSON content from the stream
                using (StreamReader reader = new StreamReader(stream))
                {
                    string json = reader.ReadToEnd();
                    _translations = JsonConvert.DeserializeObject<Dictionary<string, string>>(json) ??
                                    new Dictionary<string, string>();
                }
            }

            // Check if the deserialization was successful
            if (_translations.Count <= 0)
                throw new JsonException("Failed to deserialize translation file");
        }
        
        public string GetTranslation(string key, string[]? parameters = null)
        {
            _translations.TryGetValue(key, out string? translation);

            return translation ?? key;
        }

        public static L10N Get()
        {
            return Instance;
        }
    }
}