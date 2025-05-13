using System.Reflection;
using CLEA.EasySaveCore.Models;
using CLEA.EasySaveCore.Utilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CLEA.EasySaveCore.L10N;

public class L10N<TJob> where TJob : IJob
{
    private static readonly L10N<TJob> Instance = new L10N<TJob>();

    private LangIdentifier _currentLang;
    private Dictionary<string, string> _translations = new Dictionary<string, string>();

    private L10N()
    {
        _currentLang = Languages.EnUs;
        LoadTranslations();
    }

    public void SetLanguage(LangIdentifier lang)
    {
        if (!Languages.SupportedLangs.Contains(lang))
            throw new ArgumentException("Translation lang not found");

        _currentLang = lang;
        LoadTranslations();
        Utilities.Logger<TJob>.Log(LogLevel.Information, $"Language changed to [{_currentLang.Name}]");
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
        using (var stream = assembly.GetManifestResourceStream(resourceName))
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
        if (!_translations.TryGetValue(key, out var translation))
            throw new KeyNotFoundException($"Translation key '{key}' not found for language '{_currentLang}'");

        return translation;
    }

    public static L10N<TJob> Get()
    {
        return Instance;
    }
}