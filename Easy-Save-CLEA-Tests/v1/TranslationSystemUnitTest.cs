using CLEA;

namespace CLEA_Tests;

public class TranslationSystemUnitTest
{
    private static readonly L10N.Lang FakeLang = new("fake_lang", "FakeLang");
    
    [Fact]
    public void Get_ShouldReturnSingletonInstance()
    {
        var instance1 = L10N.Get();
        var instance2 = L10N.Get();

        Assert.Same(instance1, instance2);
    }

    [Fact]
    public void SetLanguage_ShouldSwitchToEnglish()
    {
        var l10N = L10N.Get();
        l10N.SetLanguage(L10N.EN_US);

        string result = l10N.GetTranslation("language");
        Assert.Equal("English", result);
    }

    [Fact]
    public void SetLanguage_ShouldThrowIfUnsupported()
    {
        var l10N = L10N.Get();

        var ex = Assert.Throws<ArgumentException>(() => l10N.SetLanguage(FakeLang));
        Assert.Equal("Translation lang not found", ex.Message);
    }

    [Fact]
    public void GetTranslation_ShouldThrowIfKeyNotFound()
    {
        var l10N = L10N.Get();
        l10N.SetLanguage(L10N.EN_US);

        Assert.Throws<KeyNotFoundException>(() => l10N.GetTranslation("nonexistent.key"));
    }
}