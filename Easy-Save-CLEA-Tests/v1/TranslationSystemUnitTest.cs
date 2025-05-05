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
        var i18N = L10N.Get();
        i18N.SetLanguage(L10N.EN_US);

        string result = i18N.GetTranslation("language");
        Assert.Equal("English", result);
    }

    [Fact]
    public void SetLanguage_ShouldThrowIfUnsupported()
    {
        var i18N = L10N.Get();

        var ex = Assert.Throws<ArgumentException>(() => i18N.SetLanguage(FakeLang));
        Assert.Equal("Translation lang not found", ex.Message);
    }

    [Fact]
    public void GetTranslation_ShouldThrowIfKeyNotFound()
    {
        var i18N = L10N.Get();
        i18N.SetLanguage(L10N.EN_US);

        Assert.Throws<KeyNotFoundException>(() => i18N.GetTranslation("nonexistent.key"));
    }
}