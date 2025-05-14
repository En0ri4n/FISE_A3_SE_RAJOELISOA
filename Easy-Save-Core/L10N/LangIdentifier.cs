namespace CLEA.EasySaveCore.L10N;

public class LangIdentifier(string langId, string name)
{
    public readonly string LangId = langId;
    public readonly string Name = name;
}

public static class Languages
{
    public static readonly LangIdentifier EnUs = new LangIdentifier("en_us", "English (United States)");
    public static readonly LangIdentifier FrFr = new LangIdentifier("fr_fr", "Français (France)");
    public static readonly LangIdentifier IdId = new LangIdentifier("id_id", "Indonesia (Indonesia)");
    public static readonly LangIdentifier MgMg = new LangIdentifier("mg_mg", "Malagasy (Madagascar)");
    public static readonly LangIdentifier ZhCn = new LangIdentifier("zh_cn", "中文 (简体中文 - 中国)");


    public static readonly List<LangIdentifier> SupportedLangs = [EnUs, FrFr, IdId, MgMg, ZhCn];
}