using CLEA.EasySaveCore;
using CLEA.EasySaveCore.L10N;

namespace CLEA_Tests;

public class TranslationSystemUnitTest
{
    private static readonly LangIdentifier FakeLang = new("fake_lang", "FakeLang");
    
    [SkippableFact]
    public void Get_ShouldReturnSingletonInstance()
    {
        Skip.IfNot(EasySaveCore.Version.Major == 1);
        
        var instance1 = L10N.Get();
        var instance2 = L10N.Get();

        Assert.Same(instance1, instance2);
    }

    [SkippableFact]
    public void SetLanguage_ShouldSwitchToEnglish()
    {
        Skip.IfNot(EasySaveCore.Version.Major == 1);
        
        var l10N = L10N.Get();
        l10N.SetLanguage(Languages.EnUs);

        string result = l10N.GetTranslation("language");
        Assert.Equal("English", result);
    }

    [SkippableFact]
    public void SetLanguage_ShouldThrowIfUnsupported()
    {
        Skip.IfNot(EasySaveCore.Version.Major == 1);
        
        var l10N = L10N.Get();

        var ex = Assert.Throws<ArgumentException>(() => l10N.SetLanguage(FakeLang));
        Assert.Equal("Translation lang not found", ex.Message);
    }

    [SkippableFact]
    public void GetTranslation_ShouldThrowIfKeyNotFound()
    {
        Skip.IfNot(EasySaveCore.Version.Major == 1);
        
        var l10N = L10N.Get();
        l10N.SetLanguage(Languages.EnUs);

        Assert.Throws<KeyNotFoundException>(() => l10N.GetTranslation("nonexistent.key"));
    }
}