using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CLEA
{
    public class EasySave
    {
        public static readonly Version Version = new(1, 0, 0);
        private static readonly EasySave Instance = new();

        public ILogger Logger { get; }

        private EasySave()
        {
            using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
            Logger = factory.CreateLogger("EasySave-CLEA");
            Logger.LogInformation("EasySave-CLEA started");
        }
        
        public static EasySave Get()
        {
            return Instance;
        }
    }
    
    public class Program
    {
        public static void Main(string[] args)
        {
            EasySave.Get().Logger.Log(LogLevel.Debug, "Starting EasySave-CLEA");
            EasySave.Get().Logger.Log(LogLevel.Information, "EasySave-CLEA started");
            L10N l10N = L10N.Get();
            
            Console.WriteLine($"{l10N.GetTranslation("main.title")}");
        }
    }

    public class L10N
    {
        public static readonly Lang EN_US = new("en_us", "English");
        public static readonly Lang FR_FR = new("fr_fr", "Français");
        private static readonly List<Lang> SupportedLangs = [EN_US, FR_FR];
        
        private static readonly L10N Instance = new();
        
        private Lang _currentLang;
        private Dictionary<string, string> _translations = new();
        
        private L10N()
        {
            _currentLang = FR_FR;
            LoadTranslations();
        }
        
        public void SetLanguage(Lang lang)
        {
            if(!SupportedLangs.Contains(lang))
                throw new ArgumentException("Translation lang not found");
            
            _currentLang = lang;
            LoadTranslations();
        }

        private void LoadTranslations()
        {
            _translations.Clear();

            string filePath = Path.Combine("l10n", $"{_currentLang.LangId}.json");

            if (!File.Exists(filePath))
                throw new FileNotFoundException("Translation file not found", filePath);

            string json = File.ReadAllText(filePath);
            _translations = JsonConvert.DeserializeObject<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();

            if (_translations.Count <= 0)
                throw new JsonException("Failed to deserialize translation file");
        }

        public string GetTranslation(string key)
        {
            if (!_translations.TryGetValue(key, out var translation))
                throw new KeyNotFoundException($"Translation key '{key}' not found for language '{_currentLang}'");
            
            return translation;
        }
        
        public static L10N Get()
        {
            return Instance;
        }

        public class Lang
        {
            public string LangId { get; }
            private string _name;
            
            public Lang(string langId, string name)
            {
                LangId = langId;
                _name = name;
            }
        }
    }
}