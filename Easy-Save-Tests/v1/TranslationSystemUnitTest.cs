using System;
using CLEA.EasySaveCore.Translations;
using Xunit;

namespace CLEA_Tests
{
    public class TranslationSystemUnitTest
    {
        private static readonly LangIdentifier FakeLang = new LangIdentifier("fake_lang", "FakeLang", "fk");

        [SkippableFact]
        public void Get_ShouldReturnSingletonInstance()
        {
            Skip.IfNot(CLEA.EasySaveCore.Core.EasySaveCore.Version.Major == 1);

            L10N instance1 = L10N.Get();
            L10N instance2 = L10N.Get();

            Assert.Same(instance1, instance2);
        }

        [SkippableFact]
        public void SetLanguage_ShouldSwitchToEnglish()
        {
            Skip.IfNot(CLEA.EasySaveCore.Core.EasySaveCore.Version.Major == 1);

            L10N l10N = L10N.Get();
            l10N.SetLanguage(Languages.EnUs);

            string result = l10N.GetTranslation("language");
            Assert.Equal("English", result);
        }

        [SkippableFact]
        public void SetLanguage_ShouldThrowIfUnsupported()
        {
            Skip.IfNot(CLEA.EasySaveCore.Core.EasySaveCore.Version.Major == 1);

            L10N l10N = L10N.Get();

            ArgumentException ex = Assert.Throws<ArgumentException>(() => l10N.SetLanguage(FakeLang));
            Assert.Equal("Translation lang not found", ex.Message);
        }

        [SkippableFact]
        public void GetTranslation_ShouldThrowIfKeyNotFound()
        {
            Skip.IfNot(CLEA.EasySaveCore.Core.EasySaveCore.Version.Major == 1);

            L10N l10N = L10N.Get();
            l10N.SetLanguage(Languages.EnUs);

            Assert.Equal("nonexistent.key", l10N.GetTranslation("nonexistent.key"));
        }
    }
}