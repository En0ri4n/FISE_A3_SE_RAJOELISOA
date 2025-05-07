namespace CLEA.EasySaveCore.L10N;

public class LangIdentifier(string langId, string name)
{
    public string LangId { get; } = langId;
    private string _name = name;
}

public static class Languages
{
    public static readonly LangIdentifier EnUs = new LangIdentifier("en_us", "English (United States)");
    public static readonly LangIdentifier FrFr = new("fr_fr", "Français (France)");

    public static readonly List<LangIdentifier> SupportedLangs = [EnUs, FrFr];
}