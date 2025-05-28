using System.Collections.Generic;

namespace CLEA.EasySaveCore.Translations
{
    public class LangIdentifier
    {
        public LangIdentifier(string langId, string name, string isoCode)
        {
            LangId = langId;
            Name = name;
            IsoCode = isoCode;
        }

        public string LangId { get; }
        public string Name { get; }
        public string IsoCode { get; }
    }

    public static class Languages
    {
        public static readonly LangIdentifier EnUs = new LangIdentifier("en_us", "English (United States)", "US");
        public static readonly LangIdentifier FrFr = new LangIdentifier("fr_fr", "Français (France)", "FR");
        public static readonly LangIdentifier IdId = new LangIdentifier("id_id", "Indonesia (Indonesia)", "ID");
        public static readonly LangIdentifier MgMg = new LangIdentifier("mg_mg", "Malagasy (Madagascar)", "MG");
        public static readonly LangIdentifier ZhCn = new LangIdentifier("zh_cn", "中文 (简体中文 - 中国)", "CN");

        public static readonly List<LangIdentifier> SupportedLangs = new List<LangIdentifier>
        {
            EnUs,
            FrFr,
            IdId,
            MgMg,
            ZhCn
        };
    }
}